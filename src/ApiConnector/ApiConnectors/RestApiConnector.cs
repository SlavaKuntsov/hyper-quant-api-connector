using System.Net;
using System.Text;
using System.Text.Json;
using ApiConnector.Interfaces.Rest;
using ApiConnector.Models;

namespace ApiConnector.ApiConnectors;

public class RestApiConnector(HttpClient httpClient)
	: ApiConnector, IRestApiConnector
{
	private readonly HttpClient _httpClient = httpClient;

	public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(
		string pair,
		int periodInSec,
		int? sort = null,
		DateTimeOffset? from = null,
		DateTimeOffset? to = null,
		long? count = 0) // change default value to null 
	{
		var periodInMinutes = periodInSec / 60;

		if (sort != 1 && sort != -1 && sort != null)
			throw new ArgumentException("Sort must be 1(asc) or -1(desc).");

		if (from > to)
			throw new ArgumentException("From must be less than or equal to To.");

		var query = new StringBuilder($"v2/candles/trade:{periodInMinutes}m:t{pair}/hist?");

		if (sort != null)
			query.Append($"&sort={sort}");

		if (from != null)
		{
			var fromInMilliseconds = from.Value.ToUnixTimeMilliseconds();
			query.Append($"&start={fromInMilliseconds}");
		}

		if (to != null)
		{
			var toInMilliseconds = to.Value.ToUnixTimeMilliseconds();
			query.Append($"&end={toInMilliseconds}");
		}

		if (count != null)
			query.Append($"&limit={count}");

		Console.WriteLine(query);

		var res = await _httpClient
			.GetAsync(query.ToString());

		if (res.StatusCode == (HttpStatusCode)500)
			throw new HttpRequestException($"Error: {res.StatusCode} - {res.ReasonPhrase}.");

		var candlesJson = await ConvertToJsonFromResponseAsync(res);

		var candles = new List<Candle>(candlesJson.Count());
		// or
		// var candleJson = default(IList<Candle>);

		// first variant of convert the json response to models
		foreach (var candle in candlesJson)
		{
			var highPrice = candle[3].GetDecimal();
			var lowPrice = candle[4].GetDecimal();
			var volume = candle[5].GetDecimal();

			var averagePrice = (highPrice + lowPrice) / 2;

			var totalPrice = averagePrice * volume;

			var newCandle = new Candle
			{
				Pair = pair,
				OpenPrice = candle[1].GetDecimal(),
				ClosePrice = candle[2].GetDecimal(),
				HighPrice = highPrice,
				LowPrice = lowPrice,
				TotalPrice = totalPrice,
				TotalVolume = volume,
				OpenTime = DateTimeOffset.FromUnixTimeMilliseconds(candle[0].GetInt64())
			};

			Console.WriteLine(newCandle.ToString());

			candles.Add(newCandle);
		}

		return null;
	}

	public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
	{
		var res = await _httpClient
			.GetAsync($"v2/trades/t{pair}/hist?limit={maxCount}");

		if (res.StatusCode == (HttpStatusCode)500)
			throw new HttpRequestException($"Error: {res.StatusCode} - {res.ReasonPhrase}.");

		var tradesJson = await ConvertToJsonFromResponseAsync(res);

		// second variant of convert the json response to models
		// use GetInt64 because we get numbers that exceed the int type limit
		var trades = tradesJson.Select(tradeData => new Trade
		{
			Pair = pair,
			Id = tradeData[0].GetInt64().ToString(),
			Time = DateTimeOffset.FromUnixTimeMilliseconds(tradeData[1]
				.GetInt64()),
			Amount = tradeData[2].GetDecimal(),
			Price = tradeData[3].GetDecimal(),
			Side = tradeData[2].GetDecimal() > 0
				? "buy"
				: "sell" // side - How much was bought (positive) or sold (negative)
		});


		foreach (var trade in trades)
			Console.WriteLine(trade.ToString());

		return trades;
	}

	public Task<IEnumerable<Trade>> GetTickerAsync(string pair)
	{
		throw new NotImplementedException();
	}

	private async Task<IEnumerable<JsonElement[]>> ConvertToJsonFromResponseAsync(HttpResponseMessage res)
	{
		var content = await res.Content.ReadAsStringAsync();

		// System.Text.Json.JsonException: The JSON value could not be converted to System.Int64
		// so we need to use JsonElement
		var tradesJson = JsonSerializer.Deserialize<IEnumerable<JsonElement[]>>(content);

		return tradesJson;
	}
}