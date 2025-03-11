using ApiConnector.Models;

namespace ApiConnector.Interfaces.Rest;

public interface IRestApiConnector
{
	Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount);
	Task<IEnumerable<Candle>> GetCandleSeriesAsync(
		string pair,
		int periodInSec,
		DateTimeOffset? from,
		DateTimeOffset? to = null,
		long? count = 0);
}