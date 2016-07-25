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
    public class Inventory : BaseRpc
    {
        public Inventory(Client client) : base(client)
        {
        }

        public async Task<ReleasePokemonResponse> TransferPokemon(ulong pokemonId)
        {
            var message = new ReleasePokemonMessage
            {
                PokemonId = pokemonId
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.ReleasePokemon, message);
            return await PostProtoPayload<Request, ReleasePokemonResponse>(ApiUrl, request);
        }

        public async Task<EvolvePokemonResponse> EvolvePokemon(ulong pokemonId)
        {
            var message = new EvolvePokemonMessage
            {
                PokemonId = pokemonId
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.EvolvePokemon, message);
            return await PostProtoPayload<Request, EvolvePokemonResponse>(ApiUrl, request);
        }

        public async Task<GetInventoryResponse> GetInventory()
        {
            var request = RequestBuilder.GetRequestEnvelope(RequestType.GetInventory, new GetInventoryMessage());
            return await PostProtoPayload<Request, GetInventoryResponse>(ApiUrl, request);
        }

        public async Task<RecycleInventoryItemResponse> RecycleItem(ItemId itemId, int amount)
        {
            var message = new RecycleInventoryItemMessage
            {
                ItemId = itemId,
                Count = amount
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.RecycleInventoryItem, message);
            return await PostProtoPayload<Request, RecycleInventoryItemResponse>(ApiUrl, request);
        }

        public async Task<UseItemXpBoostResponse> UseItemXpBoost()
        {
            var message = new UseItemXpBoostMessage()
            {
                ItemId = ItemId.ItemLuckyEgg
            };

            var request = RequestBuilder.GetRequestEnvelope(RequestType.UseItemXpBoost, message);
            return await PostProtoPayload<Request, UseItemXpBoostResponse>(ApiUrl, request);
        }
    }
}