using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace Asos.AzMon.CustomMetrics.Functions
{
    public class ServiceBusPercentageUsed
    {
        private const string EveryFiveMinutes = "* */5 * * * *";

        [FunctionName("ServiceBusPercentageUsed")]
        public static async Task Run(
            [TimerTrigger(EveryFiveMinutes)] TimerInfo input,
            [Inject] ServiceBusPercentageUsedMetricsGenerator metricsGenerator)
        {
            await metricsGenerator.CalculateMetrics();
        }
    }
}