using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Asos.AzMon.CustomMetrics.Model;
using Asos.AzMon.CustomMetrics.Model.MetricsApi;
using Microsoft.Azure.Management.ServiceBus.Fluent;
using Newtonsoft.Json;

namespace Asos.AzMon.CustomMetrics.MetricsClient
{
    internal static class Extensions
    {
        private const string JsonMediaType = "application/json";

        public static StringContent ToStringContent(this object instance)
        {
            var jsonContent = JsonConvert.SerializeObject(instance);

            return new StringContent(jsonContent, Encoding.UTF8, JsonMediaType);
        }

        public static List<Series> ToSeries(this List<ServiceBusEntity> serviceBusEntityData)
        {
            var series = new List<Series>();

            foreach (var entity in serviceBusEntityData)
            {
                var capacityUsedPercent = entity.PercentageOfCapacityUsed;

                var seriesData = new Series();
                seriesData.DimensionValues.Add(entity.Name);
                seriesData.Count = 1;
                seriesData.Min = capacityUsedPercent;
                seriesData.Max = capacityUsedPercent;
                seriesData.Sum = capacityUsedPercent;

                series.Add(seriesData);                
            }

            return series;
        }

        public static async Task<List<ServiceBusEntity>> GetQueueSizeData(this IServiceBusNamespace serviceBusNamespace)
        {
            var results = new List<ServiceBusEntity>();
            foreach (var queue in await serviceBusNamespace.Queues.ListAsync())
            {                
                var entity = new ServiceBusEntity(serviceBusNamespace.Id, queue.CurrentSizeInBytes, queue.MaxSizeInMB, queue.Name);

                results.Add(entity);
            }

            return results;
        }
        
        public static async Task<List<ServiceBusEntity>> GetTopicSizeData(this IServiceBusNamespace serviceBusNamespace)
        {
            var results = new List<ServiceBusEntity>();
            foreach (var topic in await serviceBusNamespace.Topics.ListAsync())
            {
                var entity = new ServiceBusEntity(serviceBusNamespace.Id, topic.CurrentSizeInBytes, topic.MaxSizeInMB, topic.Name);
                
                results.Add(entity);
            }

            return results;
        }
    }
}