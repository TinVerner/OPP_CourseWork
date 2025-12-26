using System.Windows;
using System.Windows.Input;
using ООП_Курсовой.ViewModels;
using ООП_Курсовой.SupportClasses;

namespace ООП_Курсовой.Views;

public partial class MainWindowView : Window
{
    public static NavigationService? NavigationService { get; private set; }

    public MainWindowView()
    {
        InitializeComponent();

        NavigationService = new NavigationService();
        NavigationService.SetContentControl(MainContent);
        InitializeNavigation();

        var vm = new MainWindowViewModel(NavigationService);
        DataContext = vm;

        vm.OpenLoginRequested = () => 
        {
            var loginWindow = new LoginView();
            loginWindow.Show();
            this.Close();
        };

        vm.CloseRequested = () => this.Close();
    }

    private void InitializeNavigation()
    {
        if (NavigationService == null) return;

        NavigationService.RegisterPage("Home", () => new Pages.HomePage());
        NavigationService.RegisterPage("MenuRolls", () => new Pages.MenuRollsPage());
        NavigationService.RegisterPage("MenuSets", () => new Pages.MenuSetsPage());
        NavigationService.RegisterPage("CustomRolls", () => new Pages.CustomRollsPage());
        NavigationService.RegisterPage("CustomSets", () => new Pages.CustomSetsPage());
        NavigationService.RegisterPage("Profile", () => new Pages.ProfilePage());
        NavigationService.RegisterPage("Cart", () => new Pages.CartPage());
        NavigationService.RegisterPage("Orders", () => new Pages.OrdersPage());
        NavigationService.RegisterPage("AdminPanel", () => new Pages.AdminPanelPage());
        NavigationService.RegisterPage("CreateCustomRoll", () => new Pages.CreateCustomRollPage());
        NavigationService.RegisterPage("CreateCustomSet", () => new Pages.CreateCustomSetPage());

        NavigationService.NavigateTo("Home");
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

