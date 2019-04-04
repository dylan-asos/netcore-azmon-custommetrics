using System;
using Newtonsoft.Json;

namespace Asos.AzMon.CustomMetrics.Model.MetricsApi
{
    public class MetricPayload
    {
        public MetricPayload()
        {
            Data = new Data();
        }

        [JsonProperty("time")]
        public DateTime Time { get; set; }

        [JsonProperty("data")]
        public Data Data { get; set; }
    }
}