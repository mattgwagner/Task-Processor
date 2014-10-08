using Common.Logging;
using Quartz;
using Quartz.Listener;

namespace TaskProcessor.Utilities
{
    public class JobListener : JobListenerSupport
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public override string Name { get { return "LogJobListener"; } }

        public override void JobWasExecuted(IJobExecutionContext context, JobExecutionException jobException)
        {
            // for now if nothing has gone wrong we won't log anything
            if (jobException == null) return;

            Log.ErrorFormat("An exception occurred running job {0} (Refire Immediately? {1} Unschedule trigger? {2} Unschedule all triggers? {3})",
                jobException,
                context.JobDetail.JobType,
                jobException.RefireImmediately,
                jobException.UnscheduleFiringTrigger,
                jobException.UnscheduleAllTriggers
            );
        }
    }
}