using MessagePack;
using System.Buffers;
using MessagePack.Formatters;
using System;

namespace Cysharp.Serialization.MessagePack
{
    public class UlidMessagePackFormatter : IMessagePackFormatter<Ulid>
    {
        public Ulid Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            var bin = reader.ReadBytes();
            if (bin == null)
            {
                throw new MessagePackSerializationException(string.Format("Unexpected msgpack code {0} ({1}) encountered.", MessagePackCode.Nil, MessagePackCode.ToFormatName(MessagePackCode.Nil)));
            }

            var seq = bin.Value;
            if (seq.IsSingleSegment)
            {
                return new Ulid(seq.First.Span);
            }
            else
            {
                Span<byte> buf = stackalloc byte[16];
                seq.CopyTo(buf);
                return new Ulid(buf);
            }
        }

        public void Serialize(ref MessagePackWriter writer, Ulid value, MessagePackSerializerOptions options)
        {
            const int Length = 16;

            writer.WriteBinHeader(Length);
            var buffer = writer.GetSpan(Length);
            value.TryWriteBytes(buffer);
            writer.Advance(Length);
        }
    }

    public class UlidMessagePackResolver : IFormatterResolver
    {
        public static IFormatterResolver Instance = new UlidMessagePackResolver();

        UlidMessagePackResolver()
        {

        }

        public IMessagePackFormatter<T> GetFormatter<T>()
        {
            return Cache<T>.formatter;
        }

        static class Cache<T>
        {
            public static readonly IMessagePackFormatter<T> formatter;

            static Cache()
            {
                if (typeof(T) == typeof(Ulid))
                {
                    formatter = (IMessagePackFormatter<T>)(object)new UlidMessagePackFormatter();
                }
            }
        }
    }
}