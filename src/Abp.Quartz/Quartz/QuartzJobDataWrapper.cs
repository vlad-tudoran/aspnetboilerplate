using System;
using System.Linq;
using Abp.BackgroundJobs;
using Quartz;

namespace Abp.Quartz.Quartz
{
    public class QuartzJobDataWrapper
    {
        #region Private constants

        private const string AbpBackgroundJobType = "AbpJobType";
        private const string AbpBackgroundJobArgs = "AbpJobArgs";
        #endregion

        #region Public static methods

        /// <summary>
        /// Serializes the IBackgroundJob type and its argument in JobDataMap, to be passed as an aggregated argument for an IJob
        /// </summary>
        public static JobDataMap WrapJobData<TJob, TArgs>(TArgs args) where TJob : IBackgroundJob<TArgs>
        {
            var jobType = typeof(TJob);

            return new JobDataMap()
            {
                { AbpBackgroundJobType, jobType.AssemblyQualifiedName },
                { AbpBackgroundJobArgs, args }
            };
        }

        /// <summary>
        /// Serializes an IBackgroundJob&lt;string&gt; job and its argument, to be passed as an aggregated argument for an IJob.
        /// This convenience method can be used when the BackgroundJob doesn't actually require and argument, as long as the jobType implements IBackgroundJob&lt;string&gt;
        /// </summary>
        public static JobDataMap WrapJobType(Type jobType, string argument = null)
        {
            var backgroundJobIface = typeof(IBackgroundJob<>);
            if (!jobType.GetInterfaces()
                .Any(i => i.IsGenericType &&
                          i.GetGenericTypeDefinition() == backgroundJobIface &&
                          i.GetGenericArguments().SingleOrDefault() == typeof(string)))
            {
                throw new ArgumentException($"Job type does not implement ${backgroundJobIface.Name}.");
            }

            return new JobDataMap()
            {
                { AbpBackgroundJobType, jobType.AssemblyQualifiedName },
                { AbpBackgroundJobArgs, argument ?? string.Empty }
            };
        }

        public static void UnwrapJobData(JobDataMap jobDataMap, out Type jobType, out object jobArgs)
        {
            string jobTypeStr = jobDataMap[AbpBackgroundJobType] as string;
            jobArgs = jobDataMap[AbpBackgroundJobArgs];
            jobType = Type.GetType(jobTypeStr);
        }
        #endregion
    }
}
