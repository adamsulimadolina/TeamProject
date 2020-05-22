﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamProject.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FormGenerator.Models;
using TeamProject.Models;
using TeamProject.Models.FieldFieldDependencyModels;
using AutoMapper;
using TeamProject.Helpers;
using TeamProject.Models.FormGeneratorModels;
using TeamProject.Generators;

namespace TeamProject
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(
                    "Host=database-1.cryqhlbutjmy.us-east-1.rds.amazonaws.com;Database=postgres;Username=postgres;Password=projektzespolowy"));
            services.AddIdentity<MyUser,IdentityRole>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.AddScoped<FormGeneratorContext>();
            services.AddScoped<IFieldDependenciesRepository, EFFieldDependenciesRepository>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddAutoMapper(typeof(Startup));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider, RoleManager<IdentityRole> roleManager, UserManager<MyUser> userManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
           
            app.UseSession();
            app.UseAuthentication();

         
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "",
                    template: "DependencyCreator",
                    defaults: new { controller = "FieldDependency", action = "Index" });
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Forms}/{action=Index}");
                routes.MapRoute(
                    name: "",
                    template: "{controller}/{action}/{id?}");
                routes.MapRoute(
                    name:"SaveTranslation",
                    template:"save_translation",
                    defaults:new {controller="UserAnswerLists",action="SaveTranslation"});
                routes.MapRoute(
                    name: "AddLog",
                    template: "addlog",
                    defaults:new {controller="Forms",action= "AddLog"});
            });
            app.UseCookiePolicy();

            Seed.SeedRoles(roleManager);
            Seed.SeedUsers(userManager);
            
        }
    }
}

