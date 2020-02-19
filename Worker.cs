using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Google_DNS_Updater
{
    public class Worker : BackgroundService
    {
        private static readonly HttpClient http = new HttpClient{};

        private readonly ILogger<Worker> _logger;

        private readonly IConfiguration configuration;

        private string Username => configuration.GetValue<string>(nameof(Username));

        private string Password => configuration.GetValue<string>(nameof(Password));

        private string Hostname => configuration.GetValue<string>(nameof(Hostname));

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            this._logger = logger;
            this.configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting!");

                var json = await http.GetStringAsync($"https://{Username}:{Password}@domains.google.com/nic/update?hostname={Hostname}");

                _logger.LogInformation("Updated Google DNS record: {json}", json);

                await Task.Delay((int)TimeSpan.FromHours(4).TotalSeconds, stoppingToken);
            }
        }
    }
}
