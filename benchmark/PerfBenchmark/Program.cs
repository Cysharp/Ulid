using BenchmarkDotNet.Running;

namespace PerfBenchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            // System.Guid.NewGuid().TryWriteBytes()
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }
    }
}
