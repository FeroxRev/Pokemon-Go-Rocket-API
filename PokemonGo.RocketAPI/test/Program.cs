using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using PokemonGo.RocketAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            string t1 = "AAAAAAAAAAAAAAAAAAAAAAAAAAA";
            string t2 = "BBBBBBBBBBBBBBBBBBBBBBBBBBB";
            AuthTicket auticket = new AuthTicket
            {
                Start = Google.Protobuf.ByteString.CopyFrom(t1, Encoding.UTF8),
                ExpireTimestampMs = 0,
                End = Google.Protobuf.ByteString.CopyFrom(t1, Encoding.UTF8),
            };   
            RequestBuilder rb = new RequestBuilder("AAAAAAAAAAAAAAAA", PokemonGo.RocketAPI.Enums.AuthType.Ptc, 10, 10, 10, auticket);

            var message = new DownloadSettingsMessage
            {
                Hash = "05daf51635c82611d1aac95c0b051d3ec088a930"
            };

            var requestEnvelops = rb.GetRequestEnvelope(RequestType.DownloadSettings, message);
        }
    }
}
