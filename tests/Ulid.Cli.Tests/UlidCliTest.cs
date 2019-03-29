using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using CliUtil = Ulid.Cli.Util;
using FluentAssertions;

namespace Ulid.Cli.Tests
{
    public class UlidCliTest
    {
        [Fact]
        public async Task Default()
        {
            var first = CliUtil.CreateUlid(DateTimeOffset.Now, null);
            await Task.Delay(1).ConfigureAwait(false);
            // empty string is treated same as null
            var second = CliUtil.CreateUlid(DateTimeOffset.Now, "");
            first.Time.Should().NotBe(second.Time);
            first.Random.Should().NotBeEquivalentTo(second.Random);
        }
        [Fact]
        public void SameTime()
        {
            var d = new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var first = CliUtil.CreateUlid(d, null);
            var second = CliUtil.CreateUlid(d, null);
            first.Time.Should().Be(d);
            second.Time.Should().Be(d);
            first.Random.Should().NotBeEquivalentTo(second.Random);
        }
        [Theory]
        [InlineData('1')]
        [InlineData('A')]
        public void SameRandomness(char c)
        {
            var r1 = new string(c, 16).ToLower();
            var r2 = new string(c, 16).ToUpper();
            var d = DateTimeOffset.Parse("1970-1-1T00:00:00Z");

            var first = CliUtil.CreateUlid(DateTimeOffset.Now, r1);
            var second = CliUtil.CreateUlid(DateTimeOffset.Now, r2);
            first.Random.Should().BeEquivalentTo(second.Random);
        }
        [Fact]
        public void InvalidRandomness()
        {
            var d = DateTimeOffset.Now;
            // short length
            Assert.Throws<ArgumentOutOfRangeException>(() => CliUtil.CreateUlid(d, "A"));
            // invalid character
            Assert.Throws<InvalidOperationException>(() => CliUtil.CreateUlid(d, new string('+', 16)));
        }
        [Fact]
        public void Base32()
        {
            var pairs = new KeyValuePair<string, byte[]>[]
            {
                new KeyValuePair<string, byte[]>("00000000", new byte[]{ 0, 0, 0, 0, 0 }),
                new KeyValuePair<string, byte[]>("ZZZZZZZZ", new byte[]{ 0xff, 0xff, 0xff, 0xff, 0xff }),
                new KeyValuePair<string, byte[]>(new string('1', 8), new byte[]{ 0x08, 0x42, 0x10, 0x84, 0x21 })
            };
            foreach(var pair in pairs)
            {
                var buf = new byte[5];
                CliUtil.ConvertBase32ToBytes(pair.Key, buf, 0);
                buf.Should().BeEquivalentTo(pair.Value);
            }
        }
        [Fact]
        public void Base32SameAsUlidGenerated()
        {
            var ulid = System.Ulid.Parse("0123456789ABCDEFGHJKMNPQRS");
            var b32 = ulid.ToString();
            var b = ulid.ToByteArray();
            var actual = new byte[b.Length];
            CliUtil.ConvertBase32ToBytes(b32, actual, 2);
            actual.Should().BeEquivalentTo(b);
        }
    }
}
