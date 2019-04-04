using System.Threading.Tasks;
using Asos.AzMon.CustomMetrics.Application;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace Asos.AzMon.CustomMetrics.MetricsClient
{
    public class MetricsApiAccessTokenProvider
    {
        private readonly CredentialConfiguration _settings;

        public MetricsApiAccessTokenProvider(CredentialConfiguration settings)
        {
            _settings = settings;
        }

        public async Task<string> GetAccessToken()
        {
            var clientCredentials = new ClientCredential(_settings.ClientId, _settings.Secret);

            var context = new AuthenticationContext($"https://login.microsoftonline.com/{_settings.TenantId}");

            var result = await context.AcquireTokenAsync("https://monitoring.azure.com/", clientCredentials);

            return result.AccessToken;
        }
    }
}