using System;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Requests;
using static POGOProtos.Networking.Envelopes.RequestEnvelope.Types;

namespace PokemonGo.RocketAPI.Helpers
{
    public class RequestBuilder
    {
        private readonly Client _client;
        private ulong _nextRequestId;

        public RequestBuilder(Client client)
        {
            _client = client;
            //_nextRequestId = 1469378659230941192;
            _nextRequestId = Convert.ToUInt64((new Random()).NextDouble() * Math.Pow(10, 18));  //1469378659230941192;
        }

        public RequestEnvelope GetRequestEnvelope(params Request[] customRequests)
        {
            return new RequestEnvelope
            {
                StatusCode = 2, //1

                RequestId = _nextRequestId++, //3
                Requests = { customRequests }, //4

                //Unknown6 = , //6
                Latitude = _client.CurrentLatitude, //7
                Longitude = _client.CurrentLongitude, //8
                Altitude = _client.CurrentAltitude, //9
                AuthTicket = _client.AuthTicket, //11
                Unknown12 = 989 //12
            };
        }

        public RequestEnvelope GetInitialRequestEnvelope(params Request[] customRequests)
        {
            return new RequestEnvelope
            {
                StatusCode = 2, //1

                RequestId = _nextRequestId++, //3
                Requests = { customRequests }, //4

                //Unknown6 = , //6
                Latitude = _client.CurrentLatitude, //7
                Longitude = _client.CurrentLongitude, //8
                Altitude = _client.CurrentAltitude, //9
                AuthInfo = new AuthInfo
                {
                    Provider = _client.AuthType == AuthType.Google ? "google" : "ptc",
                    Token = new AuthInfo.Types.JWT
                    {
                        Contents = _client.AuthToken,
                        Unknown2 = 14
                    }
                }, //10
                Unknown12 = 989 //12
            };
        }

        public RequestEnvelope GetRequestEnvelope(RequestType type, IMessage message)
        {
            return GetRequestEnvelope(new Request()
            {
                RequestType = type,
                RequestMessage = message.ToByteString()
            });

        }
    }
}