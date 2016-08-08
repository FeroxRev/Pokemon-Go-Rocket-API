using POGOProtos.Networking.Envelopes;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using PokemonGo.RocketAPI.Helpers;
using PokemonGo.RocketAPI.HttpClient;

namespace PokemonGo.RocketAPI
{
    public class Client
    {
        public Rpc.Login Login;
        public Rpc.Player Player;
        public Rpc.Download Download;
        public Rpc.Inventory Inventory;
        public Rpc.Map Map;
        public Rpc.Fort Fort;
        public Rpc.Encounter Encounter;
        public Rpc.Misc Misc;
        public POGOProtos.Networking.Signature.Types.DeviceInfo DeviceInfo { get; }

        public IApiStrategy ApiFailure { get; set; }
        public ISettings Settings { get; }
        public string AuthToken { get; set; }

        public double CurrentLatitude { get; internal set; }
        public double CurrentLongitude { get; internal set; }
        public double CurrentAltitude { get; internal set; }

        public AuthType AuthType => Settings.AuthType;

        internal readonly PokemonHttpClient PokemonHttpClient = new PokemonHttpClient();
        internal string ApiUrl { get; set; }
        internal AuthTicket AuthTicket { get; set; }

        internal readonly RequestBuilder RequestBuilder;

        public Client(ISettings settings, IApiStrategy apiFailureStrategy, POGOProtos.Networking.Signature.Types.DeviceInfo deviceInfo)
        {
            Settings = settings;
            ApiFailure = apiFailureStrategy;
            RequestBuilder = new RequestBuilder(this);
            DeviceInfo = deviceInfo;

            Login = new Rpc.Login(this);
            Player = new Rpc.Player(this);
            Download = new Rpc.Download(this);
            Inventory = new Rpc.Inventory(this);
            Map = new Rpc.Map(this);
            Fort = new Rpc.Fort(this);
            Encounter = new Rpc.Encounter(this);
            Misc = new Rpc.Misc(this);

            Player.SetCoordinates(Settings.DefaultLatitude, Settings.DefaultLongitude, Settings.DefaultAltitude);
        }
    }
}