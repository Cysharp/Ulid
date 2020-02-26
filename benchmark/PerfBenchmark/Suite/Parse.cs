extern alias newUlid;
using BenchmarkDotNet.Attributes;
using System;
using newUlid::System;

namespace PerfBenchmark.Suite
{
    [Config(typeof(BenchmarkConfig))]
    public class Parse
    {
        string guid;
        string ulid;
        string nulid;

        [GlobalSetup]
        public void Setup()
        {
            var g = Guid.NewGuid();
            guid = g.ToString();
            ulid = new Ulid(g.ToByteArray()).ToString();
            nulid = new NUlid.Ulid(g.ToByteArray()).ToString();
        }

        [Benchmark(Baseline = true)]
        public Guid Guid_()
        {
            return Guid.Parse(guid);
        }

        [Benchmark]
        public Ulid Ulid_()
        {
            return Ulid.Parse(ulid);
        }

        [Benchmark]
        public NUlid.Ulid NUlid_()
        {
            return NUlid.Ulid.Parse(nulid);
        }
    }
}
