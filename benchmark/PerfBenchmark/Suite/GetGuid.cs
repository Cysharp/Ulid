extern alias Ulid_1_0_0;
extern alias newUlid;
using BenchmarkDotNet.Attributes;
using System;
using OldUlid = Ulid_1_0_0::System.Ulid;
using NewUlid = newUlid::System.Ulid;

namespace PerfBenchmark.Suite
{
    [Config(typeof(BenchmarkConfig))]
    public class GetGuid
    {
        OldUlid oldUlid;
        NewUlid ulid;
        NUlid.Ulid nulid;

        [GlobalSetup]
        public void Setup()
        {
            var guid = Guid.NewGuid();
            oldUlid = new OldUlid(guid.ToByteArray());
            ulid = new NewUlid(guid.ToByteArray());
            nulid = new NUlid.Ulid(guid.ToByteArray());
        }

        [Benchmark]
        public Guid OldUlid_()
        {
            return oldUlid.ToGuid();
        }

        [Benchmark]
        public Guid Ulid_()
        {
            return ulid.ToGuid();
        }

        [Benchmark]
        public Guid NUlid_()
        {
            return nulid.ToGuid();
        }
    }
}
