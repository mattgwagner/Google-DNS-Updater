using Common.Logging;
using Quartz;
using System;
using System.IO;
using System.Net;

namespace Google_DNS_Updater.Service
{
    public class UpdateJob : IJob
    {
        private readonly ILog Log = LogManager.GetLogger<UpdateJob>();

        public void Execute(IJobExecutionContext context)
        {
            try
            {
                Log.InfoFormat("Running update for hostname {0}", Program.Hostname);

                var uri = new UriBuilder
                {
                    Scheme = "https",
                    Host = "domains.google.com",
                    Path = "nic/update",
                    Query = "hostname=" + Program.Hostname
                };

                var request = (HttpWebRequest)HttpWebRequest.Create(uri.Uri);

                request.Credentials = new NetworkCredential(Program.Username, Program.Password);

                request.UserAgent = "DnsUpdateClient";

                WebResponse response = request.GetResponse();

                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    Log.InfoFormat("Update completed, {0}", reader.ReadToEnd());
                }
            }
            catch (Exception ex)
            {
                Log.ErrorFormat("Error updating record", ex);
            }
        }
    }
}