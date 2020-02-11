#if SystemTextJson
using System.Text.Json;
using System.Text.Json.Serialization;

namespace System._Ulid
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
            return Ulid.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, Ulid value, JsonSerializerOptions options)
        {
            var toString = value.ToString();
            writer.WriteStringValue(toString);
        }
    }
}
#endif