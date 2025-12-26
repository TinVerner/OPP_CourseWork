using System.Windows;
using ООП_Курсовой.ViewModels;

namespace ООП_Курсовой.Views.Pages;

public partial class CreateCustomRollPage : System.Windows.Controls.UserControl
{
    private CreateCustomRollWindowViewModel? _viewModel;

    public CreateCustomRollPage()
    {
        InitializeComponent();
        Loaded += CreateCustomRollPage_Loaded;
    }

    private void CreateCustomRollPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            _viewModel = new CreateCustomRollWindowViewModel();
            
            // Настройка навигации через AdminWindowView или MainWindowView
            var adminWindow = Application.Current.MainWindow as AdminWindowView;
            if (adminWindow != null && adminWindow.DataContext is AdminWindowViewModel adminVm)
            {
                _viewModel.OpenAdminPanelRequested = () => adminVm.AdminPanelCommand.Execute(null);
                _viewModel.OpenUsersRequested = () => adminVm.ViewUsersCommand.Execute(null);
                _viewModel.OpenMenuItemsRequested = () => adminVm.ViewMenuItemsCommand.Execute(null);
                _viewModel.OpenProfileRequested = () => adminVm.ProfileCommand.Execute(null);
                _viewModel.OpenCustomRollsRequested = NavigateToCustomRolls;
            }
            else
            {
                var mainWindow = Application.Current.MainWindow as MainWindowView;
                if (mainWindow != null && mainWindow.DataContext is MainWindowViewModel mainVm)
                {
                    _viewModel.OpenHomeRequested = () => mainVm.GoHomeCommand.Execute(null);
                    _viewModel.OpenMenuRequested = () => mainVm.GoMenuRollsCommand.Execute(null);
                    _viewModel.OpenCartRequested = () => mainVm.GoCartCommand.Execute(null);
                    _viewModel.OpenOrdersRequested = () => mainVm.GoOrdersCommand.Execute(null);
                    _viewModel.OpenCustomRollsRequested = NavigateToCustomRolls;
                    _viewModel.OpenCustomSetsRequested = () => mainVm.GoCustomSetsCommand.Execute(null);
                    _viewModel.OpenProfileRequested = () => mainVm.GoProfileCommand.Execute(null);
                    _viewModel.OpenAdminPanelRequested = () => mainVm.GoAdminPanelCommand.Execute(null);
                }
            }
            
            _viewModel.OpenCustomRollsRequested = () =>
            {
                if (MainWindowView.NavigationService != null)
                    MainWindowView.NavigationService.NavigateTo("CustomRolls");
            };

            _viewModel.OpenCreateRollRequested = () => 
            {
                if (AdminWindowView.NavigationService != null)
                {
                    AdminWindowView.NavigationService.NavigateTo("CreateCustomRoll");
                }
                else if (MainWindowView.NavigationService != null)
                {
                    MainWindowView.NavigationService.NavigateTo("CreateCustomRoll");
                }
            };
            _viewModel.OpenCreateSetRequested = () => 
            {
                if (AdminWindowView.NavigationService != null)
                {
                    AdminWindowView.NavigationService.NavigateTo("CreateCustomSet");
                }
                else if (MainWindowView.NavigationService != null)
                {
                    MainWindowView.NavigationService.NavigateTo("CreateCustomSet");
                }
            };
            
            DataContext = _viewModel;
        }
    }

    private void NavigateToCustomRolls()
    {
        if (MainWindowView.NavigationService != null)
        {
            MainWindowView.NavigationService.NavigateTo("CustomRolls");
            return;
        }

        var mainWindow = new MainWindowView();
        Application.Current.MainWindow = mainWindow;
        mainWindow.Show();
        
        mainWindow.Loaded += (s, e) =>
        {
            if (MainWindowView.NavigationService != null)
            {
                MainWindowView.NavigationService.NavigateTo("CustomRolls");
            }
        };
    }
}

