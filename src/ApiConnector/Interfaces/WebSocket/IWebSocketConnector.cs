using ApiConnector.Models;

namespace ApiConnector.Interfaces.WebSocket;

public interface IWebSocketConnector
{
	event Action<Candle> CandleSeriesProcessing;

	Task SubscribeCandles(string pair, int periodInSec);
	Task UnsubscribeCandles();

	event Action<Trade> NewBuyTrade;
	event Action<Trade> NewSellTrade;

	// remove the field maxcount since it is not used anywhere
	Task SubscribeTrades(string pair);
	Task UnsubscribeTrades();
}