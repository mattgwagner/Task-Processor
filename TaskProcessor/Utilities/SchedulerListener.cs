using Common.Logging;
using Quartz;
using Quartz.Listener;

namespace TaskProcessor.Utilities
{
    public class SchedulerListener : SchedulerListenerSupport
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public override void SchedulerError(string msg, SchedulerException cause)
        {
            Log.ErrorFormat("An error occurred with the quartz scheduler with message: {0}.",
                cause,
                msg);
        }
    }
}