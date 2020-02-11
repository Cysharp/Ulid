#if SystemTextJson
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System
{
    public class UlidJsonConverter : JsonConverter<Ulid>
    {
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
                return Ulid.Parse(reader.GetString());
            }
            catch (ArgumentException e)
            {
                throw new JsonException("Ulid invalid: length must be 26", e);
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
            var toString = value.ToString();
            writer.WriteStringValue(toString);
        }
    }
}
#endif
