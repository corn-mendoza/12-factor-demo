using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Pivotal.Workshop.Models;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using WebUICore.Models;
using WebUICore.ViewModels;

namespace WebUICore.Controllers
{
    public class HomeController : Controller
    {
        private IOptionsSnapshot<ConfigServerData> IConfigServerData { get; set; }

        private ILogger<ConfigurationController> _logger;
        public CloudFoundryServicesOptions CloudFoundryServices { get; set; }
        public CloudFoundryApplicationOptions CloudFoundryApplication { get; set; }
        private IConfiguration Config { get; set; }
        private IConfigurationRoot ConfigRoot { get; set; }

        private Dictionary<string, string> connects = new Dictionary<string, string>();

        public HomeController(
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
            return View(new CloudFoundryViewModel(
                CloudFoundryApplication == null ? new CloudFoundryApplicationOptions() : CloudFoundryApplication,
                CloudFoundryServices == null ? new CloudFoundryServicesOptions() : CloudFoundryServices,
                IConfigServerData.Value,
                connects));
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
