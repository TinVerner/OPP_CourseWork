using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.SupportClasses;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.ViewModels;

public class UsersAdminWindowViewModel : BaseViewModel
{
    public class UserDisplay : INotifyPropertyChanged
    {
        public long Id { get; set; }
        public string Login { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Firstname { get; set; } = "";
        public string Lastname { get; set; } = "";
        public string Address { get; set; } = "";
        public int RoleId { get; set; }
        public string RoleName { get; set; } = "";
        
        private bool _isBlocked;
        public bool IsBlocked
        {
            get => _isBlocked;
            set
            {
                if (_isBlocked != value)
                {
                    _isBlocked = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsNotBlocked));
                }
            }
        }
        
        public bool IsNotBlocked => !IsBlocked;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private ObservableCollection<UserDisplay> _allUsers = new();
    private ObservableCollection<UserDisplay> _filteredUsers = new();
    private string _searchText = string.Empty;

    public ObservableCollection<UserDisplay> Users
    {
        get => _filteredUsers;
        set => SetProperty(ref _filteredUsers, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                ApplyFilter();
            }
        }
    }

    public ICommand LoadUsersCommand { get; }
    public ICommand BlockUserCommand { get; }
    public ICommand UnblockUserCommand { get; }
    public ICommand DeleteUserCommand { get; }
    public ICommand EditUserCommand { get; }
    public ICommand AdminPanelCommand { get; }
    public ICommand CreateRollCommand { get; }
    public ICommand CreateSetCommand { get; }
    public ICommand ViewUsersCommand { get; }
    public ICommand ViewMenuItemsCommand { get; }
    public ICommand ProfileCommand { get; }
    public ICommand LogoutCommand { get; }

    public Action? OpenAdminPanelRequested { get; set; }
    public Action? OpenCreateRollRequested { get; set; }
    public Action? OpenCreateSetRequested { get; set; }
    public Action? OpenUsersRequested { get; set; }
    public Action? OpenMenuItemsRequested { get; set; }
    public Action? OpenProfileRequested { get; set; }
    public Action<long>? OpenEditUserRequested { get; set; }

    public UsersAdminWindowViewModel()
    {
        LoadUsersCommand = new AsyncCommand(LoadUsersAsync);
        BlockUserCommand = new AsyncCommand<long>(BlockUserAsync);
        UnblockUserCommand = new AsyncCommand<long>(UnblockUserAsync);
        DeleteUserCommand = new AsyncCommand<long>(DeleteUserAsync);
        EditUserCommand = new Command<long>(OpenEditUser);

        LoadUsersCommand.Execute(null);
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            using var userRepo = new UserRepository();

            var allUsers = await userRepo.GetAllAsync();
            var usersWithRoles = new List<User>();

            foreach (var user in allUsers)
            {
                var userWithRole = await userRepo.GetWithRoleByIdAsync(user.Id);
                if (userWithRole != null)
                    usersWithRoles.Add(userWithRole);
            }

            var sortedUsers = usersWithRoles
                .OrderBy(u => u.Lastname)
                .ThenBy(u => u.Firstname)
                .ToList();

            _allUsers = new ObservableCollection<UserDisplay>(sortedUsers.Select(u => new UserDisplay
            {
                Id = u.Id,
                Login = u.Login,
                Email = u.Email,
                Phone = u.Phone,
                Firstname = u.Firstname,
                Lastname = u.Lastname,
                Address = u.Address ?? "",
                RoleId = u.RoleId,
                RoleName = u.Role?.Name ?? "",
                IsBlocked = u.IsBlocked
            }));
            
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
        }
    }

    private void ApplyFilter()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Users = new ObservableCollection<UserDisplay>(_allUsers);
            return;
        }

        var searchLower = SearchText.Trim().ToLower();
        var filtered = _allUsers.Where(u =>
            !string.IsNullOrEmpty(u.Login) &&
            u.Login.ToLower().Contains(searchLower)
        ).ToList();

        Users = new ObservableCollection<UserDisplay>(filtered);
    }

    private async Task BlockUserAsync(long userId)
    {
        try
        {
            using var userRepo = new UserRepository();
            var user = await userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                MessageBox.Show("Пользователь не найден.");
                return;
            }

            if (user.Id == CurrentUser.Id)
            {
                MessageBox.Show("Нельзя заблокировать самого себя.");
                return;
            }

            if (user.IsBlocked)
            {
                MessageBox.Show("Пользователь уже заблокирован.");
                return;
            }

            user.IsBlocked = true;
            userRepo.Update(user);
            await userRepo.SaveChangesAsync();

            MessageBox.Show("Пользователь заблокирован.");
            await LoadUsersAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка блокировки: {ex.Message}");
        }
    }

    private async Task UnblockUserAsync(long userId)
    {
        try
        {
            using var userRepo = new UserRepository();
            var user = await userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                MessageBox.Show("Пользователь не найден.");
                return;
            }

            if (!user.IsBlocked)
            {
                MessageBox.Show("Пользователь уже активен.");
                return;
            }

            user.IsBlocked = false;
            userRepo.Update(user);
            await userRepo.SaveChangesAsync();

            MessageBox.Show("Пользователь разблокирован.");
            await LoadUsersAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка разблокировки: {ex.Message}");
        }
    }

    private async Task DeleteUserAsync(long userId)
    {
        var result = MessageBox.Show(
            "Вы точно хотите удалить этого пользователя?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            using var userRepo = new UserRepository();
            var user = await userRepo.GetByIdAsync(userId);
            if (user == null)
            {
                MessageBox.Show("Пользователь не найден.");
                return;
            }

            if (user.Id == CurrentUser.Id)
            {
                MessageBox.Show("Нельзя удалить самого себя.");
                return;
            }

            if (user.RoleId == 1)
            {
                MessageBox.Show("Нельзя удалить администратора.");
                return;
            }

            userRepo.Remove(user);
            await userRepo.SaveChangesAsync();

            MessageBox.Show("Пользователь удалён.");
            await LoadUsersAsync();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка удаления: {ex.Message}");
        }
    }

    private void OpenEditUser(long userId)
    {
        OpenEditUserRequested?.Invoke(userId);
    }
}

