using Common.Logging;
using Quartz;
using Quartz.Spi;
using System;

namespace TaskProcessor.Utilities
{
    public class JobFactory : IJobFactory
    {
        private static readonly ILog Log = LogManager.GetCurrentClassLogger();

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            // Configure your IoC container to return a job instance based on jobType

            var jobDetail = bundle.JobDetail;

            Type jobType = jobDetail.JobType;

            Log.TraceFormat("Getting instance of job type {0}", jobType);

            throw new NotImplementedException();
        }

        public void ReturnJob(IJob job)
        {
            // Not needed.
        }
    }
}