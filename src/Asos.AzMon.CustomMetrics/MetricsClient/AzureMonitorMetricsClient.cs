using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Asos.AzMon.CustomMetrics.Application;
using Asos.AzMon.CustomMetrics.Model.MetricsApi;

namespace Asos.AzMon.CustomMetrics.MetricsClient
{
    public class AzureMonitorMetricsClient
    {
        private const string AzureMonitorRegionalEndpoint = "https://{0}.monitoring.azure.com";
        private readonly HttpClient _client;
        private readonly MetricsApiAccessTokenProvider _accessTokenProvider;

        public AzureMonitorMetricsClient(CredentialConfiguration credentialConfiguration, IHttpClientFactory httpClientFactory)
        {
            _client = httpClientFactory.CreateClient();
            _accessTokenProvider = new MetricsApiAccessTokenProvider(credentialConfiguration);
        }

        public async Task CreateMetric(string region, string resourceId, MetricPayload detail)
        {
            var payload = detail.ToStringContent();
            var regionIngressEndpoint = string.Format(AzureMonitorRegionalEndpoint, region);

            var request = new HttpRequestMessage
                {RequestUri = new Uri(string.Concat(regionIngressEndpoint, resourceId, "/metrics")), Content = payload, Method = HttpMethod.Post};
           
            var token = await _accessTokenProvider.GetAccessToken();
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            await _client.SendAsync(request);
        }
    }
}