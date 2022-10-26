using Azure.Architecture.Extractor.Dependencies.Abstractions;
using Azure.Architecture.Extractor.Models;

namespace Azure.Architecture.Extractor.Dependencies;

class SqlDependencyParser : IServiceDependencyParser
{
    public string DependencyType => "SQL";

    public string DependencyQuery => $"""
            AppDependencies
            | where DependencyType =~ "{DependencyType}"
            | project Target, Name, AppRoleName
            | distinct Target, Name, AppRoleName
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
                var target = item.Target;
                if (paths.Add((serviceName, target.Trim())))
                {
                    service.AddDependency(target.Trim(), DependencyKind.Internal, DependencyType);
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