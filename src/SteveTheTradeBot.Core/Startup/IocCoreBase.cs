using System;
using System.Reflection;
using Autofac;
using Bumbershoot.Utilities.Serializer;
using SteveTheTradeBot.Core.Components.Projects;
using SteveTheTradeBot.Core.Components.Users;
using SteveTheTradeBot.Core.Framework.CommandQuery;
using SteveTheTradeBot.Core.Framework.Event;
using SteveTheTradeBot.Core.Framework.MessageUtil;
using SteveTheTradeBot.Core.Framework.Subscriptions;
using SteveTheTradeBot.Dal.Models.Projects;
using SteveTheTradeBot.Dal.Models.SystemEvents;
using SteveTheTradeBot.Dal.Models.Users;
using SteveTheTradeBot.Dal.Persistence;
using FluentValidation;
using Serilog;
using MediatR;
using SteveTheTradeBot.Core.Components.Broker;
using SteveTheTradeBot.Core.Components.Storage;
using SteveTheTradeBot.Core.Components.ThirdParty;
using SteveTheTradeBot.Core.Components.ThirdParty.Valr;
using IValidatorFactory = SteveTheTradeBot.Dal.Validation.IValidatorFactory;
using ValidatorFactoryBase = SteveTheTradeBot.Dal.Validation.ValidatorFactoryBase;

namespace SteveTheTradeBot.Core.Startup
{
    public abstract class IocCoreBase
    {
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);

        protected void SetupCore(ContainerBuilder builder)
        {
            SetupMongoDb(builder);
            SetupManagers(builder);
            SetupTools(builder);
            SetupValidation(builder);
            SetupMediator(builder);
        }

        private void SetupMediator(ContainerBuilder builder)
        {
            // mediator itself
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            // request & notification handlers
            builder.Register<ServiceFactory>(context =>
            {
                var c = context.Resolve<IComponentContext>();
                return t => c.Resolve(t);
            });

            builder.RegisterAssemblyTypes(typeof(IocCoreBase).GetTypeInfo().Assembly)
                .Where(t => typeof(INotification).IsAssignableFrom(t) || t.IsClosedTypeOf(typeof(IRequestHandler<,>)) ||
                            t.IsClosedTypeOf(typeof(INotificationHandler<>)))
                .AsImplementedInterfaces(); // via assembly scan
        }

        protected virtual void SetupMongoDb(ContainerBuilder builder)
        {
            builder.Register(GetInstanceOfIGeneralUnitOfWorkFactory).SingleInstance();
            builder.Register(Delegate).As<IGeneralUnitOfWork>();
            builder.Register(x => x.Resolve<IGeneralUnitOfWork>().UserGrants);
            builder.Register(x => x.Resolve<IGeneralUnitOfWork>().Projects);
            builder.Register(x => x.Resolve<IGeneralUnitOfWork>().Users);
            builder.Register(x => x.Resolve<IGeneralUnitOfWork>().SystemCommands);
            builder.Register(x => x.Resolve<IGeneralUnitOfWork>().SystemEvents);
        }

        protected abstract IGeneralUnitOfWorkFactory GetInstanceOfIGeneralUnitOfWorkFactory(IComponentContext arg);

        #region Private Methods

        private IGeneralUnitOfWork Delegate(IComponentContext x)
        {
            try
            {
                return x.Resolve<IGeneralUnitOfWorkFactory>().GetConnection();
            }
            catch (Exception e)
            {
                _log.Error("IocCoreBase:Delegate " + e.Message, e);
                _log.Error(e.Source);
                _log.Error(e.StackTrace);
                throw;
            }
        }

        private static void SetupManagers(ContainerBuilder builder)
        {
            builder.RegisterType<ProjectLookup>().As<IProjectLookup>();
            builder.RegisterType<RoleManager>().As<IRoleManager>();
            builder.RegisterType<UserLookup>().As<IUserLookup>();
            builder.RegisterType<UserGrantLookup>().As<IUserGrantLookup>();
        }

        private static void SetupValidation(ContainerBuilder builder)
        {
            builder.RegisterType<AutofacValidatorFactory>().As<IValidatorFactory>();
            builder.RegisterType<UserValidator>().As<IValidator<User>>();
            builder.RegisterType<ProjectValidator>().As<IValidator<Project>>();
            builder.RegisterType<UserGrantValidator>().As<IValidator<UserGrant>>();
            builder.RegisterType<UserValidator>().As<IValidator<User>>();
        }

        private void SetupTools(ContainerBuilder builder)
        {
            builder.Register(x => new RedisMessenger(Settings.Instance.RedisHost)).As<IMessenger>().SingleInstance();
            builder.RegisterType<MediatorCommander>();
            builder.Register(x=>new CommanderPersist(x.Resolve<MediatorCommander>(),x.Resolve<IRepository<SystemCommand>>(), x.Resolve<IStringify>(), x.Resolve<IEventStoreConnection>())).As<ICommander>();
            builder.RegisterType<SubscriptionNotifications>().SingleInstance();
            builder.RegisterType<StringifyJson>().As<IStringify>().SingleInstance();
            builder.RegisterType<EventStoreConnection>().As<IEventStoreConnection>();
            builder.RegisterType<UpdateHistoricalData>().As<IUpdateHistoricalData>();
            builder.RegisterType<ValrHistoricalDataApi>().As<IHistoricalDataApi>();
            builder.RegisterType<HistoricalDataPlayer>().As<IHistoricalDataPlayer>();
            builder.RegisterType<TradeHistoryStore>().As<ITradeHistoryStore>();
            
            
            builder.Register(x => new TradePersistenceFactory(Settings.Instance.NpgsqlConnection)).As<ITradePersistenceFactory>().SingleInstance();
            builder.Register(x => x.Resolve<TradePersistenceFactory>().GetTradePersistence().Result);
            builder.Register(x => x.Resolve<TradePersistenceFactory>().GetTradePersistence().Result).As<TradePersistenceStoreContext>();
        }
        
        #endregion

        #region Nested type: AutofacValidatorFactory

        private class AutofacValidatorFactory : ValidatorFactoryBase
        {
            private readonly Func<IComponentContext> _context;

            public AutofacValidatorFactory(Func<IComponentContext> context)
            {
                _context = context;
            }

            protected override void TryResolve<T>(out IValidator<T> output)
            {
                _context().TryResolve(out output);
            }
        }

        #endregion
    }
}
