using Google.Protobuf;
using POGOProtos.Networking.Envelopes;
using PokemonGo.RocketAPI.Exceptions;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Extensions
{
    using System;
    using System.Collections.Generic;
    public enum ApiOperation
    {
        Retry,
        Abort
    }

    public interface IApiStrategy
    {
        Task HandleApiStart(RequestEnvelope request);
        Task<ApiOperation> HandleApiFailure(RequestEnvelope request, ResponseEnvelope response);
        void HandleApiSuccess(RequestEnvelope request, ResponseEnvelope response);
    }

    public static class HttpClientExtensions
    {
        public static async Task<IMessage[]> PostProtoPayload<TRequest>(this System.Net.Http.HttpClient client, 
            string url, RequestEnvelope requestEnvelope,
            IApiStrategy strategy,
            params Type[] responseTypes) where TRequest : IMessage<TRequest>
        {
            await strategy.HandleApiStart(requestEnvelope);

            var result = new IMessage[responseTypes.Length];
            for (var i = 0; i < responseTypes.Length; i++)
            {
                result[i] = Activator.CreateInstance(responseTypes[i]) as IMessage;
                if (result[i] == null)
                {
                    throw new ArgumentException($"ResponseType {i} is not an IMessage");
                }
            }

            ResponseEnvelope response;
            while ((response = await PostProto<TRequest>(client, url, requestEnvelope)).Returns.Count != responseTypes.Length)
            {
                var operation = await strategy.HandleApiFailure(requestEnvelope, response);
                if (operation == ApiOperation.Abort)
                {
                    throw new InvalidResponseException($"Expected {responseTypes.Length} responses, but got {response.Returns.Count} responses");
                }
            }

            strategy.HandleApiSuccess(requestEnvelope, response);

            for (var i = 0; i < responseTypes.Length; i++)
            {
                var payload = response.Returns[i];
                result[i].MergeFrom(payload);
            }
            return result;
        }

        public static async Task<TResponsePayload> PostProtoPayload<TRequest, TResponsePayload>(this System.Net.Http.HttpClient client,
            string url, RequestEnvelope requestEnvelope, IApiStrategy strategy) where TRequest : IMessage<TRequest>
            where TResponsePayload : IMessage<TResponsePayload>, new()
        {
            await strategy.HandleApiStart(requestEnvelope);
            var response = await PostProto<TRequest>(client, url, requestEnvelope);

            while (response.Returns.Count == 0)
            {
                var operation = await strategy.HandleApiFailure(requestEnvelope, response);
                if (operation == ApiOperation.Abort)
                {
                    break;
                }

                response = await PostProto<TRequest>(client, url, requestEnvelope);
            }

            if (response.Returns.Count == 0)
                throw new InvalidResponseException();

            strategy.HandleApiSuccess(requestEnvelope, response);

            //Decode payload
            //todo: multi-payload support
            var payload = response.Returns[0];
            var parsedPayload = new TResponsePayload();
            parsedPayload.MergeFrom(payload);

            return parsedPayload;
        }

        public static async Task<ResponseEnvelope> PostProto<TRequest>(this System.Net.Http.HttpClient client, string url,
            RequestEnvelope requestEnvelope) where TRequest : IMessage<TRequest>
        {
            //Encode payload and put in envelop, then send
            var data = requestEnvelope.ToByteString();
            var result = await client.PostAsync(url, new ByteArrayContent(data.ToByteArray()));

            //Decode message
            var responseData = await result.Content.ReadAsByteArrayAsync();
            var codedStream = new CodedInputStream(responseData);
            var decodedResponse = new ResponseEnvelope();
            decodedResponse.MergeFrom(codedStream);

            return decodedResponse;
        }
    }
}