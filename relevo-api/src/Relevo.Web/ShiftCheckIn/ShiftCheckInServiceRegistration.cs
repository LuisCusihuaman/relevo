using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Relevo.Web.ShiftCheckIn;

public static class ShiftCheckInServiceRegistration
{
  public static IServiceCollection AddShiftCheckInProvider(this IServiceCollection services, ConfigurationManager config)
  {
    // Check configuration value for UseOracleForSetup
    // This allows functional tests to override the default behavior
    var useOracleForSetup =
      config.GetValue("UseOracleForSetup", false) ||
      config.GetValue("UseOracleForSetupOverride", false);

    // In Testing environment, don't register anything - let the test factory handle it
    var isTesting = string.Equals(config.GetValue<string>("ASPNETCORE_ENVIRONMENT"), "Testing", StringComparison.OrdinalIgnoreCase);
    if (isTesting)
      return services;

    if (useOracleForSetup)
      services.AddSingleton<IShiftCheckInDataProvider, OracleShiftCheckInDataProvider>();
    else
      services.AddSingleton<IShiftCheckInDataProvider, ShiftCheckInDataStore>();

    return services;
  }
}
