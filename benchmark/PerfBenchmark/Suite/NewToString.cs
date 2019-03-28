using BenchmarkDotNet.Attributes;
using System;

namespace PerfBenchmark.Suite
{
    [Config(typeof(BenchmarkConfig))]
    public class NewToString
    {
        [Benchmark(Baseline = true)]
        public string Guid_()
        {
            return Guid.NewGuid().ToString();
        }

        [Benchmark]
        public string Ulid_()
        {
            return Ulid.NewUlid().ToString();
        }

        [Benchmark]
        public string NUlid_()
        {
            return NUlid.Ulid.NewUlid().ToString();
        }
    }
}
