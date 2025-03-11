using System.Text.Json;
using ApiConnector.Interfaces.Rest;
using ApiConnector.Models;

namespace ApiConnector.ApiConnectrors;

public class RestApiConnector(HttpClient httpClient)
	: ApiConnector, IRestApiConnector
{
	private readonly HttpClient _httpClient = httpClient;

	public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(
		string pair,
		int periodInSec,
		DateTimeOffset? from,
		DateTimeOffset? to = null,
		long? count = 0)
	{
		var res = await _httpClient
			.GetAsync($"candles/trade:{periodInSec}m:{pair}/hist");

		if (!res.IsSuccessStatusCode)
			throw new HttpRequestException($"Error: {res.StatusCode}");

		return null;
	}

	public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
	{
		var res = await _httpClient
			.GetAsync($"v2/trades/t{pair}/hist?limit={maxCount}");

		var content = await res.Content.ReadAsStringAsync();

		// System.Text.Json.JsonException: The JSON value could not be converted to System.Int64
		// so we need to use JsonElement
		var tradesJson = JsonSerializer.Deserialize<IEnumerable<JsonElement[]>>(content);

		// use GetInt64 because we get numbers that exceed the int type limit
		var trades = tradesJson.Select(tradeData => new Trade
		{
			Pair = pair,
			Id = tradeData[0].GetInt64().ToString(),
			Time = DateTimeOffset.FromUnixTimeMilliseconds(tradeData[1]
				.GetInt64()),
			Amount = tradeData[2].GetDecimal(),
			Price = tradeData[3].GetDecimal(),
			Side = tradeData[2].GetDecimal() > 0 ? "buy" : "sell"
		});
		// side - How much was bought (positive) or sold (negative)

		foreach (var trade in trades)
			Console.WriteLine(trade.ToString());

		return trades;
	}

	public Task<IEnumerable<Trade>> GetTradesAsync(string pair)
	{
		throw new NotImplementedException();
	}
}