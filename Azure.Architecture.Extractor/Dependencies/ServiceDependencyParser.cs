using Azure.Architecture.Extractor.Dependencies.Abstractions;
using Azure.Architecture.Extractor.Models;

namespace Azure.Architecture.Extractor.Dependencies;

class ServiceDependencyParser : IDependencyQueryProvider, IDependencyParser
{
    public string DependencyQuery => """" 
            (AppTraces
            | distinct AppRoleName, _SubscriptionId
            | union 
            AppDependencies
            | distinct AppRoleName, _SubscriptionId
            | union 
            AppRequests
            | distinct AppRoleName, _SubscriptionId)
            | distinct AppRoleName, SubscriptionId = _SubscriptionId 
            | where AppRoleName != ''
            """";

    public void ParseDependencyResult(DependencyContext context, AzureMonitorQueryResult queryResult)
    {
        List<Service> services = new();
        foreach (var service in queryResult.Deserialize<QueryModel>())
        {
            if (!string.IsNullOrEmpty(service.AppRoleName))
            {
                services.Add(Service.Create(service.AppRoleName, service.SubscriptionId));
            }
        }

        context.SetServices(services.ToArray());
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    private class QueryModel
    {
        public string AppRoleName { get; init; } = null!;
        public string SubscriptionId { get; init; } = null!;
    }
}