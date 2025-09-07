using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Relevo.Web.Setup;

public static class SetupServiceRegistration
{
  public static IServiceCollection AddSetupProvider(this IServiceCollection services, ConfigurationManager config)
  {
    var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";
    bool useOracleForSetup = environmentName == "Testing" ? false : config.GetValue("UseOracleForSetup", false);
    
    if (useOracleForSetup)
    {
      services.AddSingleton<ISetupDataProvider, OracleSetupDataProvider>();
    }
    else
    {
      services.AddSingleton<ISetupDataProvider, SetupDataStore>();
    }
    return services;
  }
}


