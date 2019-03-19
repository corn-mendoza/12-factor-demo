using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pivotal.Utilities;
using Pivotal.Workshop.Models;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.Collections.Generic;
using WebUICore.ViewModels;

namespace WebUICore.Controllers
{
    public class ConfigurationController : Controller
    {
        private IOptionsSnapshot<ConfigServerData> IConfigServerData { get; set; }

        private ILogger<ConfigurationController> _logger;
        public CloudFoundryServicesOptions CloudFoundryServices { get; set; }
        public CloudFoundryApplicationOptions CloudFoundryApplication { get; set; }
        private IConfiguration Config { get; set; }
        private IConfigurationRoot ConfigRoot { get; set; }

        private Dictionary<string, string> connects = new Dictionary<string, string>();

        public ConfigurationController(
            ILogger<ConfigurationController> logger,
            IOptionsSnapshot<ConfigServerData> configServerData,
            IOptions<CloudFoundryApplicationOptions> appOptions,
            IOptions<CloudFoundryServicesOptions> servOptions,
            IConfiguration configApp,
            IConfigurationRoot configRoot
        )
        {
            if (configServerData != null)
                IConfigServerData = configServerData;

            _logger = logger;
            CloudFoundryServices = servOptions.Value;
            CloudFoundryApplication = appOptions.Value;
            Config = configApp;
            ConfigRoot = configRoot;
        }

        public IActionResult Index()
        {
            _logger?.LogDebug("Index");

            var _index = Environment.GetEnvironmentVariable("INSTANCE_INDEX");
            if (_index == null)
            {
                _index = "Running Local";
            }

            var _prodmode = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (_prodmode == null)
            {
                _prodmode = "Production";
            }

            var _port = Environment.GetEnvironmentVariable("PORT");
            if (_port == null)
            {
                _port = "localhost";
            }

            ViewData["Index"] = $"Application Instance: {_index}";
            ViewData["ProdMode"] = $"ASPNETCORE Environment: {_prodmode}";
            ViewData["Port"] = $"Port: {_port}";
            ViewData["Uptime"] = $"Uptime: {DateTime.Now.TimeOfDay.Subtract(TimeSpan.FromMilliseconds(Environment.TickCount))}";

            if (_index != "Running Local")
            {

                ViewData["appId"] = CloudFoundryApplication.ApplicationId;
                ViewData["appName"] = CloudFoundryApplication.ApplicationName;
                ViewData["uri0"] = CloudFoundryApplication.ApplicationUris[0];
                ViewData["disk"] = Config["vcap:application:limits:disk"];
                ViewData["sourceString"] = "appsettings.json/Config Server";

                if (Config.GetSection("spring") != null)
                {
                    ViewData["AccessTokenUri"] = Config["spring:cloud:config:access_token_uri"];
                    ViewData["ClientId"] = Config["spring:cloud:config:client_id"];
                    ViewData["ClientSecret"] = Config["spring:cloud:config:client_secret"];
                    ViewData["Enabled"] = Config["spring:cloud:config:enabled"];
                    ViewData["Environment"] = Config["spring:cloud:config:env"];
                    ViewData["FailFast"] = Config["spring:cloud:config:failFast"];
                    ViewData["Label"] = Config["spring:cloud:config:label"];
                    ViewData["Name"] = Config["spring:cloud:config:name"];
                    ViewData["Password"] = Config["spring:cloud:config:password"];
                    ViewData["Uri"] = Config["spring:cloud:config:uri"];
                    ViewData["Username"] = Config["spring:cloud:config:username"];
                    ViewData["ValidateCertificates"] = Config["spring:cloud:config:validate_certificates"];
                }
            }
            else
            {
                ViewData["AccessTokenUri"] = "Not Available";
                ViewData["ClientId"] = "Not Available";
                ViewData["ClientSecret"] = "Not Available";
                ViewData["Enabled"] = "Not Available";
                ViewData["Environment"] = "Not Available";
                ViewData["FailFast"] = "Not Available";
                ViewData["Label"] = "Not Available";
                ViewData["Name"] = "Not Available";
                ViewData["Password"] = "Not Available";
                ViewData["Uri"] = "Not Available";
                ViewData["Username"] = "Not Available";
                ViewData["ValidateCertificates"] = "Not Available";
            }

            var cstrings = Config.GetSection("ConnectionStrings");
            foreach (var s in cstrings.GetChildren())
            {
                if (!string.IsNullOrEmpty(s.Value))
                {
                    string connect = s.Value;

                    if (s.Value.Contains("Password"))
                    {
                        connect = StringCleaner.GetDisplayString("Password=", ";", connect, "*****");
                    }
                    if (s.Value.Contains("User ID"))
                    {
                        connect = StringCleaner.GetDisplayString("User ID=", ";", connect, "*****");
                    }

                    connects.Add(s.Key, connect);
                }
            }

            return View(new CloudFoundryViewModel(
                CloudFoundryApplication == null ? new CloudFoundryApplicationOptions() : CloudFoundryApplication,
                CloudFoundryServices == null ? new CloudFoundryServicesOptions() : CloudFoundryServices,
                IConfigServerData.Value,
                connects));
        }

        /// <summary>
        /// Reloads the configuration.
        /// </summary>
        /// <returns></returns>
        public IActionResult ReloadConfig()
        {
            ConfigRoot.Reload();
            return RedirectToAction(nameof(ConfigurationController.Index), "Configuration");
        }
    }
}