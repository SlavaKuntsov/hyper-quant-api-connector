using ApiConnector.Interfaces.Rest;
using ApiConnector.Models;

namespace ApiConnector.ApiConnectrors;

public class RestApiConnector : IRestApiConnector
{
	public Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
	{
		throw new NotImplementedException();
	}

	public Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
	{
		throw new NotImplementedException();
	}
}