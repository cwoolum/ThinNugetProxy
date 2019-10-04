using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using BaGet.Core;
using BaGet.Core.Content;
using BaGet.Core.Server.Extensions;
using BaGet.Protocol;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BaGet.Extensions
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureBaGet(
            this IServiceCollection services,
            IConfiguration configuration,
            bool httpServices = false)
        {
            services.ConfigureAndValidate<BaGetOptions>(configuration);
            services.ConfigureAndValidate<SearchOptions>(configuration.GetSection(nameof(BaGetOptions.Search)));
            services.ConfigureAndValidate<MirrorOptions>(configuration.GetSection(nameof(BaGetOptions.Mirror)));

            if (httpServices)
            {
                services.ConfigureHttpServices();
            }

            services.AddTransient<IServiceIndexService, BaGetServiceIndex>();
            services.AddTransient<IUrlGenerator, BaGetUrlGenerator>();
            services.AddSingleton<ISearchService, NullSearchService>();
            services.AddTransient<IPackageContentService, DatabasePackageContentService>();
            services.AddTransient<IPackageMetadataService, DatabasePackageMetadataService>();
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
                var options = provider.GetRequiredService<IOptionsSnapshot<MirrorOptions>>();


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
                var options = provider.GetRequiredService<IOptions<BaGetOptions>>().Value;

                var assembly = Assembly.GetEntryAssembly();
                var assemblyName = assembly.GetName().Name;
                var assemblyVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "0.0.0";

                var client = new HttpClient(new HttpClientHandler
                {
                    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
                });

                client.DefaultRequestHeaders.Add("User-Agent", $"{assemblyName}/{assemblyVersion}");
                client.Timeout = TimeSpan.FromSeconds(options.Mirror.PackageDownloadTimeoutSeconds);

                return client;
            });

            services.AddSingleton<DownloadsImporter>();
            services.AddSingleton<IPackageDownloadsSource, PackageDownloadsJsonSource>();

            return services;
        }


    }
}
