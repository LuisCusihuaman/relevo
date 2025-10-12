using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Relevo.Web.Setup;

public static class SetupServiceRegistration
{
  public static IServiceCollection AddSetupProvider(this IServiceCollection services, ConfigurationManager config)
  {
    // Check configuration value for UseOracleForSetup
    // This allows functional tests to override the default behavior
    bool useOracleForSetup = config.GetValue("UseOracleForSetup", false) || 
                             config.GetValue("UseOracleForSetupOverride", false) || 
                             config.GetValue("ASPNETCORE_ENVIRONMENT", "").Equals("Testing", StringComparison.OrdinalIgnoreCase);
    
    if (useOracleForSetup)    else
    {
      services.AddSingleton<ISetupDataProvider, SetupDataStore>();
    }
    return services;
  }
}


