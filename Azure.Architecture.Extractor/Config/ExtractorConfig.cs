namespace Azure.Architecture.Extractor.Config;

public class ExtractorConfig
{
    public string[] IntenralHttpHosts { get; init; } = Array.Empty<string>();
    public int LogAnalyticsQueryDays { get; init; } = 7;
    public string WorkspaceId { get; init; } = null!;
}
