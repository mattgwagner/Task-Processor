using Common.Logging;
using Quartz;
using Quartz.Impl.Matchers;
using Quartz.Impl.Triggers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TaskProcessor.Jobs
{
    /// <summary>
    /// This job acts as a dynamic scheduling agent by polling a data source for configured triggers and adding
    /// them to the running scheduler, removing old ones
    /// </summary>
    [DisallowConcurrentExecution]
    public class JobScheduler : IJob
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// This is a list of job types that we dynamically schedule
        /// </summary>
        private static readonly IEnumerable<Type> DynamicJobs = new[] { typeof(TestJob) };

        /// <summary>
        /// The amount of time the scheduler should wait before trying to start jobs after picking them up from the database.
        /// I'm not terribly fond of this, but it's a band-aide for race conditions where jobs that are already queued/running
        /// were getting re-run when re-added to the scheduler.
        /// </summary>
        private static readonly TimeSpan STARTUP_DELAY = TimeSpan.FromSeconds(1);

        /// <summary>
        /// The group name to use when adding dynamically scheduled jobs and triggers to the running scheduler
        /// </summary>
        public const String GroupName = "DynamicJobs";

        public new void Execute(IJobExecutionContext context)
        {
            Log.Info("Starting JobScheduler job...");

            foreach (var type in DynamicJobs)
            {
                try
                {
                    // Build up a job based on the integration job class and then go to the database
                    // for scheduled triggers on which to run the job
                    var job = JobBuilder.Create()
                        .OfType(type)
                        .WithIdentity(type.FullName, JobScheduler.GroupName)
                        .Build();

                    var schedules = GetConfiguredSchedules(type);

                    var triggers = GetTriggers(schedules);

                    // Remove the old job info from the scheduler before re-adding
                    context.Scheduler.DeleteJob(JobKey.Create(type.FullName, JobScheduler.GroupName));

                    context.Scheduler.ScheduleJob(job, triggers, true);
                }
                catch (Exception ex)
                {
                    Log.FatalFormat("Error scheduling an integration job for type {0}", ex, type.FullName);
                }
            }

            // After we've re-scheduled the jobs, print out a debug list of the triggers
            LogActiveTriggers(context.Scheduler);

            Log.Info("Finished loading job triggers!");
        }

        public IEnumerable<Job> GetConfiguredSchedules(Type jobType)
        {
            // TODO Load the dynamic schedules from some source, i.e. database, web service, XML file, etc.

            throw new NotImplementedException();
        }

        public virtual Quartz.Collection.ISet<ITrigger> GetTriggers(IEnumerable<Job> schedules)
        {
            // We need to convert the schedules we have stored into a set of Quartz.net triggers
            // which will allow us to just re-schedule all triggers of one job at the same time.

            var jobs = from s in schedules
                       select TriggerBuilder.Create()

                           // Schedule in the correct time zone
                           // Tell Quartz to try to run the job immediately upon discovering it "misfired", which means all worker threads were taken at the time
                           .WithSchedule(CronScheduleBuilder.CronSchedule(s.CronTrigger).InTimeZone(s.TimeZone).WithMisfireHandlingInstructionFireAndProceed())

                           // Not really required, but might be useful for debugging output
                           .WithDescription(s.Comments)

                           // Give the scheduler time to finish before trying to start jobs, this was causing a race condition
                           // and duplicate job launches
                           .StartAt(DateTime.UtcNow.Add(STARTUP_DELAY))

                           // Put it all together
                           .Build();

            return new Quartz.Collection.HashSet<ITrigger>(jobs);
        }

        public virtual void LogActiveTriggers(IScheduler scheduler)
        {
            foreach (var triggerKey in scheduler.GetTriggerKeys(GroupMatcher<TriggerKey>.GroupEquals(JobScheduler.GroupName)))
            {
                var trigger = (CronTriggerImpl)scheduler.GetTrigger(triggerKey);

                Log.DebugFormat("Trigger for {3}-{0}, Cron {1}, Next Fire {2}", trigger.JobKey.Name, trigger.CronExpressionString, trigger.GetNextFireTimeUtc(), trigger.JobKey.Group);
            }
        }
    }

    /// <summary>
    /// Represents a scheduled integration job in the system.
    /// </summary>
    public class Job
    {
        /// <summary>
        /// Unique identifier for the schedule
        /// </summary>
        public Guid ID { get; set; }

        /// <summary>
        /// The fully qualified (with namespace) job class name
        /// </summary>
        public String Class { get; set; }

        /// <summary>
        /// Whether or not the job is enabled
        /// </summary>
        public Boolean Enabled { get; set; }

        /// <summary>
        /// The schedule for the job to run (seconds minutes hours day-of-month month day-of-week)
        /// </summary>
        /// <remarks>
        /// See http://quartznet.sourceforge.net/tutorial/lesson_6.html for cron trigger explanation
        /// </remarks>
        public String CronTrigger { get; set; }

        /// <summary>
        /// The time zone identifier for the cron trigger
        /// </summary>
        public TimeZoneInfo TimeZone { get; set; }

        /// <summary>
        /// Comments about the particular job, i.e. what it's doing or why
        /// </summary>
        public String Comments { get; set; }

        public Job()
        {
            // Defaults to UTC time
            this.TimeZone = TimeZoneInfo.Utc;
        }
    }
}