using NugetProxy.Core;
using NugetProxy.Core.Content;
using NugetProxy.Core.Server.Extensions;
using NugetProxy.Protocol;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using System.Net;
using System.Net.Http;
using System.Reflection;

namespace NugetProxy.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureNugetProxy(
            this IServiceCollection services,
            IConfiguration configuration,
            bool httpServices = false)
        {
            services.ConfigureAndValidate<MirrorOptions>(configuration.GetSection("Mirror"));

            if (httpServices)
            {
                services.ConfigureHttpServices();
            }

            services.AddSingleton<IServiceIndexService, NugetProxyServiceIndex>();
            services.AddSingleton<IUrlGenerator, NugetProxyUrlGenerator>();
            services.AddSingleton<ISearchService, NullSearchService>();
            services.AddSingleton<IPackageContentService, DatabasePackageContentService>();
            services.AddMirrorServices();

            return services;
        }

        /// <summary>
        /// Add the services that mirror an upstream package source.
        /// </summary>
        /// <param name="services">The defined services.</param>
        public static IServiceCollection AddMirrorServices(this IServiceCollection services)
        {
            services.AddTransient<MirrorService>();

            services.AddTransient<IMirrorService>(provider =>
            {
                return provider.GetRequiredService<MirrorService>();
            });

            services.AddSingleton<NuGetClient>();
            services.AddSingleton(provider =>
            {
                var httpClient = provider.GetRequiredService<HttpClient>();
                var options = provider.GetRequiredService<IOptions<MirrorOptions>>();

                return new NuGetClientFactory(
                    httpClient,
                    options.Value.PackageSource.ToString(),
                    options.Value.AccessToken);
            });

            services.AddSingleton(provider =>
            {
                var assembly = Assembly.GetEntryAssembly();
                var assemblyName = assembly.GetName().Name;
                var assemblyVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

                var client = new HttpClient(new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                });

                client.DefaultRequestHeaders.Add("User-Agent", $"{assemblyName}/{assemblyVersion}");

                return client;
            });

            services.AddSingleton<IPackageDownloadsSource, PackageDownloadsJsonSource>();

            return services;
        }
    }
}
