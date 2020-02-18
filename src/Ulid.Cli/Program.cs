using ConsoleAppFramework;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace Ulid.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await Host.CreateDefaultBuilder()
                .ConfigureLogging(x => x.ReplaceToSimpleConsole())
                .RunConsoleAppFrameworkAsync<UlidBatch>(args)
                .ConfigureAwait(false);
        }
    }
    public class UlidBatch : ConsoleAppBase
    {
        public void New(
            [Option("t", "timestamp(converted to UTC, ISO8601 format recommended)")]string timestamp = null,
            [Option("r", "randomness bytes(formatted as Base32, must be 16 characters, case insensitive)")]string randomness = null,
            [Option("b", "output as base64 format, or output base32 if false")]bool base64 = false,
            [Option("min", "min-randomness(use 000...)")]bool minRandomness = false,
            [Option("max", "max-randomness(use ZZZ...)")]bool maxRandomness = false)
        {
            var t = string.IsNullOrEmpty(timestamp) ? DateTimeOffset.Now : DateTime.Parse(timestamp);
            string r = randomness;
            if (r == null)
            {
                if (minRandomness)
                {
                    r = "0000000000000000";
                }
                else if (maxRandomness)
                {
                    r = "ZZZZZZZZZZZZZZZZ";
                }
            }

            var ulid = Util.CreateUlid(t, r);
            Console.Write(base64 ? ulid.ToBase64() : ulid.ToString());
        }
    }
}
