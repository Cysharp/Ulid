﻿extern alias Ulid_1_0_0;
extern alias newUlid;
using BenchmarkDotNet.Attributes;
using System;
using OldUlid = Ulid_1_0_0::System.Ulid;
using NewUlid = newUlid::System.Ulid;

namespace PerfBenchmark.Suite
{
    [Config(typeof(BenchmarkConfig))]
    public class GetHashCode
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
        public int Guid_()
        {
            return guid.GetHashCode();
        }

        [Benchmark]
        public int OldUlid_()
        {
            return oldUlid.GetHashCode();
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
