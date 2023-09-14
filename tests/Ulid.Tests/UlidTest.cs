using System;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace UlidTests
{
    public class UlidTest
    {
        [Fact]
        public void New_ByteEquals_ToString_Equals()
        {
            for (int i = 0; i < 100; i++)
            {
                {
                    var ulid = Ulid.NewUlid();
                    var nulid = new NUlid.Ulid(ulid.ToByteArray());

                    ulid.ToByteArray().Should().BeEquivalentTo(nulid.ToByteArray());
                    ulid.ToString().Should().Be(nulid.ToString());
                    ulid.Equals(ulid).Should().BeTrue();
                    ulid.Equals(Ulid.NewUlid()).Should().BeFalse();
                }
                {
                    var nulid = NUlid.Ulid.NewUlid();
                    var ulid = new Ulid(nulid.ToByteArray());

                    ulid.ToByteArray().Should().BeEquivalentTo(nulid.ToByteArray());
                    ulid.ToString().Should().Be(nulid.ToString());
                    ulid.Equals(ulid).Should().BeTrue();
                    ulid.Equals(Ulid.NewUlid()).Should().BeFalse();
                }
            }
        }

        [Fact]
        public void Compare_Time()
        {
            var times = new DateTimeOffset[]
            {
                new DateTime(2012,12,4),
                new DateTime(2011,12,31),
                new DateTime(2012,1,5),
                new DateTime(2013,12,4),
                new DateTime(2016,12,4),
            };

            times.Select(x => Ulid.NewUlid(x)).OrderBy(x => x).Select(x => x.Time).Should().BeEquivalentTo(times.OrderBy(x => x));
        }

        [Fact]
        public void HashCode()
        {
            var ulid = Ulid.Parse("01ARZ3NDEKTSV4RRFFQ69G5FAV");

            Assert.Equal(-1363483029, ulid.GetHashCode());
        }

        [Fact]
        public void Parse()
        {
            for (int i = 0; i < 100; i++)
            {
                var nulid = NUlid.Ulid.NewUlid();
                Ulid.Parse(nulid.ToString()).ToByteArray().Should().BeEquivalentTo(nulid.ToByteArray());
            }
        }

        [Fact]
        public void Randomness()
        {
            var d = DateTime.Parse("1970/1/1 00:00:00Z");
            var r = new byte[10];
            var first = Ulid.NewUlid(d, r);
            var second = Ulid.NewUlid(d, r);
            first.ToString().Should().BeEquivalentTo(second.ToString());
            // Console.WriteLine($"first={first.ToString()}, second={second.ToString()}");
        }

        [Fact]
        public void GuidInterop()
        {
            var ulid = Ulid.NewUlid();
            var guid = ulid.ToGuid();
            var ulid2 = new Ulid(guid);

            ulid2.Should().BeEquivalentTo(ulid, "a Ulid-Guid roundtrip should result in identical values");
        }

        [Fact]
        public void UlidCompareTo()
        {
            var largeUlid = Ulid.MaxValue;
            var smallUlid = Ulid.MinValue;

            largeUlid.CompareTo(smallUlid).Should().Be(1);
            smallUlid.CompareTo(largeUlid).Should().Be(-1);
            smallUlid.CompareTo(smallUlid).Should().Be(0);

            object smallObject = (object)smallUlid;
            largeUlid.CompareTo(smallUlid).Should().Be(1);
            largeUlid.CompareTo(null).Should().Be(1);
            largeUlid.Invoking(u=> u.CompareTo("")).Should().Throw<ArgumentException>();
        }

        [Fact]
        public void GuidComparison()
        {
            var data_smaller = new byte[] { 0, 255, 255, 255, 255, 255, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var data_larger = new byte[] { 1, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            var ulid_smaller = new Ulid(data_smaller);
            var ulid_larger = new Ulid(data_larger);

            var guid_smaller = ulid_smaller.ToGuid();
            var guid_larger = ulid_larger.ToGuid();

            ulid_smaller.CompareTo(ulid_larger).Should().BeLessThan(0, "a Ulid comparison should compare byte to byte");
            guid_smaller.CompareTo(guid_larger).Should().BeLessThan(0, "a Ulid to Guid cast should preserve order");
        }

        [Fact]
        public void UlidParseRejectsInvalidStrings()
        {
            Assert.Throws<ArgumentException>(() => Ulid.Parse("1234"));
            Assert.Throws<ArgumentException>(() => Ulid.Parse(Guid.NewGuid().ToString()));
        }

        [Fact]
        public void UlidTryParseFailsForInvalidStrings()
        {
            Assert.False(Ulid.TryParse("1234", out _));
            Assert.False(Ulid.TryParse(Guid.NewGuid().ToString(), out _));
        }

#if NET6_0_OR_GREATER
        [Fact]
        public void UlidTryFormatReturnsStringAndLength()
        {
            var asString = "01ARZ3NDEKTSV4RRFFQ69G5FAV";
            var ulid = Ulid.Parse(asString);
            var destination = new char[26];
            var largeDestination = new char[27];

            ulid.TryFormat(destination, out int length, default, null).Should().BeTrue();
            destination.Should().BeEquivalentTo(asString);
            length.Should().Be(26);

            ulid.TryFormat(largeDestination, out int largeLength, default, null).Should().BeTrue();
            largeDestination.AsSpan().Slice(0,26).ToArray().Should().BeEquivalentTo(asString);
            largeLength.Should().Be(26);
        }
        
        [Fact]
        public void UlidTryFormatReturnsFalseWhenInvalidDestination()
        {
            var asString = "01ARZ3NDEKTSV4RRFFQ69G5FAV";
            var ulid = Ulid.Parse(asString);
            var formatted = new char[25];

            ulid.TryFormat(formatted, out int length, default, null).Should().BeFalse();
            formatted.Should().BeEquivalentTo(new char[25]);
            length.Should().Be(0);
        }
#endif
#if NET7_0_OR_GREATER

        [Fact]
        public void IParsable()
        {
            for (int i = 0; i < 100; i++)
            {
                var nulid = NUlid.Ulid.NewUlid();
                Ulid.Parse(nulid.ToString(),null).ToByteArray().Should().BeEquivalentTo(nulid.ToByteArray());

                Ulid.TryParse(nulid.ToString(), null, out Ulid ulid).Should().BeTrue();
                ulid.ToByteArray().Should().BeEquivalentTo(nulid.ToByteArray());
            }
        }

        [Fact]
        public void ISpanParsable()
        {
            for (int i = 0; i < 100; i++)
            {
                var nulid = NUlid.Ulid.NewUlid();
                Ulid.Parse(nulid.ToString().AsSpan(), null).ToByteArray().Should().BeEquivalentTo(nulid.ToByteArray());

                Ulid.TryParse(nulid.ToString().AsSpan(), null, out Ulid ulid).Should().BeTrue();
                ulid.ToByteArray().Should().BeEquivalentTo(nulid.ToByteArray());
            }
        }
#endif
        
        [Fact]
        public void TryFormatUtf8Bytes()
        {
            var value = Ulid.NewUlid();
            var utf8Value = System.Text.Encoding.UTF8.GetBytes(value.ToString());

            var result = new byte[26];
            value.TryFormat(result, out var bytesWritten, Array.Empty<char>(), null).Should().BeTrue();
            bytesWritten.Should().Be(26);
            result.Should().Equal(utf8Value);
        }
    }
}

