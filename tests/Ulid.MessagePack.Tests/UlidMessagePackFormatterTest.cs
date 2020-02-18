using FluentAssertions;
using MessagePack;
using System;
using Xunit;

namespace UlidTests
{
    public class UlidMessagePackFormatterTest
    {
        [MessagePackObject(true)]
        public class TestSerializationClass
        {
            public Ulid value { get; set; }
        }

        MessagePackSerializerOptions GetOptions()
        {
            var resolver = MessagePack.Resolvers.CompositeResolver.Create(
                Cysharp.Serialization.MessagePack.UlidMessagePackResolver.Instance,
                MessagePack.Resolvers.StandardResolver.Instance);
            return MessagePackSerializerOptions.Standard.WithResolver(resolver);
        }

        [Fact]
        public void SerializeTest()
        {
            var groundTruth = new TestSerializationClass()
            {
                value = Ulid.NewUlid()
            };

            var serialized = MessagePackSerializer.Serialize(groundTruth, GetOptions());
            var deserialized = MessagePackSerializer.Deserialize<TestSerializationClass>(serialized, GetOptions());
            deserialized.value.Should().BeEquivalentTo(groundTruth.value, "MSGPACK serialize roundtrip");
        }
    }
}
