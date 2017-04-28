using System;
using System.Threading;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.TestBase;
using Shouldly;
using Xunit;

namespace Abp.Quartz.Tests
{
    public class BackgroundJobTests : AbpIntegratedTestBase<AbpQuartzTestModule>
    {
        private readonly IBackgroundJobManager _backgroundJobManager;

        public BackgroundJobTests()
        {
            _backgroundJobManager = LocalIocManager.Resolve<IBackgroundJobManager>();

            EnqueueBackgroundJob();
        }

        private void EnqueueBackgroundJob()
        {
            _backgroundJobManager.EnqueueAsync<BackgroundJobTest, int>(10);
        }

        [Fact]
        public void QuartzScheduler_ShouldExecuteBackgroundJob()
        {
            Thread.Sleep(TimeSpan.FromSeconds(10));

            var backgroundDependency = LocalIocManager.Resolve<IBackgroundJobDependency>();
            backgroundDependency.ExecutionCount.ShouldBe(1);
            backgroundDependency.JobArgument.ShouldBe(10);
        }
    }

    public interface IBackgroundJobDependency
    {
        int JobArgument { get; set; }
        int ExecutionCount { get; set; }
    }

    public class BackgroundJobDependency : IBackgroundJobDependency, ISingletonDependency
    {
        public int JobArgument { get; set; }
        public int ExecutionCount { get; set; }
    }

    public class BackgroundJobTest : IBackgroundJob<int>, ITransientDependency
    {
        public IBackgroundJobDependency Dependency { get; set; }

        public void Execute(int argument)
        {
            Dependency.JobArgument = argument;
            Dependency.ExecutionCount++;
        }
    }
}
