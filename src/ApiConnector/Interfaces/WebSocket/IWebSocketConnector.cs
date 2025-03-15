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

	// remove the field maxcount since it is not used anywhere
	Task SubscribeTrades(string pair);
	Task UnsubscribeTrades(string pair);
}