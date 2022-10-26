using Azure.Monitor.Query.Models;

namespace Azure.Architecture.Extractor.Dependencies;

public class AzureMonitorQueryResult
{
    private readonly LogsBatchQueryResultCollection _queryResult;
    private readonly string _queryId;

    public AzureMonitorQueryResult(LogsBatchQueryResultCollection queryResult, string queryId)
    {
        _queryResult = queryResult ?? throw new ArgumentNullException(nameof(queryResult));
        _queryId = queryId ?? throw new ArgumentNullException(nameof(queryId));
    }

    public IEnumerable<T> Deserialize<T>() => _queryResult.GetResult<T>(_queryId);
}