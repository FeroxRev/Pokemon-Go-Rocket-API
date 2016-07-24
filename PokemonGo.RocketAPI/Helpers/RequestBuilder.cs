using PokemonGo.RocketAPI.Enums;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Requests;
using static POGOProtos.Networking.Envelopes.RequestEnvelope.Types;

namespace PokemonGo.RocketAPI.Helpers
{
    public static class RequestBuilder
    {
        public static RequestEnvelope GetRequestEnvelope(string authToken, AuthType authType, double latitude,
            double longitude, double altitude, AuthTicket authTicket = null, params Request[] customRequests)
        {
            return new RequestEnvelope
            {
                StatusCode = 2, //1

                RequestId = 1469378659230941192, //3
                Requests = {customRequests}, //4

                //Unknown6 = , //6
                Latitude = latitude, //7
                Longitude = longitude, //8
                Altitude = altitude, //9
                AuthInfo = new AuthInfo
                {
                    Provider = authType == AuthType.Google ? "google" : "ptc",
                    Token = new AuthInfo.Types.JWT
                    {
                        Contents = authToken,
                        Unknown2 = 14
                    }
                }, //10
                AuthTicket = authTicket, //11
                Unknown12 = 989 //12
            };
        }
    }
}