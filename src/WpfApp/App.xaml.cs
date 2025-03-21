﻿using System.Windows;
using System.Windows.Threading;
using ApiConnector.ApiConnectors;
using ApiConnector.CryptocurrencyConverter;
using ApiConnector.Interfaces.Rest;
using ApiConnector.Interfaces.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using WpfApp.MVVM.View;
using WpfApp.MVVM.ViewModel;
using CryptocurrencyConverter = ApiConnector.CryptocurrencyConverter.CryptocurrencyConverter;

namespace WpfApp;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	private static IServiceProvider? _serviceProvider;

	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);
		this.DispatcherUnhandledException += App_DispatcherUnhandledException;

		var services = new ServiceCollection();

		services.AddTransient<MainViewModel>();
		services.AddTransient<MainWindow>();

		services.AddHttpClient<IRestApiConnector, RestApiConnector>(client =>
		{
			client.BaseAddress = new Uri("https://api-pub.bitfinex.com");
		});

		services.AddHttpClient<ICryptocurrencyConverter, CryptocurrencyConverter>(client =>
		{
			client.BaseAddress = new Uri("https://api.cryptapi.io");
		});

		services.AddSingleton<IWebSocketConnector>(
			new WebSocketConnector("wss://api-pub.bitfinex.com/ws/2"));

		_serviceProvider = services.BuildServiceProvider();

		var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
		mainWindow.Show();
	}

	private static void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
	{
		MessageBox.Show(
			$"Ошибка: {e.Exception.Message}",
			"Критическая ошибка",
			MessageBoxButton.OK,
			MessageBoxImage.Error);
		e.Handled = true;
	}
}