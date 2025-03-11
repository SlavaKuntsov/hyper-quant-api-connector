using ApiConnector.ApiConnectrors;
using ApiConnector.Interfaces.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsoleApp1;

public class Program
{
	public static async Task Main(string[] args)
	{
		var host = CreateHostBuilder(args).Build();

		var restApiConnector = host.Services.GetRequiredService<IRestApiConnector>();

		await restApiConnector.GetNewTradesAsync("BTCUSD", 5);
	}

	private static IHostBuilder CreateHostBuilder(string[] args)
	{
		return Host.CreateDefaultBuilder(args)
			.ConfigureServices((context, services) =>
			{
				services.AddHttpClient<IRestApiConnector, RestApiConnector>((serviceProvider, client) =>
				{
					client.BaseAddress = new Uri("https://api-pub.bitfinex.com");
				});
			});
	}
}