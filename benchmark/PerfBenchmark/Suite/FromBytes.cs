extern alias Ulid_1_0_0;
extern alias newUlid;
using BenchmarkDotNet.Attributes;
using System;
using OldUlid = Ulid_1_0_0::System.Ulid;
using NewUlid = newUlid::System.Ulid;

namespace PerfBenchmark.Suite
{
    [Config(typeof(BenchmarkConfig))]
    public class FromBytes
    {
        Guid originalGuid;
        Guid guid;
        OldUlid oldUlid;
        NewUlid ulid;
        NUlid.Ulid nulid;

        [GlobalSetup]
        public void Setup()
        {
            originalGuid = Guid.NewGuid();
        }


        [Benchmark(Baseline = true)]
        public void Guid_()
        {
            guid = new Guid(originalGuid.ToByteArray());
        }        
        
        [Benchmark]
        public void OldUlid_()
        {
            oldUlid = new OldUlid(originalGuid.ToByteArray());
        }

        [Benchmark]
        public void Ulid_()
        {
            ulid = new NewUlid(originalGuid.ToByteArray());
        }

        [Benchmark]
        public void NUlid_()
        {
            nulid = new NUlid.Ulid(originalGuid.ToByteArray());
        }
    }
}
