# HyperQuant Api Connector
### Задача
1. Написать коннектор к API Bitfinex ([ссылка на API](https://docs.bitfinex.com/docs/introduction)) для запросов на:
	- Rest
	- WebSocket
2. Реализовать расчет баланса портфеля на балансе которого есть некоторые валюты.
### Реализация
Базовый интерфейс `ITestConnector` по *Interface Segregation Principle* был разделен на `IRestApiConnector` и `IWebSocketConnector` интерфейсы.

Регистрация коннекторов и конвертера в *DI*:
``` csharp
using Microsoft.Extensions.DependencyInjection;

services.AddHttpClient<IRestApiConnector, RestApiConnector>(client =>  
{  
    client.BaseAddress = new Uri("https://api-pub.bitfinex.com");  
});  
  
services.AddHttpClient<ICryptocurrencyConverter, CryptocurrencyConverter>(client =>  
{  
    client.BaseAddress = new Uri("https://api.cryptapi.io");  
});  
  
services.AddSingleton<IWebSocketConnector>(  
    new WebSocketConnector("wss://api-pub.bitfinex.com/ws/2"));
    
```

Для конверета валют был использован сервсис [CryptAPI](https://docs.cryptapi.io/#operation/convert).
## Models
Кроме `Candle` и `Trade` была добавлена модель `Ticker`.
В моделе `Trade` и `Candle` было добавлено поле `MessageType` для обозначения типа WebSocket сообщения (*"te" (trade executed), "tu" (trade updated), "fte" (funding trade executed), "ftu" (funding trade updated), "cu" (candle updated)*).
## API Connectors
### Rest
Методы, которые должны вернуть последовательность `IEnumerable`, возвращают значения через `yield return`, чтобы в некоторых случаях, когда возвращаются десятки тысяч обьектов, можно было избежать выделения памяти сразу под всю последовательность.

**Интерфейс**
``` csharp
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
```
**Использование**
``` csharp
await foreach (var trade in _restApiConnector.GetCandleSeriesAsync(
	tradingPair,  
	candlePeriodInSec,  
	null,  
	DateTimeOffset.UtcNow.AddHours(-1),  
	null,  
	amount))
{
    Candles.Add(trade);
}  
```
``` csharp
await foreach (var trade in _restApiConnector.GetNewTradesAsync(tradingPair, amount))  
{
    Trades.Add(trade);
}
```
``` csharp
var ticker = await _restApiConnector.GetTickerAsync(tradingPair);
```
### WebSocket
Т.к нужно реализовать только методы `SubscribeCandles` и `SubscribeTrades`, то методы по подкючению и откючению, отправке, получению и обработке сообщений должны быть реализованны уже внутри них. 

**Интерфейс**
``` csharp
public interface IWebSocketConnector  
{  
    event Action<Candle> CandleSeriesProcessing;  
  
    // all parameters except Pair and Period in sec have been deleted because websocket query don't use them  
    Task SubscribeCandles(string pair, int periodInSec);  
    Task UnsubscribeCandles();  
  
    event Action<Trade> NewBuyTrade;  
    event Action<Trade> NewSellTrade;  
  
    // remove the parameter maxCount since it is not used anywhere  
    Task SubscribeTrades(string pair);  
    Task UnsubscribeTrades();  
}
```
**Использование** (Wpf)
``` csharp
[RelayCommand]  
private async Task SubscribeCandlesAsync()  
{  
    CandlesWebSocket.Clear();  
  
    _webSocketConnector.CandleSeriesProcessing += OnWebSocketConnectorOnCandleProcessing;  
  
    await _webSocketConnector.SubscribeCandles(tradingPair, candlePeriodInSec);  
}  
  
[RelayCommand]  
private async Task UnsubscribeCandlesAsync()  
{  
    _webSocketConnector.CandleSeriesProcessing -= OnWebSocketConnectorOnCandleProcessing;  
  
    await _webSocketConnector.UnsubscribeCandles();  
}  
  
private void OnWebSocketConnectorOnCandleProcessing(Candle candle)  
{  
    if (candle.MessageType == "snapshot")  
       CandlesWebSocket.Add(candle);  
    else  
       CandlesWebSocket.Insert(0, candle);  
}
```
В функции `OnWebSocketConnectorOnCandleProcessing` есть условие для корректного вывода обьектов с сортировкой по дате (новые записи поялвяются вначале списка).
## Converter
Для реализации конвертера был создан класс `CryptocurrencyConverter` реализующий интерфес с методом `Convert`.
``` csharp
public interface ICryptocurrencyConverter  
{  
    Task<decimal> Convert(Cryptocurrency cryptocurrency, string to);  
}
```
**Использование**
``` csharp
var convertedValue = await _cryptocurrencyConverter.Convert(  
    new Cryptocurrency(portfolio.Cryptocurrency, portfolio.Amount),  
    portfolio.Currency);
```
В параметрах принимает обьект типа `Cryptocurrency` и валюту в которую нужно перевести.
``` csharp
public class Cryptocurrency  
{  
    public Cryptocurrency(string currency, decimal amount)  
    {  
       Currency = currency;  
       Amount = amount;  
    }  
  
    public string Currency { get; set; }  
    public decimal Amount { get; set; }  
}
```
#### P.S.
`ConsoleApp` в директории `src` просьба игнорировать (исользовался при разработке).