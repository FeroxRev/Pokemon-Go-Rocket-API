using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.Login;
using POGOProtos.Inventory;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;

namespace PokemonGo.RocketAPI
{
    public class Client
    {
        private readonly HttpClient _httpClient;
        private string _apiUrl;
        private AuthTicket _authTicket;
        private AuthType _authType = AuthType.Google;

        public Client(ISettings settings)
        {
            Settings = settings;
            SetCoordinates(Settings.DefaultLatitude, Settings.DefaultLongitude, Settings.DefaultAltitude);

            //Setup HttpClient and create default headers
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                AllowAutoRedirect = false
            };
            _httpClient = new HttpClient(new RetryHandler(handler));
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Niantic App");
            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type",
                "application/x-www-form-urlencoded");
        }

        public ISettings Settings { get; }
        public string AuthToken { get; set; }

        public double CurrentLatitude { get; private set; }
        public double CurrentLongitude { get; private set; }
        public double CurrentAltitude { get; private set; }

        private RequestBuilder RequestBuilder => new RequestBuilder(AuthToken, _authType, CurrentLatitude, CurrentLongitude, CurrentAltitude, _authTicket);

        private void SetCoordinates(double lat, double lng, double altitude)
        {
            CurrentLatitude = lat;
            CurrentLongitude = lng;
            CurrentAltitude = altitude;
        }

        public async Task DoGoogleLogin()
        {
            _authType = AuthType.Google;

            GoogleLogin.TokenResponseModel tokenResponse = null;
            if (Settings.GoogleRefreshToken != string.Empty)
            {
                tokenResponse = await GoogleLogin.GetAccessToken(Settings.GoogleRefreshToken);
                AuthToken = tokenResponse?.id_token;
            }

            if (AuthToken == null)
            {
                var deviceCode = await GoogleLogin.GetDeviceCode();
                tokenResponse = await GoogleLogin.GetAccessToken(deviceCode);
                Settings.GoogleRefreshToken = tokenResponse?.refresh_token;
                AuthToken = tokenResponse?.id_token;
            }
        }

        /// <summary>
        ///     For GUI clients only. GUI clients don't use the DoGoogleLogin, but call the GoogleLogin class directly
        /// </summary>
        /// <param name="type"></param>
        public void SetAuthType(AuthType type)
        {
            _authType = type;
        }

        public async Task DoPtcLogin(string username, string password)
        {
            AuthToken = await PtcLogin.GetAccessToken(username, password);
            _authType = AuthType.Ptc;
        }

        public async Task<PlayerUpdateResponse> UpdatePlayerLocation(double latitude, double longitude, double altitude)
        {
            SetCoordinates(latitude, longitude, altitude);
            var message = new PlayerUpdateMessage
            {
                Latitude = CurrentLatitude,
                Longitude = CurrentLongitude
            };

            var updatePlayerLocationRequestEnvelope = RequestBuilder.GetRequestEnvelope(
                new Request
                {
                    RequestType = RequestType.PlayerUpdate,
                    RequestMessage = message.ToByteString()
                });

            return await _httpClient.PostProtoPayload<Request, PlayerUpdateResponse>($"https://{_apiUrl}/rpc", updatePlayerLocationRequestEnvelope);
        }

        public async Task SetServer()
        {
            #region Standard intial request messages in right Order

            var getPlayerMessage = new GetPlayerMessage();
            var getHatchedEggsMessage = new GetHatchedEggsMessage();
            var getInventoryMessage = new GetInventoryMessage
            {
                LastTimestampMs = DateTime.UtcNow.ToUnixTime()
            };
            var checkAwardedBadgesMessage = new CheckAwardedBadgesMessage();
            var downloadSettingsMessage = new DownloadSettingsMessage
            {
                Hash = "05daf51635c82611d1aac95c0b051d3ec088a930"
            };

            #endregion

            var serverRequest = RequestBuilder.GetRequestEnvelope(
                new Request
                {
                    RequestType = RequestType.GetPlayer,
                    RequestMessage = getPlayerMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.GetHatchedEggs,
                    RequestMessage = getHatchedEggsMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.GetInventory,
                    RequestMessage = getInventoryMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.CheckAwardedBadges,
                    RequestMessage = checkAwardedBadgesMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.DownloadSettings,
                    RequestMessage = downloadSettingsMessage.ToByteString()
                });


            var serverResponse = await _httpClient.PostProto<Request>(Resources.RpcUrl, serverRequest);

            if (serverResponse.AuthTicket == null)
                throw new AccessTokenExpiredException();

            _authTicket = serverResponse.AuthTicket;
            _apiUrl = serverResponse.ApiUrl;
        }

        public async Task<GetPlayerResponse> GetOwnProfile()
        {
            var request = RequestBuilder.GetRequestEnvelope(RequestType.GetPlayer, new GetPlayerMessage());
            return await _httpClient.PostProtoPayload<Request, GetPlayerResponse>($"https://{_apiUrl}/rpc", request);
        }

        public async Task<DownloadSettingsResponse> GetSettings()
        {
            var message = new DownloadSettingsMessage
            {
                Hash = "05daf51635c82611d1aac95c0b051d3ec088a930"
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.DownloadSettings, message);
            return await _httpClient.PostProtoPayload<Request, DownloadSettingsResponse>($"https://{_apiUrl}/rpc", request);
        }

        public async Task<DownloadItemTemplatesResponse> GetItemTemplates()
        {
            var request = RequestBuilder.GetRequestEnvelope(RequestType.DownloadItemTemplates, new DownloadItemTemplatesMessage());
            return await _httpClient.PostProtoPayload<Request, DownloadItemTemplatesResponse>($"https://{_apiUrl}/rpc", request);
        }


        public async Task<GetMapObjectsResponse> GetMapObjects()
        {
            #region Messages

            var getMapObjectsMessage = new GetMapObjectsMessage
            {
                CellId = {S2Helper.GetNearbyCellIds(CurrentLongitude, CurrentLatitude)},
                SinceTimestampMs = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                Latitude = CurrentLatitude,
                Longitude = CurrentLongitude
            };
            var getHatchedEggsMessage = new GetHatchedEggsMessage();
            var getInventoryMessage = new GetInventoryMessage
            {
                LastTimestampMs = DateTime.UtcNow.ToUnixTime()
            };
            var checkAwardedBadgesMessage = new CheckAwardedBadgesMessage();
            var downloadSettingsMessage = new DownloadSettingsMessage
            {
                Hash = "05daf51635c82611d1aac95c0b051d3ec088a930"
            };

            #endregion

            var request = RequestBuilder.GetRequestEnvelope(
                new Request
                {
                    RequestType = RequestType.GetMapObjects,
                    RequestMessage = getMapObjectsMessage.ToByteString()
                },
                new Request
                {
                    RequestType = RequestType.GetHatchedEggs,
                    RequestMessage = getHatchedEggsMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.GetInventory,
                    RequestMessage = getInventoryMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.CheckAwardedBadges,
                    RequestMessage = checkAwardedBadgesMessage.ToByteString()
                }, new Request
                {
                    RequestType = RequestType.DownloadSettings,
                    RequestMessage = downloadSettingsMessage.ToByteString()
                });

            return await _httpClient.PostProtoPayload<Request, GetMapObjectsResponse>($"https://{_apiUrl}/rpc", request);
        }

        public async Task<FortDetailsResponse> GetFort(string fortId, double fortLatitude, double fortLongitude)
        {
            var message = new FortDetailsMessage
            {
                FortId = fortId,
                Latitude = fortLatitude,
                Longitude = fortLongitude
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.FortDetails, message);
            return await _httpClient.PostProtoPayload<Request, FortDetailsResponse>($"https://{_apiUrl}/rpc", request);
        }

        public async Task<FortSearchResponse> SearchFort(string fortId, double fortLat, double fortLng)
        {
            var message = new FortSearchMessage
            {
                FortId = fortId,
                FortLatitude = fortLat,
                FortLongitude = fortLng,
                PlayerLatitude = CurrentLatitude,
                PlayerLongitude = CurrentLongitude
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.FortSearch, message);
            return await _httpClient.PostProtoPayload<Request, FortSearchResponse>($"https://{_apiUrl}/rpc", request);
        }

        public async Task<EncounterResponse> EncounterPokemon(ulong encounterId, string spawnPointGuid)
        {
            var message = new EncounterMessage
            {
                EncounterId = encounterId,
                SpawnPointId = spawnPointGuid,
                PlayerLatitude = CurrentLatitude,
                PlayerLongitude = CurrentLongitude
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.Encounter, message);
            return await _httpClient.PostProtoPayload<Request, EncounterResponse>($"https://{_apiUrl}/rpc", request);
        }

        public async Task<UseItemCaptureResponse> UseCaptureItem(ulong encounterId, ItemId itemId, string spawnPointGuid)
        {
            var message = new UseItemCaptureMessage
            {
                EncounterId = encounterId,
                ItemId = itemId,
                SpawnPointGuid = spawnPointGuid
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.UseItemCapture, message);
            return await _httpClient.PostProtoPayload<Request, UseItemCaptureResponse>($"https://{_apiUrl}/rpc", request);
        }

        public async Task<CatchPokemonResponse> CatchPokemon(ulong encounterId, string spawnPointGuid, ItemId pokeballItemId)
        {
            var message = new CatchPokemonMessage
            {
                EncounterId = encounterId,
                Pokeball = (int) pokeballItemId,
                SpawnPointGuid = spawnPointGuid,
                HitPokemon = true,
                NormalizedReticleSize = 1.950,
                SpinModifier = 1,
                NormalizedHitPosition = 1
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.CatchPokemon, message);
            return await _httpClient.PostProtoPayload<Request, CatchPokemonResponse>($"https://{_apiUrl}/rpc", request);
        }

        public async Task<ReleasePokemonResponse> TransferPokemon(ulong pokemonId)
        {
            var message = new ReleasePokemonMessage
            {
                PokemonId = pokemonId
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.ReleasePokemon, message);
            return await _httpClient.PostProtoPayload<Request, ReleasePokemonResponse>($"https://{_apiUrl}/rpc", request);
        }

        public async Task<EvolvePokemonResponse> EvolvePokemon(ulong pokemonId)
        {
            var message = new EvolvePokemonMessage
            {
                PokemonId = pokemonId
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.EvolvePokemon, message);
            return await _httpClient.PostProtoPayload<Request, EvolvePokemonResponse>($"https://{_apiUrl}/rpc", request);
        }

        public async Task<GetInventoryResponse> GetInventory()
        {
            var request = RequestBuilder.GetRequestEnvelope(RequestType.GetInventory, new GetInventoryMessage());
            return await _httpClient.PostProtoPayload<Request, GetInventoryResponse>($"https://{_apiUrl}/rpc", request);
        }

        public async Task<RecycleInventoryItemResponse> RecycleItem(ItemId itemId, int amount)
        {
            var message = new RecycleInventoryItemMessage
            {
                ItemId = itemId,
                Count = amount
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.RecycleInventoryItem, message);
            return await _httpClient.PostProtoPayload<Request, RecycleInventoryItemResponse>($"https://{_apiUrl}/rpc", request);
        }

        public async Task<UseItemXpBoostResponse> UseItemXpBoost()
        {
            var message = new UseItemXpBoostMessage()
            {
                ItemId = ItemId.ItemLuckyEgg
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.UseItemXpBoost, message);
            return await _httpClient.PostProtoPayload<Request, UseItemXpBoostResponse>($"https://{_apiUrl}/rpc", request);


        }
    }
}