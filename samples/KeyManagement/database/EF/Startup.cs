using System;
using System.Linq;
using IdentityModel;
using IdentityServer4.KeyManagement.EntityFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace sample
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }
        public ILoggerFactory LoggerFactory { get; set; }

        public Startup(IConfiguration config, IWebHostEnvironment environment)
        {
            Configuration = config;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var cn = Configuration.GetConnectionString("db");

            services.AddDataProtection()
                .PersistKeysToDatabase(new DatabaseKeyManagementOptions
                {
                    ConfigureDbContext = b => b.UseSqlServer(cn),
                    LoggerFactory = LoggerFactory,
                });

            var builder = services.AddIdentityServer()
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Config.GetClients())
                .AddSigningKeyManagement(
                    options => // configuring options is optional :)
                    {
                        options.DeleteRetiredKeys = true;
                        options.KeyType = IdentityServer4.KeyManagement.KeyType.RSA;
                        options.InitializationDuration = TimeSpan.FromSeconds(5);
                        options.InitializationSynchronizationDelay = TimeSpan.FromSeconds(1);
                        options.KeyActivationDelay = TimeSpan.FromSeconds(10);
                        options.KeyExpiration = options.KeyActivationDelay * 2;
                        options.KeyRetirement = options.KeyActivationDelay * 3;
                        options.Licensee = "your licensee";
                        options.License = "your license key";
                    })
                    .PersistKeysToDatabase(new DatabaseKeyManagementOptions {
                        ConfigureDbContext = b => b.UseSqlServer(cn),
                    })
                    .ProtectKeysWithDataProtection();
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
        }
    }
}
