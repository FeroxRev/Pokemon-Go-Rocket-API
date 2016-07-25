using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PokemonGo.RocketAPI.Helpers;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;

namespace PokemonGo.RocketAPI.Rpc
{
    public class Download : BaseRpc
    {
        public Download(Client client) : base(client)
        {
        }
        public async Task<DownloadSettingsResponse> GetSettings()
        {
            var message = new DownloadSettingsMessage
            {
                Hash = "05daf51635c82611d1aac95c0b051d3ec088a930"
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.DownloadSettings, message);
            return await PostProtoPayload<Request, DownloadSettingsResponse>(ApiUrl, request);
        }

        public async Task<DownloadItemTemplatesResponse> GetItemTemplates()
        {
            var request = RequestBuilder.GetRequestEnvelope(RequestType.DownloadItemTemplates, new DownloadItemTemplatesMessage());
            return await PostProtoPayload<Request, DownloadItemTemplatesResponse>(ApiUrl, request);
        }

    }
}
