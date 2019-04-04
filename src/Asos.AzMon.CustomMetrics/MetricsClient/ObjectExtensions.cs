using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace Asos.AzMon.CustomMetrics.MetricsClient
{
    internal static class ObjectExtensions
    {
        private const string JsonMediaType = "application/json";

        public static StringContent ToStringContent(this object instance)
        {
            var jsonContent = JsonConvert.SerializeObject(instance);

            return new StringContent(jsonContent, Encoding.UTF8, JsonMediaType);
        }
    }
}