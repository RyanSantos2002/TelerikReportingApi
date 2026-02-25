using Microsoft.Extensions.DependencyInjection.Extensions;
using Telerik.Reporting.Services;
using Telerik.Reporting.Services.AspNetCore; // Namespace necessário para o AddTelerikReporting
using Telerik.WebReportDesigner.Services;
using Microsoft.AspNetCore.Localization;
using System.Globalization;
using System.Diagnostics;

[assembly: global::Telerik.Licensing.EvidenceAttribute("eyJhbGciOiJSUzI1NiIsInR5cCI6IlRlbGVyaWsgTGljZW5zZSBFdmlkZW5jZSJ9.eyJjb2RlIjoiUkVQT1JUSU5HIiwidHlwZSI6InBlcnBldHVhbCIsImV4cGlyYXRpb24iOjE2NjYyOTgyMTcsInVzZXJJZCI6ImNkNzY1M2YyLTZhZTItNDM5MC05OWM2LTM3NWU2ZWNhNTZjYiIsImxpY2Vuc2VJZCI6ImI1ZjIzNDIyLWQwM2MtNGIwYi05MTE3LWEyNzhkNDkzNjk3MCJ9.GqoyIlXPQEim3z6x-0HyFyBu5laSdZ5UNgqkuYCC24UYZodZ67LVatuCoaSCVIVmaqPuD_TeAbnEz8kGc59iL4NCsQBgVewvuaWs-nbUPeRIjBlmkoXGxz81jzHsIBhvgchgdAMcaX-dhFQiRFtgXFOya5NNFgU5MoDVIiIiIA8TaUdlJZiZ0tPhchhO9yoyZGaQFODz4WFkQ-5JAFhoPsqViisOtu-holknGVLcjF2Kc_yHPq5m2a-ExeBTf-Q7JyiBu8tk64BvjYD8jfBuWFcMgXacGdzz9yykaVjLLVcTJ4FfaDAffsEpNsF9kKy6mBulQO6IGCkJxagrRj9MXg")]
[assembly: global::Telerik.Licensing.EvidenceAttribute("eyJhbGciOiJSUzI1NiIsInR5cCI6IlRlbGVyaWsgTGljZW5zZSBFdmlkZW5jZSJ9.eyJjb2RlIjoiUkVQT1JUSU5HIiwidHlwZSI6InRyaWFsIiwiZXhwaXJhdGlvbiI6MTc3MTcwNzAyOCwidXNlcklkIjoiY2Q3NjUzZjItNmFlMi00MzkwLTk5YzYtMzc1ZTZlY2E1NmNiIiwibGljZW5zZUlkIjoiYjVmMjM0MjItZDAzYy00YjBiLTkxMTctYTI3OGQ0OTM2OTcwIn0.2Cnc61ed8QosHOHJwr33AIJ6BpeRXyNHcQzSQ-A0bhoxXTilsh-lbq9USZdAx4AJ3rfQqc_y9EVn3BnDvEPvFJ84WkCkYC2HgWXO0fVDWeGhexbaJof1A9SiCSniJ9kUNBrRyAdPW_IVf96Vx6kvFJLE5-KQpScYK932TUpj8Gr9fMheZmgkcyObM33m7v_Ryox8QkUS_p20R_JnidzxW4PoEVghUJ1CVBXXEXR1tWmR-kOwbzpH9cON2QatRwcBrEKDUuXG75A3CDhMuM_Un2DEoZpqAFnGCJo96kY36pfXnB6OplwR3OK8utPgvcMX00xWVPuQggXv61SWgWpozA")]

// Ativa o log interno da Telerik para um arquivo na raiz do projeto
Trace.Listeners.Add(new TextWriterTraceListener(File.CreateText("telerik_log.txt")));
Trace.AutoFlush = true;

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