using Common.Logging;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Specialized;
using TaskProcessor.Utilities;
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
                x.EnableServiceRecovery(s => s.RestartService(delayInMinutes: 1)); // restart after 1 minute
            });
        }

        private class TaskService : ServiceControl
        {
            private static readonly ILog Log = LogManager.GetCurrentClassLogger();

            private readonly IScheduler scheduler;

            public TaskService()
            {
                var properties = new NameValueCollection();

                // add any customizations to the scheduler here
                // properties["quartz.threadPool.threadCount"] = 10;

                this.scheduler = new StdSchedulerFactory(properties).GetScheduler();

                // Configure JobFactory to use your IoC container for job instances
                // this.scheduler.JobFactory = new JobFactory();
            }

            public bool Start(HostControl hostControl)
            {
                Log.Info("Starting Service...");

                var Config = Configuration.LoadFromFile("Jobs.xml");

                foreach (var config in Config.Jobs)
                {
                    if (!config.Enabled)
                    {
                        Log.DebugFormat("Job {0} is disabled and is not being added to the scheduler.", config.Class);
                        continue;
                    }

                    var job = JobBuilder.Create()
                        .OfType(Type.GetType(config.Class))
                        .WithDescription(config.Comments)
                        .Build();

                    var cron = CronScheduleBuilder
                        .CronSchedule(config.CronTrigger)
                        // Might want to make the timezone EST, UTC, et al.
                        // .InTimeZone(TimeZone.CurrentTimeZone)
                        // Might want to change the misfire instructions per-job?
                        .WithMisfireHandlingInstructionFireAndProceed();

                    var trigger = TriggerBuilder.Create()
                        .ForJob(job)
                        .WithSchedule(cron)
                        .StartNow()
                        .Build();

                    this.scheduler.ScheduleJob(job, trigger);

                    Log.InfoFormat("Scheduled Job {0}. Next fire at {1} UTC.", config.Class, trigger.GetNextFireTimeUtc());
                }

                // add listeners
                this.scheduler.ListenerManager.AddTriggerListener(new TriggerListener(this.scheduler));
                this.scheduler.ListenerManager.AddSchedulerListener(new SchedulerListener());
                this.scheduler.ListenerManager.AddJobListener(new JobListener());

                this.scheduler.Start();

                return this.scheduler.IsStarted;
            }

            public bool Stop(HostControl hostControl)
            {
                Log.Info("Stopping Service...");

                // wait for any running jobs to complete. this may not be the desired behavior if you have long running jobs!
                this.scheduler.Shutdown(waitForJobsToComplete: true);

                return this.scheduler.IsShutdown;
            }
        }
    }
}