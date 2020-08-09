using CephissusBackend.Contracts;
using CephissusBackend.Entities;
using CephissusBackend.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace CephissusBackend
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers().AddNewtonsoftJson();
            services.AddDbContext<CephissusContext>(builder => builder.UseSqlite(Configuration.GetConnectionString("Cephissus")));
            services.AddTransient<HttpClient>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            }).AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.None;
                options.ExpireTimeSpan = TimeSpan.FromDays(30);
                options.Events.OnRedirectToLogin = context =>
                {
                    context.Response.StatusCode = 401;
                    return Task.CompletedTask;
                };
            });
            services.AddAuthorization();
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseHsts();

            app.UseCors(builder =>
            {
                builder.AllowCredentials();
                builder.AllowAnyMethod();
                builder.AllowAnyHeader();
                builder.WithOrigins("https://localhost:4242");
            });

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(routeBuilder =>
            {
                routeBuilder.MapControllers().RequireAuthorization();
            });
        }
    }
}
