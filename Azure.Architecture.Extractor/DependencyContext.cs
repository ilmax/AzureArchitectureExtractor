using Azure.Architecture.Extractor.Config;
using Azure.Architecture.Extractor.Models;

namespace Azure.Architecture.Extractor;

public class DependencyContext
{
    private Service[] _services = Array.Empty<Service>();

    public Service[] Services => _services;

    public Service? FindServiceByName(string serviceName)
    {
        if (string.IsNullOrEmpty(serviceName))
        {
            return null;
        }
        return Services.SingleOrDefault(s => s.Name == serviceName) ?? throw new InvalidOperationException($"Unable to find service {serviceName}");
    }

    public void SetServices(Service[] services)
    {
        if (Services.Length > 0)
        {
            throw new InvalidOperationException();
        }

        _services = services ?? throw new ArgumentNullException(nameof(services));
    }
}