using Microsoft.EntityFrameworkCore;
using Azure.Architecture.Extractor.Config;
using Azure.Architecture.Extractor;
using Azure.Architecture.Extractor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddArchitectureExtractorServices(
    opt => opt.UseSqlite("Data source=SQLite.db"), 
    builder.Configuration.GetSection("Config").Get<ExtractorConfig>()!);

var app = builder.Build();

// Create the db
await using var scope = app.Services.CreateAsyncScope();
var service = scope.ServiceProvider.GetRequiredService<IServiceMediator>();
await service.InitializeDbAsync();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
