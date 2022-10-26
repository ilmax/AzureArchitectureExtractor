using Azure.Architecture.Extractor.Dependencies.Abstractions;
using Azure.Architecture.Extractor.Models;

namespace Azure.Architecture.Extractor.Dependencies;

class CosmosDbDependencyParser : IServiceDependencyParser
{
    public string DependencyType => "Azure DocumentDB";

    public string DependencyQuery => $"""
            AppDependencies
            | where DependencyType =~ "{DependencyType}"
            | where Name in ("Query documents", "Create/query document")
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
                var target = item.Data;
                if (paths.Add((serviceName, target)))
                {
                    service.AddDependency(target, DependencyKind.Internal, DependencyType);
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