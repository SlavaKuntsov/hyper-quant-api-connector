using System.Collections.ObjectModel;
using ApiConnector.CryptocurrencyConverter;
using ApiConnector.Interfaces.Rest;
using ApiConnector.Interfaces.WebSocket;
using ApiConnector.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp.MVVM.Model;

namespace WpfApp.MVVM.ViewModel;

public partial class MainViewModel : ObservableObject
{
	private readonly ICryptocurrencyConverter _cryptocurrencyConverter;

	private readonly IRestApiConnector _restApiConnector;

	private readonly IWebSocketConnector _webSocketConnector;

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
	private ObservableCollection<Portfolio> portfolios =
	[
		new() { Cryptocurrency = "BTC", Amount = 1, Currency = "USD" },
		new() { Cryptocurrency = "USD", Amount = 100_000, Currency = "BTC" },
		new() { Cryptocurrency = "XRP", Amount = 15000, Currency = "USD" },
		new() { Cryptocurrency = "XMR", Amount = 50, Currency = "USD" }
		// new() { Cryptocurrency = "ETH", Amount = 30, Currency = "USD" }
	];

	[ObservableProperty]
	private int? sort;

	[ObservableProperty]
	private string ticker;

	[ObservableProperty]
	private DateTime? toDate;

	[ObservableProperty]
	private string toTime;

	[ObservableProperty]
	private ObservableCollection<Trade> trades = [];

	[ObservableProperty]
	private ObservableCollection<Trade> tradesWebSocket = [];

	[ObservableProperty]
	private string tradingPair = "BTCUSD";

	public MainViewModel(
		ICryptocurrencyConverter cryptocurrencyConverter,
		IRestApiConnector restApiConnector,
		IWebSocketConnector webSocketConnector)
	{
		_cryptocurrencyConverter = cryptocurrencyConverter;
		_restApiConnector = restApiConnector;
		_webSocketConnector = webSocketConnector;
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

		var from = CombineDateAndTime(FromDate, FromTime)
					?? DateTimeOffset.UtcNow.AddDays(-1);

		var to = CombineDateAndTime(ToDate, ToTime)
				?? DateTimeOffset.UtcNow;

		await foreach (var trade in _restApiConnector.GetCandleSeriesAsync(
							tradingPair,
							candlePeriodInSec,
							sort,
							from,
							to,
							amount))
			Candles.Add(trade);
	}

	[RelayCommand]
	private async Task SubscribeTradesAsync()
	{
		TradesWebSocket.Clear();
		
		_webSocketConnector.NewBuyTrade += OnWebSocketConnectorOnNewBuyTrade;
		_webSocketConnector.NewSellTrade += OnWebSocketConnectorOnNewSellTrade;

		await _webSocketConnector.SubscribeTrades(tradingPair);
	}

	[RelayCommand]
	private async Task UnsubscribeTradesAsync()
	{
		_webSocketConnector.NewBuyTrade -= OnWebSocketConnectorOnNewBuyTrade;
		_webSocketConnector.NewSellTrade -= OnWebSocketConnectorOnNewSellTrade;
		
		await _webSocketConnector.UnsubscribeTrades(tradingPair);
	}
	
	private void OnWebSocketConnectorOnNewSellTrade(Trade trade)
	{
		TradesWebSocket.Insert(0, trade);
	}
	
	private void OnWebSocketConnectorOnNewBuyTrade(Trade trade)
	{
		TradesWebSocket.Insert(0, trade);
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