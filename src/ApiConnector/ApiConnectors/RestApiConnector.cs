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

	// change return type from Task<IEnumerable<Candle>> to IAsyncEnumerable<Candle> 
	public async IAsyncEnumerable<Candle> GetCandleSeriesAsync(
		string pair,
		int periodInSec,
		int? sort = null,
		DateTimeOffset? from = null,
		DateTimeOffset? to = null,
		long? count = 0) // change default value from 0 to null 
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
		{
			var errorJson = await res.Content.ReadAsStringAsync();

			throw new HttpRequestException($"Error: {errorJson}");
		}

		var candlesJson = await ConvertToJsonArrayFromResponseAsync(res);

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

			yield return newCandle;
		}
	}

	public async IAsyncEnumerable<Trade> GetNewTradesAsync(string pair, int maxCount)
	{
		var res = await _httpClient
			.GetAsync($"v2/trades/t{pair}/hist?limit={maxCount}");

		if (res.StatusCode == (HttpStatusCode)500)
		{
			var errorJson = await res.Content.ReadAsStringAsync();

			throw new HttpRequestException($"Error: {errorJson}");
		}

		var tradesJson = await ConvertToJsonArrayFromResponseAsync(res);

		foreach (var tradeData in tradesJson)
		{
			var trade = new Trade
			{
				Pair = pair,
				Id = tradeData[0].GetInt64().ToString(),
				Time = DateTimeOffset.FromUnixTimeMilliseconds(tradeData[1].GetInt64()),
				Amount = tradeData[2].GetDecimal(),
				Price = tradeData[3].GetDecimal(),
				Side = tradeData[2].GetDecimal() > 0 ? "buy" : "sell"
			};

			yield return trade;
		}
	}

	public async Task<Ticker> GetTickerAsync(string pair)
	{
		var res = await _httpClient
			.GetAsync($"v2/ticker/t{pair}");

		if (res.StatusCode == (HttpStatusCode)500)
		{
			var errorJson = await res.Content.ReadAsStringAsync();

			throw new HttpRequestException($"Error: {errorJson}");
		}

		var tickerJson = await ConvertToDecimalFromResponseAsync(res);

		var ticker = new Ticker
		{
			Pair = pair,
			BidPrice = tickerJson[0],
			BidSize = tickerJson[1],
			AskPrice = tickerJson[2],
			AskSize = tickerJson[3],
			DailyChange = tickerJson[4],
			DailyChangeRelative = tickerJson[5],
			LastPrice = tickerJson[6],
			Volume = tickerJson[7],
			High = tickerJson[8],
			Low = tickerJson[9]
		};

		return ticker;
	}

	private static async Task<decimal[]> ConvertToDecimalFromResponseAsync(HttpResponseMessage res)
	{
		var content = await res.Content.ReadAsStringAsync();

		return JsonSerializer.Deserialize<decimal[]>(content);
	}

	private static async Task<IEnumerable<JsonElement[]>> ConvertToJsonArrayFromResponseAsync(HttpResponseMessage res)
	{
		var content = await res.Content.ReadAsStringAsync();

		// System.Text.Json.JsonException: The JSON value could not be converted to System.Int64
		// so we need to use JsonElement
		return JsonSerializer.Deserialize<IEnumerable<JsonElement[]>>(content);
	}
}