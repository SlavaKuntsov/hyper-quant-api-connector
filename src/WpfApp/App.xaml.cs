using System.Windows;

using ApiConnector.ApiConnectors;
using ApiConnector.CryptocurrencyConverter;
using ApiConnector.Interfaces.Rest;

using Microsoft.Extensions.DependencyInjection;

namespace WpfApp;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		var services = new ServiceCollection();

		services.AddHttpClient<IRestApiConnector, RestApiConnector>(client =>
		{
			client.BaseAddress = new Uri("https://api-pub.bitfinex.com");
		});

		services.AddHttpClient<ICryptocurrencyConverter, CryptocurrencyConverter>(client =>
		{
			client.BaseAddress = new Uri("https://api.cryptapi.io");
		});
	}
}