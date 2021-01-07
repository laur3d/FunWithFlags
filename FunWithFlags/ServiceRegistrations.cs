namespace FunWithFlags
{
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.FeatureManagement;

    public static class ServiceRegistrations
    {
        public static void ConfigureServices(IServiceCollection builderServices, IConfiguration configuration)
        {
            builderServices.AddFeatureFlags(configuration);
        }

        private static IServiceCollection AddFeatureFlags(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddSingleton<IConfiguration>(configuration).AddFeatureManagement();
            serviceCollection.AddAzureAppConfiguration();
            return serviceCollection;
        }
    }
}