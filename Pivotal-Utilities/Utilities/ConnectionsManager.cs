using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pivotal.Utilities
{
    public class ConnectionsManager
    {

        /// <summary>
        /// Updates the connection strings with a User Provided Service of the same name
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public static void UpdateConnectionStrings(IConfiguration configuration)
        {
            //DumpConfiguration(configuration);

            var cstrings = configuration.GetSection("ConnectionStrings");
            foreach (var s in cstrings.GetChildren())
            {
                Console.WriteLine($"{s.Key} : {s.Value}");
                var newConnect = GetConfigurationConnectionString(configuration, s.Key);
            }
        }

        /// <summary>
        /// Gets the configuration connection string.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="dbName">Name of the database.</param>
        /// <returns></returns>
        public static string GetConfigurationConnectionString(IConfiguration configuration, string dbName)
        {
            // Use the Bound Service for connection string if it is found in a User Provided Service
            string sourceString = "appsettings.json/Config Server";
            string dbString = configuration.GetConnectionString(dbName);

            var user_section = configuration.GetSection("vcap:services:user-provided");
            //Console.WriteLine($"User Provided => {configuration["services:user-provided"].ToString()}");

            var _connect = user_section.GetChildren();
            
            foreach (var _c in _connect)
            {
                if (_c["name"] == dbName)
                {
                    sourceString = "User Provided Service";
                    var _con = _c["credentials:connectionstring"];
                    Console.WriteLine($"Found connection string in {_con} for {dbName}");
                    configuration.GetSection("ConnectionStrings")[dbName] = _con;
                    dbString = _con;
                }
            }

            Console.WriteLine($"{dbName} using connection string from {sourceString}");

            return dbString;
        }

        public static void DumpConfiguration(IConfiguration configuration)
        {
            var _connect = configuration.GetChildren();

            foreach (var _c in _connect)
            {
                Console.WriteLine($"Child => {_c.Key}");
                foreach (var _c1 in _c.GetChildren())
                {
                    Console.WriteLine($"{_c.Key} => {_c1.Key}");
                    foreach (var _c2 in _c1.GetChildren())
                    {
                        Console.WriteLine($"{_c.Key} => {_c1.Key} => {_c2.Key}");
                        foreach (var _c3 in _c2.GetChildren())
                        {
                            Console.WriteLine($"{_c.Key} => {_c1.Key} => {_c2.Key} => {_c3.Key}");
                            foreach (var _c4 in _c3.GetChildren())
                            {
                                Console.WriteLine($"{_c.Key} => {_c1.Key} => {_c2.Key} => {_c3.Key} => {_c4.Key}");
                            }
                        }
                    }
                }
            }
        }
    }
}

