using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharpReverseProxy;
using System.Threading.Tasks;
using System;

namespace RelayServer
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
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            else
                app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseProxy(new List<ProxyRule>
            {
                new ProxyRule
                {
                    AddForwardedHeader = false,
                    Matcher = uri => Task.Run(() =>
                        Program.Settings.FirstOrDefault(a => a.DomainName.Equals(uri.Host.Host)) != null
                            && !uri.Path.ToString().StartsWith("/.well-known")),
                    RequestModifier = (req, _) => Task.Run(() =>
                        req.RequestUri = new Uri(req.RequestUri
                            .ToString()
                            .Replace(
                                req.RequestUri.Authority,
                                Program.Settings.FirstOrDefault(a => a.DomainName.Equals(req.RequestUri.Host)).LocalIp)))
                }
            }, res => Task.Run(() => Console.WriteLine($"Proxied \"{res.OriginalUri}\" to \"{res.ProxiedUri}\".")));

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
