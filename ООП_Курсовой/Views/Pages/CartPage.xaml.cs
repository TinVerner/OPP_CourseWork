using System.Windows;
using ООП_Курсовой.ViewModels;
using ООП_Курсовой.Views;

namespace ООП_Курсовой.Views.Pages;

public partial class CartPage : System.Windows.Controls.UserControl
{
    private CartWindowViewModel? _viewModel;

    public CartPage()
    {
        InitializeComponent();
        Loaded += CartPage_Loaded;
    }

    private void CartPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            _viewModel = new CartWindowViewModel();
            
            var mainWindow = Application.Current.MainWindow as MainWindowView;
            if (mainWindow != null && mainWindow.DataContext is MainWindowViewModel mainVm)
            {
                _viewModel.OpenHomeRequested = () => mainVm.GoHomeCommand.Execute(null);
                _viewModel.OpenMenuRequested = () => mainVm.GoMenuRollsCommand.Execute(null);
                _viewModel.OpenCustomRollsRequested = () => mainVm.GoCustomRollsCommand.Execute(null);
                _viewModel.OpenCustomSetsRequested = () => mainVm.GoCustomSetsCommand.Execute(null);
                _viewModel.OpenProfileRequested = () => mainVm.GoProfileCommand.Execute(null);
                _viewModel.OpenOrdersRequested = () => mainVm.GoOrdersCommand.Execute(null);
                _viewModel.OpenAdminPanelRequested = () => mainVm.GoAdminPanelCommand.Execute(null);
            }
            
            _viewModel.OpenCheckoutRequested = () => 
            { 
                var checkoutWindow = new CheckoutWindowView { Owner = Application.Current.MainWindow };
                var result = checkoutWindow.ShowDialog();
                
                if (result == true || _viewModel.Items.Count == 0)
                {
                    _viewModel.LoadData();
                }
            };
            
            DataContext = _viewModel;
            _viewModel.LoadData();
        }
    }
}

