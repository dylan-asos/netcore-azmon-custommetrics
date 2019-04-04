using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Asos.AzMon.CustomMetrics.MetricsClient;
using Asos.AzMon.CustomMetrics.Model;
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
                var resourceId = serviceBusNamespace.Id;
                var regionName = serviceBusNamespace.Region.Name;

                _logger.LogInformation($"Processing namespace {serviceBusNamespace.Name} in region {regionName}");

                var payload = new MetricPayload {Time = DateTime.UtcNow};
                var metricData = payload.Data.MetricData;
                metricData.Metric = "CapacityUsedPercent";
                metricData.MetricNamespace = "ASOS Custom Metrics";
                metricData.DimensionNames.Add("EntityName");

                await ProcessTopics(serviceBusNamespace, resourceId, metricData);

                await ProcessQueues(serviceBusNamespace, resourceId, metricData);

                if (metricData.Series.Count > 0)
                {
                    await _metricsClient.CreateMetric(regionName, resourceId, payload);
                }
            });
        }

        private async Task ProcessQueues(IServiceBusNamespace serviceBusNamespace, string resourceId,CustomMetric data)
        {
            foreach (var queue in await serviceBusNamespace.Queues.ListAsync())
            {
                _logger.LogInformation($"Processing queue {queue.Name}");
                
                var entity = CreateEntity(resourceId, queue.CurrentSizeInBytes, queue.MaxSizeInMB, queue.Name);

                data.Series.Add(CreateMetricSeries(entity));
            }
        }

        private async Task ProcessTopics( IServiceBusNamespace serviceBusNamespace, string resourceId,
            CustomMetric data)
        {
            foreach (var topic in await serviceBusNamespace.Topics.ListAsync())
            {
                _logger.LogInformation($"Processing topic {topic.Name}");

                var entity = CreateEntity(resourceId, topic.CurrentSizeInBytes, topic.MaxSizeInMB, topic.Name);
                
                data.Series.Add(CreateMetricSeries(entity));
            }
        }
        
        private static ServiceBusEntity CreateEntity(string namespaceResourceId, long currentSizeInBytes, long maxSizeInMb, string name)
        {
            return  new ServiceBusEntity()
            {
                NamespaceResourceId = namespaceResourceId, CurrentSizeInBytes = currentSizeInBytes, MaxSizeInMb = maxSizeInMb, Name = name
            };
        }

        private decimal CalculateCapacityPercentageUsed(ServiceBusEntity entity, string resourceId)
        {
            const decimal megaBytesInBytes = 1048576;

            var topicMaxSizeInBytes = entity.MaxSizeInMb * megaBytesInBytes;

            var capcityUsedPercent = entity.CurrentSizeInBytes > topicMaxSizeInBytes
                ? 100
                : decimal.Round(entity.CurrentSizeInBytes / topicMaxSizeInBytes * 100, 5);
           
            if (capcityUsedPercent > 1)
            {
                _logger.LogWarning($"entity named {entity.Name} in resource {resourceId} is {capcityUsedPercent}% of capacity");
            }

            return capcityUsedPercent;
        }

        private Series CreateMetricSeries(ServiceBusEntity entity)
        {
            var capacityUsedPercent = CalculateCapacityPercentageUsed(entity, entity.NamespaceResourceId);

            var seriesData = new Series();
            seriesData.DimensionValues.Add(entity.Name);
            seriesData.Count = 1;
            seriesData.Min = capacityUsedPercent;
            seriesData.Max = capacityUsedPercent;
            seriesData.Sum = capacityUsedPercent;
            return seriesData;
        }
    }
}