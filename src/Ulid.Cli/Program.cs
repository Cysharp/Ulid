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
        [Command("new", "create new ULID")]
        public void New([Option("b", "output as base64 format")]bool base64 = false)
        {
            var ulid = System.Ulid.NewUlid(DateTimeOffset.Now);
            Console.Write(base64 ? ulid.ToBase64() : ulid.ToString());
        }
    }
}
