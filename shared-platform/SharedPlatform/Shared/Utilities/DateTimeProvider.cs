namespace SharedPlatform.Shared.Utilities;

public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
    DateTimeOffset OffsetNow { get; }
    DateTimeOffset OffsetUtcNow { get; }
}

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
    public DateTimeOffset OffsetNow => DateTimeOffset.Now;
    public DateTimeOffset OffsetUtcNow => DateTimeOffset.UtcNow;
}