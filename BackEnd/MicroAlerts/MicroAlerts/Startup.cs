using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace MicroAlerts
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
            // Add Application Insights
            services.AddApplicationInsightsTelemetry();

            // Add SQL Server / Db connectivity
            services.AddDbContext<AlertEntityDbContext>(opts => opts.UseSqlServer(Configuration["SQLServer:ConnectionString"],
                providerOptions => providerOptions.EnableRetryOnFailure()));                        
               
            // Add Service Bus
            services.AddSingleton<ITopicClient>(GetServiceBusTopic);

            // Add controllers
            services.AddControllers();
        }

        private ITopicClient GetServiceBusTopic(IServiceProvider options)
        {
            try
            {
                string ServiceBusConnectionString = Configuration["ServiceBus:ConnectionString"];
                string TopicName = Configuration["ServiceBus:TopicName"];
                TopicClient _topicClient = new TopicClient(ServiceBusConnectionString, TopicName);

                return _topicClient;
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to initialize Service Bus for Alerts Microservice", ex);
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, AlertEntityDbContext aContext)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}