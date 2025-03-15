namespace ApiConnector.Models;

public class Trade
{
	public Trade(
		string pair,
		string id,
		DateTimeOffset time,
		decimal amount,
		decimal price,
		string side)
	{
		Pair = pair;
		Id = id;
		Time = time;
		Amount = amount;
		Price = price;
		Side = side;
	}

	public Trade(
		string pair,
		string id,
		string messageType,
		DateTimeOffset time,
		decimal amount,
		decimal price)
	{
		Id = id;
		MessageType = messageType;
		Pair = pair;
		Time = time;
		Amount = amount;
		Price = price;
	}

	/// <summary>
	///     Валютная пара
	/// </summary>
	public string Pair { get; set; }

	/// <summary>
	///     Цена трейда
	/// </summary>
	public decimal? Price { get; set; }

	/// <summary>
	///     Объем трейда
	/// </summary>
	public decimal? Amount { get; set; }

	/// <summary>
	///     Направление (buy/sell)
	/// </summary>
	public string Side { get; set; }

	/// <summary>
	///     Время трейда
	/// </summary>
	public DateTimeOffset Time { get; set; }

	// Поле добавленно для отображения типа WebSocket сообщения
	/// <summary>
	///     Тип WebSocket сообщения
	/// </summary>
	public string MessageType { get; set; } = string.Empty;

	/// <summary>
	///     Id трейда
	/// </summary>
	public string Id { get; set; }

	public override string ToString()
	{
		return @$"{nameof(Pair)} = {Pair}, 
	{nameof(MessageType)} = {MessageType}, 
	{nameof(Price)} = {Price}, 
	{nameof(Amount)} = {Amount}, 
	{nameof(Side)} = {Side}, 
	{nameof(Time)} = {Time}, 
	{nameof(Id)} = {Id}";
	}
}