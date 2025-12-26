using System.Windows;
using ООП_Курсовой.ViewModels;

namespace ООП_Курсовой.Views.Pages;

public partial class CustomSetsPage : System.Windows.Controls.UserControl
{
    private CustomSetsWindowViewModel? _viewModel;

    public CustomSetsPage()
    {
        InitializeComponent();
        Loaded += CustomSetsPage_Loaded;
    }

    private async void CustomSetsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            _viewModel = new CustomSetsWindowViewModel();
            
            var mainWindow = Application.Current.MainWindow as MainWindowView;
            if (mainWindow != null && mainWindow.DataContext is MainWindowViewModel mainVm)
            {
                _viewModel.OpenHomeRequested = () => mainVm.GoHomeCommand.Execute(null);
                _viewModel.OpenMenuRollsRequested = () => mainVm.GoMenuRollsCommand.Execute(null);
                _viewModel.OpenMenuSetsRequested = () => mainVm.GoMenuSetsCommand.Execute(null);
                _viewModel.OpenCustomRollsRequested = () => mainVm.GoCustomRollsCommand.Execute(null);
                _viewModel.OpenProfileRequested = () => mainVm.GoProfileCommand.Execute(null);
                _viewModel.OpenCartRequested = () => mainVm.GoCartCommand.Execute(null);
                _viewModel.OpenOrdersRequested = () => mainVm.GoOrdersCommand.Execute(null);
                _viewModel.OpenAdminPanelRequested = () => mainVm.GoAdminPanelCommand.Execute(null);
            }
            
            _viewModel.OpenCreateSetRequested = () =>
            {
                if (MainWindowView.NavigationService != null)
                {
                    MainWindowView.NavigationService.NavigateTo("CreateCustomSet");
                }
            };
            
            _viewModel.OpenDetailsRequested = (set) =>
            {
                var detailsWindow = new MenuDetailsWindowView(set) { Owner = Application.Current.MainWindow };
                detailsWindow.ShowDialog();
            };
            
            DataContext = _viewModel;
        }

        await _viewModel.LoadDataAsync();
    }
}
