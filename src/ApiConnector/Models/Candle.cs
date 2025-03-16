namespace ApiConnector.Models;

public class Candle
{
	public Candle(
		string pair,
		string messageType,
		decimal openPrice,
		decimal closePrice,
		decimal highPrice,
		decimal lowPrice,
		decimal totalPrice,
		decimal totalVolume,
		DateTimeOffset openTime)
	{
		Pair = pair;
		MessageType = messageType;
		OpenPrice = openPrice;
		ClosePrice = closePrice;
		HighPrice = highPrice;
		LowPrice = lowPrice;
		TotalPrice = totalPrice;
		TotalVolume = totalVolume;
		OpenTime = openTime;
	}
	
	public Candle(
		string pair,
		decimal openPrice,
		decimal closePrice,
		decimal highPrice,
		decimal lowPrice,
		decimal totalPrice,
		decimal totalVolume,
		DateTimeOffset openTime)
	{
		Pair = pair;
		OpenPrice = openPrice;
		ClosePrice = closePrice;
		HighPrice = highPrice;
		LowPrice = lowPrice;
		TotalPrice = totalPrice;
		TotalVolume = totalVolume;
		OpenTime = openTime;
	}

	/// <summary>
	///     Валютная пара
	/// </summary>
	public string Pair { get; set; }

	/// <summary>
	///     Цена открытия
	/// </summary>
	public decimal OpenPrice { get; set; }

	/// <summary>
	///     Максимальная цена
	/// </summary>
	public decimal HighPrice { get; set; }

	/// <summary>
	///     Минимальная цена
	/// </summary>
	public decimal LowPrice { get; set; }

	/// <summary>
	///     Цена закрытия
	/// </summary>
	public decimal ClosePrice { get; set; }


	/// <summary>
	///     Partial (Общая сумма сделок)
	/// </summary>
	public decimal TotalPrice { get; set; }

	/// <summary>
	///     Partial (Общий объем)
	/// </summary>
	public decimal TotalVolume { get; set; }

	/// <summary>
	///     Время
	/// </summary>
	public DateTimeOffset OpenTime { get; set; }

	// Поле добавленно для отображения типа WebSocket сообщения
	/// <summary>
	///     Тип WebSocket сообщения
	/// </summary>
	public string MessageType { get; set; } = string.Empty;

	public override string ToString()
	{
		return @$"{nameof(Pair)} - {Pair}
	{nameof(OpenPrice)}: {OpenPrice}
	{nameof(HighPrice)}: {HighPrice}
	{nameof(LowPrice)}: {LowPrice}
	{nameof(ClosePrice)}: {ClosePrice}
	{nameof(TotalPrice)}: {TotalPrice}
	{nameof(TotalVolume)}: {TotalVolume}
	{nameof(OpenTime)}: {OpenTime}";
	}
}