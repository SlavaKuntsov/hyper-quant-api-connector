using System.Windows;
using WpfApp.MVVM.ViewModel;

namespace WpfApp.MVVM.View;

/// <summary>
///     Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	public MainWindow(MainViewModel viewModel)
	{
		DataContext = viewModel;
		InitializeComponent();
	}
}