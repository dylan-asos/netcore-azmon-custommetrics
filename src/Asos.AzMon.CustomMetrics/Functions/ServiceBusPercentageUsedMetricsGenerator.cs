using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asos.AzMon.CustomMetrics.MetricsClient;
using Asos.AzMon.CustomMetrics.Model.MetricsApi;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Microsoft.Extensions.Logging;

namespace Asos.AzMon.CustomMetrics.Functions
{
    public class ServiceBusPercentageUsedMetricsGenerator
    {
        private readonly IAzure _azureManagementClient;
        private readonly AzureMonitorMetricsClient _metricsClient;
        private readonly ILogger _logger;

        public ServiceBusPercentageUsedMetricsGenerator(IAzure azureManagementClient, AzureMonitorMetricsClient metricsClient, ILogger logger)
        {
            _azureManagementClient = azureManagementClient;
            _metricsClient = metricsClient;
            _logger = logger;
        }

        public async Task CalculateMetrics()
        {
            var processingWork = new List<Task>();
            var namespaces = await _azureManagementClient.ServiceBusNamespaces.ListAsync();

            foreach (var serviceBusNamespace in namespaces)
            {
                var namespaceProcessor = ProcessServiceBusNamespace(serviceBusNamespace);

                processingWork.Add(namespaceProcessor);
            }

            await Task.WhenAll(processingWork);

            _logger.LogInformation("Done all namespaces, waiting for next timer trigger....");
        }

        private Task ProcessServiceBusNamespace(IServiceBusNamespace serviceBusNamespace)
        {
            return Task.Run(async () =>
            {
                _logger.LogInformation($"Processing namespace {serviceBusNamespace.Name} in region {serviceBusNamespace.Region.Name}");

                var payload = new MetricPayload {Time = DateTime.UtcNow};
                var metricData = payload.Data.MetricData;
                metricData.Metric = "CapacityUsedPercent";
                metricData.MetricNamespace = "ASOS Custom Metrics";
                metricData.DimensionNames.Add("EntityName");

                var entitySizes = await serviceBusNamespace.GetTopicSizeData();
                entitySizes.AddRange(await serviceBusNamespace.GetQueueSizeData());

                metricData.Series.AddRange(entitySizes.ToSeries());

                if (metricData.Series.Count > 0)
                {
                    await _metricsClient.CreateMetric(serviceBusNamespace.Region.Name, serviceBusNamespace.Id, payload);
                }
            });
        }
    }
}