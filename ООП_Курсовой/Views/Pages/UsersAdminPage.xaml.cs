using System.Windows;
using ООП_Курсовой.ViewModels;

namespace ООП_Курсовой.Views.Pages;

public partial class UsersAdminPage : System.Windows.Controls.UserControl
{
    private UsersAdminWindowViewModel? _viewModel;

    public UsersAdminPage()
    {
        InitializeComponent();
        Loaded += UsersAdminPage_Loaded;
    }

    private void UsersAdminPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            _viewModel = new UsersAdminWindowViewModel();
            
            var adminWindow = Application.Current.MainWindow as AdminWindowView;
            if (adminWindow != null && adminWindow.DataContext is AdminWindowViewModel adminVm)
            {
                _viewModel.OpenAdminPanelRequested = () => adminVm.AdminPanelCommand.Execute(null);
                _viewModel.OpenCreateRollRequested = () => adminVm.CreateRollCommand.Execute(null);
                _viewModel.OpenCreateSetRequested = () => adminVm.CreateSetCommand.Execute(null);
                _viewModel.OpenUsersRequested = () => adminVm.ViewUsersCommand.Execute(null);
                _viewModel.OpenMenuItemsRequested = () => adminVm.ViewMenuItemsCommand.Execute(null);
                _viewModel.OpenProfileRequested = () => adminVm.ProfileCommand.Execute(null);
            }
            
            _viewModel.OpenEditUserRequested = userId =>
            {
                var editWindow = new EditUserWindowView(userId) { Owner = Application.Current.MainWindow };
                editWindow.ShowDialog();
                _viewModel.LoadUsersCommand.Execute(null);
            };
            
            DataContext = _viewModel;
        }
    }
}

