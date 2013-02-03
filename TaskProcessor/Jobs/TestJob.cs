using Common.Logging;
using Quartz;

namespace TaskProcessor.Jobs
{
    public class TestJob : IJob
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public void Execute(IJobExecutionContext context)
        {
            Log.Info("TestJob is executing...");
        }
    }
}