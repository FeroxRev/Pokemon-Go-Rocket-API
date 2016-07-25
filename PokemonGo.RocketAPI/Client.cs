using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Google.Protobuf;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Exceptions;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.HttpClient;
using PokemonGo.RocketAPI.Login;
using POGOProtos.Inventory;
using POGOProtos.Inventory.Item;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Requests;
using POGOProtos.Networking.Requests.Messages;
using POGOProtos.Networking.Responses;

namespace PokemonGo.RocketAPI
{
    public class Client
    {
        public Rpc.Login Login => new Rpc.Login(this);
        public Rpc.Player Player => new Rpc.Player(this);
        public Rpc.Download Download => new Rpc.Download(this);
        public Rpc.Inventory Inventory => new Rpc.Inventory(this);
        public Rpc.Map Map => new Rpc.Map(this);
        public Rpc.Fort Fort => new Rpc.Fort(this);
        public Rpc.Encounter Encounter => new Rpc.Encounter(this);
        public Rpc.Misc Misc => new Rpc.Misc(this);

        public ISettings Settings { get; }
        public string AuthToken { get; set; }

        public double CurrentLatitude { get; internal set; }
        public double CurrentLongitude { get; internal set; }
        public double CurrentAltitude { get; internal set; }

        public AuthType AuthType { get; set; } = AuthType.Google;

        internal readonly PokemonHttpClient PokemonHttpClient = new PokemonHttpClient();
        internal string ApiUrl { get; set; }
        internal AuthTicket AuthTicket { get; set; }

        public Client(ISettings settings)
        {
            Settings = settings;
            Player.SetCoordinates(Settings.DefaultLatitude, Settings.DefaultLongitude, Settings.DefaultAltitude);
        }
    }
}