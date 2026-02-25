using Microsoft.Extensions.DependencyInjection.Extensions;
using Telerik.Reporting.Services;
using Telerik.WebReportDesigner.Services;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURAÇÃO DE LOCALIZAÇÃO (Tradução) ---
// Isso define que o servidor deve processar recursos em PT-BR
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new[] { new CultureInfo("pt-BR") };
    options.DefaultRequestCulture = new RequestCulture("pt-BR");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddOpenApi();

// --- 2. CONFIGURANDO CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// --- 3. CONFIGURANDO TELERIK REPORTING SERVICES ---
string reportsPath = Path.Combine(builder.Environment.ContentRootPath, "Reports");

builder.Services.TryAddSingleton<IReportServiceConfiguration>(sp =>
    new ReportServiceConfiguration
    {
        ReportingEngineConfiguration = sp.GetService<IConfiguration>(),
        HostAppId = "TelerikReportingApi",
        Storage = new Telerik.Reporting.Cache.File.FileStorage(),
        ReportSourceResolver = new UriReportSourceResolver(reportsPath)
    });

// --- 4. CONFIGURANDO TELERIK WEB REPORT DESIGNER SERVICES ---
builder.Services.TryAddSingleton<IReportDesignerServiceConfiguration>(sp => 
    new ReportDesignerServiceConfiguration
    {
        DefinitionStorage = new FileDefinitionStorage(reportsPath),
        // Certifique-se de que a pasta 'Resources' contenha seus arquivos .resx
        ResourceStorage = new ResourceStorage(Path.Combine(reportsPath, "Resources")),
        SharedDataSourceStorage = new FileSharedDataSourceStorage(Path.Combine(reportsPath, "SharedDataSources")),
        SettingsStorage = new FileSettingsStorage(Path.Combine(builder.Environment.ContentRootPath, "Settings"))
    });

var app = builder.Build();

// --- 5. ATIVANDO MIDDLEWARES (A ordem importa!) ---

// Ativa a localização antes de qualquer rota ou controller
app.UseRequestLocalization();

app.UseCors("AllowAll");

// Permite servir arquivos padrão (como index.html) e arquivos estáticos da wwwroot
app.UseDefaultFiles();
app.UseStaticFiles();

// Opcional: Descomente se for usar Swagger/OpenApi em desenvolvimento
// if (app.Environment.IsDevelopment()) { app.MapOpenApi(); }

app.UseAuthorization();

app.MapControllers();

app.Run();

// Record padrão do template
record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}