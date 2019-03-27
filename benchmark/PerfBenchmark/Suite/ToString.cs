using BenchmarkDotNet.Attributes;
using System;

namespace PerfBenchmark.Suite
{
    [Config(typeof(BenchmarkConfig))]
    public class ToString
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
        public string Guid_()
        {
            return guid.ToString();
        }

        [Benchmark]
        public string Ulid_()
        {
            return ulid.ToString();
        }

        [Benchmark]
        public string NUlid_()
        {
            return nulid.ToString();
        }
    }
}
