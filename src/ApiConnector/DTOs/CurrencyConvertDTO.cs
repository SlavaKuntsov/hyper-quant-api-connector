using System.Text.Json.Serialization;

namespace ApiConnector.DTOs;

public class CurrencyConvertDTO
{
	[JsonPropertyName("status")]
	public string Status { get; set; }

	[JsonPropertyName("value_coin")]
	public string ValueCoin { get; set; }

	[JsonPropertyName("exchange_rate")]
	public string ExchangeRate { get; set; }

	[JsonPropertyName("error")]
	public string Error { get; set; }
}