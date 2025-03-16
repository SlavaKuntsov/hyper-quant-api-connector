namespace ApiConnector.DTOs;

public class RequestMessageDTO
{
	public string @event { get; set; }
	public string channel { get; set; }
	public string chanId { get; set; }
	public string symbol { get; set; }
	public string key { get; set; }

	public override string ToString()
	{
		return $@"{nameof(@event)} - {@event},
	{nameof(channel)} - {channel},
	{nameof(chanId)} - {chanId},
	{nameof(key)} - {key},
	{nameof(symbol)} - {symbol}";
	}
}