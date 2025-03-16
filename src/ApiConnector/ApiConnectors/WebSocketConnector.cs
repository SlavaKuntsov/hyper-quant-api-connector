using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ApiConnector.DTOs;
using ApiConnector.Interfaces.WebSocket;
using ApiConnector.Models;

namespace ApiConnector.ApiConnectors;

public class WebSocketConnector : IWebSocketConnector, IDisposable
{
	private static string _candlePair;

	private static string _tradePair;

	private static string _candleChanId;

	private static string _tradeChanId;

	private readonly string _baseUrl;

	private readonly ClientWebSocket _webSocketClient;

	private bool _isFirstTrade = true;

	private CancellationTokenSource _receiveCancellationTokenSource;

	public WebSocketConnector(string baseUrl)
	{
		_baseUrl = baseUrl;
		_webSocketClient = new ClientWebSocket();
	}

	public void Dispose()
	{
		_webSocketClient.Dispose();
		_receiveCancellationTokenSource.Dispose();
	}

	public event Action<Candle> CandleSeriesProcessing;
	public event Action<Trade> NewBuyTrade;
	public event Action<Trade> NewSellTrade;

	// all parameters except Pair and Period in sec have been deleted because websocket qeury dont use them
	public async Task SubscribeCandles(string pair, int periodInSec)
	{
		var periodInMinutes = periodInSec / 60;

		if (_webSocketClient.State == WebSocketState.Open)
			return;

		await _webSocketClient.ConnectAsync(
			new Uri(_baseUrl),
			CancellationToken.None);

		var reqMessage = new RequestMessageDTO
		{
			@event = "subscribe",
			channel = "candles",
			key = $"trade:{periodInMinutes}m:t{pair}"
		};

		Console.WriteLine(reqMessage);

		_candlePair = pair;

		await SendMessageAsync(reqMessage);

		_receiveCancellationTokenSource = new CancellationTokenSource();

		_ = ReceiveMessagesAsync(
			ProcessCandleResponseMessage,
			_receiveCancellationTokenSource.Token);
	}

	public async Task UnsubscribeCandles()
	{
		if (_webSocketClient.State != WebSocketState.Open)
			return;

		if (_receiveCancellationTokenSource != null)
			await _receiveCancellationTokenSource.CancelAsync();

		var reqMessage = new RequestMessageDTO
		{
			@event = "unsubscribe",
			chanId = _candleChanId
		};

		await SendMessageAsync(reqMessage);
	}

	public async Task SubscribeTrades(string pair)
	{
		if (_webSocketClient.State == WebSocketState.Open)
			return;

		await _webSocketClient.ConnectAsync(
			new Uri(_baseUrl),
			CancellationToken.None);

		var reqMessage = new RequestMessageDTO
		{
			@event = "subscribe",
			channel = "trades",
			symbol = $"t{pair}"
		};

		_tradePair = pair;

		await SendMessageAsync(reqMessage);

		_receiveCancellationTokenSource = new CancellationTokenSource();

		_ = ReceiveMessagesAsync(
			ProcessTradeResponseMessage,
			_receiveCancellationTokenSource.Token);
	}

	public async Task UnsubscribeTrades()
	{
		if (_webSocketClient.State != WebSocketState.Open)
			return;

		if (_receiveCancellationTokenSource != null)
			await _receiveCancellationTokenSource.CancelAsync();

		var reqMessage = new RequestMessageDTO
		{
			@event = "unsubscribe",
			chanId = _tradeChanId
		};

		await SendMessageAsync(reqMessage);
	}

