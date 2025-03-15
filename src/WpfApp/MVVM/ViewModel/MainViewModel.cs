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
	private int candlePeriodInSec = 60;

	[ObservableProperty]
	private ObservableCollection<Candle> candles = [];

	[ObservableProperty]
	private DateTime? fromDate;

	[ObservableProperty]
	private string fromTime;

	[ObservableProperty]
	private decimal? portfolioBalance = 0;

	[ObservableProperty]
	private int? sort = null;

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
	private DateTime? toDate;

	[ObservableProperty]
	private string toTime;

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
	private async Task GetCandleSeriesAsync()
	{
		Candles.Clear();

		Console.WriteLine("Getting Candles ");

		var from = CombineDateAndTime(FromDate, FromTime) ?? DateTimeOffset.UtcNow.AddHours(-1);
		var to = CombineDateAndTime(ToDate, ToTime) ?? DateTimeOffset.UtcNow;

		await foreach (var trade in _restApiConnector.GetCandleSeriesAsync(
							tradingPair,
							candlePeriodInSec,
							sort,
							from,
							to,
							amount))
		{
			Console.WriteLine(trade.ToString());
			Candles.Add(trade);
		}
	}

	[RelayCommand]
	private async Task GetNewTradesAsync()
	{
		Trades.Clear();

		Console.WriteLine("Getting new trades");

		await foreach (var trade in _restApiConnector.GetNewTradesAsync(tradingPair, amount))
			Trades.Add(trade);
	}

	[RelayCommand]
	private async Task GetTickerAsync()
	{
		var tickerModel = await _restApiConnector.GetTickerAsync(tradingPair);

		Ticker = tickerModel.ToString();
	}

	private DateTimeOffset? CombineDateAndTime(DateTime? date, string time)
	{
		if (date == null)
			return null;

		if (string.IsNullOrEmpty(time))
			return new DateTimeOffset(date.Value.Date);

		if (TimeSpan.TryParse(time, out var timeSpan))
			return new DateTimeOffset(date.Value.Date + timeSpan);

		throw new ArgumentException("Invalid time format. Please use HH:mm.");
	}
}