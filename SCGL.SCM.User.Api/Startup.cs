using Autofac;
using Autofac.Extensions.DependencyInjection;
using Lazarus.Common.Authentication;
using Lazarus.Common.DI;
using Lazarus.Common.Infrastructure;
using Lazarus.Common.Nexus.Database;
using Lazarus.Common.Utilities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SCGL.SCM.User.Api.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SCGL.SCM.User.Api
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
            string buildVersion = Environment.GetEnvironmentVariable("BUILD_VERSION");
            services
                    .AddControllers();
            services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();
            services.AddHttpContextAccessor();
            services.AddDbContext<DbDataContext>(options =>
             options.UseSqlServer(Configuration.GetConnectionString("DbDataContext")).UseLazyLoadingProxies());
            services.AddDbContext<DbReadDataContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DbReadDataContext")).UseLazyLoadingProxies());
            services.AddDbContext<NexusDataContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("NexusDatabase")));


            services.AddCors(o => o.AddPolicy("MyPolicy", builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyMethod()
                       .AllowAnyHeader();
            }));

            services.AddSwaggerGen(c => Swagger.Gen(Configuration, c, "User"));

        }
        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.RegisterModule(new MediatorModule());
            builder.RegisterModule(new ProcessingModule());
            builder.RegisterModule(new RegisterServiceModule());
            builder.RegisterModule(new RegisterEventModule());
            builder.RegisterModule(new SharedModule());
        }
        public static ILifetimeScope AutofacContainer { get; set; }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var pathBase = Configuration["API_PATH_BASE"];
            if (!string.IsNullOrWhiteSpace(pathBase))
            {
                app.UsePathBase($"/{pathBase.TrimStart('/')}");
            }

            app.UseDeveloperExceptionPage();

            AppConfigUtilities._configuration = Configuration;
   
            
        
            AutofacContainer = app.ApplicationServices.GetAutofacRoot();
            DomainEvents._Container = AutofacContainer.BeginLifetimeScope();

            //  DependencyConfig.RegisterEvent();
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseSwagger(c => Swagger.Use(Configuration, c));
            app.UseSwaggerUI(c => Swagger.UseSwaggerUI(c, env, "User"));

            app.UseMiddleware<BasicAuthMiddleware>();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
