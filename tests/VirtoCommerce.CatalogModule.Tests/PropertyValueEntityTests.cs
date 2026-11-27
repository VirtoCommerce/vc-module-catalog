using System;
using VirtoCommerce.CatalogModule.Core.Model;
using VirtoCommerce.CatalogModule.Data.Model;
using VirtoCommerce.Platform.Core.Common;
using Xunit;

namespace VirtoCommerce.CatalogModule.Tests
{
    public class PropertyValueEntityTests
    {
        [Theory]
        [InlineData("2026-11-20T10:00:00.000Z", 20, 10)]
        [InlineData("2026-11-20T13:00:00+03:00", 20, 10)]
        [InlineData("2026-11-20T07:00:00-03:00", 20, 10)]
        [InlineData("2026-11-20T10:00:00", 20, 10)]
        [InlineData("2026-11-20T01:00:00+03:00", 19, 22)]
        [InlineData("2026-11-20T23:00:00-03:00", 21, 2)]
        public void FromModel_DateTimeString_StoresUtc(string value, int expectedDay, int expectedHour)
        {
            var entity = FromDateTimeValue(value);
            var expected = new DateTime(2026, 11, expectedDay, expectedHour, 0, 0, DateTimeKind.Utc);

            Assert.Equal(expected.Ticks, entity.DateTimeValue.Value.Ticks);
            Assert.Equal(DateTimeKind.Utc, entity.DateTimeValue.Value.Kind);
        }

        [Fact]
        public void FromModel_DateTimeString_PreservesFractionalSeconds()
        {
            var entity = FromDateTimeValue("2026-11-20T10:00:00.1234567Z");
            var expected = new DateTime(2026, 11, 20, 10, 0, 0, DateTimeKind.Utc).AddTicks(1234567);

            Assert.Equal(expected.Ticks, entity.DateTimeValue.Value.Ticks);
            Assert.Equal(DateTimeKind.Utc, entity.DateTimeValue.Value.Kind);
        }

        [Theory]
        [InlineData(DateTimeKind.Utc)]
        [InlineData(DateTimeKind.Local)]
        [InlineData(DateTimeKind.Unspecified)]
        public void FromModel_DateTime_PreservesTicksAndKind(DateTimeKind kind)
        {
            var value = new DateTime(2026, 11, 20, 10, 0, 0, kind).AddTicks(1234567);

            var entity = FromDateTimeValue(value);

            Assert.Equal(value.Ticks, entity.DateTimeValue.Value.Ticks);
            Assert.Equal(value.Kind, entity.DateTimeValue.Value.Kind);
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-a-date")]
        public void FromModel_InvalidDateTimeString_ThrowsFormatException(string value)
        {
            Assert.Throws<FormatException>(() => FromDateTimeValue(value));
        }

        private static PropertyValueEntity FromDateTimeValue(object value)
        {
            var propertyValue = new PropertyValue
            {
                PropertyName = "Date",
                ValueType = PropertyValueType.DateTime,
                Value = value,
            };

            return new PropertyValueEntity().FromModel(propertyValue, new PrimaryKeyResolvingMap());
        }
    }
}
