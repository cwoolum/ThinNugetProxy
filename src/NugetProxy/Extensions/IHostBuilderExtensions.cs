using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace NugetProxy.Extensions
{
    // See https://github.com/aspnet/MetaPackages/blob/master/src/Microsoft.AspNetCore/WebHost.cs
    public static class IHostBuilderExtensions
    {
        public static IHostBuilder ConfigureNugetProxyConfiguration(this IHostBuilder builder, string[] args)
        {
            return builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddEnvironmentVariables();

                config
                    .SetBasePath(Environment.CurrentDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

                if (args != null)
                {
                    config.AddCommandLine(args);
                }
            });
        }

        public static IHostBuilder ConfigureNugetProxyLogging(this IHostBuilder builder)
        {
            return builder
                .ConfigureLogging((context, logging) =>
                {
                    logging.AddConfiguration(context.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                });
        }

        public static IHostBuilder ConfigureNugetProxyServices(this IHostBuilder builder)
        {
            return builder
                .ConfigureServices((context, services) => services.ConfigureNugetProxy(context.Configuration));
        }
    }
}
