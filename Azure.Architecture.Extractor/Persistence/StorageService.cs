using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Azure.Architecture.Extractor.Models;
using Azure.Architecture.Extractor.Services;
using static Azure.Architecture.Extractor.Services.UpdateResult;

namespace Azure.Architecture.Extractor.Persistence;

internal class StorageService
{
    private readonly AppDbContext _appDbContext;

    public StorageService(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext ?? throw new ArgumentNullException(nameof(appDbContext));
    }

    public async Task<UpdateResult> StoreAsync(DependencyContext dependencyContext)
    {
        if (dependencyContext == null)
        {
            throw new ArgumentNullException(nameof(dependencyContext));
        }

        int newServices = 0, newDependencies = 0;

        try
        {
            await _appDbContext.Database.EnsureCreatedAsync();

            var servicesFromDb = await _appDbContext.Services
                .Include(s => s.Dependencies)
                .ToListAsync();

            foreach (var service in dependencyContext.Services)
            {
                var serviceFromDb = servicesFromDb.SingleOrDefault(s => s.Name == service.Name);
                if (serviceFromDb is null)
                {
                    _appDbContext.Services.Add(service);
                    newServices++;
                }
                else
                {
                    newDependencies += serviceFromDb.MergeWith(service);
                }
            }

            await _appDbContext.SaveChangesAsync();
            if (newServices + newDependencies > 0)
            {
                return new Success(newServices, newDependencies);
            }

            return new NoChanges();
        }
        catch (Exception ex)
        {
            return new Failure(ex);
        }
    }

    public async Task<IEnumerable<Service>> GetAllAsync(string[]? dependencies)
    {
        if (dependencies?.Length > 0)
        {
            return await _appDbContext.Services
            .Include(s => s.Dependencies.Where(d => dependencies.Contains(d.Type)))
            .ToListAsync();

        }

        return await _appDbContext.Services
            .Include(s => s.Dependencies)
            .ToListAsync();
    }

    public async Task<IEnumerable<Service>> GetByNamesAsync(string[] names, string[] dependencies)
    {
        // The lambda parameter.
        var serviceParameter = Expression.Parameter(typeof(Service), "s");

        // Build the individual conditions to check against.
        var orConditions = names
            .Select(keyword => (Expression<Func<Service, bool>>)(i => i.Name.Contains(keyword)))
            .Select(lambda => (Expression)Expression.Invoke(lambda, serviceParameter))
            .ToList();

        // Combine the individual conditions to an expression tree of nested ORs.
        var orExpressionTree = orConditions
            .Skip(1)
            .Aggregate(
                orConditions.First(),
                (current, expression) => Expression.OrElse(expression, current));

        // Build the final predicate (a lambda expression), so we can use it inside of `.Where()`.
        var predicateExpression = (Expression<Func<Service, bool>>)Expression.Lambda(
            orExpressionTree,
            serviceParameter);

        if (dependencies.Length > 0)
        {
            return await _appDbContext.Services
                .Include(s => s.Dependencies.Where(d => dependencies.Contains(d.Type)))
                .Where(predicateExpression)
                .ToListAsync();
        }

        return await _appDbContext.Services
            .Include(s => s.Dependencies)
            .Where(predicateExpression)
            .ToListAsync();
    }

    public async Task<string[]> GetAllServiceNameAsync() =>
        await _appDbContext.Services.Select(s => s.Name).ToArrayAsync();

    public async Task<string[]> GetAllDependencies() =>
        await _appDbContext.Set<ServiceDependency>().Select(d => d.Type).Distinct().ToArrayAsync();

    public async Task ReinitializeDbAsync(bool delete)
    {
        if (delete)
        {
            await _appDbContext.Database.EnsureDeletedAsync();
        }

        await _appDbContext.Database.EnsureCreatedAsync();
    }
}