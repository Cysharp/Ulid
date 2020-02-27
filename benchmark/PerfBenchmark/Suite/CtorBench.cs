extern alias newUlid;
using BenchmarkDotNet.Attributes;
using newUlid::System;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace PerfBenchmark.Suite
{
    public class CtorInnerBench
    {

        static readonly Vector128<byte> TimeReverseMask = Vector128.Create(5, 4, 3, 2, 1, 0, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80);
        static readonly Vector128<byte> CtorReverseMask = Vector128.Create((byte)5, 4, 3, 2, 1, 0, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15);
        private long timestampMilliseconds;
        private ulong random1;
        private ulong random2;

        [GlobalSetup]
        public void SetUp()
        {
            timestampMilliseconds = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            var random = new Random();
            random1 = ((ulong)random.Next() << 48) ^ ((ulong)random.Next() << 16) ^ ((ulong)random.Next());
            random2 = ((ulong)random.Next() << 48) ^ ((ulong)random.Next() << 16) ^ ((ulong)random.Next());
        }

        [Benchmark]
        public Ulid CreateScalar()
        {
            var vec = Vector128.CreateScalarUnsafe(timestampMilliseconds).AsByte();
            //var vec = Vector128.Create(((ulong)timestampMilliseconds & 0xFFFF_FFFF_FFFF) | (random.Next() << 48), random.Next()).AsByte();
            var shuffled = Ssse3.Shuffle(vec, TimeReverseMask);
            Ulid ulid = Unsafe.As<Vector128<byte>, Ulid>(ref shuffled);
            //return;
            // Get first byte of randomness from Ulid Struct.
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 6), random1); // randomness0~7(but use 0~1 only)
            Unsafe.WriteUnaligned(ref Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 8), random2); // randomness2~9
            return ulid;
        }        
        
        [Benchmark]
        public Ulid Create()
        {
            var vec = Vector128.Create(((ulong)timestampMilliseconds & 0xFFFF_FFFF_FFFF) | (random1 << 48), random2).AsByte();
            var shuffled = Ssse3.Shuffle(vec, CtorReverseMask);
            return Unsafe.As<Vector128<byte>, Ulid>(ref shuffled);
        }
    }
}
