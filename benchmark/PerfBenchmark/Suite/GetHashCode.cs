extern alias newUlid;
using BenchmarkDotNet.Attributes;
using System;
using newUlid::System;

namespace PerfBenchmark.Suite
{
    [Config(typeof(BenchmarkConfig))]
    public class GetHashCode
    {
        Guid guid;
        Ulid ulid;
        NUlid.Ulid nulid;

        [GlobalSetup]
        public void Setup()
        {
            guid = Guid.NewGuid();
            ulid = new Ulid(guid.ToByteArray());
            nulid = new NUlid.Ulid(guid.ToByteArray());
        }

        [Benchmark(Baseline = true)]
        public int Guid_()
        {
            return guid.GetHashCode();
        }

        [Benchmark]
        public int Ulid_()
        {
            return ulid.GetHashCode();
        }

        [Benchmark]
        public int NUlid_()
        {
            return nulid.GetHashCode();
        }
    }
}
