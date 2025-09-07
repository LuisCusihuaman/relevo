using Relevo.Web.Configurations;
using Relevo.Web.Middleware;
using FastEndpoints;
using FastEndpoints.Swagger;
using Serilog;
using Serilog.Extensions.Logging;


var builder = WebApplication.CreateBuilder(args);

var logger = Log.Logger = new LoggerConfiguration()
  .Enrich.FromLogContext()
  .WriteTo.Console()
  .CreateLogger();

logger.Information("Starting web host");

builder.AddLoggerConfigs();

var appLogger = new SerilogLoggerFactory(logger)
    .CreateLogger<Program>();

builder.Services.AddOptionConfigs(builder.Configuration, appLogger, builder);
builder.Services.AddServiceConfigs(appLogger, builder);

// CORS: allow Vite dev server to call API (frontend runs on :5174)
const string CorsPolicyName = "FrontendDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: CorsPolicyName, policy =>
        policy.WithOrigins(
                "http://localhost:5174",
                "https://localhost:5174"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

builder.Services.AddFastEndpoints()
                .SwaggerDocument(o =>
                {
                  o.ShortSchemaNames = true;
                });

var app = builder.Build();

app.UseCors(CorsPolicyName);

// Add routing middleware first
app.UseRouting();

// Add authentication middleware
app.UseClerkAuthentication();

// Add authorization middleware (required for FastEndpoints)
app.UseAuthorization();

// Configure endpoints (this must come after authorization)
app.UseEndpoints(endpoints => { });

await app.UseAppMiddleware();

app.Run();

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program { }
