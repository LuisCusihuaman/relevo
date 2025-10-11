using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Relevo.Web.Setup;

public static class SetupServiceRegistration
{
  public static IServiceCollection AddSetupProvider(this IServiceCollection services, ConfigurationManager config)
  {
    // Check configuration value for UseOracleForSetup
    // This allows functional tests to override the default behavior
    bool useOracleForSetup = config.GetValue("UseOracleForSetup", false);
    
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


