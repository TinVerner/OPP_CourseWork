using System.Windows;
using ООП_Курсовой.ViewModels;
using ООП_Курсовой.SupportClasses;

namespace ООП_Курсовой.Views;

public partial class AdminWindowView : Window
{
    public static NavigationService? NavigationService { get; private set; }

    public AdminWindowView()
    {
        InitializeComponent();

        NavigationService = new NavigationService();
        NavigationService.SetContentControl(AdminContent);
        InitializeNavigation();

        var vm = new AdminWindowViewModel(NavigationService);
        DataContext = vm;

        vm.RequestClose = () => this.Close();
        vm.OpenUsersRequested = () => { };
        vm.OpenCreateItemRequested = () => { };
        vm.OpenMenuItemsRequested = () => { };
        vm.OpenOrdersRequested = () => { };
        vm.OpenCreateRollRequested = () => 
        { 
            NavigationService?.NavigateTo("CreateCustomRoll");
        };
        vm.OpenCreateSetRequested = () => 
        { 
            NavigationService?.NavigateTo("CreateCustomSet");
        };
        vm.OpenAdminPanelRequested = () => { };
        vm.OpenProfileRequested = () => 
        { 
            NavigationService?.NavigateTo("AdminProfile");
        };
        vm.OpenLoginRequested = () => 
        {
            var loginWindow = new LoginView();
            loginWindow.Show();
            this.Close();
        };
    }

    private void InitializeNavigation()
    {
        if (NavigationService == null) return;

        // Регистрируем страницы
        NavigationService.RegisterPage("AdminPanel", () => new Pages.AdminPanelPage());
        NavigationService.RegisterPage("UsersAdmin", () => new Pages.UsersAdminPage());
        NavigationService.RegisterPage("AdminMenuItems", () => new Pages.AdminMenuItemsPage());
        NavigationService.RegisterPage("AdminOrders", () => new Pages.AdminOrdersPage());
        NavigationService.RegisterPage("AdminCreateItem", () => new Pages.AdminCreateItemPage());
        NavigationService.RegisterPage("CreateCustomRoll", () => new Pages.CreateCustomRollPage());
        NavigationService.RegisterPage("CreateCustomSet", () => new Pages.CreateCustomSetPage());
        NavigationService.RegisterPage("AdminProfile", () => new Pages.AdminProfilePage());

        NavigationService.NavigateTo("AdminPanel");
    }
}

