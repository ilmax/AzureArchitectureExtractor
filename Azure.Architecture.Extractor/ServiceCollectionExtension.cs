using Azure.Architecture.Extractor.Config;
using Azure.Architecture.Extractor.Dependencies.Abstractions;
using Azure.Architecture.Extractor.Dependencies.DataReader;
using Azure.Architecture.Extractor.Graph;
using Azure.Architecture.Extractor.Persistence;
using Azure.Architecture.Extractor.Services;
using Azure.Identity;
using Azure.Monitor.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Architecture.Extractor;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddArchitectureExtractorServices(
        this IServiceCollection services, 
        Action<DbContextOptionsBuilder> configureContext, 
        ExtractorConfig configuration)
    {
        services.AddDbContext<AppDbContext>(configureContext)
        .AddSingleton<LogsQueryClient>(new LogsQueryClient(new DefaultAzureCredential()))
        .AddTransient<AzureMonitorDataReader>()
        .AddTransient<IServiceMediator, ServiceMediator>()
        .AddTransient<GraphBuilder>()
        .AddTransient<StorageService>()
        .AddMemoryCache()
        .AddDependencyParsers()
        .AddSingleton(configuration);

        return services;
    }

    private static IServiceCollection AddDependencyParsers(this IServiceCollection serviceCollection)
    {
        var types = typeof(IServiceDependencyParser).Assembly.GetTypes()
            .Where(t => !t.IsAbstract && !t.IsInterface && t.IsAssignableTo(typeof(IServiceDependencyParser)));

        foreach (var type in types)
        {
            serviceCollection.AddTransient(typeof(IServiceDependencyParser), type);
        }

        return serviceCollection;
    }
}
