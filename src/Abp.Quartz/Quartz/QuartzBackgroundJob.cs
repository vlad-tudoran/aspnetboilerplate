using System;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Castle.Core.Logging;
using Quartz;

namespace Abp.Quartz.Quartz
{
    public class QuartzBackgroundJob: JobBase, ITransientDependency
    {
        #region Properties

        public IIocResolver Resolver { get; set; } 
        #endregion

        #region Constructor

        public QuartzBackgroundJob()
        {
            Logger = NullLogger.Instance;
        }
        #endregion

        #region IJob interface

        public override void Execute(IJobExecutionContext context)
        {
            object backgroundJobObj = null;

            try
            {
                Type backgroundJobType;
                object backgroundJobArgs;

                QuartzJobDataWrapper.UnwrapJobData(context.JobDetail.JobDataMap, out backgroundJobType, out backgroundJobArgs);

                backgroundJobObj = Resolver.Resolve(backgroundJobType);
                var executeMethod = backgroundJobType.GetMethod(nameof(BackgroundJob<object>.Execute));

                executeMethod.Invoke(backgroundJobObj, new [] { backgroundJobArgs });
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            finally
            {
                Resolver.Release(backgroundJobObj);
            }
        }
        #endregion
    }
}
