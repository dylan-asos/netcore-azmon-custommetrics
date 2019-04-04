using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Willezone.Azure.WebJobs.Extensions.DependencyInjection;

namespace Asos.AzMon.CustomMetrics.Application
{
    internal class ServiceProviderBuilder : IServiceProviderBuilder
    {
        private readonly ILoggerFactory _loggerFactory;

        public ServiceProviderBuilder(ILoggerFactory loggerFactory) =>
            _loggerFactory = loggerFactory;

        public IServiceProvider Build() => new ServiceCollection()
            .ConfigureApplication(_loggerFactory)
            .BuildServiceProvider();
    }
}