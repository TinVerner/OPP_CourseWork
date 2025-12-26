using System.Windows;
using ООП_Курсовой.ViewModels;

namespace ООП_Курсовой.Views.Pages;

public partial class AdminOrdersPage : System.Windows.Controls.UserControl
{
    private AdminOrdersWindowViewModel? _viewModel;

    public AdminOrdersPage()
    {
        InitializeComponent();
        Loaded += AdminOrdersPage_Loaded;
    }

    private void AdminOrdersPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            _viewModel = new AdminOrdersWindowViewModel();
            
            var adminWindow = Application.Current.MainWindow as AdminWindowView;
            if (adminWindow != null && adminWindow.DataContext is AdminWindowViewModel adminVm)
            {
                _viewModel.OpenAdminPanelRequested = () => adminVm.AdminPanelCommand.Execute(null);
            }
            
            _viewModel.OpenOrderDetailsRequested = (id) =>
            {
                var detailsWindow = new OrderDetailsWindowView(id) { Owner = Application.Current.MainWindow };
                detailsWindow.ShowDialog();
            };
            
            DataContext = _viewModel;
        }
    }
}

