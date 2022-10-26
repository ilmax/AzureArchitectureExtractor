using Azure.Architecture.Extractor.Config;
using Azure.Architecture.Extractor.Dependencies.Abstractions;
using Azure.Architecture.Extractor.Models;

namespace Azure.Architecture.Extractor.Dependencies;

class HttpDependencyParser : IServiceDependencyParser
{
    private readonly ExtractorConfig _extractorConfig;

    public HttpDependencyParser(ExtractorConfig extractorConfig)
    {
        _extractorConfig = extractorConfig ?? throw new ArgumentNullException(nameof(extractorConfig));
    }

    public string DependencyType => """
        "HTTP", "Http (tracked component)"
        """;

    public string DependencyQuery => $"""
            AppDependencies
            | where DependencyType has_any ({DependencyType})
            | where Target !has "login.microsoftonline.com"
            | where Target !has "127.0.0.1"
            | where Target !has "169.254.169.254"
            | project Target, Name, AppRoleName
            | distinct Target, Name, AppRoleName
            """;

    private DependencyKind GetDependencyKind(string target)
    {
        //if (target.Contains("azure-api.net", StringComparison.OrdinalIgnoreCase) ||
        //    target.Contains("service.signalr.net", StringComparison.OrdinalIgnoreCase) ||
        //    target.Contains("vault.azure.net", StringComparison.OrdinalIgnoreCase))
        //{
        //    return DependencyKind.Internal;
        //}

        if (_extractorConfig.IntenralHttpHosts?.Any(host => target.Contains(host, StringComparison.OrdinalIgnoreCase)) == true)
        {
            return DependencyKind.Internal;
        }

        return DependencyKind.External;
    }

    public void ParseDependencyResult(DependencyContext context, AzureMonitorQueryResult queryResult)
    {
        HashSet<(string, string)> paths = new();
        foreach (var item in queryResult.Deserialize<QueryModel>())
        {
            var service = context.FindServiceByName(item.AppRoleName);

            if (service is not null)
            {
                var target = item.Target;
                var index = target.IndexOf(" |", StringComparison.OrdinalIgnoreCase);
                if (index > 0)
                {
                    target = target.Substring(0, index);
                }
                if (paths.Add((item.AppRoleName, target)))
                {
                    var dependencyKind = GetDependencyKind(target);
                    if (dependencyKind == DependencyKind.Internal)
                    {
                        var apiCall = item.Name;
                        var firstSlashIndex = apiCall.IndexOf("/", StringComparison.OrdinalIgnoreCase);
                        var secondSlashIndex = apiCall.IndexOf("/", firstSlashIndex + 1, StringComparison.OrdinalIgnoreCase);
                        var targetService = apiCall.AsSpan().Slice(firstSlashIndex, secondSlashIndex - firstSlashIndex).ToString();
                        target += targetService;
                    }
                    service.AddDependency(target, dependencyKind, "HTTP");
                }
            }
        }
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class QueryModel
    {
        public string AppRoleName { get; init; } = null!;
        public string Target { get; init; } = null!;
        public string Name { get; init; } = null!;
    }
}