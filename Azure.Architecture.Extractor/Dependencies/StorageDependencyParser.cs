using Azure.Architecture.Extractor.Dependencies.Abstractions;
using Azure.Architecture.Extractor.Models;

namespace Azure.Architecture.Extractor.Dependencies;

class StorageDependencyParser : IServiceDependencyParser
{
    public string DependencyType => "Azure blob";

    public string DependencyQuery => $"""
            AppDependencies
            | where DependencyType =~ "{DependencyType}"
            | where Data !has "logging"
            | project Target, Name, Data, AppRoleName
            | distinct Target, Name, Data, AppRoleName
            """;

    public void ParseDependencyResult(DependencyContext context, AzureMonitorQueryResult queryResult)
    {
        HashSet<(string, string)> paths = new();
        foreach (var item in queryResult.Deserialize<QueryModel>())
        {
            var serviceName = item.AppRoleName;
            var service = context.FindServiceByName(serviceName);

            if (service is not null)
            {
                var target = new UriBuilder(item.Data);
                target.Query = null;
                var indexOfSlash = target.Path.IndexOfAny(new[] { '/', '?' }, 1);
                if (indexOfSlash > 0)
                {
                    target.Path = target.Path.Substring(0, indexOfSlash);
                }

                if (paths.Add((serviceName, target.Uri.AbsoluteUri)))
                {
                    service.AddDependency(target.Uri.AbsoluteUri, DependencyKind.Internal, DependencyType);
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
        public string Data { get; init; } = null!;
    }
}