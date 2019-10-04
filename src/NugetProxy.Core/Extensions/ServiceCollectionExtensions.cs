using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace NugetProxy.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection ConfigureAndValidate<TOptions>(
            this IServiceCollection services,
            IConfiguration config)
          where TOptions : class
        {
            services.Configure<TOptions>(config);
            services.AddSingleton<IPostConfigureOptions<TOptions>, ValidatePostConfigureOptions<TOptions>>();

            return services;
        }
    }
}
