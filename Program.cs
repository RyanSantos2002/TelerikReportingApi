using Microsoft.Extensions.DependencyInjection.Extensions;
using Telerik.Reporting.Services;
using Telerik.WebReportDesigner.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddControllers();
// Configurando CORS para permitir acesso do FrontM8
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Configurando Telerik Reporting Services
string reportsPath = Path.Combine(builder.Environment.ContentRootPath, "Reports");
builder.Services.TryAddSingleton<IReportServiceConfiguration>(sp =>
    new ReportServiceConfiguration
    {
        ReportingEngineConfiguration = sp.GetService<IConfiguration>(),
        HostAppId = "TelerikReportingApi",
        Storage = new Telerik.Reporting.Cache.File.FileStorage(),
        ReportSourceResolver = new UriReportSourceResolver(reportsPath)
    });

// Configurando Telerik Web Report Designer Services
builder.Services.TryAddSingleton<IReportDesignerServiceConfiguration>(sp => 
    new ReportDesignerServiceConfiguration
    {
        DefinitionStorage = new Telerik.WebReportDesigner.Services.FileDefinitionStorage(reportsPath),
        ResourceStorage = new Telerik.WebReportDesigner.Services.ResourceStorage(Path.Combine(reportsPath, "Resources")),
        SharedDataSourceStorage = new Telerik.WebReportDesigner.Services.FileSharedDataSourceStorage(Path.Combine(reportsPath, "SharedDataSources")),
        SettingsStorage = new Telerik.WebReportDesigner.Services.FileSettingsStorage(Path.Combine(builder.Environment.ContentRootPath, "Settings"))
    });

var app = builder.Build();

// COMENTE ESTAS LINHAS ABAIXO temporariamente
// if (app.Environment.IsDevelopment())
// {
//     app.MapOpenApi();
// }

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
