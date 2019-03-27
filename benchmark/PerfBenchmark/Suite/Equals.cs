using BenchmarkDotNet.Attributes;
using System;

namespace PerfBenchmark.Suite
{
    [Config(typeof(BenchmarkConfig))]
    public class Equals
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
        public bool Guid_()
        {
            return guid.Equals(guid);
        }

        [Benchmark]
        public bool Ulid_()
        {
            return ulid.Equals(ulid);
        }

        [Benchmark]
        public bool NUlid_()
        {
            return nulid.Equals(nulid);
        }
    }
}
