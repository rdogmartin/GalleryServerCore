using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GalleryServer.Business;
using GalleryServer.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Gs.Web.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gs.Web
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
            services.AddDbContext<GalleryDb>(options => options.UseSqlServer(Configuration.GetConnectionString("GalleryCore")));

            services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<GalleryDb>().AddDefaultTokenProviders();

            services.AddMemoryCache();

            services.AddMvc().AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Account/Manage");
                    options.Conventions.AuthorizePage("/Account/Logout");
                });

            // Register no-op EmailSender used by account confirmation and password reset during development
            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
            services.AddSingleton<IEmailSender, EmailSender>();
            services.AddSingleton<IMemoryCache, MemoryCache>();
            //services.AddSingleton<CacheController>();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles(); // For the wwwroot folder

            // Map the 'angular' directory to the /a path in the URL and serve static content out of it. We could have instead called UseStaticFiles & UseDefaultFiles.
            app.UseFileServer(new FileServerOptions()
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), @"angular")),
                RequestPath = new PathString("/a"),
                EnableDirectoryBrowsing = false
            });

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });

            // Handle client side routes
            app.Run(async (context) =>
            {
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync(System.IO.Path.Combine(env.WebRootPath, "index.html"));
            });

            WebHelper.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>(), app.ApplicationServices.GetRequiredService<IMemoryCache>());

            GalleryServer.Web.Controller.GalleryController.InitializeGspApplication();
        }
    }
}
