using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Pivotal.Extensions.Configuration.ConfigServer;
using Steeltoe.Security.DataProtection.CredHubCore;

namespace WebUICore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = new LoggerFactory().AddConsole(LogLevel.Trace);
            CreateWebHostBuilder(args, logger).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args, ILoggerFactory logger) =>
            WebHost.CreateDefaultBuilder(args)
                    .UseCloudFoundryHosting()
                    .ConfigureAppConfiguration(b => b.AddConfigServer(logger))
                    .UseCredHubInterpolation(logger)
                    .UseStartup<Startup>();
    }
}
