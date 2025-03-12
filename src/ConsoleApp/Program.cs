using ApiConnector.ApiConnectors;
using ApiConnector.CryptocurrencyConverter;
using ApiConnector.Interfaces.Rest;
using ApiConnector.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ConsoleApp;

public static class Program
{
	public static async Task Main(string[] args)
	{
		var host = CreateHostBuilder(args).Build();

		var restApiConnector = host.Services.GetRequiredService<IRestApiConnector>();
		var cryptocurrencyConverter = host.Services.GetRequiredService<ICryptocurrencyConverter>();


		await foreach (var candle in restApiConnector.GetCandleSeriesAsync(
							"BTCUSD",
							60,
							null,
							DateTimeOffset.UtcNow.AddHours(-2),
							DateTimeOffset.UtcNow.AddHours(-1),
							1))
			Console.WriteLine(candle);

		await foreach (var trade in restApiConnector.GetNewTradesAsync("BTCUSD", 1))
			Console.WriteLine(trade);

		var ticker = await restApiConnector.GetTickerAsync("BTCUSD");
		Console.WriteLine(ticker);

		var convert = await cryptocurrencyConverter.Convert(
			new Cryptocurrency("BTC", 10_000),
			"USD");
		Console.WriteLine(convert);
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

				services.AddHttpClient<ICryptocurrencyConverter, CryptocurrencyConverter>(client =>
				{
					client.BaseAddress = new Uri("https://api.cryptapi.io");
				});
			});
	}
}