using ApiConnector.ApiConnectors;
using ApiConnector.Interfaces.Rest;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsoleApp;

public static class Program
{
	public static async Task Main(string[] args)
	{
		var host = CreateHostBuilder(args).Build();

		var restApiConnector = host.Services.GetRequiredService<IRestApiConnector>();

		// await restApiConnector.GetCandleSeriesAsync(
		// 	"BTCUSD",
		// 	60,
		// 	null,
		// 	DateTimeOffset.UtcNow.AddHours(-2),
		// 	DateTimeOffset.UtcNow.AddHours(-1),
		// 	null);
		// await restApiConnector.GetNewTradesAsync("BTCUSD", 5);
		await restApiConnector.GetTickerAsync("BTCUSD");
	}

	private static IHostBuilder CreateHostBuilder(string[] args)
	{
		return Host.CreateDefaultBuilder(args)
			.ConfigureServices(services =>
			{
				services.AddHttpClient<IRestApiConnector, RestApiConnector>(client =>
				{
					client.BaseAddress = new Uri("https://api-pub.bitfinex.com");
				});
			});
	}
}