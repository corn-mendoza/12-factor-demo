using FortuneService.Client;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pivotal.Workshop.Models;
using RabbitMQ.Client;
using Steeltoe.Common.Discovery;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebUICore.ViewModels;


namespace WebUICore.Controllers
{
    public class ServicesController : Controller
    {
        private IOptionsSnapshot<ConfigServerData> IConfigServerData { get; set; }

        private ILogger<ServicesController> _logger;
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

        public ServicesController(
            ILogger<ServicesController> logger,
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

        public async Task<IActionResult> Index()
        {
            _logger?.LogDebug("RandomFortune");

            if (_fortunes != null)
            {
                ViewData["FortuneUrl"] = _fortunesConfig.Value.RandomFortuneURL;

                var fortune = await _fortunes.RandomFortuneAsync();

                var _fortuneHistory = RedisCacheStore?.GetString("FortuneHistory");
                if (!string.IsNullOrEmpty(_fortuneHistory))
                    fortunes = JsonConvert.DeserializeObject<List<string>>(_fortuneHistory);

                fortunes.Insert(0, fortune.Text);

                if (fortunes.Count > 10)
                {
                    fortunes.RemoveAt(10);
                }

                string fortuneoutput = JsonConvert.SerializeObject(fortunes);
                RedisCacheStore?.SetString("FortuneHistory", fortuneoutput);

                HttpContext.Session.SetString("MyFortune", fortune.Text);

                var _appInstCount = RedisCacheStore?.GetString("AppInstance");
                if (!string.IsNullOrEmpty(_appInstCount))
                {
                    _logger?.LogInformation($"App Session Data: {_appInstCount}");
                    appInstCount = JsonConvert.DeserializeObject<SortedList<int, int>>(_appInstCount);
                }

                var _srvInstCount = RedisCacheStore?.GetString("SrvInstance");
                if (!string.IsNullOrEmpty(_srvInstCount))
                {
                    _logger?.LogInformation($"Servlet Session Data: {_srvInstCount}");
                    srvInstCount = JsonConvert.DeserializeObject<SortedList<int, int>>(_srvInstCount);
                }

                var _count2 = srvInstCount.GetValueOrDefault(fortune.InstanceIndex, 0);
                srvInstCount[fortune.InstanceIndex] = ++_count2;

                string output2 = JsonConvert.SerializeObject(srvInstCount);
                RedisCacheStore?.SetString("SrvInstance", output2);

                ViewData["MyFortune"] = fortune.Text;
                ViewData["FortuneIndex"] = $"{fortune.InstanceIndex}";
                ViewData["FortuneDiscoveryUrl"] = discoveryClient.GetInstances("fortuneService")?[fortune.InstanceIndex]?.Uri?.ToString();
            }

            return View(new ServicesViewModel(
                CloudFoundryApplication == null ? new CloudFoundryApplicationOptions() : CloudFoundryApplication,
                CloudFoundryServices == null ? new CloudFoundryServicesOptions() : CloudFoundryServices,
                IConfigServerData.Value,
                discoveryClient,
                appInstCount,
                srvInstCount,
                fortunes));
        }

        /// <summary>
        /// Resets the service stats.
        /// </summary>
        /// <returns></returns>
        public IActionResult ResetServiceStats()
        {
            srvInstCount = new SortedList<int, int>();

            string output2 = JsonConvert.SerializeObject(srvInstCount);
            RedisCacheStore?.SetString("SrvInstance", output2);

            return RedirectToAction(nameof(ServicesController.Index), "Services");
        }
    }
}