using NugetProxy.Extensions;

using McMaster.Extensions.CommandLineUtils;

using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using System;

namespace NugetProxy
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var app = new CommandLineApplication
            {
                Name = "NugetProxy",
                Description = "A light-weight NuGet service",
            };

            app.HelpOption(inherited: true);

            app.OnExecute(() =>
            {
                CreateWebHostBuilder(args).Build().Run();
            });

            app.Execute(args);
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return new HostBuilder()
                .ConfigureNugetProxyConfiguration(args)
                .ConfigureNugetProxyServices()
                .ConfigureNugetProxyLogging();
        }
    }
}
