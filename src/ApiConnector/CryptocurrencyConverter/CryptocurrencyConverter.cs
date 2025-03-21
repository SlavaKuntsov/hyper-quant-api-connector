﻿using System.Globalization;
using System.Net;
using System.Text.Json;
using ApiConnector.DTOs;
using ApiConnector.Models;

namespace ApiConnector.CryptocurrencyConverter;

public class CryptocurrencyConverter(HttpClient httpClient) : ICryptocurrencyConverter
{
	private readonly HttpClient _httpClient = httpClient;

	public async Task<decimal> Convert(Cryptocurrency cryptocurrency, string to)
	{
		var res = await _httpClient
			.GetAsync($"{to}/convert?value={cryptocurrency.Amount}&from={cryptocurrency.Currency}");

		var content = await res.Content.ReadAsStringAsync();

		var currency = JsonSerializer.Deserialize<CurrencyConvertDTO>(content);

		if (res.StatusCode == (HttpStatusCode)500 || res.StatusCode == (HttpStatusCode)400)
			throw new HttpRequestException($"Error: {res.StatusCode} - {currency.Error}.");

		// use cultures to avoid exception
		if (!decimal.TryParse(
				currency.ValueCoin,
				NumberStyles.Any,
				CultureInfo.InvariantCulture,
				out var value))
			throw new JsonException("Failed to parse 'value_coin' as decimal.");

		return value;
	}
}