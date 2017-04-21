using System;
using Abp.BackgroundJobs;
using Abp.Configuration.Startup;
using Castle.MicroKernel.Registration;

namespace Abp.Quartz.Quartz.Configuration
{
    public static class AbpQuartzConfigurationExtensions
    {
        /// <summary>
        ///     Used to configure ABP Quartz module.
        /// </summary>
        public static IAbpQuartzConfiguration AbpQuartz(this IModuleConfigurations configurations)
        {
            return configurations.AbpConfiguration.Get<IAbpQuartzConfiguration>();
        }

        public static void UseQuartz(this IBackgroundJobConfiguration backgroundJobConfiguration, Action<IAbpQuartzConfiguration> configureAction = null)
        {
            backgroundJobConfiguration.AbpConfiguration.IocManager.IocContainer.Register(
                Component.For<IBackgroundJobManager, IQuartzScheduleJobManager>()
                    .ImplementedBy<QuartzScheduleJobManager>()
                    .LifestyleSingleton()
            );

            configureAction?.Invoke(backgroundJobConfiguration.AbpConfiguration.Modules.AbpQuartz());
        }
    }
}
