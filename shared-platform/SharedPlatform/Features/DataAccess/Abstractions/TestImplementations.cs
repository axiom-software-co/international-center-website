namespace SharedPlatform.Features.DataAccess.Abstractions;

/// <summary>
/// Test implementations of domain contracts for RED phase testing
/// These will be replaced by actual ServicesDomain implementations in GREEN phase
/// </summary>
internal sealed class TestService : IService
{
    public Guid ServiceId { get; init; } = Guid.NewGuid();
    public string Title { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string DeliveryMode { get; init; } = string.Empty;
    public Guid CategoryId { get; init; } = Guid.NewGuid();
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}

internal sealed class TestServiceId : IServiceId
{
    public Guid Value { get; }

    public TestServiceId(Guid value) => Value = value;

    public static TestServiceId New() => new(Guid.NewGuid());
}

internal sealed class TestServiceSlug : IServiceSlug
{
    public string Value { get; }

    public TestServiceSlug(string value) => Value = value ?? throw new ArgumentNullException(nameof(value));

    public static TestServiceSlug From(string value) => new(value);
}

internal sealed class TestServiceCategoryId : IServiceCategoryId
{
    public Guid Value { get; }

    public TestServiceCategoryId(Guid value) => Value = value;

    public static TestServiceCategoryId New() => new(Guid.NewGuid());
}