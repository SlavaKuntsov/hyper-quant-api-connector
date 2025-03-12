using ApiConnector.Models;

namespace ApiConnector.Interfaces.Rest;

public interface IRestApiConnector
{
	IAsyncEnumerable<Candle> GetCandleSeriesAsync(
		string pair,
		int periodInSec,
		int? sort = null,
		DateTimeOffset? from = null,
		DateTimeOffset? to = null,
		long? count = 0);

	IAsyncEnumerable<Trade> GetNewTradesAsync(string pair, int maxCount);

	Task<Ticker> GetTickerAsync(string pair);
}