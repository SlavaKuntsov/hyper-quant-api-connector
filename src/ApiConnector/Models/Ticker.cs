namespace ApiConnector.Models;

public class Ticker
{
	/// <summary>
	///     Валютная пара
	/// </summary>
	public string Pair { get; set; }

	/// <summary>
	///     Цена последнего наивысшего бида
	/// </summary>
	public decimal BidPrice { get; set; }

	/// <summary>
	///     Сумма 25 наивысших бид-размеров
	/// </summary>
	public decimal BidSize { get; set; }

	/// <summary>
	///     Цена последнего наименьшего аска
	/// </summary>
	public decimal AskPrice { get; set; }

	/// <summary>
	///     Сумма 25 наименьших аск-размеров
	/// </summary>
	public decimal AskSize { get; set; }

	/// <summary>
	///     Изменение цены с вчерашнего дня
	/// </summary>
	public decimal DailyChange { get; set; }

	/// <summary>
	///     Относительное изменение цены с вчерашнего дня (в процентах)
	/// </summary>
	public decimal DailyChangeRelative { get; set; }

	/// <summary>
	///     Цена последней сделки
	/// </summary>
	public decimal LastPrice { get; set; }

	/// <summary>
	///     Дневной объем
	/// </summary>
	public decimal Volume { get; set; }

	/// <summary>
	///     Дневной максимум
	/// </summary>
	public decimal High { get; set; }

	/// <summary>
	///     Дневной минимум
	/// </summary>
	public decimal Low { get; set; }

	public override string ToString()
	{
		return @$"{nameof(Pair)}: {Pair}
    {nameof(BidPrice)}: {BidPrice}
    {nameof(BidSize)}: {BidSize}
    {nameof(AskPrice)}: {AskPrice}
    {nameof(AskSize)}: {AskSize}
    {nameof(DailyChange)}: {DailyChange}
    {nameof(DailyChangeRelative)}: {DailyChangeRelative}%
    {nameof(LastPrice)}: {LastPrice}
    {nameof(Volume)}: {Volume}
    {nameof(High)}: {High}
    {nameof(Low)}: {Low}";
	}
}