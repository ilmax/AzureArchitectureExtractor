using Azure.Monitor.Query;
using Azure.Architecture.Extractor.Dependencies.Abstractions;
using Azure.Architecture.Extractor.Config;

namespace Azure.Architecture.Extractor.Dependencies.DataReader;

internal class AzureMonitorDataReader
{
    private readonly LogsQueryClient _logsQueryClient;
    private readonly IEnumerable<IServiceDependencyParser> _dependencyParsers;
    private readonly ExtractorConfig _extractorConfig;

    public AzureMonitorDataReader(LogsQueryClient logsQueryClient, IEnumerable<IServiceDependencyParser> dependencyParsers, ExtractorConfig extractorConfig)
    {
        _logsQueryClient = logsQueryClient ?? throw new ArgumentNullException(nameof(logsQueryClient));
        _dependencyParsers = dependencyParsers ?? throw new ArgumentNullException(nameof(dependencyParsers));
        _extractorConfig = extractorConfig ?? throw new ArgumentNullException(nameof(extractorConfig));
    }

    public async Task<DependencyContext> ReadDependenciesAsync()
    {
        if (string.IsNullOrEmpty(_extractorConfig.WorkspaceId))
        {
            throw new InvalidOperationException("Please make sure the workspace id value is configured int app config");
        }

        var dependencyContext = new DependencyContext();

        await RunApplicationInsightQueryAsync(_extractorConfig.WorkspaceId, new ServiceDependencyParser(), dependencyContext);

        foreach (var dependencyParser in _dependencyParsers)
        {
            await RunApplicationInsightQueryAsync(_extractorConfig.WorkspaceId, dependencyParser, dependencyContext);
        }

        return dependencyContext;
    }

    private async Task RunApplicationInsightQueryAsync<TQueryParser>(string workspaceId, TQueryParser queryParser, DependencyContext context)
        where TQueryParser : IDependencyQueryProvider, IDependencyParser
    {
        var batch = new LogsBatchQuery();

        var query = batch.AddWorkspaceQuery(
            workspaceId: workspaceId,
            query: queryParser.DependencyQuery,
            timeRange: new QueryTimeRange(TimeSpan.FromDays(_extractorConfig.LogAnalyticsQueryDays)));

        var operationResponse = await _logsQueryClient.QueryBatchAsync(batch);

        if (!operationResponse.GetRawResponse().IsError)
        {
            queryParser.ParseDependencyResult(context, new AzureMonitorQueryResult(operationResponse.Value, query));
        }
    }
}