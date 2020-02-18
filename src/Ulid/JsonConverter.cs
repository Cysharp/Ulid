#if SystemTextJson
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace System
{
    public partial struct Ulid
    {
        static readonly byte[] Base32Bytes = Text.Encoding.UTF8.GetBytes("0123456789ABCDEFGHJKMNPQRSTVWXYZ");

        // HACK: This is the same method as ToString, but working on a byte span.
        // Probably won't change for some time.
        private void WriteStringifyBytes(Span<byte> span)
        {
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
        }

        public class UlidJsonConverter : JsonConverter<Ulid>
        {
            // HACK: this is the same as parsing from char, but using bytes instead of UTF-16 chars
            private void ParseSpan(ReadOnlySpan<byte> base32, Span<byte> result)
            {
                result[15] = (byte)((CharToBase32[base32[24]] << 5) | CharToBase32[base32[25]]); // eliminate bounds-check of span

                result[0] = (byte)((CharToBase32[base32[0]] << 5) | CharToBase32[base32[1]]);
                result[1] = (byte)((CharToBase32[base32[2]] << 3) | (CharToBase32[base32[3]] >> 2));
                result[2] = (byte)((CharToBase32[base32[3]] << 6) | (CharToBase32[base32[4]] << 1) | (CharToBase32[base32[5]] >> 4));
                result[3] = (byte)((CharToBase32[base32[5]] << 4) | (CharToBase32[base32[6]] >> 1));
                result[4] = (byte)((CharToBase32[base32[6]] << 7) | (CharToBase32[base32[7]] << 2) | (CharToBase32[base32[8]] >> 3));
                result[5] = (byte)((CharToBase32[base32[8]] << 5) | CharToBase32[base32[9]]);

                result[6] = (byte)((CharToBase32[base32[10]] << 3) | (CharToBase32[base32[11]] >> 2));
                result[7] = (byte)((CharToBase32[base32[11]] << 6) | (CharToBase32[base32[12]] << 1) | (CharToBase32[base32[13]] >> 4));
                result[8] = (byte)((CharToBase32[base32[13]] << 4) | (CharToBase32[base32[14]] >> 1));
                result[9] = (byte)((CharToBase32[base32[14]] << 7) | (CharToBase32[base32[15]] << 2) | (CharToBase32[base32[16]] >> 3));
                result[10] = (byte)((CharToBase32[base32[16]] << 5) | CharToBase32[base32[17]]);
                result[11] = (byte)((CharToBase32[base32[18]] << 3) | CharToBase32[base32[19]] >> 2);
                result[12] = (byte)((CharToBase32[base32[19]] << 6) | (CharToBase32[base32[20]] << 1) | (CharToBase32[base32[21]] >> 4));
                result[13] = (byte)((CharToBase32[base32[21]] << 4) | (CharToBase32[base32[22]] >> 1));
                result[14] = (byte)((CharToBase32[base32[22]] << 7) | (CharToBase32[base32[23]] << 2) | (CharToBase32[base32[24]] >> 3));
            }


            /// <summary>
            /// Read a Ulid value represented by a string from JSON.
            /// </summary>
            /// <param name="reader"></param>
            /// <param name="typeToConvert"></param>
            /// <param name="options"></param>
            /// <returns>The parsed value</returns>
            public override Ulid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                try
                {
                    if (reader.TokenType != JsonTokenType.String)
                        throw new JsonException("Expected string");

                    Span<byte> result = stackalloc byte[16];
                    if (reader.HasValueSequence)
                    {
                        // Parse using ValueSequence
                        var seq = reader.ValueSequence;
                        if (seq.Length != 26)
                            throw new JsonException("Ulid invalid: length must be 26");
                        Span<byte> buf = stackalloc byte[26];
                        seq.CopyTo(buf);
                        ParseSpan(buf, result);
                        return MemoryMarshal.Read<Ulid>(result);
                    }
                    else
                    {
                        // Parse usign ValueSpan
                        var buf = reader.ValueSpan;
                        if (buf.Length != 26)
                            throw new JsonException("Ulid invalid: length must be 26");
                        ParseSpan(buf, result);
                    }
                    return MemoryMarshal.Read<Ulid>(result);
                }
                catch (IndexOutOfRangeException e)
                {
                    throw new JsonException("Ulid invalid: length must be 26", e);
                }
                catch (OverflowException e)
                {
                    throw new JsonException("Ulid invalid: invalid character", e);
                }
            }

            public override void Write(Utf8JsonWriter writer, Ulid value, JsonSerializerOptions options)
            {
                Span<byte> buf = stackalloc byte[26];
                value.WriteStringifyBytes(buf);
                writer.WriteStringValue(buf);
            }
        }
    }
}
#endif
