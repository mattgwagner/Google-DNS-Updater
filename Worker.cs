using System;
using System.Net.Http;
using System.Text;
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
        private string Hostname => configuration.GetValue<string>(nameof(Hostname));

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            this._logger = logger;
            this.configuration = configuration;

            http.DefaultRequestHeaders.Add("User-Agent", "Google-DNS-Updater");

            var bytes = Encoding.ASCII.GetBytes($"{configuration.GetValue<string>("User")}:{configuration.GetValue<string>("Password")}");

            http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(bytes));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Starting to Update {hostname}", Hostname);

                try
                {              
                    var json = await http.GetAsync($"https://domains.google.com/nic/update?hostname={Hostname}");

                    _logger.LogInformation("Updated Google DNS record: {json}", json);                    
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error updating record!");
                }

                await Task.Delay((int)TimeSpan.FromHours(4).TotalSeconds, stoppingToken);
            }
        }
    }
}
