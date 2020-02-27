extern alias Ulid_1_0_0;
extern alias newUlid;
using BenchmarkDotNet.Attributes;
using System;
using OldUlid = Ulid_1_0_0::System.Ulid;
using NewUlid = newUlid::System.Ulid;

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
        public string OldUlid_()
        {
            return OldUlid.NewUlid().ToString();
        }        
        
        [Benchmark]
        public string Ulid_()
        {
            return NewUlid.NewUlid().ToString();
        }

        [Benchmark]
        public string NUlid_()
        {
            return NUlid.Ulid.NewUlid().ToString();
        }
    }
}
