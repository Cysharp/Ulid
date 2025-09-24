using System;
using System.ComponentModel;
using Xunit;

namespace UlidTests
{
    public class UlidTypeConverterTests
    {
        private readonly TypeConverter _ulidConverter = TypeDescriptor.GetConverter(typeof(Ulid));
        private readonly Ulid _testUlid = Ulid.NewUlid();

        [Fact]
        public void UlidCanConvertFromString()
        {
            var converted = _ulidConverter.ConvertFrom(_testUlid.ToString());

            Assert.Equal(_testUlid, converted);
        }

        [Fact]
        public void UlidCanConvertFromGuid()
        {
            var guid = _testUlid.ToGuid();

            var converted = _ulidConverter.ConvertFrom(guid);

            Assert.Equal(_testUlid, converted);
        }

        [Fact]
        public void UlidCanCovertToString()
        {
            var converted = _ulidConverter.ConvertTo(_testUlid, typeof(string));

            Assert.Equal(_testUlid.ToString(), converted);
        }

        [Fact]
        public void UlidCanConvertToGuid()
        {
            var converted = _ulidConverter.ConvertTo(_testUlid, typeof(Guid));
            Assert.Equal(_testUlid.ToGuid(), converted);
        }
    }
}
