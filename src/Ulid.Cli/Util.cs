using System;

namespace Ulid.Cli
{
    internal static class Util
    {
        public static System.Ulid CreateUlid(DateTimeOffset timestamp, string randomness)
        {
            const int RandomnessStringLength = 16;
            if (string.IsNullOrEmpty(randomness))
            {
                return System.Ulid.NewUlid(timestamp);
            }
            else
            {
                if (randomness.Length != RandomnessStringLength)
                {
                    throw new ArgumentOutOfRangeException($"randomness value must have 80 bit length({RandomnessStringLength} characters)");
                }
                // normalize character case
                randomness = randomness.ToUpper();
                Span<byte> randomnessBytes = stackalloc byte[10];
                Util.ConvertBase32ToBytes(randomness.AsSpan(), randomnessBytes, 0);
                return System.Ulid.NewUlid(timestamp, randomnessBytes);
            }
        }
        public static void ConvertBase32ToBytes(ReadOnlySpan<char> cp, Span<byte> bytes, byte omitBits = 0)
        {
            if(cp.Length * 5 - omitBits > bytes.Length * 8)
            {
                throw new ArgumentOutOfRangeException(nameof(bytes), "result buffer is not enough large");
            }
            byte currentBit = 0;
            byte currentOffset = 0;
            bytes[0] = (byte)((GetBase32Value(cp[0]) << (3 + omitBits)) & 0xff);
            currentBit = (byte)(5 - omitBits);
            for (int i = 1; i < cp.Length; i++)
            {
                switch (currentBit)
                {
                    case 0:
                    case 1:
                    case 2:
                        bytes[currentOffset] |= (byte)(GetBase32Value(cp[i]) << (3 - currentBit));
                        currentBit += 5;
                        break;
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                        bytes[currentOffset] |= (byte)(GetBase32Value(cp[i]) >> (currentBit - 3));
                        if (currentBit != 3 && currentOffset + 1 < bytes.Length)
                        {
                            bytes[currentOffset + 1] |= (byte)((GetBase32Value(cp[i]) << (11 - currentBit)) & 0xff);
                        }
                        currentBit = (byte)(currentBit - 3);
                        currentOffset += 1;
                        break;
                    default:
                        break;
                }
            }
        }
        // copy from Ulid
        static readonly byte[] CharToBase32 = new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 255, 255, 255, 255, 255, 255, 255, 10, 11, 12, 13, 14, 15, 16, 17, 255, 18, 19, 255, 20, 21, 255, 22, 23, 24, 25, 26, 255, 27, 28, 29, 30, 31, 255, 255, 255, 255, 255, 255, 10, 11, 12, 13, 14, 15, 16, 17, 255, 18, 19, 255, 20, 21, 255, 22, 23, 24, 25, 26, 255, 27, 28, 29, 30, 31 };
        static byte GetBase32Value(char b)
        {
            if(b > CharToBase32.Length)
            {
                throw new InvalidOperationException($"invalid base32 character({b})");
            }
            var ret = CharToBase32[b];
            if(ret == 255)
            {
                throw new InvalidOperationException($"invalid base32 character({b})");
            }
            return ret;
        }
    }
}