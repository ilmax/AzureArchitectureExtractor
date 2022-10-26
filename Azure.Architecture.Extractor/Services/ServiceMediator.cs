using Azure.Architecture.Extractor.Dependencies.DataReader;
using Azure.Architecture.Extractor.Graph;
using Azure.Architecture.Extractor.Persistence;
using static Azure.Architecture.Extractor.Services.UpdateResult;

namespace Azure.Architecture.Extractor.Services;

// TODO find a better name for me :)
internal class ServiceMediator : IServiceMediator
{
    private readonly AzureMonitorDataReader _azureMonitorDataReader;
    private readonly StorageService _storageService;
    private readonly GraphBuilder _graphBuilder;

    public ServiceMediator(AzureMonitorDataReader azureMonitorDataReader, StorageService storageService, GraphBuilder graphBuilder)
    {
        _azureMonitorDataReader = azureMonitorDataReader ?? throw new ArgumentNullException(nameof(azureMonitorDataReader));
        _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
        _graphBuilder = graphBuilder ?? throw new ArgumentNullException(nameof(graphBuilder));
    }

    public async Task<UpdateResult> ExtractDataAsync()
    {
        try
        {
            var context = await _azureMonitorDataReader.ReadDependenciesAsync();

            return await _storageService.StoreAsync(context);
        }
        catch (Exception ex)
        {
            return new Failure(ex);
        }
    }

    public async Task<(IEnumerable<string> Services, IEnumerable<string> Dependencies)> GetServiceAndDependenciesAsync()
    {
        var services = await _storageService.GetAllServiceNameAsync();
        var dependencies = await _storageService.GetAllDependencies();

        return (services, dependencies);
    }

    public async Task ReinitializeDbAsync()
    {
        await _storageService.ReinitializeDbAsync(delete: true);
    }

    public async Task InitializeDbAsync()
    {
        await _storageService.ReinitializeDbAsync(delete: false);
    }

    public async Task<string> GenerateGraphAsync(string[] selectedServices, string[] selectedDependencies)
    {
        var services = await _storageService.GetAllServiceNameAsync();
        switch (selectedServices.Length)
        {
            case > 1 when services.Length == selectedDependencies.Length:
                return _graphBuilder.Generate(await _storageService.GetAllAsync(selectedDependencies));

            case > 1:
                return _graphBuilder.Generate(await _storageService.GetByNamesAsync(selectedServices, selectedDependencies));

            default:
                return "";
        }
    }
}