	private async Task SendMessageAsync(RequestMessageDTO reqMessage)
	{
		try
		{
			var json = JsonSerializer.SerializeToUtf8Bytes(reqMessage);

			var buffer = new ReadOnlyMemory<byte>(json);

			await _webSocketClient.SendAsync(
				buffer,
				WebSocketMessageType.Text,
				true,
				CancellationToken.None);
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}

	private async Task ReceiveMessagesAsync(Func<string, Task> processFunc, CancellationToken cancellationToken)
	{
		var buffer = new byte[1024 * 4]; // 1024 * 4 because it's MTU

		while (_webSocketClient.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
			try
			{
				WebSocketReceiveResult res;

				var fullMessage = new StringBuilder();

				// adding a loop for connecting message chunks
				do
				{
					res = await _webSocketClient.ReceiveAsync(
						new ArraySegment<byte>(buffer),
						cancellationToken);

					if (res.MessageType != WebSocketMessageType.Close)
					{
						var resMessage = Encoding.UTF8.GetString(buffer, 0, res.Count);

						fullMessage.Append(resMessage);

						continue;
					}

					await _webSocketClient.CloseAsync(
						WebSocketCloseStatus.NormalClosure,
						"close",
						cancellationToken);
				} while (!res.EndOfMessage);

				processFunc(fullMessage.ToString());
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);

				break;
			}
	}

	private async Task ProcessCandleResponseMessage(string message)
	{
		using var jsonDocument = JsonDocument.Parse(message);

		var jsonElement = jsonDocument.RootElement;

		if (jsonElement.ValueKind == JsonValueKind.Object)
		{
			if (jsonElement.TryGetProperty("chanId", out var chanIdValue))
				_candleChanId = chanIdValue.ToString();

			return;
		}

		Candle candle;

		if (_isFirstTrade)
		{
			foreach (var candleData in jsonElement[1].EnumerateArray())
			{
				var highPrice = candleData[3].GetDecimal();
				var lowPrice = candleData[4].GetDecimal();
				var volume = candleData[5].GetDecimal();

				var averagePrice = (highPrice + lowPrice) / 2;

				var totalPrice = averagePrice * volume;

				candle = new Candle(
					_candlePair,
					"snapshot",
					candleData[1].GetDecimal(),
					candleData[2].GetDecimal(),
					highPrice,
					lowPrice,
					totalPrice,
					volume,
					DateTimeOffset.FromUnixTimeMilliseconds(candleData[0].GetInt64())
				);

				CandleSeriesProcessing?.Invoke(candle);
			}

			_isFirstTrade = false;
		}
		else
		{
			if (jsonElement[1].ToString() == "hb")
				return;

			var candleData = jsonElement[1];

			var highPrice = candleData[3].GetDecimal();
			var lowPrice = candleData[4].GetDecimal();
			var volume = candleData[5].GetDecimal();

			var averagePrice = (highPrice + lowPrice) / 2;

			var totalPrice = averagePrice * volume;

			candle = new Candle(
				_candlePair,
				"cu", // cu(candle update)
				candleData[1].GetDecimal(),
				candleData[2].GetDecimal(),
				highPrice,
				lowPrice,
				totalPrice,
				volume,
				DateTimeOffset.FromUnixTimeMilliseconds(candleData[0].GetInt64())
			);

			CandleSeriesProcessing?.Invoke(candle);
		}
	}

	private async Task ProcessTradeResponseMessage(string message)
	{
		using var jsonDocument = JsonDocument.Parse(message);

		var jsonElement = jsonDocument.RootElement;

		if (jsonElement.ValueKind == JsonValueKind.Object)
		{
			if (jsonElement.TryGetProperty("chanId", out var chanIdValue))
				_tradeChanId = chanIdValue.ToString();

			return;
		}

		Trade trade;

		if (_isFirstTrade)
		{
			foreach (var tradeData in jsonElement[1].EnumerateArray())
			{
				trade = new Trade(
					_tradePair,
					tradeData[0].ToString(),
					"snapshot",
					DateTimeOffset.FromUnixTimeMilliseconds(tradeData[1].GetInt64()),
					tradeData[2].GetDecimal(),
					tradeData[3].GetDecimal(),
					tradeData[2].GetDecimal() > 0 ? "buy" : "sell"
				);


				NewBuyTrade?.Invoke(trade);
			}

			_isFirstTrade = false;
		}
		else
		{
			if (jsonElement[1].ToString() == "hb")
				return;

			var tradeData = jsonElement[2];

			trade = new Trade(
				_tradePair,
				tradeData[0].ToString(),
				jsonElement[1].ToString(),
				DateTimeOffset.FromUnixTimeMilliseconds(tradeData[1].GetInt64()),
				tradeData[2].GetDecimal(),
				tradeData[3].GetDecimal(),
				tradeData[2].GetDecimal() > 0 ? "buy" : "sell"
			);

			if (tradeData[2].GetDecimal() > 0)
				NewBuyTrade?.Invoke(trade);
			else
				NewSellTrade?.Invoke(trade);
		}
	}
}