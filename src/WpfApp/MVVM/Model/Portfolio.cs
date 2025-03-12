namespace WpfApp.MVVM.Model;

/// <summary>
///     Портфель
/// </summary>
public class Portfolio
{
	public string Cryptocurrency { get; set; }
	public decimal Amount { get; set; }
	public string Currency { get; set; }
	public decimal? ConvertedValue { get; set; }
}