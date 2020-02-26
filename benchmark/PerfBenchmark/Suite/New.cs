extern alias newUlid;
using BenchmarkDotNet.Attributes;
using System;
using newUlid::System;

namespace PerfBenchmark.Suite
{
    [Config(typeof(BenchmarkConfig))]
    public class New
    {
        [Benchmark(Baseline = true)]
        public Guid Guid_()
        {
            return Guid.NewGuid();
        }

        [Benchmark]
        public Ulid Ulid_()
        {
            return Ulid.NewUlid();
        }

        [Benchmark]
        public NUlid.Ulid NUlid_()
        {
            return NUlid.Ulid.NewUlid();
        }
    }
}
