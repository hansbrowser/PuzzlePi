using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Worker.Models;
using Worker.Services;

namespace Worker
{
	public class Startup
	{
		private readonly IHostEnvironment _env;

		public Startup(IConfiguration configuration, IHostEnvironment env)
		{
			Configuration = configuration;
			_env = env;

			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(Configuration)
				.CreateLogger();
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddOptions();
			services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

			services.AddTransient<MidiService, MidiService>();

			services.AddHostedService<Worker>();

		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IServiceProvider sp)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}


			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
			});
		}
	}
}