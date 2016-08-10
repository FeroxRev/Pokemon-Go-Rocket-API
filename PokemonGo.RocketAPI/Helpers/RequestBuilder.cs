using Google.Protobuf;
using POGOProtos.Networking.Envelopes;
using POGOProtos.Networking.Requests;
using PokemonGo.RocketAPI.Enums;
using PokemonGo.RocketAPI.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace PokemonGo.RocketAPI.Helpers
{
    public class RequestBuilder
    {
        private readonly Stopwatch _internalWatch = new Stopwatch();
        private readonly Client _client;
        private ulong _nextRequestId;

        public RequestBuilder(Client client)
        {
            _client = client;
            _nextRequestId = Convert.ToUInt64((new Random()).NextDouble() * Math.Pow(10, 18));  //1469378659230941192;
            if (!_internalWatch.IsRunning) _internalWatch.Start();
        }

        private Unknown6 GenerateSignature(IEnumerable<IMessage> requests)
        {
            var sig = new POGOProtos.Networking.Signature();
            sig.TimestampSinceStart = (ulong)_internalWatch.ElapsedMilliseconds;
            sig.Timestamp = (ulong)DateTime.UtcNow.ToUnixTime();
            sig.SensorInfo = new POGOProtos.Networking.Signature.Types.SensorInfo()
            {
                AccelNormalizedZ = GenRandom(9.8),
                AccelNormalizedX = GenRandom(0.02),
                AccelNormalizedY = GenRandom(0.3),
                TimestampSnapshot = (ulong)_internalWatch.ElapsedMilliseconds - 230,
                MagnetometerX = GenRandom(0.12271042913198471),
                MagnetometerY = GenRandom(-0.015570580959320068),
                MagnetometerZ = GenRandom(0.010850906372070313),
                AngleNormalizedX = GenRandom(17.950439453125),
                AngleNormalizedY = GenRandom(-23.36273193359375),
                AngleNormalizedZ = GenRandom(-48.8250732421875),
                AccelRawX = GenRandom(-0.0120010357350111),
                AccelRawY = GenRandom(-0.04214850440621376),
                AccelRawZ = GenRandom(0.94571763277053833),
                GyroscopeRawX = GenRandom(7.62939453125e-005),
                GyroscopeRawY = GenRandom(-0.00054931640625),
                GyroscopeRawZ = GenRandom(0.0024566650390625),
                AccelerometerAxes = 3
            };
            sig.DeviceInfo = _client.DeviceInfo;
            sig.LocationFix.Add(new POGOProtos.Networking.Signature.Types.LocationFix()
            {
                Provider = "network",

                //Unk4 = 120,
                Latitude = (float)_client.CurrentLatitude,
                Longitude = (float)_client.CurrentLongitude,
                Altitude = (float)_client.CurrentAltitude,
                TimestampSinceStart = (ulong)_internalWatch.ElapsedMilliseconds - 200,
                Floor = 3,
                LocationType = 1
            });

            //Compute 10
            var x = new System.Data.HashFunction.xxHash(32, 0x1B845238);
            var firstHash = BitConverter.ToUInt32(x.ComputeHash(_client.AuthTicket.ToByteArray()), 0);
            x = new System.Data.HashFunction.xxHash(32, firstHash);
            var locationBytes = BitConverter.GetBytes(_client.CurrentLatitude).Reverse()
                .Concat(BitConverter.GetBytes(_client.CurrentLongitude).Reverse())
                .Concat(BitConverter.GetBytes(_client.CurrentAltitude).Reverse()).ToArray();
            sig.LocationHash1 = BitConverter.ToUInt32(x.ComputeHash(locationBytes), 0);
            //Compute 20
            x = new System.Data.HashFunction.xxHash(32, 0x1B845238);
            sig.LocationHash2 = BitConverter.ToUInt32(x.ComputeHash(locationBytes), 0);
            //Compute 24
            x = new System.Data.HashFunction.xxHash(64, 0x1B845238);
            var seed = BitConverter.ToUInt64(x.ComputeHash(_client.AuthTicket.ToByteArray()), 0);
            x = new System.Data.HashFunction.xxHash(64, seed);
            foreach (var req in requests)
                sig.RequestHash.Add(BitConverter.ToUInt64(x.ComputeHash(req.ToByteArray()), 0));

            //static for now
            sig.Unk22 = ByteString.CopyFrom(new byte[16] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F });


            Unknown6 val = new Unknown6();
            val.RequestType = 6;
            val.Unknown2 = new Unknown6.Types.Unknown2();
            val.Unknown2.Unknown1 = ByteString.CopyFrom(Encrypt(sig.ToByteArray()));
            return val;
        }

        private byte[] Encrypt(byte[] bytes)
        {
            var outputLength = 32 + bytes.Length + (256 - (bytes.Length % 256));
            var ptr = Marshal.AllocHGlobal(outputLength);
            var ptrOutput = Marshal.AllocHGlobal(outputLength);
            FillMemory(ptr, (uint)outputLength, 0);
            FillMemory(ptrOutput, (uint)outputLength, 0);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            try
            {
                int outputSize = outputLength;
                EncryptNative(ptr, bytes.Length, new byte[32], 32, ptrOutput, out outputSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            var output = new byte[outputLength];
            Marshal.Copy(ptrOutput, output, 0, outputLength);
            Marshal.FreeHGlobal(ptr);
            Marshal.FreeHGlobal(ptrOutput);
            return output;
        }

        [DllImport("encrypt32.dll", EntryPoint = "encrypt", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Cdecl)]
        static extern private void EncryptNative(IntPtr arr, int length, byte[] iv, int ivsize, IntPtr output, out int outputSize);
        [DllImport("kernel32.dll", EntryPoint = "RtlFillMemory", SetLastError = false)]
        static extern void FillMemory(IntPtr destination, uint length, byte fill);

        public RequestEnvelope GetRequestEnvelope(params Request[] customRequests)
        {
            var e = new RequestEnvelope
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
            e.Unknown6.Add(GenerateSignature(customRequests));
            return e;
        }

        public RequestEnvelope GetInitialRequestEnvelope(params Request[] customRequests)
        {
            var e = new RequestEnvelope
            {
                StatusCode = 2, //1

                RequestId = _nextRequestId++, //3
                Requests = { customRequests }, //4

                //Unknown6 = , //6
                Latitude = _client.CurrentLatitude, //7
                Longitude = _client.CurrentLongitude, //8
                Altitude = _client.CurrentAltitude, //9
                AuthInfo = new POGOProtos.Networking.Envelopes.RequestEnvelope.Types.AuthInfo
                {
                    Provider = _client.AuthType == AuthType.Google ? "google" : "ptc",
                    Token = new POGOProtos.Networking.Envelopes.RequestEnvelope.Types.AuthInfo.Types.JWT
                    {
                        Contents = _client.AuthToken,
                        Unknown2 = 14
                    }
                }, //10
                Unknown12 = 989 //12
            };
            return e;
        }



        public RequestEnvelope GetRequestEnvelope(RequestType type, IMessage message)
        {
            return GetRequestEnvelope(new Request()
            {
                RequestType = type,
                RequestMessage = message.ToByteString()
            });

        }

        private static readonly Random RandomDevice = new Random();

        public static double GenRandom(double num)
        {
                var randomFactor = 0.3f;
                var randomMin = (num * (1 - randomFactor));
                var randomMax = (num * (1 + randomFactor));
                var randomizedDelay = RandomDevice.NextDouble() * (randomMax - randomMin) + randomMin; ;
                return randomizedDelay;;
        }
    }
}