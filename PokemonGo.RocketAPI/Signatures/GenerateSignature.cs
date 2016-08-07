using Google.Protobuf;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Requests;
using PokemonGo.RocketAPI.Extensions;
using System;
using System.Collections.Generic;
using System.Data.HashFunction;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PokemonGo.RocketAPI.Signatures
{
    public class GenerateSignature
    {
        private readonly ulong START_TIME;

        public GenerateSignature()
        {
            START_TIME = (ulong)DateTime.UtcNow.ToUnixTime();
        }

        private int generateLocation1(AuthTicket authTicket, double lat, double lon, double alt)
        {
            xxHash hash = new xxHash(32, 0x1B845238);
            var firstHash = BitConverter.ToUInt32(hash.ComputeHash(authTicket.ToByteArray()), 0);
            List<byte> locationBytes = new List<byte>();
            locationBytes.AddRange(BitConverter.GetBytes(lat));
            locationBytes.AddRange(BitConverter.GetBytes(lon));
            locationBytes.AddRange(BitConverter.GetBytes(alt));

            hash = new xxHash(32, firstHash);
            return BitConverter.ToInt32(hash.ComputeHash(locationBytes), 0);
        }

        private int generateLocation2(AuthTicket authTicket, double lat, double lon, double alt)
        {
            xxHash hash = new xxHash(32, 0x1B845238);
            List<byte> locationBytes = new List<byte>();
            locationBytes.AddRange(BitConverter.GetBytes(lat));
            locationBytes.AddRange(BitConverter.GetBytes(lon));
            locationBytes.AddRange(BitConverter.GetBytes(alt));
            return BitConverter.ToInt32(hash.ComputeHash(locationBytes), 0);
        }

        private UInt64 generateRequestHash(AuthTicket authTicket, Request request)
        {
            xxHash hash = new xxHash(64, 0x1B845238);
            var firstHash = BitConverter.ToUInt64(hash.ComputeHash(authTicket.ToByteArray()), 0);

            hash = new xxHash(64, firstHash);
            return BitConverter.ToUInt64(hash.ComputeHash(request.ToByteArray()), 0);
        }

        [DllImport("encrypt.dll")]
        //private static extern int encrypt([MarshalAs(UnmanagedType.LPArray)] byte[] input, int input_size, [MarshalAs(UnmanagedType.LPArray)] byte[] iv, int iv_size, [MarshalAs(UnmanagedType.LPArray)] byte[] output, ref int output_size);
        private static extern int encrypt([MarshalAs(UnmanagedType.LPArray)] byte[] input, int input_size, [MarshalAs(UnmanagedType.LPArray)] byte[] iv, int iv_size, [MarshalAs(UnmanagedType.LPArray)] byte[] output, ref int output_size);

        private byte[] generatesign(POGOProtos.Signature.Signature sign)
        {
            Random rnd = new Random();
            Byte[] iv = new Byte[32];
            rnd.NextBytes(iv);

            int output_size = 0;
            byte[] input = sign.ToByteArray();
            int ret = encrypt(input, input.Length, iv, iv.Length, null, ref output_size);
            byte[] output = new byte[output_size];
            ret = encrypt(input, input.Length, iv, iv.Length, output, ref output_size);

            return output;
        }

        public RequestEnvelope generateUnknown6(RequestEnvelope request)
        {
            var sign = new POGOProtos.Signature.Signature();

            if (request.AuthTicket != null)
            {
                sign.LocationHash1 = (uint)generateLocation1(request.AuthTicket, request.Latitude, request.Longitude, request.Altitude);
                sign.LocationHash2 = (uint)generateLocation2(request.AuthTicket, request.Latitude, request.Longitude, request.Altitude);
                foreach (Request req in request.Requests)
                {
                    var hash = generateRequestHash(request.AuthTicket, req);
                    sign.RequestHash.Add(hash);
                }

                Random rnd = new Random();
                Byte[] bytes = new Byte[32];
                rnd.NextBytes(bytes);

                sign.Unk22 = Google.Protobuf.ByteString.CopyFrom(bytes);
                sign.Timestamp = (ulong)DateTime.UtcNow.ToUnixTime();
                sign.TimestampSinceStart = sign.Timestamp - START_TIME;

                byte[] gensign = generatesign(sign);

                Unknown6 u6 = new Unknown6()
                {
                    RequestType = 6,
                    Unknown2 = new Unknown6.Types.Unknown2
                    {
                        Unknown1 = Google.Protobuf.ByteString.CopyFrom(gensign)
                    }
                };
                request.Unknown6.Add(u6);
            }

            return request;
        }
    }
}
