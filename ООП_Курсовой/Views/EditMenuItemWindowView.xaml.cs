using System.Windows;
using ООП_Курсовой.ViewModels;

namespace ООП_Курсовой.Views;

public partial class EditMenuItemWindowView : Window
{
    private Action? _refreshAction;

    public EditMenuItemWindowView(long menuItemId, Action? refreshAction = null)
    {
        InitializeComponent();
        _refreshAction = refreshAction;
        var vm = new EditMenuItemWindowViewModel(menuItemId);
        DataContext = vm;

        vm.RequestClose = () => Close();
        vm.RequestRefresh = () => _refreshAction?.Invoke();
    }
}

