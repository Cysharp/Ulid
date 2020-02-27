extern alias Ulid_1_0_0;
extern alias newUlid;
using BenchmarkDotNet.Attributes;
using System;
using OldUlid = Ulid_1_0_0::System.Ulid;
using NewUlid = newUlid::System.Ulid;

namespace PerfBenchmark.Suite
{
    [Config(typeof(BenchmarkConfig))]
    public class GetTime
    {
        OldUlid oldUlid;
        NewUlid ulid;

        [GlobalSetup]
        public void Setup()
        {
            var time = DateTimeOffset.Now;
            oldUlid = OldUlid.NewUlid(time);
            ulid = NewUlid.NewUlid(time);
        }

        [Benchmark(Baseline = true)]
        public DateTimeOffset OldUlid_()
        {
            return oldUlid.Time;
        }

        [Benchmark]
        public DateTimeOffset Ulid_()
        {
            return ulid.Time;
        }
    }    
}
