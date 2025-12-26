using System.Windows;
using ООП_Курсовой.ViewModels;

namespace ООП_Курсовой.Views.Pages;

public partial class CustomRollsPage : System.Windows.Controls.UserControl
{
    private CustomRollsWindowViewModel? _viewModel;

    public CustomRollsPage()
    {
        InitializeComponent();
        Loaded += CustomRollsPage_Loaded;
    }

    private async void CustomRollsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            _viewModel = new CustomRollsWindowViewModel();
            
            var mainWindow = Application.Current.MainWindow as MainWindowView;
            if (mainWindow != null && mainWindow.DataContext is MainWindowViewModel mainVm)
            {
                _viewModel.OpenHomeRequested = () => mainVm.GoHomeCommand.Execute(null);
                _viewModel.OpenMenuRollsRequested = () => mainVm.GoMenuRollsCommand.Execute(null);
                _viewModel.OpenMenuSetsRequested = () => mainVm.GoMenuSetsCommand.Execute(null);
                _viewModel.OpenCustomSetsRequested = () => mainVm.GoCustomSetsCommand.Execute(null);
                _viewModel.OpenProfileRequested = () => mainVm.GoProfileCommand.Execute(null);
                _viewModel.OpenCartRequested = () => mainVm.GoCartCommand.Execute(null);
                _viewModel.OpenOrdersRequested = () => mainVm.GoOrdersCommand.Execute(null);
                _viewModel.OpenAdminPanelRequested = () => mainVm.GoAdminPanelCommand.Execute(null);
            }
            
            _viewModel.OpenCreateRollRequested = () =>
            {
                if (MainWindowView.NavigationService != null)
                {
                    MainWindowView.NavigationService.NavigateTo("CreateCustomRoll");
                }
            };
            
            _viewModel.OpenDetailsRequested = (roll) =>
            {
                var detailsWindow = new MenuDetailsWindowView(roll) { Owner = Application.Current.MainWindow };
                detailsWindow.ShowDialog();
            };
            
            DataContext = _viewModel;
        }

        await _viewModel.LoadDataAsync();
    }
}
