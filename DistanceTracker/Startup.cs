using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DistanceTracker.DALs;
using DistanceTracker.Utils;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;

namespace DistanceTracker
{
	public class Startup
	{
		public Startup(IWebHostEnvironment env)
		{
			Environment = env;
		}

		public IWebHostEnvironment Environment { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			if (Environment.EnvironmentName == Environments.Development)
			{
				services.AddControllersWithViews().AddRazorRuntimeCompilation();
			}

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0).AddNewtonsoftJson(options =>
			{
				options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
				options.SerializerSettings.Converters.Add(new NumberToStringConverter());
			});

			services.AddTransient<Settings>();
			services.AddTransient<GeneralDAL>();
			services.AddTransient<PlayerDAL>();
			services.AddTransient<LeaderboardDAL>();
			services.AddTransient<LeaderboardEntryDAL>();
			services.AddTransient<LeaderboardEntryHistoryDAL>();
			services.AddTransient<SteamDAL>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app)
		{
			if (Environment.EnvironmentName == Environments.Development)
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseCookiePolicy();
			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}");
			});
		}
	}
}
