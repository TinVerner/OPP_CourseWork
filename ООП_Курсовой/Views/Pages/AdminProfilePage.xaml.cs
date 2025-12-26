using System.Windows;
using ООП_Курсовой.ViewModels;

namespace ООП_Курсовой.Views.Pages;

public partial class AdminProfilePage : System.Windows.Controls.UserControl
{
    private ProfileViewModel? _viewModel;

    public AdminProfilePage()
    {
        InitializeComponent();
        Loaded += AdminProfilePage_Loaded;
    }

    private void AdminProfilePage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            _viewModel = new ProfileViewModel();
            
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
            
            CurrentPasswordBox.PasswordChanged += (_, __) =>
            {
                _viewModel.CurrentPassword = CurrentPasswordBox.Password;
            };

            NewPasswordBox.PasswordChanged += (_, __) =>
            {
                _viewModel.NewPassword = NewPasswordBox.Password;
            };

            ConfirmPasswordBox.PasswordChanged += (_, __) =>
            {
                _viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
            };
            
            DataContext = _viewModel;
            
            _ = _viewModel.LoadAsync();
        }
    }
}

