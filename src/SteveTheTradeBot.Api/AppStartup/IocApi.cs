using System;
using System.Reflection;
using Autofac;
using Autofac.Builder;
using Autofac.Extensions.DependencyInjection;
using SteveTheTradeBot.Api.Components;
using SteveTheTradeBot.Api.Components.Projects;
using SteveTheTradeBot.Api.Components.Users;
using SteveTheTradeBot.Api.GraphQl;
using SteveTheTradeBot.Api.Security;
using SteveTheTradeBot.Core;
using SteveTheTradeBot.Core.Startup;
using SteveTheTradeBot.Dal.MongoDb;
using SteveTheTradeBot.Dal.Persistence;
using HotChocolate;
using Serilog;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using SteveTheTradeBot.Api.Components.Integration;
using SteveTheTradeBot.Core.Components.Notifications;

namespace SteveTheTradeBot.Api.AppStartup
{
    public class IocApi : IocCoreBase
   
    {
        private static bool _isInitialized;
        private static readonly object _locker = new object();
        private static IocApi _instance;
        private static IServiceCollection _services;
        private static readonly ILogger _log = Log.ForContext(MethodBase.GetCurrentMethod().DeclaringType);
        private ContainerBuilder _builder;
        private Lazy<IContainer> _container;
        private ILifetimeScope _getAutofacRoot;

        public IocApi()
        {
            _container = new Lazy<IContainer>(() => _builder.Build());
            SetBuilder(new ContainerBuilder());
            if (_services != null) _builder.Populate(_services);
        }
        public static void Populate(IServiceCollection services)
        {
            if (Instance._container.IsValueCreated) throw new Exception("Need to call Populate before first instance call.");
            _services = services;
        }

        #region Overrides of IocCoreBase

        protected override IGeneralUnitOfWorkFactory GetInstanceOfIGeneralUnitOfWorkFactory(IComponentContext arg)
        {
            
            _log.Information($"Connecting to :{Settings.Instance.MongoConnection} [{Settings.Instance.MongoDatabase}]");
            try
            {
                return new MongoConnectionFactory(Settings.Instance.MongoConnection, Settings.Instance.MongoDatabase);
            }
            catch (Exception e)
            {
                _log.Error($"Error connecting to the database:{e.Message}", e);
                throw;
            }
        }

        #endregion

        #region Private Methods

        private static void SetupGraphQl(ContainerBuilder builder)
        {
            builder.RegisterType<CommandResultType>().SingleInstance();

            builder.RegisterType<ErrorFilter>().As<IErrorFilter>();

            builder.RegisterType<DefaultQuery>();
            builder.RegisterType<DefaultMutation>();
            builder.RegisterType<DefaultSubscription>().SingleInstance();
            builder.RegisterType<SubscriptionSubscribe>().SingleInstance();
            builder.RegisterType<RealTimeNotificationsMessageType>();


            /*user*/
            builder.RegisterType<UserType>();
            builder.RegisterType<UsersQueryType>();
            builder.RegisterType<UserCreateUpdateType>();
            builder.RegisterType<UsersMutationType>();
            builder.RegisterType<RoleType>();
            builder.RegisterType<RegisterType>();

            /*project*/
            builder.RegisterType<ProjectType>();
            builder.RegisterType<OpenIdSettings>();
            builder.RegisterType<ProjectsQueryType>();
            builder.RegisterType<ProjectCreateUpdateType>();
            builder.RegisterType<ProjectsMutation>();
            builder.RegisterType<ProjectsMutationType>();


            builder.RegisterType<HttpContextAccessor>().As<IHttpContextAccessor>().SingleInstance();
        }

        private void SetupCommonControllers(ContainerBuilder builder)
        {
        }

        private void SetupTools(ContainerBuilder builder)
        {
            builder.RegisterType<ObjectIdGenerator>().As<IIdGenerator>().SingleInstance();
            builder.RegisterType<SlackNotification>().As<INotificationChannel>().SingleInstance();
        }

        #endregion

        #region Instance

        public static IocApi Instance
        {
            get
            {
                if (_isInitialized) return _instance;
                lock (_locker)
                {
                    if (!_isInitialized)
                    {
                        _instance = new IocApi();
                        _isInitialized = true;
                    }
                }

                return _instance;
            }
        }

        public IContainer Container =>  _container.Value;


        public T Resolve<T>()
        {
            if (_getAutofacRoot != null) return _getAutofacRoot.Resolve<T>();
            return Container.Resolve<T>();
        }

        #endregion



     
        public void SetBuilder(ContainerBuilder builder)
        {
            _builder = builder;
            SetupCore(_builder);
            SetupCommonControllers(_builder);
            SetupGraphQl(_builder);
            SetupTools(_builder);
        }

        public void SetContainer(ILifetimeScope getAutofacRoot)
        {
            _getAutofacRoot = getAutofacRoot;
        }
    }
}