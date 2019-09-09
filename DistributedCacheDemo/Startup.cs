using DistributedCacheDemo.Cache;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace DistributedCacheDemo
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddDistributedMemoryCache(
                options => 
                {
                    options.SizeLimit = null;
                    options.ExpirationScanFrequency = TimeSpan.FromMinutes(30);
                });
            //services.AddDistributedSqlServerCache(
            //    options =>
            //    {
            //        options.ConnectionString = "";
            //        options.DefaultSlidingExpiration = TimeSpan.FromMinutes(20);
            //        options.ExpiredItemsDeletionInterval = TimeSpan.FromMinutes(30);
            //        options.SchemaName = "";
            //        options.TableName = "";
            //    });
            //services.AddDistributedRedisCache(
            //    options =>
            //    {
            //        options.ConfigurationOptions = new StackExchange.Redis.ConfigurationOptions()
            //        {
            //        };
            //        options.InstanceName = "";
            //    });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
