using Common.Logging;
using Quartz;
using Quartz.Impl;
using Topshelf;

namespace TaskProcessor
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.Service<TaskService>();

                x.SetDisplayName("Task-Processor");
                x.SetServiceName("Task-Processor");

                x.RunAsLocalSystem();
                x.StartAutomatically();
                x.EnableServiceRecovery(s => s.RestartService(1)); // restart after 1 minute
            });
        }

        private class TaskService : ServiceControl
        {
            private static readonly ILog Log = LogManager.GetCurrentClassLogger();

            private readonly IScheduler scheduler;

            public TaskService()
            {
                this.scheduler = new StdSchedulerFactory().GetScheduler();

                // this.scheduler.JobFactory = new NinjectJobFactory(new IOCModule());
            }

            public bool Start(HostControl hostControl)
            {
                Log.Info("Starting Service...");

                var Config = Configuration.LoadFromFile("Jobs.xml");

                foreach (var config in Config.Jobs)
                {
                    if (!config.Enabled) continue;

                    var job = config.GenerateJob();

                    var trigger = TriggerBuilder.Create()
                        .ForJob(job)
                        .WithCronSchedule(config.CronTrigger)
                        .StartNow()
                        .Build();

                    this.scheduler.ScheduleJob(job, trigger);
                }

                return true;
            }

            public bool Stop(HostControl hostControl)
            {
                Log.Info("Stopping Service...");

                this.scheduler.Standby();

                return true;
            }
        }
    }
}