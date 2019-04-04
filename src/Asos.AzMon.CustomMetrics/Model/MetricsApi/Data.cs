using Newtonsoft.Json;

namespace Asos.AzMon.CustomMetrics.Model.MetricsApi
{
    public class Data
    {
        public Data()
        {
            MetricData = new CustomMetric();
        }

        [JsonProperty("baseData")]
        public CustomMetric MetricData { get; set; }
    }
}