using FortuneService.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Pivotal.Discovery.Client;
using Pivotal.Extensions.Configuration.ConfigServer;
using Pivotal.Utilities;
using Pivotal.Workshop.Models;
using Steeltoe.CircuitBreaker.Hystrix;
using Steeltoe.CloudFoundry.Connector.RabbitMQ;
using Steeltoe.CloudFoundry.Connector.Redis;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using Steeltoe.Management.CloudFoundry;
using Steeltoe.Security.DataProtection;
using Steeltoe.Security.DataProtection.CredHub;
using Steeltoe.Security.DataProtection.CredHubCore;
using System;

namespace WebUICore
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env, ILoggerFactory logFactory)
        {
            Configuration = configuration;
            Environment = env;
            this.logFactory = logFactory;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }
        private ILoggerFactory logFactory;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddOptions();

            // Application Configuration
            services.Configure<ConfigServerData>(Configuration.GetSection("workshopConfig"));

            // Optional: Adds ConfigServerClientOptions to service container
            services.ConfigureConfigServerClientOptions(Configuration);

            // Optional:  Adds IConfiguration and IConfigurationRoot to service container
            services.AddConfiguration(Configuration);

            // Optional:  Adds CloudFoundryApplicationOptions and CloudFoundryServicesOptions to service container
            services.ConfigureCloudFoundryOptions(Configuration);

            // Add managment endpoint services
            services.AddCloudFoundryActuators(Configuration);

            // Add your own IInfoContributor, making sure to register with the interface
            //services.AddSingleton<IInfoContributor, ArbitraryInfoContributor>();

            // Add your own IHealthContributor, registered with the interface
            //services.AddSingleton<IHealthContributor, CustomHealthContributor>();

            // Add management components which collect and forwards metrics to 
            // the Cloud Foundry Metrics Forwarder service
            // Remove comments below to enable
            // services.AddMetricsActuator(Configuration);
            // services.AddMetricsForwarderExporter(Configuration);

            // Enable Redis function if not offline
            if (!Environment.IsDevelopment())
            {
                // Use Redis cache on CloudFoundry to DataProtection Keys
                services.AddRedisConnectionMultiplexer(Configuration);
                services.AddDataProtection()
                    .PersistKeysToRedis()
                    .SetApplicationName("webuicore");
            }
            // End Redis

            // Load Fortune Service Options 
            services.Configure<FortuneServiceOptions>(Configuration.GetSection("fortuneService"));
            // End load Fortune Service

            // Add service client library for calling the Fortune Service
            services.AddScoped<IFortuneService, FortuneServiceClient>();
            // End add service client

            // Add Service Discovery - remove development env check if running eureka locally
            if (!Environment.IsDevelopment())
            {
                services.AddDiscoveryClient(Configuration);
            }
            // End Service Discovery

            // Add Credhub Client
            services.Configure<CredHubOptions>(Configuration.GetSection("CredHubClient"));
            services.AddCredHubClient(Configuration, logFactory);
            //

            // Add Session Caching function
            if (Environment.IsDevelopment())
            {
                services.AddDistributedMemoryCache();
            }
            else
            {
                // Use Redis cache on CloudFoundry to store session data
                services.AddDistributedRedisCache(Configuration);
            }
            services.AddSession();
            // End Session Cache

            // Add Circuit Breaker function
            services.AddHystrixCommand<FortuneServiceCommand>("FortuneService", Configuration);
            services.AddHystrixMetricsStream(Configuration);
            // End Add CB

            // Add RabbitMQ function
            services.AddRabbitMQConnection(Configuration);
            // End RabbitMQ

            // Update the connection strings from appSettings.json or Config Server from any User Provided Service of the same name
            // User Provided Service will take presidence over other sources
            ConnectionsManager.UpdateConnectionStrings(Configuration);
            var dbString = Configuration.GetConnectionString("AttendeeContext");

            try
            {
                services.AddDbContext<AttendeeContext>(options => options.UseSqlServer(dbString));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
            // End connection strings

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            // Add management endpoints into pipeline
            app.UseCloudFoundryActuators();

            // Add metrics collection to the app
            // Remove comment below to enable
            // app.UseMetricsActuator();

            // Use Discovery Client
            app.UseDiscoveryClient();

            // Use Hystrix
            app.UseHystrixRequestContext();

            // Use Session Cache
            app.UseSession();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
