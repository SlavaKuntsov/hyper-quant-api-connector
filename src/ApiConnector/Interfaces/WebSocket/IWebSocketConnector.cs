using ApiConnector.Models;

namespace ApiConnector.Interfaces.WebSocket;

public interface IWebSocketConnector
{
	event Action<Candle> CandleSeriesProcessing;

	// all parameters except Pair and Period in sec have been deleted because websocket query don't use them
	Task SubscribeCandles(string pair, int periodInSec);
	Task UnsubscribeCandles();

	event Action<Trade> NewBuyTrade;
	event Action<Trade> NewSellTrade;

	// remove the parameter maxCount since it is not used anywhere
	Task SubscribeTrades(string pair);
	Task UnsubscribeTrades();
}