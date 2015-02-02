using Common.Logging;
using Quartz;
using System;
using System.Net.Http;

namespace Google_DNS_Updater.Service
{
    public class UpdateJob : IJob
    {
        private readonly ILog Log = LogManager.GetLogger<UpdateJob>();

        public async void Execute(IJobExecutionContext context)
        {
            Log.InfoFormat("Running update for hostname {0}", Program.Hostname);

            String url = String.Format("https://{0}:{1}@domains.google.com/nic/update?hostname={2}",
                Program.Username,
                Program.Password,
                Program.Hostname);

            using (HttpClient client = new HttpClient())
            {
                await client.GetAsync(url);
            }

            Log.Info("Update completed.");
        }
    }
}