using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Helpers;
using POGOProtos.Networking.Envelopes;

namespace PokemonGo.RocketAPI.Rpc
{
    public class BaseRpc
    {
        protected Client _client;
        protected RequestBuilder RequestBuilder => new RequestBuilder(_client.AuthToken, _client.AuthType, _client.CurrentLatitude, _client.CurrentLongitude, _client.CurrentAltitude, _client.AuthTicket);
        protected string ApiUrl => $"https://{_client.ApiUrl}/rpc";
        protected BaseRpc(Client client)
        {
            _client = client;
        }

        public async Task<TResponsePayload> PostProtoPayload<TRequest, TResponsePayload>(
            string url, RequestEnvelope requestEnvelope) where TRequest : IMessage<TRequest>
            where TResponsePayload : IMessage<TResponsePayload>, new()
        {
            return await _client.PokemonHttpClient.PostProtoPayload<TRequest, TResponsePayload>(url, requestEnvelope);
        }

        public async Task<ResponseEnvelope> PostProto<TRequest>(string url,
            RequestEnvelope requestEnvelope) where TRequest : IMessage<TRequest>
        {
            return await _client.PokemonHttpClient.PostProto<TRequest>(url, requestEnvelope);
        }
    }
}
