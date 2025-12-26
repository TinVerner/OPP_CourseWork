using System.Windows;
using ООП_Курсовой.ViewModels;

namespace ООП_Курсовой.Views.Pages;

public partial class CreateCustomSetPage : System.Windows.Controls.UserControl
{
    private CreateCustomSetWindowViewModel? _viewModel;

    public CreateCustomSetPage()
    {
        InitializeComponent();
        Loaded += CreateCustomSetPage_Loaded;
    }

    private void CreateCustomSetPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            _viewModel = new CreateCustomSetWindowViewModel();
            
            var adminWindow = Application.Current.MainWindow as AdminWindowView;
            if (adminWindow != null && adminWindow.DataContext is AdminWindowViewModel adminVm)
            {
                _viewModel.OpenAdminPanelRequested = () => adminVm.AdminPanelCommand.Execute(null);
                _viewModel.OpenUsersRequested = () => adminVm.ViewUsersCommand.Execute(null);
                _viewModel.OpenMenuItemsRequested = () => adminVm.ViewMenuItemsCommand.Execute(null);
                _viewModel.OpenProfileRequested = () => adminVm.ProfileCommand.Execute(null);
                _viewModel.OpenCustomSetsRequested = NavigateToCustomSets;
            }
            else
            {
                var mainWindow = Application.Current.MainWindow as MainWindowView;
                if (mainWindow != null && mainWindow.DataContext is MainWindowViewModel mainVm)
                {
                    _viewModel.OpenHomeRequested = () => mainVm.GoHomeCommand.Execute(null);
                    _viewModel.OpenMenuRequested = () => mainVm.GoMenuRollsCommand.Execute(null);
                    _viewModel.OpenCartRequested = () => mainVm.GoCartCommand.Execute(null);
                    _viewModel.OpenCustomRollsRequested = () => mainVm.GoCustomRollsCommand.Execute(null);
                    _viewModel.OpenCustomSetsRequested = NavigateToCustomSets;
                    _viewModel.OpenProfileRequested = () => mainVm.GoProfileCommand.Execute(null);
                    _viewModel.OpenAdminPanelRequested = () => mainVm.GoAdminPanelCommand.Execute(null);
                }
            }
            
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

    private void NavigateToCustomSets()
    {
        if (MainWindowView.NavigationService != null)
        {
            MainWindowView.NavigationService.NavigateTo("CustomSets");
            return;
        }

        var mainWindow = new MainWindowView();
        Application.Current.MainWindow = mainWindow;
        mainWindow.Show();
        
        mainWindow.Loaded += (s, e) =>
        {
            if (MainWindowView.NavigationService != null)
            {
                MainWindowView.NavigationService.NavigateTo("CustomSets");
            }
        };
    }
}

