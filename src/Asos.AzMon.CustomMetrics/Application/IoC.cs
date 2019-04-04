using Asos.AzMon.CustomMetrics.MetricsClient;
using Microsoft.Azure.WebJobs.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using Asos.AzMon.CustomMetrics.Functions;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;

namespace Asos.AzMon.CustomMetrics.Application
{
    internal static class IoC
    {
        public static IServiceCollection ConfigureApplication(this IServiceCollection services, ILoggerFactory loggerFactory, IConfiguration config = null) => services
            .AddSingleton(config ?? LoadApplicationConfig())
            .AddCredentialConfiguration()
            .AddHttpClient()
            .AddMetricsGenerator()
            .AddMetricsClient()
            .AddAzureManagementClient()
            .AddLogging(loggerFactory);

        private static IServiceCollection AddLogging(this IServiceCollection services, ILoggerFactory loggerFactory) => services
            .AddSingleton(_ => loggerFactory.CreateLogger(LogCategories.CreateFunctionUserCategory("Common")));

        private static IServiceCollection AddCredentialConfiguration(this IServiceCollection services) => services
            .AddSingleton<CredentialConfiguration>();

        private static IServiceCollection AddMetricsGenerator(this IServiceCollection services) => services
            .AddTransient<ServiceBusPercentageUsedMetricsGenerator>();

        private static IServiceCollection AddMetricsClient(this IServiceCollection services) => services
            .AddTransient<AzureMonitorMetricsClient>();

        private static IServiceCollection AddAzureManagementClient(this IServiceCollection services)
        {
            return services.AddSingleton(sp =>
            {
                var configuration = sp.GetService<CredentialConfiguration>();

                var credentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(
                    configuration.ClientId, configuration.Secret,
                    configuration.TenantId, AzureEnvironment.AzureGlobalCloud);

                return Azure
                    .Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithSubscription(configuration.SubscriptionId);
            });
        }

        private static IConfiguration LoadApplicationConfig()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Environment.CurrentDirectory)
                .AddJsonFile("local.settings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            return configuration;
        }
    }
}