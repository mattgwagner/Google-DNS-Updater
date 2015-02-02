using Common.Logging;
using Quartz;
using Quartz.Impl;
using System;
using System.Configuration;
using Topshelf;

namespace Google_DNS_Updater.Service
{
    internal class Program
    {
        public static String Hostname { get { return ConfigurationManager.AppSettings["Hostname"]; } }

        public static String Username { get { return ConfigurationManager.AppSettings["Username"]; } }

        public static String Password { get { return ConfigurationManager.AppSettings["Password"]; } }

        private static int UpdateIntervalInMinutes { get { return int.Parse(ConfigurationManager.AppSettings["UpdateIntervalInMinutes"] ?? "720"); } }

        private static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<TaskService>();

                // x.SetDescription("Google Dynamic DNS Updater");
                x.SetDisplayName("Google-Dynamic-DNS-Updater");
                x.SetServiceName("Google-Dynamic-DNS-Updater");

                x.RunAsLocalSystem();
                x.StartAutomatically();
                x.EnableServiceRecovery(s => s.RestartService(1)); // restart after 1 minute
            });
        }

        private class TaskService : ServiceControl
        {
            private static readonly ILog Log = LogManager.GetLogger<TaskService>();

            private readonly IScheduler scheduler;

            public TaskService()
            {
                this.scheduler = new StdSchedulerFactory().GetScheduler();
            }

            public bool Start(HostControl hostControl)
            {
                Log.Info("Starting Google Dynamic DNS Updater Servcice");

                IJobDetail job =
                    JobBuilder
                    .Create()
                    .OfType<UpdateJob>()
                    .Build();

                ITrigger trigger =
                    TriggerBuilder
                    .Create()
                    .ForJob(job)
                    .StartNow()
                    .WithSimpleSchedule(x =>
                        x
                        .WithIntervalInMinutes(Program.UpdateIntervalInMinutes)
                        .RepeatForever()
                    )
                    .Build();

                this.scheduler.ScheduleJob(job, trigger);

                this.scheduler.Start();

                return true;
            }

            public bool Stop(HostControl hostControl)
            {
                Log.Info("Stopping Google DNS Updater Service");

                this.scheduler.Shutdown(waitForJobsToComplete: false);

                return true;
            }
        }
    }
}