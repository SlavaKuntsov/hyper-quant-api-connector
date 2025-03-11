namespace ApiConnector.Models;

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