using BenchmarkDotNet.Attributes;
using System;

namespace PerfBenchmark.Suite
{
    [Config(typeof(BenchmarkConfig))]
    public class CompareTo
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
            return guid.CompareTo(guid);
        }

        [Benchmark]
        public int Ulid_()
        {
            return ulid.CompareTo(ulid);
        }

        [Benchmark]
        public int NUlid_()
        {
            return nulid.CompareTo(nulid);
        }
    }
}
