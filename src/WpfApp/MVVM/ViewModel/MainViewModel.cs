using System.Collections.ObjectModel;
using ApiConnector.CryptocurrencyConverter;
using ApiConnector.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WpfApp.MVVM.Model;

namespace WpfApp.MVVM.ViewModel;

public partial class MainViewModel : ObservableObject
{
	private readonly ICryptocurrencyConverter _cryptocurrencyConverter;

	[ObservableProperty] private ObservableCollection<Portfolio> portfolios = new()
	{
		new Portfolio { Cryptocurrency = "BTC", Amount = 1, Currency = "USD" },
		new Portfolio { Cryptocurrency = "USD", Amount = 100_000, Currency = "BTC" },
		new Portfolio { Cryptocurrency = "XRP", Amount = 15000, Currency = "USD" },
		// new Portfolio { Cryptocurrency = "XMR", Amount = 50, Currency = "USD" }, // in converter api this coin not found
		new Portfolio { Cryptocurrency = "ADA", Amount = 30, Currency = "USD" },
		new Portfolio { Cryptocurrency = "ETH", Amount = 30, Currency = "USD" }
	};

	public MainViewModel(ICryptocurrencyConverter cryptocurrencyConverter)
	{
		_cryptocurrencyConverter = cryptocurrencyConverter;
	}

	[RelayCommand]
	private async Task ConvertPortfolioAsync()
	{
		foreach (var portfolio in Portfolios)
			portfolio.ConvertedValue = await _cryptocurrencyConverter.Convert(
				new Cryptocurrency(portfolio.Cryptocurrency, portfolio.Amount),
				portfolio.Currency);

		// Оповещаем UI об изменении коллекции (принудительно обновляем список)
		Portfolios = new ObservableCollection<Portfolio>(Portfolios);
	}
}