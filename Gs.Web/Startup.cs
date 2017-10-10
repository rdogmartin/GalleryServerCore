using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using GalleryServer.Business;
using GalleryServer.Data;
using GalleryServer.Web.Security;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Gs.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

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

            services.AddIdentity<GalleryUser, GalleryRole>()
                .AddRoleManager<GalleryRoleManager>()
                .AddEntityFrameworkStores<GalleryDb>()
                .AddDefaultTokenProviders();

            services.AddMemoryCache();

            //option =>
            //{
            //    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme
            //}
            services.AddAuthentication()
                .AddCookie(cfg => cfg.SlidingExpiration = true)
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;

                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = Configuration["Tokens:Issuer"],
                        ValidAudience = Configuration["Tokens:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Tokens:Key"]))
                    };
                });

            services.AddAuthorization(options =>
            {
                //options.AddPolicy(GlobalConstants.PolicyViewAlbumOrAsset, policy =>
                //{
                //    policy.RequireAuthenticatedUser();
                //    policy.Requirements.Add(new ViewAlbumOrAssetRequirement());
                //});
                options.AddPolicy(GlobalConstants.PolicyAdministrator, policy => policy.Requirements.Add(new AdminRequirement()));
                //options.AddPolicy("Administrator", policy => policy.RequireClaim("EmployeeNumber", "1", "2", "3", "4", "5"));
            });

            // Register no-op EmailSender used by account confirmation and password reset during development
            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
            services.AddSingleton<IEmailSender, EmailSender>();
            //services.AddSingleton<IMemoryCache, MemoryCache>();
            //services.AddSingleton<CacheController>();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            //services.AddSingleton<IAuthorizationHandler, ViewAlbumOrAssetHandler>();
            services.AddSingleton<IAuthorizationHandler, SiteAdminHandler>();
            services.AddSingleton<IAuthorizationHandler, GalleryAdminHandler>();

            //services.AddCors();

            //services.AddMvc();
            services.AddMvc().AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Account/Manage");
                    options.Conventions.AuthorizePage("/Account/Logout");
                });
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
                routes.MapRoute(name: "mvc", template: "{controller}/{action=Index}/{id?}");
            });

            // Handle client side routes
            app.Run(async (context) =>
            {
                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync(System.IO.Path.Combine(env.WebRootPath, "index.html"));
            });

            //var scopeFactory = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>();

            using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                //var signInManager = scope.ServiceProvider.GetRequiredService<SignInManager<GalleryUser>>();
                //var roleManager = scope.ServiceProvider.GetRequiredService<GalleryRoleManager>();

                DiHelper.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>(),
                    app.ApplicationServices.GetRequiredService<IMemoryCache>(),
                    scope.ServiceProvider.GetRequiredService<SignInManager<GalleryUser>>(), // app.ApplicationServices.GetService<SignInManager<GalleryUser>>(),
                    scope.ServiceProvider.GetRequiredService<GalleryRoleManager>(),
                    env
                    ); //app.ApplicationServices.GetRequiredService<GalleryRoleManager>());
            }

            //app.UseCors(builder => builder.AllowAnyOrigin()); // https://docs.microsoft.com/en-us/aspnet/core/security/cors

            GalleryServer.Web.Controller.GalleryController.InitializeGspApplication();
        }
    }
}
