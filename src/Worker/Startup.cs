using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

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

	}
}