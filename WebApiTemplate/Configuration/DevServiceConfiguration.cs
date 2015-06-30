﻿using System;
using System.Threading.Tasks;
using Autofac;
using Owin;
using Serilog;
using webapitmpl.App_Start;
using webapitmpl.Utility;
using LogEventLevel = Serilog.Events.LogEventLevel;
using TraceCategories = System.Web.Http.Tracing.TraceCategories;

namespace webapitmpl.Configuration
{
    /// <summary>
    /// Configuration for development
    /// </summary>
    internal class DevServiceConfiguration : IServiceConfiguration
    {
        public void Configure(Microsoft.Owin.Hosting.StartOptions startOptions)
        {
            startOptions.Urls.Add("http://localhost:8999");
        }

        public async Task Configure(IAppBuilder app, Func<IAppBuilder, Task> runServer)
        {
            var builder = new ContainerBuilder();

            // Register primary services
            builder.RegisterModule<ProviderServicesModule>();

            builder.RegisterModule(
                new OwinServicesModule
                {
                    AppBuilder = app
                });

            // Allow documentation to be generated
            builder.RegisterType<DocsStartup>().SingleInstance().As<IStartup>();

            // Setup for WebAPI
            builder.RegisterModule(
                new WebApiServicesModule
                {
                });

            // Insert logging services
            builder.RegisterModule(
                new LoggingServicesModule
                {
                    ConfiguringLogging = ConfigureLogging
                });

            // Support CORS
            builder.RegisterType<AuthStartup>()
                .Keyed<IStartup>(AuthStartup.Id);

            // Support Swagger
            builder.RegisterType<DocsStartup>()
                .Keyed<IStartup>(DocsStartup.Id);

            using (IContainer container = builder.Build())
            {
                Action<object> runStartup = ServiceConfiguration.GetStartupForContainerRunner(container);
                runStartup(OwinStartup.Id);
                runStartup(LoggingStartup.Id);
                runStartup(AuthStartup.Id);
                runStartup(WebApiStartup.Id);
                runStartup(DocsStartup.Id);

                await runServer(app);
            }
        }

        public Serilog.LoggerConfiguration ConfigureLogging(Serilog.LoggerConfiguration logging)
        {
            var webApiHushFilter = new LevelFromTypeLogEventFilter(
                LogEventLevel.Warning,
                new []
                {
                    TraceCategories.ActionCategory,
                    TraceCategories.ControllersCategory,
                    TraceCategories.FiltersCategory,
                    TraceCategories.FormattingCategory,
                    TraceCategories.MessageHandlersCategory,
                    TraceCategories.ModelBindingCategory,
                    // TraceCategories.RequestCategory,
                    //TraceCategories.RoutingCategory
                });

            // Allow warning and error events to show in the console
            logging = logging.WriteTo.Logger(
                nested => nested
                    .Filter.With(webApiHushFilter)
                    .WriteTo.LiterateConsole()
                );

            return logging;
        }

    }
}
