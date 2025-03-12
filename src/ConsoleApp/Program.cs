using ApiConnector.ApiConnectors;
using ApiConnector.CryptocurrencyConverter;
using ApiConnector.Interfaces.Rest;
using ApiConnector.Interfaces.WebSocket;
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
		var webSocketConnector = host.Services.GetRequiredService<IWebSocketConnector>();
		var cryptocurrencyConverter = host.Services.GetRequiredService<ICryptocurrencyConverter>();


		// await foreach (var candle in restApiConnector.GetCandleSeriesAsync(
		// 					"BTCUSD",
		// 					60,
		// 					null,
		// 					DateTimeOffset.UtcNow.AddHours(-2),
		// 					DateTimeOffset.UtcNow.AddHours(-1),
		// 					1))
		// 	Console.WriteLine(candle);
		//
		// await foreach (var trade in restApiConnector.GetNewTradesAsync("BTCUSD", 1))
		// 	Console.WriteLine(trade);
		//
		// var ticker = await restApiConnector.GetTickerAsync("BTCUSD");
		// Console.WriteLine(ticker);
		//
		// var convert = await cryptocurrencyConverter.Convert(
		// 	new Cryptocurrency("BTC", 10_000),
		// 	"USD");
		// Console.WriteLine(convert);
		
		// using (var webSocketConnector)
		
		await webSocketConnector.SubscribeTrades("BTCUSD");

		webSocketConnector.NewBuyTrade += trade =>
		{
			Console.WriteLine("--------------	INVOKE	-------------");
			Console.WriteLine(trade.ToString());
			Console.WriteLine("--------------	INVOKE	END	   -------------");
		};

		await Task.Delay(10000); // Ждем 5 секунд

		Console.WriteLine("отписка");
		await webSocketConnector.UnsubscribeTrades("BTCUSD");
		Console.WriteLine("заверщение");
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

				services.AddSingleton<IWebSocketConnector>(client =>
					new WebSocketConnector("wss://api-pub.bitfinex.com/ws/2"));
			});
	}
}