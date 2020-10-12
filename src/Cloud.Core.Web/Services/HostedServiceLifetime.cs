using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cloud.Core.Web.Services
{
    /// <summary>
    /// Class HostedServiceLifetime will consume a collection of IHostedServices and manage starting/stopping the services.
    /// </summary>
    internal class HostedServiceLifetime
    {
        private readonly ILogger<HostedServiceLifetime> _logger;
        internal readonly IEnumerable<IHostedService> HostedServices;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        /// <summary>
        /// Initializes a new instance of the <see cref="HostedServiceLifetime"/> class.
        /// </summary>
        /// <param name="hostedServices">The hosted services to manage lifetime of.</param>
        /// <param name="logger">Logger to log out additional information.</param>
        public HostedServiceLifetime(IEnumerable<IHostedService> hostedServices, ILogger<HostedServiceLifetime> logger = null)
        {
            // Ensure the hosted service collection is defaulted if null.
            hostedServices ??= new List<IHostedService>();

            HostedServices = hostedServices;
            _logger = logger;

            // Attach to domain wide exceptions and exit events.
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) => _logger?.LogError(eventArgs.ExceptionObject as Exception, $"Unhandled exception captured: {eventArgs.ExceptionObject}");
            AppDomain.CurrentDomain.ProcessExit += async (sender, eventArgs) =>
            {
                _logger?.LogWarning("Application exit captured");
                _cancellation.Cancel();
                await StopServices();
            };
            Console.CancelKeyPress += async (sender, eventArgs) =>
            {
                _logger?.LogWarning("Application cancel keys triggered"); 
                eventArgs.Cancel = true;
                _cancellation.Cancel();
                await StopServices();
            };

            // Kick off the services.
            StartServices();
        }

        /// <summary>Starts each of the IHostedService's in the service collection.</summary>
        public void StartServices()
        {
            foreach (IHostedService service in HostedServices)
            {
                try
                {
                    Task.Run(() => service.StartAsync(_cancellation.Token));
                }
                catch (Exception e)
                {
                    _logger?.LogError($"An error occurred running IHostedService {service.GetType().FullName}", e);
                }
            }
        }

        /// <summary>Stops each of the IHostedService's in the service collection.</summary>
        public async Task StopServices()
        {
            if (!_cancellation.IsCancellationRequested)
                _cancellation.Cancel();

            foreach (IHostedService service in HostedServices)
            {
                try
                {
                    await service.StopAsync(_cancellation.Token);
                }
                catch (Exception e)
                {
                    _logger?.LogError($"An error occurred stopping IHostedService {service.GetType().FullName}", e);
                }
            }
        }
    }
}
