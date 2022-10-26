namespace Azure.Architecture.Extractor.Services;

public interface IServiceMediator
{
    Task<UpdateResult> ExtractDataAsync();
    Task<string> GenerateGraphAsync(string[] selectedServices, string[] selectedDependencies);
    Task<(IEnumerable<string> Services, IEnumerable<string> Dependencies)> GetServiceAndDependenciesAsync();
    Task InitializeDbAsync();
    Task ReinitializeDbAsync();
}