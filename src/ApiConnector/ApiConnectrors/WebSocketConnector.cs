using ApiConnector.Interfaces.WebSocket;
using ApiConnector.Models;

namespace ApiConnector.ApiConnectrors;

public class WebSocketConnector : IWebSocketConnector
{
	public event Action<Trade> NewBuyTrade;
	public event Action<Trade> NewSellTrade;
	public event Action<Candle> CandleSeriesProcessing;

	public void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0)
	{
		throw new NotImplementedException();
	}

	public void SubscribeTrades(string pair, int maxCount = 100)
	{
		throw new NotImplementedException();
	}

	public void UnsubscribeCandles(string pair)
	{
		throw new NotImplementedException();
	}

	public void UnsubscribeTrades(string pair)
	{
		throw new NotImplementedException();
	}
}