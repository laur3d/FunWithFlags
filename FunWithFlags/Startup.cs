using FunWithFlags;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using System;
using Microsoft.Extensions.Configuration;

[assembly: FunctionsStartup(typeof(Startup))]
namespace FunWithFlags
{


        public class Startup : FunctionsStartup
        {
            private IConfiguration configuration;
            public override void Configure(IFunctionsHostBuilder builder)
            {

                this.configuration = new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .AddAzureAppConfiguration(options =>
                    {
                        options.Connect(Environment.GetEnvironmentVariable("ConnectionString"))
                            .UseFeatureFlags();
                    }).Build();

                ServiceRegistrations.ConfigureServices(builder.Services, this.configuration);
            }
        }
}
