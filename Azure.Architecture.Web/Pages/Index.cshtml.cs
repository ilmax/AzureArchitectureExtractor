using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Azure.Architecture.Extractor.Services;
using static Azure.Architecture.Extractor.Services.UpdateResult;

namespace Azure.Architecture.Web.Pages;

public class IndexModel : PageModel
{
    private readonly IServiceMediator _extractorService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(IServiceMediator extractorService, ILogger<IndexModel> logger)
    {
        _extractorService = extractorService ?? throw new ArgumentNullException(nameof(extractorService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task OnGet()
    {
        await PopulateServiceAndDependenciesAsync();
    }

    public async Task OnPostPopulate()
    {
        UpdateResult = await _extractorService.ExtractDataAsync();

        // TBD
        switch (UpdateResult)
        {
            case Failure failure:
                _logger.LogError(failure.Exception, "Unable to load services");
                break;
        }

        await PopulateServiceAndDependenciesAsync();
    }

    private async Task PopulateServiceAndDependenciesAsync()
    {
        var (services, dependencies) = await _extractorService.GetServiceAndDependenciesAsync();
        Services = new SelectList(services);
        Dependencies = new SelectList(dependencies);
    }

    public async Task OnPostInit()
    {
        await _extractorService.ReinitializeDbAsync();
        await PopulateServiceAndDependenciesAsync();
    }

    public async Task OnPostView()
    {
        DotGraph = await _extractorService.GenerateGraphAsync(SelectedServices, SelectedDependencies);
        await PopulateServiceAndDependenciesAsync();
    }

    public SelectList Services { get; set; } = null!;
    public SelectList Dependencies { get; set; } = null!;

    [BindProperty]
    public string[] SelectedServices { get; set; } = null!;

    [BindProperty]
    public string[] SelectedDependencies { get; set; } = null!;

    public string? DotGraph { get; private set; }

    public UpdateResult? UpdateResult { get; private set; }
}