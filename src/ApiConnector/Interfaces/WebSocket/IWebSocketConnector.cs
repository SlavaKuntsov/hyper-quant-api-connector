using ApiConnector.Models;

namespace ApiConnector.Interfaces.WebSocket;

public interface IWebSocketConnector
{
	event Action<Candle> CandleSeriesProcessing;

	void SubscribeCandles(
		string pair,
		int periodInSec,
		int? sort = null,
		DateTimeOffset? from = null,
		DateTimeOffset? to = null,
		long? count = null);
	void UnsubscribeCandles(string pair);


	event Action<Trade> NewBuyTrade;
	event Action<Trade> NewSellTrade;

	Task SubscribeTrades(string pair, int maxCount = 100);
	Task UnsubscribeTrades(string pair);
}