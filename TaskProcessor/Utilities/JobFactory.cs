using Quartz;
using Quartz.Spi;
using System;

namespace TaskProcessor.Utilities
{
    public class JobFactory : IJobFactory
    {
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            // Configure your IoC container to return a job instance based on jobType

            var jobDetail = bundle.JobDetail;

            Type jobType = jobDetail.JobType;

            throw new NotImplementedException();
        }

        public void ReturnJob(IJob job)
        {
            // Not needed.
        }
    }
}