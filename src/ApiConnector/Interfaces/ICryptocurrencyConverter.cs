using ApiConnector.Models;

namespace ApiConnector.CryptocurrencyConverter;

public interface ICryptocurrencyConverter
{
	Task<decimal> Convert(Cryptocurrency cryptocurrency, string to);
}