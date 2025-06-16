[assembly: FunctionsStartup(typeof(Startup))]
namespace FeatureFlags.Sample;
public class Startup : FunctionsStartup
{
    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        var context = builder.GetContext();
        builder.ConfigurationBuilder
               .SetBasePath(context.ApplicationRootPath)
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddEnvironmentVariables();
    }

    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddFeatureManagement()
                           .AddFeatureFilter<PercentageFilter>()
                           .AddFeatureFilter<TargetingFilter>();
    }
}
