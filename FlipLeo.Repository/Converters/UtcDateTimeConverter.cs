using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FlipLeo.Repository.Converters;

/// <summary>
/// SQL Server's datetime column has no time zone, so EF reads values back as DateTimeKind.Unspecified.
/// Those serialize to JSON without a trailing "Z" and the browser treats them as local time,
/// shifting them by the UTC offset. We store UTC, so mark values as UTC when reading them.
/// </summary>
public class UtcDateTimeConverter() : ValueConverter<DateTime, DateTime>(
    v => v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : v,
    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
