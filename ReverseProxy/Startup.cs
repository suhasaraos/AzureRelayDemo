using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureRelayDemo.ReverseProxy
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });


            var connStr = Configuration.GetConnectionString("RelayConnectionString");
            var targetUri = new Uri(Configuration.GetConnectionString("RelayTargetUri"));

            RunAsync(connStr, targetUri).GetAwaiter().GetResult();
            }

        static async Task RunAsync(string connectionString, Uri targetUri)
        {
            var hybridProxy = new HybridConnectionReverseProxy(connectionString, targetUri);
            await hybridProxy.OpenAsync(CancellationToken.None);

            //We need to find a way to close gracefully. 
            //await hybridProxy.CloseAsync(CancellationToken.None);
        }
    }
}
