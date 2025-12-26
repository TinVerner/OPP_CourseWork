using System.Windows;
using ООП_Курсовой.ViewModels;
using System.Windows.Controls;

namespace ООП_Курсовой.Views.Pages;

public partial class AdminCreateItemPage : UserControl
{
    private AdminCreateItemViewModel? _viewModel;

    public AdminCreateItemPage()
    {
        InitializeComponent();
        Loaded += AdminCreateItemPage_Loaded;
    }

    private void AdminCreateItemPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            _viewModel = new AdminCreateItemViewModel();
            
            var adminWindow = Application.Current.MainWindow as AdminWindowView;
            if (adminWindow != null && adminWindow.DataContext is AdminWindowViewModel adminVm)
            {
                _viewModel.OpenAdminPanelRequested = () =>
                {
                    // Переходим на страницу меню
                    adminVm.ViewMenuItemsCommand.Execute(null);
                    
                    // Обновляем данные на странице меню после небольшой задержки
                    Task.Delay(300).ContinueWith(_ =>
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            var navService = AdminWindowView.NavigationService;
                            if (navService != null)
                            {
                                // Получаем страницу через NavigationService
                                var page = navService.GetCurrentPageControl() as AdminMenuItemsPage;
                                if (page?.DataContext is AdminMenuItemsViewModel vm)
                                {
                                    vm.RefreshDataRequested?.Invoke();
                                }
                            }
                        });
                    });
                };
            }
            
            _viewModel.RequestClose = () => { };
            _viewModel.OpenLoginRequested = () => { };
            _viewModel.OpenHomeRequested = () => { };
            
            DataContext = _viewModel;
        }
    }
}

