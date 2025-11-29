using Relevo.Core.Interfaces;
using Relevo.Infrastructure;
using Relevo.Infrastructure.Email;
using Relevo.Web.ShiftCheckIn;

namespace Relevo.Web.Configurations;

public static class ServiceConfigs
{
  public static IServiceCollection AddServiceConfigs(this IServiceCollection services, ILogger logger, WebApplicationBuilder builder)
  {
    // Add authorization services (required for UseAuthorization middleware)
    services.AddAuthorization();

    services.AddInfrastructureServices(builder.Configuration, logger)
            .AddMediatrConfigs();


    if (builder.Environment.IsDevelopment())
    {
      // Use a local test email server
      // See: https://ardalis.com/configuring-a-local-test-email-server/
      services.AddScoped<IEmailSender, MimeKitEmailSender>();

      // Otherwise use this:
      //builder.Services.AddScoped<IEmailSender, FakeEmailSender>();

    }
    else
    {
      services.AddScoped<IEmailSender, MimeKitEmailSender>();
    }

    logger.LogInformation("{Project} services registered", "Mediatr and Email Sender");

    // Setup repository is now registered in InfrastructureServiceExtensions
    services.AddShiftCheckInProvider(builder.Configuration); // Moved to Infrastructure layer

    // Register expiration background job
    services.AddSingleton<Relevo.Infrastructure.BackgroundJobs.ExpireHandoversJob>();
    services.AddHostedService<Relevo.Infrastructure.BackgroundJobs.ExpireHandoversBackgroundService>();

    logger.LogInformation("Expiration background service registered");

    return services;
  }


}
