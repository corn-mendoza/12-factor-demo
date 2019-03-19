using Pivotal.Workshop.Models;
using Steeltoe.Common.Discovery;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;

namespace WebUICore.ViewModels
{
    public class ServicesViewModel
    {
        public ServicesViewModel(CloudFoundryApplicationOptions appOptions, CloudFoundryServicesOptions servOptions, ConfigServerData configData, IDiscoveryClient client, SortedList<int, int> appCounts, SortedList<int, int> srvCounts, List<string> fortunes)
        {
            CloudFoundryServices = servOptions;
            CloudFoundryApplication = appOptions;
            ConfigData = configData;
            discoveryClient = client;
            ServiceInstanceCounts = srvCounts;
            FortuneHistory = fortunes;
            AppInstanceCounts = appCounts;
        }

        public IDiscoveryClient discoveryClient { get; }
        public CloudFoundryServicesOptions CloudFoundryServices { get; }
        public ConfigServerData ConfigData { get; }
        public CloudFoundryApplicationOptions CloudFoundryApplication { get; }
        public SortedList<int, int> ServiceInstanceCounts { get; }
        public SortedList<int, int> AppInstanceCounts { get; }
        public List<string> FortuneHistory { get; }

        public List<KeyValuePair<int, int>> GetServiceInstanceCounts()
        {
            List<KeyValuePair<int, int>> ret = new List<KeyValuePair<int, int>>();
            foreach(var key in ServiceInstanceCounts.Keys)
            {
                KeyValuePair<int, int> kp = new KeyValuePair<int, int>(key, ServiceInstanceCounts[key]);
                ret.Add(kp);
            }

            return ret;
        }

        public string GetServiceInstanceLabels()
        {
            List<string> ret = new List<string>();

            foreach (var key in ServiceInstanceCounts.Keys)
            {
                ret.Add(key.ToString());
            }

            Console.WriteLine($"Service Labels: {JsonConvert.SerializeObject(ret)}");

            return JsonConvert.SerializeObject(ret);
        }

        public string GetServiceInstanceValues()
        {
            List<int> ret = new List<int>();

            foreach (var val in ServiceInstanceCounts.Values)
            {
                ret.Add(val);
            }

            Console.WriteLine($"Service Counts: {JsonConvert.SerializeObject(ret)}");

            return JsonConvert.SerializeObject(ret);
        }

        public List<KeyValuePair<int, int>> GetAppInstanceCounts()
        {
            List<KeyValuePair<int, int>> ret = new List<KeyValuePair<int, int>>();
            foreach (var key in AppInstanceCounts.Keys)
            {
                KeyValuePair<int, int> kp = new KeyValuePair<int, int>(key, AppInstanceCounts[key]);
                ret.Add(kp);
            }

            return ret;
        }

    }
}
