using System.Windows;
using System.Windows.Input;
using ООП_Курсовой.ViewModels;

namespace ООП_Курсовой.Views.Pages;

public partial class HomePage : System.Windows.Controls.UserControl
{
    public HomePage()
    {
        InitializeComponent();
        
        Loaded += (s, e) =>
        {
            if (Application.Current.MainWindow?.DataContext is MainWindowViewModel vm)
            {
                DataContext = vm;
                vm.UpdateWelcomeMessage();
            }
        };
    }

    private void MenuCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.GoMenuRollsCommand.Execute(null);
    }

    private void SetsCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.GoMenuSetsCommand.Execute(null);
    }

    private void CartCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.GoCartCommand.Execute(null);
    }

    private void CustomRollsCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.GoCustomRollsCommand.Execute(null);
    }

    private void CustomSetsCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.GoCustomSetsCommand.Execute(null);
    }

    private void ProfileCard_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is MainWindowViewModel vm)
            vm.GoProfileCommand.Execute(null);
    }
}

