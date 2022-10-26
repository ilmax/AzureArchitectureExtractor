using Azure.Architecture.Extractor.Dependencies.Abstractions;
using Azure.Architecture.Extractor.Models;

namespace Azure.Architecture.Extractor.Dependencies;

class ServiceBusDependencyParser : IServiceDependencyParser
{
    public string DependencyType => "Azure Service Bus";

    public string DependencyQuery => $"""
            AppDependencies
            | where DependencyType =~ "{DependencyType}"
            | project Target, Name, AppRoleName
            | distinct Target, Name, AppRoleName
            """;

    public void ParseDependencyResult(DependencyContext context, AzureMonitorQueryResult queryResult)
    {
        foreach (var item in queryResult.Deserialize<QueryModel>())
        {
            var serviceName = item.AppRoleName;
            var service = context.FindServiceByName(serviceName);

            if (service is not null)
            {
                var target = item.Target;
                var action = item.Name; // TBD, this differentiate between send and receive
                var direction = action == "ServiceBusReceiver.Complete" ? Direction.Receiving : Direction.Sending;
                service.AddDependency(target, DependencyKind.Internal, DependencyType, direction);
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