using Relevo.Web.Configurations;
using FastEndpoints;
using FastEndpoints.Swagger;
using Serilog;
using Serilog.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Relevo.Core.Interfaces;
using Relevo.Web.Auth;

try { OracleConfiguration.BindByName = true; } catch { }

// Load .env file if it exists (search upwards)
DotNetEnv.Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

// Oracle Configuration: BindByName is now handled in DapperConnectionFactory static constructor

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

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();
builder.Services.AddClerkAuthentication(builder.Configuration);

builder.Services.AddFastEndpoints()
                .SwaggerDocument(o =>
                {
                  o.ShortSchemaNames = true;
                });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

await app.UseAppMiddleware();

app.Run();

// Make the implicit Program.cs class public, so integration tests can reference the correct assembly for host building
public partial class Program { }
