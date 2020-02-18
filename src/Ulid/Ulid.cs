using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace System // wa-o, System Namespace!?
{
    /// <summary>
    /// Represents a Universally Unique Lexicographically Sortable Identifier (ULID).
    /// Spec: https://github.com/ulid/spec
    /// </summary>
    [StructLayout(LayoutKind.Explicit, Size = 16)]
    [DebuggerDisplay("{ToString(),nq}")]
#if NETCOREAPP3_0
    [System.Text.Json.Serialization.JsonConverter(typeof(Cysharp.Serialization.Json.UlidJsonConverter))]
#endif
    public partial struct Ulid : IEquatable<Ulid>, IComparable<Ulid>
    {
        // https://en.wikipedia.org/wiki/Base32
        static readonly char[] Base32Text = "0123456789ABCDEFGHJKMNPQRSTVWXYZ".ToCharArray();
        static readonly byte[] Base32Bytes = Encoding.UTF8.GetBytes(Base32Text);
        static readonly byte[] CharToBase32 = new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 255, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 255, 255, 255, 255, 255, 255, 255, 10, 11, 12, 13, 14, 15, 16, 17, 255, 18, 19, 255, 20, 21, 255, 22, 23, 24, 25, 26, 255, 27, 28, 29, 30, 31, 255, 255, 255, 255, 255, 255, 10, 11, 12, 13, 14, 15, 16, 17, 255, 18, 19, 255, 20, 21, 255, 22, 23, 24, 25, 26, 255, 27, 28, 29, 30, 31 };
        static readonly DateTimeOffset UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static readonly Ulid MinValue = new Ulid(UnixEpoch.ToUnixTimeMilliseconds(), new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

        public static readonly Ulid MaxValue = new Ulid(DateTimeOffset.MaxValue.ToUnixTimeMilliseconds(), new byte[] { 255, 255, 255, 255, 255, 255, 255, 255, 255, 255 });

        public static readonly Ulid Empty = new Ulid();

        // Core

        // Timestamp(64bits)
        [FieldOffset(0)] readonly byte timestamp0;
        [FieldOffset(1)] readonly byte timestamp1;
        [FieldOffset(2)] readonly byte timestamp2;
        [FieldOffset(3)] readonly byte timestamp3;
        [FieldOffset(4)] readonly byte timestamp4;
        [FieldOffset(5)] readonly byte timestamp5;

        // Randomness(80bits)
        [FieldOffset(6)] readonly byte randomness0;
        [FieldOffset(7)] readonly byte randomness1;
        [FieldOffset(8)] readonly byte randomness2;
        [FieldOffset(9)] readonly byte randomness3;
        [FieldOffset(10)] readonly byte randomness4;
        [FieldOffset(11)] readonly byte randomness5;
        [FieldOffset(12)] readonly byte randomness6;
        [FieldOffset(13)] readonly byte randomness7;
        [FieldOffset(14)] readonly byte randomness8;
        [FieldOffset(15)] readonly byte randomness9;

        [IgnoreDataMember]
        public byte[] Random => new byte[]
        {
            randomness0,
            randomness1,
            randomness2,
            randomness3,
            randomness4,
            randomness5,
            randomness6,
            randomness7,
            randomness8,
            randomness9,
        };

        [IgnoreDataMember]
        public DateTimeOffset Time
        {
            get
            {
                Span<byte> buffer = stackalloc byte[8];
                buffer[0] = timestamp5;
                buffer[1] = timestamp4;
                buffer[2] = timestamp3;
                buffer[3] = timestamp2;
                buffer[4] = timestamp1;
                buffer[5] = timestamp0; // [6], [7] = 0

                var timestampMilliseconds = Unsafe.As<byte, long>(ref MemoryMarshal.GetReference(buffer));
                return DateTimeOffset.FromUnixTimeMilliseconds(timestampMilliseconds);
            }
        }

        internal Ulid(long timestampMilliseconds, XorShift64 random)
            : this()
        {
            // Get memory in stack and copy to ulid(Little->Big reverse order).
            ref var fisrtByte = ref Unsafe.As<long, byte>(ref timestampMilliseconds);
            this.timestamp0 = Unsafe.Add(ref fisrtByte, 5);
            this.timestamp1 = Unsafe.Add(ref fisrtByte, 4);
            this.timestamp2 = Unsafe.Add(ref fisrtByte, 3);
            this.timestamp3 = Unsafe.Add(ref fisrtByte, 2);
            this.timestamp4 = Unsafe.Add(ref fisrtByte, 1);
            this.timestamp5 = Unsafe.Add(ref fisrtByte, 0);

            // Get first byte of randomness from Ulid Struct.
            Unsafe.WriteUnaligned(ref randomness0, random.Next()); // randomness0~7(but use 0~1 only)
            Unsafe.WriteUnaligned(ref randomness2, random.Next()); // randomness2~9
        }

        internal Ulid(long timestampMilliseconds, ReadOnlySpan<byte> randomness)
            : this()
        {
            ref var fisrtByte = ref Unsafe.As<long, byte>(ref timestampMilliseconds);
            this.timestamp0 = Unsafe.Add(ref fisrtByte, 5);
            this.timestamp1 = Unsafe.Add(ref fisrtByte, 4);
            this.timestamp2 = Unsafe.Add(ref fisrtByte, 3);
            this.timestamp3 = Unsafe.Add(ref fisrtByte, 2);
            this.timestamp4 = Unsafe.Add(ref fisrtByte, 1);
            this.timestamp5 = Unsafe.Add(ref fisrtByte, 0);

            ref var src = ref MemoryMarshal.GetReference(randomness); // length = 10
            randomness0 = randomness[0];
            randomness1 = randomness[1];
            Unsafe.WriteUnaligned(ref randomness2, Unsafe.As<byte, ulong>(ref Unsafe.Add(ref src, 2))); // randomness2~randomness9
        }

        public Ulid(ReadOnlySpan<byte> bytes)
            : this()
        {
            if (bytes.Length != 16) throw new ArgumentException("invalid bytes length, length:" + bytes.Length);

            ref var src = ref MemoryMarshal.GetReference(bytes);
            Unsafe.WriteUnaligned(ref timestamp0, Unsafe.As<byte, ulong>(ref src)); // timestamp0~randomness1
            Unsafe.WriteUnaligned(ref randomness2, Unsafe.As<byte, ulong>(ref Unsafe.Add(ref src, 8))); // randomness2~randomness9
        }

        internal Ulid(ReadOnlySpan<char> base32)
        {
            // unroll-code is based on NUlid.

            randomness9 = (byte)((CharToBase32[base32[24]] << 5) | CharToBase32[base32[25]]); // eliminate bounds-check of span

            timestamp0 = (byte)((CharToBase32[base32[0]] << 5) | CharToBase32[base32[1]]);
            timestamp1 = (byte)((CharToBase32[base32[2]] << 3) | (CharToBase32[base32[3]] >> 2));
            timestamp2 = (byte)((CharToBase32[base32[3]] << 6) | (CharToBase32[base32[4]] << 1) | (CharToBase32[base32[5]] >> 4));
            timestamp3 = (byte)((CharToBase32[base32[5]] << 4) | (CharToBase32[base32[6]] >> 1));
            timestamp4 = (byte)((CharToBase32[base32[6]] << 7) | (CharToBase32[base32[7]] << 2) | (CharToBase32[base32[8]] >> 3));
            timestamp5 = (byte)((CharToBase32[base32[8]] << 5) | CharToBase32[base32[9]]);

            randomness0 = (byte)((CharToBase32[base32[10]] << 3) | (CharToBase32[base32[11]] >> 2));
            randomness1 = (byte)((CharToBase32[base32[11]] << 6) | (CharToBase32[base32[12]] << 1) | (CharToBase32[base32[13]] >> 4));
            randomness2 = (byte)((CharToBase32[base32[13]] << 4) | (CharToBase32[base32[14]] >> 1));
            randomness3 = (byte)((CharToBase32[base32[14]] << 7) | (CharToBase32[base32[15]] << 2) | (CharToBase32[base32[16]] >> 3));
            randomness4 = (byte)((CharToBase32[base32[16]] << 5) | CharToBase32[base32[17]]);
            randomness5 = (byte)((CharToBase32[base32[18]] << 3) | CharToBase32[base32[19]] >> 2);
            randomness6 = (byte)((CharToBase32[base32[19]] << 6) | (CharToBase32[base32[20]] << 1) | (CharToBase32[base32[21]] >> 4));
            randomness7 = (byte)((CharToBase32[base32[21]] << 4) | (CharToBase32[base32[22]] >> 1));
            randomness8 = (byte)((CharToBase32[base32[22]] << 7) | (CharToBase32[base32[23]] << 2) | (CharToBase32[base32[24]] >> 3));
        }

        // HACK: We assume the layout of a Guid is the following:
        // Int32, Int16, Int16, Int8, Int8, Int8, Int8, Int8, Int8, Int8, Int8
        // source: https://github.com/dotnet/runtime/blob/4f9ae42d861fcb4be2fcd5d3d55d5f227d30e723/src/libraries/System.Private.CoreLib/src/System/Guid.cs
        public Ulid(Guid guid)
        {
            Span<byte> buf = stackalloc byte[16];
            MemoryMarshal.Write(buf, ref guid);
            if (BitConverter.IsLittleEndian)
            {
                byte tmp;
                tmp = buf[0]; buf[0] = buf[3]; buf[3] = tmp;
                tmp = buf[1]; buf[1] = buf[2]; buf[2] = tmp;
                tmp = buf[4]; buf[4] = buf[5]; buf[5] = tmp;
                tmp = buf[6]; buf[6] = buf[7]; buf[7] = tmp;
            }
            this = MemoryMarshal.Read<Ulid>(buf);
        }

        // Factory

        public static Ulid NewUlid()
        {
            return new Ulid(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(), RandomProvider.GetXorShift64());
        }

        public static Ulid NewUlid(DateTimeOffset timestamp)
        {
            return new Ulid(timestamp.ToUnixTimeMilliseconds(), RandomProvider.GetXorShift64());
        }

        public static Ulid NewUlid(DateTimeOffset timestamp, ReadOnlySpan<byte> randomness)
        {
            if (randomness.Length != 10) throw new ArgumentException("invalid randomness length, length:" + randomness.Length);
            return new Ulid(timestamp.ToUnixTimeMilliseconds(), randomness);
        }

        public static Ulid Parse(string base32)
        {
            return new Ulid(base32.AsSpan());
        }

        public static Ulid Parse(ReadOnlySpan<char> base32)
        {
            if (base32.Length != 26) throw new ArgumentException("invalid base32 length, length:" + base32.Length);
            return new Ulid(base32);
        }

        public static Ulid Parse(ReadOnlySpan<byte> base32)
        {
            if (!TryParse(base32, out var ulid)) throw new ArgumentException("invalid base32 length, length:" + base32.Length);
            return ulid;
        }

        public static bool TryParse(string base32, out Ulid ulid)
        {
            return TryParse(base32.AsSpan(), out ulid);
        }

        public static bool TryParse(ReadOnlySpan<char> base32, out Ulid ulid)
        {
            if (base32.Length != 26)
            {
                ulid = default(Ulid);
                return false;
            }

            try
            {
                ulid = new Ulid(base32);
                return true;
            }
            catch
            {
                ulid = default(Ulid);
                return false;
            }
        }

        public static bool TryParse(ReadOnlySpan<byte> base32, out Ulid ulid)
        {
            if (base32.Length != 26)
            {
                ulid = default(Ulid);
                return false;
            }

            try
            {
                ulid = ParseCore(base32);
                return true;
            }
            catch
            {
                ulid = default(Ulid);
                return false;
            }
        }

        static Ulid ParseCore(ReadOnlySpan<byte> base32)
        {
            if (base32.Length != 26) throw new ArgumentException("invalid base32 length, length:" + base32.Length);

            var ulid = default(Ulid);

            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 15) = (byte)((CharToBase32[base32[24]] << 5) | CharToBase32[base32[25]]);

            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 0) = (byte)((CharToBase32[base32[0]] << 5) | CharToBase32[base32[1]]);
            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 1) = (byte)((CharToBase32[base32[2]] << 3) | (CharToBase32[base32[3]] >> 2));
            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 2) = (byte)((CharToBase32[base32[3]] << 6) | (CharToBase32[base32[4]] << 1) | (CharToBase32[base32[5]] >> 4));
            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 3) = (byte)((CharToBase32[base32[5]] << 4) | (CharToBase32[base32[6]] >> 1));
            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 4) = (byte)((CharToBase32[base32[6]] << 7) | (CharToBase32[base32[7]] << 2) | (CharToBase32[base32[8]] >> 3));
            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 5) = (byte)((CharToBase32[base32[8]] << 5) | CharToBase32[base32[9]]);

            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 6) = (byte)((CharToBase32[base32[10]] << 3) | (CharToBase32[base32[11]] >> 2));
            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 7) = (byte)((CharToBase32[base32[11]] << 6) | (CharToBase32[base32[12]] << 1) | (CharToBase32[base32[13]] >> 4));
            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 8) = (byte)((CharToBase32[base32[13]] << 4) | (CharToBase32[base32[14]] >> 1));
            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 9) = (byte)((CharToBase32[base32[14]] << 7) | (CharToBase32[base32[15]] << 2) | (CharToBase32[base32[16]] >> 3));
            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 10) = (byte)((CharToBase32[base32[16]] << 5) | CharToBase32[base32[17]]);
            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 11) = (byte)((CharToBase32[base32[18]] << 3) | CharToBase32[base32[19]] >> 2);
            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 12) = (byte)((CharToBase32[base32[19]] << 6) | (CharToBase32[base32[20]] << 1) | (CharToBase32[base32[21]] >> 4));
            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 13) = (byte)((CharToBase32[base32[21]] << 4) | (CharToBase32[base32[22]] >> 1));
            Unsafe.Add(ref Unsafe.As<Ulid, byte>(ref ulid), 14) = (byte)((CharToBase32[base32[22]] << 7) | (CharToBase32[base32[23]] << 2) | (CharToBase32[base32[24]] >> 3));

            return ulid;
        }

        // Convert

        public byte[] ToByteArray()
        {
            var bytes = new byte[16];
            Unsafe.WriteUnaligned(ref bytes[0], this);
            return bytes;
        }

        public bool TryWriteBytes(Span<byte> destination)
        {
            if (destination.Length < 16)
            {
                return false;
            }

            Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(destination), this);
            return true;
        }

        public string ToBase64(Base64FormattingOptions options = Base64FormattingOptions.None)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(16);
            try
            {
                TryWriteBytes(buffer);
                return Convert.ToBase64String(buffer, options);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        public bool TryWriteStringify(Span<byte> span)
        {
            if (span.Length < 26) return false;

            span[25] = Base32Bytes[randomness9 & 31]; // eliminate bounds-check of span

            // timestamp
            span[0] = Base32Bytes[(timestamp0 & 224) >> 5];
            span[1] = Base32Bytes[timestamp0 & 31];
            span[2] = Base32Bytes[(timestamp1 & 248) >> 3];
            span[3] = Base32Bytes[((timestamp1 & 7) << 2) | ((timestamp2 & 192) >> 6)];
            span[4] = Base32Bytes[(timestamp2 & 62) >> 1];
            span[5] = Base32Bytes[((timestamp2 & 1) << 4) | ((timestamp3 & 240) >> 4)];
            span[6] = Base32Bytes[((timestamp3 & 15) << 1) | ((timestamp4 & 128) >> 7)];
            span[7] = Base32Bytes[(timestamp4 & 124) >> 2];
            span[8] = Base32Bytes[((timestamp4 & 3) << 3) | ((timestamp5 & 224) >> 5)];
            span[9] = Base32Bytes[timestamp5 & 31];

            // randomness
            span[10] = Base32Bytes[(randomness0 & 248) >> 3];
            span[11] = Base32Bytes[((randomness0 & 7) << 2) | ((randomness1 & 192) >> 6)];
            span[12] = Base32Bytes[(randomness1 & 62) >> 1];
            span[13] = Base32Bytes[((randomness1 & 1) << 4) | ((randomness2 & 240) >> 4)];
            span[14] = Base32Bytes[((randomness2 & 15) << 1) | ((randomness3 & 128) >> 7)];
            span[15] = Base32Bytes[(randomness3 & 124) >> 2];
            span[16] = Base32Bytes[((randomness3 & 3) << 3) | ((randomness4 & 224) >> 5)];
            span[17] = Base32Bytes[randomness4 & 31];
            span[18] = Base32Bytes[(randomness5 & 248) >> 3];
            span[19] = Base32Bytes[((randomness5 & 7) << 2) | ((randomness6 & 192) >> 6)];
            span[20] = Base32Bytes[(randomness6 & 62) >> 1];
            span[21] = Base32Bytes[((randomness6 & 1) << 4) | ((randomness7 & 240) >> 4)];
            span[22] = Base32Bytes[((randomness7 & 15) << 1) | ((randomness8 & 128) >> 7)];
            span[23] = Base32Bytes[(randomness8 & 124) >> 2];
            span[24] = Base32Bytes[((randomness8 & 3) << 3) | ((randomness9 & 224) >> 5)];

            return true;
        }

        public bool TryWriteStringify(Span<char> span)
        {
            if (span.Length < 26) return false;

            span[25] = Base32Text[randomness9 & 31]; // eliminate bounds-check of span

            // timestamp
            span[0] = Base32Text[(timestamp0 & 224) >> 5];
            span[1] = Base32Text[timestamp0 & 31];
            span[2] = Base32Text[(timestamp1 & 248) >> 3];
            span[3] = Base32Text[((timestamp1 & 7) << 2) | ((timestamp2 & 192) >> 6)];
            span[4] = Base32Text[(timestamp2 & 62) >> 1];
            span[5] = Base32Text[((timestamp2 & 1) << 4) | ((timestamp3 & 240) >> 4)];
            span[6] = Base32Text[((timestamp3 & 15) << 1) | ((timestamp4 & 128) >> 7)];
            span[7] = Base32Text[(timestamp4 & 124) >> 2];
            span[8] = Base32Text[((timestamp4 & 3) << 3) | ((timestamp5 & 224) >> 5)];
            span[9] = Base32Text[timestamp5 & 31];

            // randomness
            span[10] = Base32Text[(randomness0 & 248) >> 3];
            span[11] = Base32Text[((randomness0 & 7) << 2) | ((randomness1 & 192) >> 6)];
            span[12] = Base32Text[(randomness1 & 62) >> 1];
            span[13] = Base32Text[((randomness1 & 1) << 4) | ((randomness2 & 240) >> 4)];
            span[14] = Base32Text[((randomness2 & 15) << 1) | ((randomness3 & 128) >> 7)];
            span[15] = Base32Text[(randomness3 & 124) >> 2];
            span[16] = Base32Text[((randomness3 & 3) << 3) | ((randomness4 & 224) >> 5)];
            span[17] = Base32Text[randomness4 & 31];
            span[18] = Base32Text[(randomness5 & 248) >> 3];
            span[19] = Base32Text[((randomness5 & 7) << 2) | ((randomness6 & 192) >> 6)];
            span[20] = Base32Text[(randomness6 & 62) >> 1];
            span[21] = Base32Text[((randomness6 & 1) << 4) | ((randomness7 & 240) >> 4)];
            span[22] = Base32Text[((randomness7 & 15) << 1) | ((randomness8 & 128) >> 7)];
            span[23] = Base32Text[(randomness8 & 124) >> 2];
            span[24] = Base32Text[((randomness8 & 3) << 3) | ((randomness9 & 224) >> 5)];

            return true;
        }

        public override string ToString()
        {
            Span<char> span = stackalloc char[26];
            TryWriteStringify(span);
            unsafe
            {
                return new string((char*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(span)), 0, 26);
            }
        }

        // Comparable/Equatable

        public override unsafe int GetHashCode()
        {
            // Simply XOR, same algorithm of Guid.GetHashCode
            fixed (void* p = &this.timestamp0)
            {
                var a = (int*)p;
                return (*a) ^ *(a + 1) ^ *(a + 2) ^ *(a + 3);
            }
        }

        public unsafe bool Equals(Ulid other)
        {
            // readonly struct can not use Unsafe.As...
            fixed (byte* a = &this.timestamp0)
            {
                byte* b = &other.timestamp0;

                {
                    var x = *(ulong*)a;
                    var y = *(ulong*)b;
                    if (x != y) return false;
                }
                {
                    var x = *(ulong*)(a + 8);
                    var y = *(ulong*)(b + 8);
                    if (x != y) return false;
                }

                return true;
            }
        }

        public override bool Equals(object obj)
        {
            return (obj is Ulid other) ? this.Equals(other) : false;
        }

        public static bool operator ==(Ulid a, Ulid b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Ulid a, Ulid b)
        {
            return !a.Equals(b);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GetResult(byte me, byte them) => me < them ? -1 : 1;

        public int CompareTo(Ulid other)
        {
            if (this.timestamp0 != other.timestamp0) return GetResult(this.timestamp0, other.timestamp0);
            if (this.timestamp1 != other.timestamp1) return GetResult(this.timestamp1, other.timestamp1);
            if (this.timestamp2 != other.timestamp2) return GetResult(this.timestamp2, other.timestamp2);
            if (this.timestamp3 != other.timestamp3) return GetResult(this.timestamp3, other.timestamp3);
            if (this.timestamp4 != other.timestamp4) return GetResult(this.timestamp4, other.timestamp4);
            if (this.timestamp5 != other.timestamp5) return GetResult(this.timestamp5, other.timestamp5);

            if (this.randomness0 != other.randomness0) return GetResult(this.randomness0, other.randomness0);
            if (this.randomness1 != other.randomness1) return GetResult(this.randomness1, other.randomness1);
            if (this.randomness2 != other.randomness2) return GetResult(this.randomness2, other.randomness2);
            if (this.randomness3 != other.randomness3) return GetResult(this.randomness3, other.randomness3);
            if (this.randomness4 != other.randomness4) return GetResult(this.randomness4, other.randomness4);
            if (this.randomness5 != other.randomness5) return GetResult(this.randomness5, other.randomness5);
            if (this.randomness6 != other.randomness6) return GetResult(this.randomness6, other.randomness6);
            if (this.randomness7 != other.randomness7) return GetResult(this.randomness7, other.randomness7);
            if (this.randomness8 != other.randomness8) return GetResult(this.randomness8, other.randomness8);
            if (this.randomness9 != other.randomness9) return GetResult(this.randomness9, other.randomness9);

            return 0;
        }

        public static explicit operator Guid(Ulid _this)
        {
            return _this.ToGuid();
        }

        /// <summary>
        /// Convert this <c>Ulid</c> value to a <c>Guid</c> value with the same comparability.
        /// </summary>
        /// <remarks>
        /// The byte arrangement between Ulid and Guid is not preserved.
        /// </remarks>
        /// <returns>The converted <c>Guid</c> value</returns>
        public Guid ToGuid()
        {
            Span<byte> buf = stackalloc byte[16];
            MemoryMarshal.Write(buf, ref this);
            if (BitConverter.IsLittleEndian)
            {
                byte tmp;
                tmp = buf[0]; buf[0] = buf[3]; buf[3] = tmp;
                tmp = buf[1]; buf[1] = buf[2]; buf[2] = tmp;
                tmp = buf[4]; buf[4] = buf[5]; buf[5] = tmp;
                tmp = buf[6]; buf[6] = buf[7]; buf[7] = tmp;
            }
            return MemoryMarshal.Read<Guid>(buf);
        }
    }
}
