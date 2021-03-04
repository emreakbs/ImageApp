using ImageApp.Bussiness.Extension;
using ImageApp.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace ImageApp
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
            services.AddDbContext<MasterContext>(y => y.UseMySQL(Environment.GetEnvironmentVariable("MYSQL_URI")));

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = Configuration["Token:Issuer"],
                    ValidAudience = Configuration["Token:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Token:SecurityKey"])),
                    ClockSkew = TimeSpan.Zero
                };
            });

            services.AddSession(opts =>
            {
                opts.IdleTimeout = TimeSpan.FromHours(4);
                opts.Cookie.IsEssential = false;
            });
            services.AddStackExchangeRedisCache(action =>
           {
               action.Configuration = Environment.GetEnvironmentVariable("REDIS_URL");
           });
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
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
                app.UseExceptionHandler("/Home/Error");
            }
            //Hata sayfalar�
            app.UseStatusCodePages(async context =>
            {
                if (context.HttpContext.Response.StatusCode == 400)
                {
                    context.HttpContext.Response.Redirect("http://www.google.com");
                }
                else if (context.HttpContext.Response.StatusCode == 404)
                {
                    context.HttpContext.Response.Redirect($"{Environment.GetEnvironmentVariable("APP_URL")}/404");
                }
                else
                {
                    context.HttpContext.Response.Redirect($"{Environment.GetEnvironmentVariable("APP_URL")}/500");
                }
            });
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseSession();

            app.UseAuthorization();

            //Migration varsa ve database g�ncel de�ilse database'i g�nceller
            using (IServiceScope serviceScope = app.ApplicationServices.CreateScope())
            {
                MasterContext context = serviceScope.ServiceProvider.GetRequiredService<MasterContext>();
                context.Database.Migrate();
            }

            //Database de kullan�c� var m� onu ontrol ediyoruz
            DatabaseInit.Instance.UserControl();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
