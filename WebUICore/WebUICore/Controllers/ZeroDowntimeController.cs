using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FortuneService.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pivotal.Utilities;
using Pivotal.Workshop.Models;
using RabbitMQ.Client;
using Steeltoe.Common.Discovery;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using WebUICore.ViewModels;

namespace WebUICore.Controllers
{
    public class ZeroDowntimeController : Controller
    {
        private IOptionsSnapshot<ConfigServerData> IConfigServerData { get; set; }

        private ILogger<ZeroDowntimeController> _logger;
        public CloudFoundryServicesOptions CloudFoundryServices { get; set; }
        public CloudFoundryApplicationOptions CloudFoundryApplication { get; set; }
        private IOptionsSnapshot<FortuneServiceOptions> _fortunesConfig;
        private IDiscoveryClient discoveryClient;
        private IDistributedCache RedisCacheStore { get; set; }
        private IConfiguration Config { get; set; }
        private IConfigurationRoot ConfigRoot { get; set; }
        private ConnectionFactory ConnectionFactory { get; set; }

        private SortedList<int, int> appInstCount = new SortedList<int, int>();
        private SortedList<int, int> srvInstCount = new SortedList<int, int>();
        private List<string> fortunes = new List<string>();

        private FortuneServiceCommand _fortunes;

        public ZeroDowntimeController(
            ILogger<ZeroDowntimeController> logger,
            IOptionsSnapshot<FortuneServiceOptions> config,
            IOptionsSnapshot<ConfigServerData> configServerData,
            FortuneServiceCommand fortunes,
            IOptions<CloudFoundryApplicationOptions> appOptions,
            IOptions<CloudFoundryServicesOptions> servOptions,
            IConfiguration configApp,
            IConfigurationRoot configRoot,
            IDistributedCache cache,
            [FromServices] IDiscoveryClient client
        )
        {
            if (configServerData != null)
                IConfigServerData = configServerData;

            _logger = logger;

            if (fortunes != null)
                _fortunes = fortunes;

            CloudFoundryServices = servOptions.Value;
            CloudFoundryApplication = appOptions.Value;
            _fortunesConfig = config;
            discoveryClient = client;
            RedisCacheStore = cache;
            Config = configApp;
            ConfigRoot = configRoot;
        }

        public IActionResult Index()
        {
            SortedList<int, int> appInstCount = new SortedList<int, int>();
            SortedList<int, int> srvInstCount = new SortedList<int, int>();
            List<string> fortunes = new List<string>();

            ViewData["AppColor"] = WebStyleUtilities.GetColorFromString(CloudFoundryApplication.ApplicationName, "blue");

            var _appInstCount = RedisCacheStore?.GetString("AppInstance");
            if (!string.IsNullOrEmpty(_appInstCount))
            {
                _logger?.LogInformation($"App Session Data: {_appInstCount}");
                appInstCount = JsonConvert.DeserializeObject<SortedList<int, int>>(_appInstCount);
            }

            var _count = appInstCount.GetValueOrDefault(CloudFoundryApplication.Instance_Index, 0);
            appInstCount[CloudFoundryApplication.Instance_Index] = ++_count;

            string output = JsonConvert.SerializeObject(appInstCount);
            RedisCacheStore?.SetString("AppInstance", output);

            return View(new ServicesViewModel(
                CloudFoundryApplication == null ? new CloudFoundryApplicationOptions() : CloudFoundryApplication,
                CloudFoundryServices == null ? new CloudFoundryServicesOptions() : CloudFoundryServices,
                IConfigServerData.Value,
                discoveryClient,
                appInstCount,
                srvInstCount,
                fortunes
                ));
        }

        /// <summary>
        /// Resets the application stats.
        /// </summary>
        /// <returns></returns>
        public IActionResult ResetApplicationStats()
        {
            appInstCount = new SortedList<int, int>();

            string output = JsonConvert.SerializeObject(appInstCount);
            RedisCacheStore?.SetString("AppInstance", output);

            return RedirectToAction(nameof(ZeroDowntimeController.Index), "ZeroDowntime");
        }
    }
}