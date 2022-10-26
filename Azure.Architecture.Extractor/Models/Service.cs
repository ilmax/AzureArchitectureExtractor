namespace Azure.Architecture.Extractor.Models;

public class Service
{
    // Used by EF
    private Service(string name, DateTimeOffset discoveredAtUtc, DateTimeOffset? updatedAtUtc, string subscription)
    {
        Name = name;
        DiscoveredAtUtc = discoveredAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        Subscription = subscription;
    }

    public string Name { get; init; }
    public ICollection<ServiceDependency> Dependencies { get; private set; } = new List<ServiceDependency>();
    public DateTimeOffset DiscoveredAtUtc { get; init; }
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public string Subscription { get; private set; }

    public static Service Create(string name, string subscription)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (subscription == null)
        {
            throw new ArgumentNullException(nameof(subscription));
        }

        return new Service(name, DateTimeOffset.UtcNow, null, subscription);
    }

    public void AddDependency(string target, DependencyKind kind, string type, Direction direction = Direction.Sending)
    {
        if (string.IsNullOrEmpty(type))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(type));
        }
        Dependencies.Add(ServiceDependency.Create(target, kind, type, direction));
    }

    public int MergeWith(Service service)
    {
        var comparer = ServiceDependencyEqualityComparer.Instance;

        if (string.IsNullOrEmpty(Subscription) && !string.IsNullOrEmpty(service.Subscription))
        {
            Subscription = service.Subscription;
        }

        int newDependencies = 0;

        foreach (var serviceDependency in service.Dependencies)
        {
            if (!Dependencies.Contains(serviceDependency, comparer))
            {
                Dependencies.Add(serviceDependency);
                UpdatedAtUtc = DateTimeOffset.UtcNow;
                newDependencies++;
            }
        }

        return newDependencies;
    }

    private class ServiceDependencyEqualityComparer : IEqualityComparer<ServiceDependency>
    {
        public static readonly ServiceDependencyEqualityComparer Instance = new();

        public bool Equals(ServiceDependency? x, ServiceDependency? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Target == y.Target && x.Type == y.Type;
        }

        public int GetHashCode(ServiceDependency obj)
        {
            return HashCode.Combine(obj.Target, obj.Type);
        }
    }
}