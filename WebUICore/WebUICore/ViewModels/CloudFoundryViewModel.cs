using Pivotal.Workshop.Models;
using Steeltoe.Common.Discovery;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System.Collections.Generic;

namespace WebUICore.ViewModels
{
    public class CloudFoundryViewModel
    {
        public CloudFoundryViewModel(CloudFoundryApplicationOptions appOptions, CloudFoundryServicesOptions servOptions, ConfigServerData configData, Dictionary<string,string> connectionStrings)
        {
            CloudFoundryServices = servOptions;
            CloudFoundryApplication = appOptions;
            ConfigData = configData;
            ConnectionStrings = connectionStrings;
        }

        public CloudFoundryServicesOptions CloudFoundryServices { get; }
        public ConfigServerData ConfigData { get; }
        public CloudFoundryApplicationOptions CloudFoundryApplication { get; }
        public Dictionary<string, string> ConnectionStrings { get; }
    }
}
