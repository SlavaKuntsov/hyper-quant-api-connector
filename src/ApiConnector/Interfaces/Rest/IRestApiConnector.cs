using ApiConnector.Models;

namespace ApiConnector.Interfaces.Rest;

public interface IRestApiConnector
{
	Task<IEnumerable<Candle>> GetCandleSeriesAsync(
		string pair,
		int periodInSec,
		int? sort = null,
		DateTimeOffset? from = null,
		DateTimeOffset? to = null,
		long? count = 0);

	Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount);

	Task<Ticker> GetTickerAsync(string pair);
}