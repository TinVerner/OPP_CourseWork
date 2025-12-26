using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.SupportClasses;

namespace ООП_Курсовой.ViewModels;

public class AdminWindowViewModel : BaseViewModel
{
    private readonly NavigationService? _navigationService;
    private Visibility _adminPanelVisibility = Visibility.Collapsed;
    public Visibility AdminPanelVisibility
    {
        get => _adminPanelVisibility;
        set => SetProperty(ref _adminPanelVisibility, value);
    }

    public ICommand ViewUsersCommand { get; }
    public ICommand ViewMenuItemsCommand { get; }
    public ICommand ViewOrdersCommand { get; }
    public ICommand CreateItemCommand { get; }
    public ICommand CreateRollCommand { get; }
    public ICommand CreateSetCommand { get; }
    public ICommand AdminPanelCommand { get; }
    public ICommand ProfileCommand { get; }
    public ICommand LogoutCommand { get; }

    public Action? RequestClose { get; set; }
    public Action? OpenUsersRequested { get; set; }
    public Action? OpenMenuItemsRequested { get; set; }
    public Action? OpenOrdersRequested { get; set; }
    public Action? OpenCreateItemRequested { get; set; }
    public Action? OpenCreateRollRequested { get; set; }
    public Action? OpenCreateSetRequested { get; set; }
    public Action? OpenAdminPanelRequested { get; set; }
    public Action? OpenProfileRequested { get; set; }
    public Action? OpenLoginRequested { get; set; }

    public AdminWindowViewModel(NavigationService? navigationService = null)
    {
        _navigationService = navigationService;
        _adminPanelVisibility = CurrentUser.RoleId == 1 ? Visibility.Visible : Visibility.Collapsed;

        ViewUsersCommand = new Command(OpenUsers);
        ViewMenuItemsCommand = new Command(OpenMenuItems);
        ViewOrdersCommand = new Command(OpenOrders);
        CreateItemCommand = new Command(OpenCreateItem);
        CreateRollCommand = new Command(OpenCreateRoll);
        CreateSetCommand = new Command(OpenCreateSet);
        AdminPanelCommand = new Command(OpenAdminPanel);
        ProfileCommand = new Command(OpenProfile);
        LogoutCommand = new Command(Logout);
    }

    private void OpenUsers()
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo("UsersAdmin");
        }
        else
        {
            OpenUsersRequested?.Invoke();
            RequestClose?.Invoke();
        }
    }

    private void OpenMenuItems()
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo("AdminMenuItems");
        }
        else
        {
            OpenMenuItemsRequested?.Invoke();
            RequestClose?.Invoke();
        }
    }

    private void OpenOrders()
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo("AdminOrders");
        }
        else
        {
            OpenOrdersRequested?.Invoke();
            RequestClose?.Invoke();
        }
    }

    private void OpenCreateItem()
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo("AdminCreateItem");
        }
        else
        {
            OpenCreateItemRequested?.Invoke();
            RequestClose?.Invoke();
        }
    }

    private void OpenCreateRoll()
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo("CreateCustomRoll");
        }
        else
        {
            OpenCreateRollRequested?.Invoke();
        }
    }

    private void OpenCreateSet()
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo("CreateCustomSet");
        }
        else
        {
            OpenCreateSetRequested?.Invoke();
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
            RequestClose?.Invoke();
        }
    }

    private void OpenProfile()
    {
        if (_navigationService != null)
        {
            _navigationService.NavigateTo("AdminProfile");
        }
        else
        {
            OpenProfileRequested?.Invoke();
        }
    }

    private void Logout()
    {
        CurrentUser.Logout();
        OpenLoginRequested?.Invoke();
        RequestClose?.Invoke();
    }
}

