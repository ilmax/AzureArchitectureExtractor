namespace Azure.Architecture.Extractor.Models;

public class ServiceDependency
{
    // Used by EF
    private ServiceDependency(string target, DependencyKind kind, string type, DateTimeOffset discoveredAtUtc, Direction direction)
    {
        Target = target;
        Kind = kind;
        Type = type;
        DiscoveredAtUtc = discoveredAtUtc;
        Direction = direction;
    }

    public string Target { get; private set; }
    public DependencyKind Kind { get; private set; }
    public string Type { get; private set; }
    public DateTimeOffset DiscoveredAtUtc { get; private set; }
    public Direction Direction { get; private set; }

    public static ServiceDependency Create(string target, DependencyKind kind, string type, Direction direction)
    {
        if (string.IsNullOrEmpty(type))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(type));
        }

        return new ServiceDependency(target, kind, type, DateTimeOffset.UtcNow, direction);
    }
}