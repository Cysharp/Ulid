using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace System
{
    internal static class RandomProvider
    {
        [ThreadStatic]
        static Random random;

        [ThreadStatic]
        static XorShift64 xorShift;

        // this random is async-unsafe, be careful to use.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Random GetRandom()
        {
            if (random == null)
            {
                random = CreateRandom();
            }
            return random;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static Random CreateRandom()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                // Span<byte> buffer = stackalloc byte[sizeof(int)];
                var buffer = new byte[sizeof(int)];
                rng.GetBytes(buffer);
                var seed = BitConverter.ToInt32(buffer, 0);
                return new Random(seed);
            }
        }

        // this random is async-unsafe, be careful to use.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static XorShift64 GetXorShift64()
        {
            if (xorShift == null)
            {
                xorShift = CreateXorShift64();
            }
            return xorShift;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        static XorShift64 CreateXorShift64()
        {
            using (var rng = RandomNumberGenerator.Create())
            {
                // Span<byte> buffer = stackalloc byte[sizeof(UInt64)];
                var buffer = new byte[sizeof(UInt64)];
                rng.GetBytes(buffer);
                var seed = BitConverter.ToUInt64(buffer, 0);
                return new XorShift64(seed);
            }
        }
    }

    internal class XorShift64
    {
        UInt64 x = 88172645463325252UL;

        public XorShift64(UInt64 seed)
        {
            if (seed != 0)
            {
                x = seed;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UInt64 Next()
        {
            x = x ^ (x << 7);
            return x = x ^ (x >> 9);
        }
    }
}