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
В моделе `Trade` было добавлено поле `MessageType` для обозначения типа WebSocket сообщения (*"te" (trade executed), "tu" (trade updated), "fte" (funding trade executed), "ftu" (funding trade updated)*).
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