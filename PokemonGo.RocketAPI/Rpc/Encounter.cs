using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;

namespace PokemonGo.RocketAPI.Rpc
{
    public class Encounter : BaseRpc
    {
        public Encounter(Client client) : base(client) { }

        public async Task<EncounterResponse> EncounterPokemon(ulong encounterId, string spawnPointGuid)
        {
            var message = new EncounterMessage
            {
                EncounterId = encounterId,
                SpawnPointId = spawnPointGuid,
                PlayerLatitude = _client.CurrentLatitude,
                PlayerLongitude = _client.CurrentLongitude
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.Encounter, message);
            return await PostProtoPayload<Request, EncounterResponse>(ApiUrl, request);
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
            return await PostProtoPayload<Request, UseItemCaptureResponse>(ApiUrl, request);
        }

        public async Task<CatchPokemonResponse> CatchPokemon(ulong encounterId, string spawnPointGuid, ItemId pokeballItemId)
        {
            var message = new CatchPokemonMessage
            {
                EncounterId = encounterId,
                Pokeball = pokeballItemId,
                SpawnPointId = spawnPointGuid,
                HitPokemon = true,
                NormalizedReticleSize = 1.950,
                SpinModifier = 1,
                NormalizedHitPosition = 1
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.CatchPokemon, message);
            return await PostProtoPayload<Request, CatchPokemonResponse>(ApiUrl, request);
        }
    }
}
