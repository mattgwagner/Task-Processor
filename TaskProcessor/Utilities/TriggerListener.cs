using Common.Logging;
using Quartz;
using Quartz.Listener;
using System;

namespace TaskProcessor.Utilities
{
    /// <summary>
    /// The purpose of this listener is to detect trigger events in Quartz and log them.
    /// This will help us debug issues with jobs not firing or not finishing.
    /// </summary>
    public class TriggerListener : TriggerListenerSupport
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public override string Name { get { return "LogTriggerListener"; } }

        private readonly IScheduler _scheduler;

        public TriggerListener(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public override void TriggerFired(ITrigger trigger, IJobExecutionContext context)
        {
            Log.DebugFormat("Trigger fired for job: {0} trigger key: {1}",
                context.JobDetail.JobType,
                trigger.Key.Name
            );
        }

        public override void TriggerMisfired(ITrigger trigger)
        {
            try
            {
                Log.WarnFormat("Trigger misfired for job type: {0} with misfire instruction {1}",
                    this._scheduler.GetJobDetail(trigger.JobKey).JobType,
                    trigger.MisfireInstruction);
            }
            catch (Exception exception)
            {
                // making sure all exceptions are handled in this method to prevent further issues with the scheduler.
                Log.Error("An exception occurred listening for a trigger misfire", exception);
            }
        }

        public override void TriggerComplete(ITrigger trigger, IJobExecutionContext context, SchedulerInstruction triggerInstructionCode)
        {
            Log.TraceFormat("Trigger complete for job {0} with scheduler instruction {1}",
                context.JobDetail.JobType,
                triggerInstructionCode
            );
        }
    }
}
