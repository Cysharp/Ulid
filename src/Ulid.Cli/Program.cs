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
        [Command("generate", "create ULID")]
        public void New([Option("b", "output as base64 format, or output base32 if false")]bool base64 = false,
            [Option("t", "timestamp(converted to UTC, ISO8601 format recommended)")]string timestamp = null,
            [Option("r", "randomness bytes(formatted as Base32, must be 16 characters, case insensitive)")]string randomness = null)
        {
            var t = string.IsNullOrEmpty(timestamp) ? DateTimeOffset.Now : DateTime.Parse(timestamp);
            var ulid = Util.CreateUlid(t, randomness);
            Console.Write(base64 ? ulid.ToBase64() : ulid.ToString());
        }
    }
}
