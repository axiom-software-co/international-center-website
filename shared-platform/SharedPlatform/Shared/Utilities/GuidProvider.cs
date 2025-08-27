namespace SharedPlatform.Shared.Utilities;

public interface IGuidProvider
{
    Guid NewGuid();
}

public class GuidProvider : IGuidProvider
{
    public Guid NewGuid() => Guid.NewGuid();
}