using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Quartz.Quartz.Configuration;
using Abp.Threading.BackgroundWorkers;
using Abp.Timing;
using Abp.UI;
using Quartz;

namespace Abp.Quartz.Quartz
{
    public class QuartzScheduleJobManager : BackgroundWorkerBase, IBackgroundJobManager, IQuartzScheduleJobManager
    {
        #region Constants

        private const string BackgroundJobsGroup = "BACKGROUND_JOB";

        private const int QuartzLowPriority = 3;
        private const int QuartzBelowNormalPriority = 4;
        private const int QuartzNormalPriority = 5;
        private const int QuartzAboveNormalPriority = 6;
        private const int QuartzHighPriority = 7;

        private readonly Dictionary<BackgroundJobPriority, int> PriorityMap = new Dictionary<BackgroundJobPriority, int>()
        {
            { BackgroundJobPriority.Low,         QuartzLowPriority },
            { BackgroundJobPriority.BelowNormal, QuartzBelowNormalPriority },
            { BackgroundJobPriority.Normal,      QuartzNormalPriority },
            { BackgroundJobPriority.AboveNormal, QuartzAboveNormalPriority },
            { BackgroundJobPriority.High,        QuartzHighPriority }
        };
        #endregion

        #region Private fields

        private readonly IBackgroundJobConfiguration _backgroundJobConfiguration;
        private readonly IAbpQuartzConfiguration _quartzConfiguration;
        #endregion

        #region Constructor

        public QuartzScheduleJobManager(IAbpQuartzConfiguration quartzConfiguration,
                                        IBackgroundJobConfiguration backgroundJobConfiguration)
        {
            _quartzConfiguration = quartzConfiguration;
            _backgroundJobConfiguration = backgroundJobConfiguration;
        }
        #endregion

        #region IQuartzScheduleJobManager interface

        public Task ScheduleAsync<TJob>(Action<JobBuilder> configureJob, Action<TriggerBuilder> configureTrigger)
            where TJob : IJob
        {
            var jobToBuild = JobBuilder.Create<TJob>();
            configureJob(jobToBuild);
            var job = jobToBuild.Build();

            var triggerToBuild = TriggerBuilder.Create();
            configureTrigger(triggerToBuild);
            var trigger = triggerToBuild.Build();

            _quartzConfiguration.Scheduler.ScheduleJob(job, trigger);

            return Task.FromResult(0);
        }
        #endregion

        #region Runnable interface

        public override void Start()
        {
            base.Start();

            if (_backgroundJobConfiguration.IsJobExecutionEnabled)
            {
                _quartzConfiguration.Scheduler.Start();
            }

            Logger.Info("Started QuartzScheduleJobManager");
        }

        public override void WaitToStop()
        {
            if (_quartzConfiguration.Scheduler != null)
            {
                try
                {
                    _quartzConfiguration.Scheduler.Standby();
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex.ToString(), ex);
                }
            }

            base.WaitToStop();

            Logger.Info("Stopped QuartzScheduleJobManager");
        }
        #endregion

        #region IBackgroundManager interface

        public virtual Task EnqueueAsync<TJob, TArgs>(TArgs args,
                                                      BackgroundJobPriority priority = BackgroundJobPriority.Normal,
                                                      TimeSpan? delay = null) where TJob : IBackgroundJob<TArgs>
        {
            var typeName = typeof(TJob).Name;
            var startAt = Clock.Now.Add(delay ?? TimeSpan.Zero);
            var description = $"{typeName} scheduled at {startAt:G}";

            var trigger = TriggerBuilder.Create()
                                        .StartAt(startAt)
                                        .WithPriority(PriorityMap[priority])
                                        .WithIdentity(BackgroundJobsGroup, Guid.NewGuid().ToString())
                                        .WithDescription(description)
                                        .Build();
            var parameters = QuartzJobDataWrapper.WrapJobData<TJob, TArgs>(args);
            var job = JobBuilder.Create<QuartzBackgroundJob>()
                                .WithIdentity(BackgroundJobsGroup, Guid.NewGuid().ToString())
                                .SetJobData(parameters)
                                .Build();

            var ret = _quartzConfiguration.Scheduler.ScheduleJob(job, trigger);
            if (ret == null)
                throw new UserFriendlyException($"Could not create Quartz job for {typeName}");

            return Task.FromResult(0);
        }
        #endregion
    }
}