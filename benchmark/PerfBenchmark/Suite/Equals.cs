extern alias Ulid_1_0_0;
extern alias newUlid;
using BenchmarkDotNet.Attributes;
using System;
using OldUlid = Ulid_1_0_0::System.Ulid;
using NewUlid = newUlid::System.Ulid;

namespace PerfBenchmark.Suite
{
    [Config(typeof(BenchmarkConfig))]
    public class Equals
    {
        Guid guid;
        OldUlid oldUlid;
        NewUlid ulid;
        NUlid.Ulid nulid;

        [GlobalSetup]
        public void Setup()
        {
            guid = Guid.NewGuid();
            oldUlid = new OldUlid(guid.ToByteArray());
            ulid = new NewUlid(guid.ToByteArray());
            nulid = new NUlid.Ulid(guid.ToByteArray());
        }

        [Benchmark(Baseline = true)]
        public bool Guid_()
        {
            return guid.Equals(guid);
        }

        [Benchmark]
        public bool OldUlid_()
        {
            return oldUlid.Equals(oldUlid);
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
