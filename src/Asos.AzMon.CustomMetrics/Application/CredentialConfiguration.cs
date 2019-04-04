using Microsoft.Extensions.Configuration;

namespace Asos.AzMon.CustomMetrics.Application
{
    public class CredentialConfiguration
    {
        public string ClientId { get;  }
        public string Secret { get;  }
        public string TenantId { get;  }
        public string SubscriptionId { get;  }

        public CredentialConfiguration(IConfiguration configuration)
        {
            ClientId = configuration["AAD.ClientId"];
            Secret = configuration["AAD.Secret"];
            TenantId = configuration["AAD.Tenant"];
            SubscriptionId = configuration["Azure.SubscriptionId"];
        }
    }
}
