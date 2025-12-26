using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.SupportClasses;
using ООП_Курсовой.Views;

namespace ООП_Курсовой.ViewModels;

public class ProfileViewModel : BaseViewModel
{

    private string _login = "";
    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    private string _firstName = "";
    public string FirstName
    {
        get => _firstName;
        set => SetProperty(ref _firstName, value);
    }

    private string _lastName = "";
    public string LastName
    {
        get => _lastName;
        set => SetProperty(ref _lastName, value);
    }

    private string _email = "";
    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    private string _phone = "";
    public string Phone
    {
        get => _phone;
        set => SetProperty(ref _phone, value);
    }

    private string _address = "";
    public string Address
    {
        get => _address;
        set => SetProperty(ref _address, value);
    }

    private string _roleText = "";
    public string RoleText
    {
        get => _roleText;
        set => SetProperty(ref _roleText, value);
    }

    private string _createdAtText = "";
    public string CreatedAtText
    {
        get => _createdAtText;
        set => SetProperty(ref _createdAtText, value);
    }

    private string _currentPassword = "";
    public string CurrentPassword
    {
        get => _currentPassword;
        set => SetProperty(ref _currentPassword, value);
    }

    private string _newPassword = "";
    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    private string _confirmPassword = "";
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }

    private Visibility _adminPanelVisibility = Visibility.Collapsed;
    public Visibility AdminPanelVisibility
    {
        get => _adminPanelVisibility;
        set => SetProperty(ref _adminPanelVisibility, value);
    }

    public ICommand SaveProfileCommand { get; }
    public Action? OpenHomeRequested { get; set; }
    public Action? OpenMenuRollsRequested { get; set; }
    public Action? OpenMenuSetsRequested { get; set; }
    public Action? OpenCustomRollsRequested { get; set; }
    public Action? OpenCustomSetsRequested { get; set; }
    public Action? OpenAdminPanelRequested { get; set; }
    public Action? OpenOrdersRequested { get; set; }
    public Action? OpenCreateRollRequested { get; set; }
    public Action? OpenCreateSetRequested { get; set; }
    public Action? OpenUsersRequested { get; set; }
    public Action? OpenMenuItemsRequested { get; set; }
    public Action? OpenProfileRequested { get; set; }

    public ProfileViewModel()
    {
        SaveProfileCommand = new AsyncCommand(SaveProfileAsync);
    }

    public async Task LoadAsync()
    {
        try
        {
            using var repo = new UserRepository();

            var user = await repo.GetWithRoleByIdAsync(CurrentUser.Id);
            if (user == null)
            {
                MessageBox.Show("Пользователь не найден.");
                return;
            }

            Login = user.Login;
            FirstName = user.Firstname;
            LastName = user.Lastname;
            Email = user.Email;
            Phone = user.Phone;
            Address = user.Address ?? "";

            RoleText = $"Роль: {user.Role?.Name ?? "—"}";
            CreatedAtText = $"Дата регистрации: {user.CreatedAt:dd.MM.yyyy HH:mm}";

            AdminPanelVisibility = user.RoleId == 1
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки профиля: {ex.Message}");
        }
    }

    private async Task SaveProfileAsync()
    {
        var login = Login.Trim();
        var firstName = FirstName.Trim();
        var lastName = LastName.Trim();
        var email = Email.Trim();
        var phone = Phone.Trim();
        var address = Address.Trim();
        var currentPwd = CurrentPassword ?? "";
        var newPwd = NewPassword ?? "";
        var confirmPwd = ConfirmPassword ?? "";

        var err = ValidateProfileInput(login, firstName, lastName, email, phone);
        if (err != null)
        {
            MessageBox.Show(err, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        bool changePassword =
            !string.IsNullOrWhiteSpace(currentPwd) ||
            !string.IsNullOrWhiteSpace(newPwd) ||
            !string.IsNullOrWhiteSpace(confirmPwd);

        if (changePassword)
        {
            if (string.IsNullOrWhiteSpace(currentPwd))
            {
                MessageBox.Show("Введите текущий пароль.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newPwd.Length < 8 ||
                !Regex.IsMatch(newPwd, @"\d") ||
                !Regex.IsMatch(newPwd, @"[!@#$%^&*(),.?""':{}|<>_\-+=]"))
            {
                MessageBox.Show("Новый пароль должен быть не короче 8 символов, содержать цифру и спецсимвол.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (newPwd != confirmPwd)
            {
                MessageBox.Show("Новый пароль и подтверждение не совпадают.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        try
        {
            using var repo = new UserRepository();

            var user = await repo.GetByIdAsync(CurrentUser.Id);
            if (user == null)
            {
                MessageBox.Show("Пользователь не найден.");
                return;
            }

            string nLogin = login.ToLowerInvariant();
            string nEmail = email.ToLowerInvariant();
            string nPhone = phone;

            var uniqueError = await repo.ValidateUpdateUserAsync(user.Id, nLogin, nEmail, nPhone);
            if (uniqueError != null)
            {
                MessageBox.Show(uniqueError, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (changePassword)
            {
                var currentHash = HashPasswordSha256(currentPwd);
                if (!string.Equals(currentHash, user.PasswordHash, StringComparison.Ordinal))
                {
                    MessageBox.Show("Текущий пароль введён неверно.",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                user.PasswordHash = HashPasswordSha256(newPwd);
            }

            user.Login = login;
            user.Firstname = firstName;
            user.Lastname = lastName;
            user.Email = email;
            user.Phone = phone;
            user.Address = string.IsNullOrWhiteSpace(address) ? null : address;

            repo.Update(user);
            await repo.SaveChangesAsync();

            CurrentUser.Login = user.Login;
            CurrentUser.Firstname = user.Firstname;
            CurrentUser.Lastname = user.Lastname;
            CurrentUser.Email = user.Email;
            CurrentUser.Phone = user.Phone;
            CurrentUser.Address = user.Address;

            var mainWindow = Application.Current.Windows.OfType<MainWindowView>().FirstOrDefault();
            if (mainWindow?.DataContext is MainWindowViewModel mainVm)
            {
                mainVm.UpdateWelcomeMessage();
            }

            MessageBox.Show("Профиль успешно обновлён.",
                "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения профиля: {ex.Message}");
        }
    }

    private string? ValidateProfileInput(string login, string firstName, string lastName,
                                        string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(login) || login.Length < 3)
            return "Логин должен содержать не менее 3 символов.";

        if (!Regex.IsMatch(firstName, @"^[А-Яа-яA-Za-z]+$"))
            return "Имя должно содержать только буквы.";

        if (!Regex.IsMatch(lastName, @"^[А-Яа-яA-Za-z]+$"))
            return "Фамилия должна содержать только буквы.";

        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return "Введите корректный email.";

        if (string.IsNullOrWhiteSpace(phone))
            return "Введите номер телефона.";

        var cleanPhone = phone.Replace(" ", "");
        if (!Regex.IsMatch(cleanPhone, @"^\+?\d{10,15}$"))
            return "Введите корректный номер телефона (только цифры и '+' в начале).";

        return null;
    }

    private static string HashPasswordSha256(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
