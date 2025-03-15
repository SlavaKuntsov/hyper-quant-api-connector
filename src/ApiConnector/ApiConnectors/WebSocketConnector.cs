﻿using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using ApiConnector.DTOs;
using ApiConnector.Interfaces.WebSocket;
using ApiConnector.Models;

namespace ApiConnector.ApiConnectors;

public class WebSocketConnector : IWebSocketConnector, IDisposable
{
	private static string _tradePair;

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

		var reqMessage = new RequestMessageDTO
		{
			@event = "subscribe",
			channel = "trades",
			symbol = $"t{pair}"
		};

		_tradePair = pair;

		SendMessageAsync(reqMessage);

		_receiveCancellationTokenSource = new CancellationTokenSource();

		_ = ReceiveMessagesAsync(_receiveCancellationTokenSource.Token);
	}

	public async Task UnsubscribeTrades(string pair)
	{
		if (_webSocketClient.State != WebSocketState.Open)
			return;

		await _receiveCancellationTokenSource?.CancelAsync();

		var reqMessage = new RequestMessageDTO
		{
			@event = "unsubscribe",
			channel = "trades",
			symbol = $"t{pair}"
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

				ProcessResponseMessage(fullMessage.ToString());
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

	private void ProcessResponseMessage(string message)
	{
		var jsonElement = JsonDocument.Parse(message).RootElement;

		if (jsonElement.ValueKind == JsonValueKind.Object)
			return;

		var trade = default(Trade);

		if (_isFirstTrade)
		{
			Console.WriteLine("IT IS SNAPSHOT");

			foreach (var tradeData in jsonElement[1].EnumerateArray())
			{
				trade = new Trade(
					_tradePair,
					tradeData[0].ToString(),
					"snapshot",
					DateTimeOffset.FromUnixTimeMilliseconds(tradeData[1].GetInt64()),
					tradeData[2].GetDecimal(),
					tradeData[3].GetDecimal()
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
				tradeData[3].GetDecimal()
			);
			
			NewBuyTrade?.Invoke(trade);
		}
	}
}