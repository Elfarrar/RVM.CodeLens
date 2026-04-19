using RVM.CodeLens.Core;
using RVM.CodeLens.Core.Workspace;
using RVM.CodeLens.Web.Api;
using RVM.CodeLens.Web.Components;
using RVM.CodeLens.Web.Services;
using Serilog;
using Serilog.Formatting.Compact;

// Must be called before any Roslyn workspace types are loaded
MsBuildInitializer.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(new CompactJsonFormatter())
    .CreateBootstrapLogger();
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console(new CompactJsonFormatter());

    var seqUrl = context.Configuration["Seq:ServerUrl"];
    if (!string.IsNullOrEmpty(seqUrl))
        configuration.WriteTo.Seq(seqUrl);
});

builder.Services.AddCodeLensCore();
builder.Services.AddSingleton<IAnalysisStateService, AnalysisStateService>();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCors(options =>
{
    options.AddPolicy("ElectronApp", policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseCors("ElectronApp");
app.UseStaticFiles();
app.UseAntiforgery();

app.MapAnalysisEndpoints();
app.MapHealthChecks("/health");

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
