using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using Xunit;

namespace UlidTests
{
    public class UlidEscapedJsonTest
    {
        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public void DeserializeEscapedUlid(bool segmented, bool dictionaryKey)
        {
            const string text = "01ARZ3NDEKTSV4RRFFQ69G5FAV";
            const string escaped = "\\u00301ARZ3NDEKTSV4RRFFQ69G5FAV";
            var json = dictionaryKey ? "{\"" + escaped + "\":1}" : "\"" + escaped + "\"";
            var bytes = Encoding.UTF8.GetBytes(json);
            var options = new JsonSerializerOptions();
            options.Converters.Add(new Cysharp.Serialization.Json.UlidJsonConverter());
            var first = new Segment(bytes.AsMemory(0, 5));
            var last = first.Append(bytes.AsMemory(5));
            var reader = segmented
                ? new Utf8JsonReader(new ReadOnlySequence<byte>(first, 0, last, last.Memory.Length))
                : new Utf8JsonReader(bytes);

            if (dictionaryKey)
            {
                var result = JsonSerializer.Deserialize<Dictionary<Ulid, int>>(ref reader, options);
                Assert.Equal(1, result[Ulid.Parse(text)]);
            }
            else
            {
                Assert.Equal(Ulid.Parse(text), JsonSerializer.Deserialize<Ulid>(ref reader, options));
            }
        }

        private sealed class Segment : ReadOnlySequenceSegment<byte>
        {
            public Segment(ReadOnlyMemory<byte> memory)
            {
                Memory = memory;
            }

            public Segment Append(ReadOnlyMemory<byte> memory)
            {
                var next = new Segment(memory) { RunningIndex = RunningIndex + Memory.Length };
                Next = next;
                return next;
            }
        }
    }
}
