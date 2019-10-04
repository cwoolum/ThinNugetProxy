using BaGet.Configuration;
using BaGet.Core;
using BaGet.Extensions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;

namespace BaGet
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseStatusCodePages();
            }

            // Run migrations if necessary.
            var options = Configuration.Get<BaGetOptions>();

            app.UsePathBase(options.PathBase);
            app.UseForwardedHeaders();
            
            app.UseMvc(routes =>
            {
                routes
                    .MapServiceIndexRoutes()
                    .MapSymbolRoutes()
                    .MapSearchRoutes()
                    .MapPackageMetadataRoutes()
                    .MapPackageContentRoutes();
            });
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureBaGet(Configuration, httpServices: true);
        }
    }
}
