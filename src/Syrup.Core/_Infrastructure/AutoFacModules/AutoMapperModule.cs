using System.Collections.Generic;
using Autofac;
using AutoMapper;
using NLog;

namespace Syrup.Core._Infrastructure.AutoFacModules
{
    public class AutoMapperModule : Module
    {
        private static readonly Logger _log = LogManager.GetCurrentClassLogger();

        protected override void Load(ContainerBuilder builder)
        {
            _log.Debug("AutoMapperModule");

            // With ugly hack to avoid problem with ILMarge
            builder.RegisterAssemblyTypes(typeof(AutoMapperModule).Assembly)
                .Where(x => !x.IsAbstract)      // to avoid abstract Profile class
                .Where(t => !t.IsNestedPrivate) // to avoid private nested MapperConfiguration+NamedProfile class
                .AssignableTo(typeof(Profile)).As<Profile>();

            //builder.RegisterInstance(new EvaluationProfile ()).As<Profile>();


            builder.Register(context => new MapperConfiguration(cfg =>
            {
                foreach (var profile in context.Resolve<IEnumerable<Profile>>())
                {
                    cfg.AddProfile(profile);
                }
            })).AsSelf().SingleInstance();

            builder.Register(c => c.Resolve<MapperConfiguration>().CreateMapper(c.Resolve))
                .As<IMapper>()
                .InstancePerLifetimeScope();
        }
    }
}