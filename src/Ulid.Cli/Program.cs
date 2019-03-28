using System;
using MicroBatchFramework;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Ulid.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new HostBuilder().RunBatchEngineAsync<UlidBatch>(args).ConfigureAwait(false);
        }
    }
    public class UlidBatch : BatchBase
    {
        static readonly char[] Base32Text = "0123456789ABCDEFGHJKMNPQRSTVWXYZ".ToCharArray();
        [Command("new", "create new ULID")]
        public void New([Option("b", "output as base64 format")]bool base64 = false,
            [Option("d", "date(converted to UTC)")]string start_date = null,
            [Option("r", "randomness bytes(formatted as Base32, must be 16 characters)")]string randomness = null)
        {
            var d = string.IsNullOrEmpty(start_date) ? DateTimeOffset.Now : DateTime.Parse(start_date);
            var ulid = CreateUlid(d, randomness);
            Console.Write(base64 ? ulid.ToBase64() : ulid.ToString());
        }
        private System.Ulid CreateUlid(DateTimeOffset d, string randomness)
        {
            const int RandomnessStringLength = 16;
            if (string.IsNullOrEmpty(randomness))
            {
                return System.Ulid.NewUlid(d);
            }
            else
            {
                if (randomness.Length != RandomnessStringLength)
                {
                    throw new ArgumentOutOfRangeException($"randomness value must have 80 bit length({RandomnessStringLength} characters)");
                }
                Span<byte> randomnessBytes = stackalloc byte[10];
                ConvertBase32ToBytes(randomness.AsSpan(), randomnessBytes);
                return System.Ulid.NewUlid(d, randomnessBytes);
            }
        }
        void ConvertBase32ToBytes(ReadOnlySpan<char> cp, Span<byte> bytes)
        {
            byte currentBit = 0;
            byte currentOffset = 0;
            for (int i = 0; i < cp.Length; i++)
            {
                switch (currentBit)
                {
                    case 0:
                    case 1:
                    case 2:
                        bytes[currentOffset] |= (byte)(GetBase32Value(cp[i]) << (5 - currentBit));
                        currentBit += 5;
                        break;
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        bytes[currentOffset] |= (byte)(GetBase32Value(cp[i]) >> (currentBit - 3));
                        if (currentBit != 3)
                        {
                            bytes[currentOffset + 1] |= (byte)((GetBase32Value(cp[i]) << (11 - currentBit)) & 0xff);
                        }
                        currentBit = (byte)(currentBit - 3);
                        currentOffset += 1;
                        break;
                    default:
                        break;
                }
            }
        }
        byte GetBase32Value(char b)
        {
            if (b >= 'A' && b <= 'Z')
            {
                return (byte)(b - 'A' + 10);
            }
            else if (b >= '0' && b <= '9')
            {
                return (byte)(b - '0');
            }
            else if (b == '=')
            {
                // padding
                return 0;
            }
            else
            {
                throw new InvalidOperationException($"invalid base32 character({b})");
            }
        }
    }
}
