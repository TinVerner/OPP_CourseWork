using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.SupportClasses;

namespace ООП_Курсовой.ViewModels;

public class MainWindowViewModel : BaseViewModel
{
    private readonly NavigationService? _navigationService;
    private string _userInfoText = "";
    public string UserInfoText
    {
        get => _userInfoText;
        set => SetProperty(ref _userInfoText, value);
    }

    private string _welcomeMessage = "";
    public string WelcomeMessage
    {
        get => _welcomeMessage;
        set => SetProperty(ref _welcomeMessage, value);
    }

    private string _rollsCount = "0";
    public string RollsCount
    {
        get => _rollsCount;
        set => SetProperty(ref _rollsCount, value);
    }

    private string _setsCount = "0";
    public string SetsCount
    {
        get => _setsCount;
        set => SetProperty(ref _setsCount, value);
    }

    private string _cartItemsCount = "0";
    public string CartItemsCount
    {
        get => _cartItemsCount;
        set => SetProperty(ref _cartItemsCount, value);
    }

    private Visibility _adminPanelVisibility = Visibility.Collapsed;
    public Visibility AdminPanelVisibility
    {
        get => _adminPanelVisibility;
        set => SetProperty(ref _adminPanelVisibility, value);
    }

    public ICommand LogoutCommand { get; }
    public ICommand GoHomeCommand { get; }
    public ICommand GoMenuRollsCommand { get; }
    public ICommand GoMenuSetsCommand { get; }
    public ICommand GoCustomSetsCommand { get; }
    public ICommand GoCustomRollsCommand { get; }
    public ICommand GoProfileCommand { get; }
    public ICommand GoAdminPanelCommand { get; }
    public ICommand GoCartCommand { get; }
    public ICommand GoOrdersCommand { get; }
    public ICommand LoadDataCommand { get; }

    public Action? OpenLoginRequested { get; set; }
    public Action? OpenMenuRollsRequested { get; set; }
    public Action? OpenMenuSetsRequested { get; set; }
    public Action? OpenCustomSetsRequested { get; set; }
    public Action? OpenCustomRollsRequested { get; set; }
    public Action? OpenProfileRequested { get; set; }
    public Action? OpenAdminPanelRequested { get; set; }
    public Action? OpenCartRequested { get; set; }
    public Action? OpenOrdersRequested { get; set; }
    public Action? CloseRequested { get; set; }

    public MainWindowViewModel(NavigationService? navigationService = null)
    {
        _navigationService = navigationService;

        if (CurrentUser.IsAuthenticated)
        {
            var timeOfDay = DateTime.Now.Hour;
            string greeting = timeOfDay < 12 ? "Доброе утро" : timeOfDay < 18 ? "Добрый день" : "Добрый вечер";
            WelcomeMessage = $"{greeting}, {CurrentUser.Firstname}!";
            UserInfoText = $"{CurrentUser.Firstname} {CurrentUser.Lastname}";
        }
        else
        {
            WelcomeMessage = "Добро пожаловать!";
            UserInfoText = "Гость";
        }

        if (CurrentUser.RoleId == 1)
            AdminPanelVisibility = Visibility.Visible;

        LogoutCommand = new Command(OnLogout);
        GoHomeCommand = new Command(OpenHome);
        GoMenuRollsCommand = new Command(OpenMenuRolls);
        GoMenuSetsCommand = new Command(OpenMenuSets);
        GoCustomSetsCommand = new Command(OpenCustomSets);
        GoCustomRollsCommand = new Command(OpenCustomRolls);
        GoProfileCommand = new Command(OpenProfile);
        GoAdminPanelCommand = new Command(OpenAdminPanel);
        GoCartCommand = new Command(OpenCart);
        GoOrdersCommand = new Command(OpenOrders);
        LoadDataCommand = new AsyncCommand(LoadDataAsync);

        LoadDataCommand.Execute(null);
        UpdateCartCount();

        Cart.Items.CollectionChanged += (s, e) => UpdateCartCount();
    }

    private async Task LoadDataAsync()
    {
        try
        {
            using var menuItemRepo = new MenuItemRepository();

            var allItems = await menuItemRepo.GetAllAsync();
            var rolls = allItems.Count(m => m.ItemType == "roll");
            var sets = allItems.Count(m => m.ItemType == "set");

            RollsCount = rolls.ToString();
            SetsCount = sets.ToString();
        }
        catch (Exception)
        {
            RollsCount = "—";
            SetsCount = "—";
        }
    }

    private void UpdateCartCount()
    {
        CartItemsCount = Cart.Items.Count.ToString();
    }

    private void OnLogout()
    {
        CurrentUser.Logout();
        OpenLoginRequested?.Invoke();
        CloseRequested?.Invoke();
    }

    private void OpenHome()
    {
        if (_navigationService != null)
        {
            UpdateWelcomeMessage();
            _navigationService.NavigateTo("Home");
        }
    }
    
    public void UpdateWelcomeMessage()
    {
        if (CurrentUser.IsAuthenticated)
        {
            var timeOfDay = DateTime.Now.Hour;
            string greeting = timeOfDay < 12 ? "Доброе утро" : timeOfDay < 18 ? "Добрый день" : "Добрый вечер";
            WelcomeMessage = $"{greeting}, {CurrentUser.Firstname}!";
            UserInfoText = $"{CurrentUser.Firstname} {CurrentUser.Lastname}";
        }
        else
        {
            WelcomeMessage = "Добро пожаловать!";
            UserInfoText = "Гость";
        }
    }

    private void OpenMenuRolls()
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo("MenuRolls");
        }
        else
        {
            OpenMenuRollsRequested?.Invoke();
            CloseRequested?.Invoke();
        }
    }

    private void OpenMenuSets()
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo("MenuSets");
        }
        else
        {
            OpenMenuSetsRequested?.Invoke();
            CloseRequested?.Invoke();
        }
    }

    private void OpenCustomSets()
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo("CustomSets");
        }
        else
        {
            OpenCustomSetsRequested?.Invoke();
            CloseRequested?.Invoke();
        }
    }

    private void OpenCustomRolls()
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo("CustomRolls");
        }
        else
        {
            OpenCustomRollsRequested?.Invoke();
            CloseRequested?.Invoke();
        }
    }

    private void OpenProfile()
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo("Profile");
        }
        else
        {
            OpenProfileRequested?.Invoke();
            CloseRequested?.Invoke();
        }
    }

    private void OpenAdminPanel()
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo("AdminPanel");
        }
        else
        {
            OpenAdminPanelRequested?.Invoke();
            CloseRequested?.Invoke();
        }
    }

    private void OpenCart()
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo("Cart");
        }
        else
        {
            OpenCartRequested?.Invoke();
        }
    }

    private void OpenOrders()
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo("Orders");
        }
        else
        {
            OpenOrdersRequested?.Invoke();
            CloseRequested?.Invoke();
        }
    }
}
