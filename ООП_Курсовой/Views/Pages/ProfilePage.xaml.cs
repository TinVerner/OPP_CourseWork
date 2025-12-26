using System.Windows;
using ООП_Курсовой.ViewModels;

namespace ООП_Курсовой.Views.Pages;

public partial class ProfilePage : System.Windows.Controls.UserControl
{
    private ProfileViewModel? _viewModel;

    public ProfilePage()
    {
        InitializeComponent();
        Loaded += ProfilePage_Loaded;
    }

    private async void ProfilePage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            _viewModel = new ProfileViewModel();
            
            var mainWindow = Application.Current.MainWindow as MainWindowView;
            if (mainWindow != null && mainWindow.DataContext is MainWindowViewModel mainVm)
            {
                _viewModel.OpenHomeRequested = () => mainVm.GoHomeCommand.Execute(null);
                _viewModel.OpenMenuRollsRequested = () => mainVm.GoMenuRollsCommand.Execute(null);
                _viewModel.OpenMenuSetsRequested = () => mainVm.GoMenuSetsCommand.Execute(null);
                _viewModel.OpenCustomRollsRequested = () => mainVm.GoCustomRollsCommand.Execute(null);
                _viewModel.OpenCustomSetsRequested = () => mainVm.GoCustomSetsCommand.Execute(null);
                _viewModel.OpenProfileRequested = () => mainVm.GoProfileCommand.Execute(null);
                _viewModel.OpenAdminPanelRequested = () => mainVm.GoAdminPanelCommand.Execute(null);
                _viewModel.OpenOrdersRequested = () => mainVm.GoOrdersCommand.Execute(null);
            }
            
            _viewModel.OpenCreateRollRequested = () =>
            {
                if (MainWindowView.NavigationService != null)
                {
                    MainWindowView.NavigationService.NavigateTo("CreateCustomRoll");
                }
            };
            
            _viewModel.OpenCreateSetRequested = () =>
            {
                if (MainWindowView.NavigationService != null)
                {
                    MainWindowView.NavigationService.NavigateTo("CreateCustomSet");
                }
            };
            
            _viewModel.OpenUsersRequested = () =>
            {
                var adminWindow = Application.Current.Windows.OfType<AdminWindowView>().FirstOrDefault();
                if (adminWindow != null && adminWindow.DataContext is AdminWindowViewModel adminVm)
                {
                    adminVm.ViewUsersCommand.Execute(null);
                }
            };
            
            _viewModel.OpenMenuItemsRequested = () =>
            {
                var adminWindow = Application.Current.Windows.OfType<AdminWindowView>().FirstOrDefault();
                if (adminWindow != null && adminWindow.DataContext is AdminWindowViewModel adminVm)
                {
                    adminVm.ViewMenuItemsCommand.Execute(null);
                }
            };
            
            DataContext = _viewModel;
            await _viewModel.LoadAsync();
            
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
        }
    }
}

