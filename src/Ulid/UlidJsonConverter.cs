#if NETCOREAPP3_1_OR_GREATER || SYSTEM_TEXT_JSON

using System;
using System.Buffers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cysharp.Serialization.Json
{
    public class UlidJsonConverter : JsonConverter<Ulid>
    {
        /// <summary>
        /// Read a Ulid value represented by a string from JSON.
        /// </summary>
        public override Ulid Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            try
            {
                if (reader.TokenType != JsonTokenType.String) throw new JsonException("Expected string");

                if (reader.HasValueSequence)
                {
                    // Parse using ValueSequence
                    var seq = reader.ValueSequence;
                    if (seq.Length != 26) throw new JsonException("Ulid invalid: length must be 26");
                    Span<byte> buf = stackalloc byte[26];
                    seq.CopyTo(buf);
                    Ulid.TryParse(buf, out var ulid);
                    return ulid;
                }
                else
                {
                    // Parse usign ValueSpan
                    var buf = reader.ValueSpan;
                    if (buf.Length != 26) throw new JsonException("Ulid invalid: length must be 26");
                    Ulid.TryParse(buf, out var ulid);
                    return ulid;
                }
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
            value.TryWriteStringify(buf);
            writer.WriteStringValue(buf);
        }
    }
}

#endif