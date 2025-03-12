using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ApiConnector.Interfaces.WebSocket;
using ApiConnector.Models;

namespace ApiConnector.ApiConnectors;

public class WebSocketConnector : IWebSocketConnector
{
	private readonly string _baseUrl;

	private readonly ClientWebSocket _webSocketClient;

	private CancellationTokenSource _receiveCancellationTokenSource;

	public WebSocketConnector(string baseUrl)
	{
		_baseUrl = baseUrl;
		_webSocketClient = new ClientWebSocket();
	}

	public event Action<Candle> CandleSeriesProcessing;
	public event Action<Trade> NewBuyTrade;
	public event Action<Trade> NewSellTrade;

	public void SubscribeCandles(
		string pair,
		int periodInSec,
		int? sort = null,
		DateTimeOffset? from = null,
		DateTimeOffset? to = null,
		long? count = null)
	{
		var periodInMinutes = periodInSec / 60;

		if (sort != 1 && sort != -1 && sort != null)
			throw new ArgumentException("Sort must be 1(asc) or -1(desc).");

		if (from > to)
			throw new ArgumentException("From must be less than or equal to To.");


		throw new NotImplementedException();
	}

	public void UnsubscribeCandles(string pair)
	{
		throw new NotImplementedException();
	}

	public async Task SubscribeTrades(string pair, int maxCount = 100)
	{
		if (_webSocketClient.State == WebSocketState.Open)
			return;

		await _webSocketClient.ConnectAsync(
			new Uri(_baseUrl),
			CancellationToken.None);

		var reqMessage = new RequestMessage
		{
			@event = "subscribe",
			channel = "trades",
			symbol = $"t{pair}"
		};

		Console.WriteLine(reqMessage.ToString());

		try
		{
			var jso1n = JsonSerializer.Serialize(reqMessage);
			Console.WriteLine(jso1n);
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

		_receiveCancellationTokenSource = new CancellationTokenSource();

		_ = ReceiveMessagesAsync(_receiveCancellationTokenSource.Token);
	}

	public async Task UnsubscribeTrades(string pair)
	{
		if (_webSocketClient.State != WebSocketState.Open)
			return;

		await _receiveCancellationTokenSource?.CancelAsync();

		var reqMessage = new RequestMessage
		{
			@event = "unsubscribe",
			channel = "trades",
			symbol = $"t{pair}"
		};

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

	private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
	{
		while (_webSocketClient.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
			try
			{
				var buffer = new byte[1024 * 4]; // 1024 * 4 because it's MTU

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

				var trade = new Trade
				{
					Pair = "BTCUSD"
				};
				
				NewBuyTrade?.Invoke(trade);

				Console.WriteLine("конец");
			}
			catch (OperationCanceledException)
			{
				break;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);

				break;
			}
	}
}

public class RequestMessage
{
	public string @event { get; set; }
	public string channel { get; set; }
	public string symbol { get; set; }

	public override string ToString()
	{
		return $@"{nameof(@event)} - {@event},
	{nameof(channel)} - {channel},
	{nameof(symbol)} - {symbol}";
	}
}