namespace Azure.Architecture.Extractor.Dependencies.Abstractions;

public interface IDependencyParser
{
    void ParseDependencyResult(DependencyContext context, AzureMonitorQueryResult queryResult);
}