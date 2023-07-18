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
        void UlidTryParseFailsForInvalidStrings()
        {
            Assert.False(Ulid.TryParse("1234", out _));
            Assert.False(Ulid.TryParse(Guid.NewGuid().ToString(), out _));
        }

        [Fact]
        void UlidInverse()
        {
            var ulid = Ulid.Parse("01H5M9J9BM30QWKHQGPY01AHPC");
            var inverseUlid = Ulid.Parse("7YETBPDPMBWZ83CE8F91ZYNE9K");

            inverseUlid.Should().BeEquivalentTo(ulid.Inverse());
        }
    }
}
