using System.Collections.ObjectModel;
using ApiConnector.CryptocurrencyConverter;
using ApiConnector.Interfaces.Rest;
using ApiConnector.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp.MVVM.Model;

namespace WpfApp.MVVM.ViewModel;

public partial class MainViewModel : ObservableObject
{
	private readonly ICryptocurrencyConverter _cryptocurrencyConverter;

	private readonly IRestApiConnector _restApiConnector;

	[ObservableProperty]
	private int amount = 5;

	[ObservableProperty]
	private decimal? portfolioBalance = 0;

	[ObservableProperty]
	private ObservableCollection<Portfolio> portfolios =
	[
		new() { Cryptocurrency = "BTC", Amount = 1, Currency = "USD" },
		new() { Cryptocurrency = "USD", Amount = 100_000, Currency = "BTC" },
		new() { Cryptocurrency = "XRP", Amount = 15000, Currency = "USD" },
		new() { Cryptocurrency = "XMR", Amount = 50, Currency = "USD" }
		// new() { Cryptocurrency = "ETH", Amount = 30, Currency = "USD" }
	];

	[ObservableProperty]
	private string ticker;

	[ObservableProperty]
	private ObservableCollection<Trade> trades = [];

	[ObservableProperty]
	private string tradingPair = "BTCUSD";

	public MainViewModel(
		ICryptocurrencyConverter cryptocurrencyConverter,
		IRestApiConnector restApiConnector)
	{
		_cryptocurrencyConverter = cryptocurrencyConverter;
		_restApiConnector = restApiConnector;
	}

	[RelayCommand]
	private async Task ConvertPortfolioAsync()
	{
		foreach (var portfolio in Portfolios)
			portfolio.ConvertedValue = await _cryptocurrencyConverter.Convert(
				new Cryptocurrency(portfolio.Cryptocurrency, portfolio.Amount),
				portfolio.Currency);

		Portfolios = new ObservableCollection<Portfolio>(Portfolios);

		PortfolioBalance = Portfolios.Sum(p => p.ConvertedValue);
	}

	[RelayCommand]
	private async Task GetNewTradesAsync()
	{
		Trades.Clear();

		await foreach (var trade in _restApiConnector.GetNewTradesAsync(tradingPair, amount))
			Trades.Add(trade);
	}

	[RelayCommand]
	private async Task GetTickerAsync()
	{
		var tickerModel = await _restApiConnector.GetTickerAsync(tradingPair);

		Ticker = tickerModel.ToString();
	}
}