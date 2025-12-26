using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.ViewModels;

public class RegistrationViewModel : BaseViewModel
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

    private string _password = "";
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private string _confirmPassword = "";
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
    }


    public ICommand RegisterCommand { get; }
    public ICommand GoToLoginCommand { get; }

    public Action? RequestClose { get; set; }
    public Action? OpenLoginRequested { get; set; }

    public RegistrationViewModel()
    {
        RegisterCommand = new AsyncCommand(RegisterAsync);
        GoToLoginCommand = new Command(OpenLogin);
    }

    private async Task RegisterAsync()
    {
        string? err = ValidateInput(Login, FirstName, LastName, Email, Phone, Password, ConfirmPassword);
        if (err != null)
        {
            MessageBox.Show(err, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        string nLogin = Login.Trim().ToLowerInvariant();
        string nEmail = Email.Trim().ToLowerInvariant();
        string nPhone = "+" + Regex.Replace(Phone, @"\D", "");

        try
        {
            using var repo = new UserRepository();

            string? uniqueError = await repo.ValidateNewUserAsync(nLogin, nEmail, nPhone);
            if (uniqueError != null)
            {
                MessageBox.Show(uniqueError, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            long? clientRoleId = await repo.GetRoleIdByNameAsync("Client");
            if (clientRoleId is null or 0)
            {
                MessageBox.Show("Роль 'Client' не найдена.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string passwordHash = HashPassword(Password);

            var user = new User
            {
                Login = nLogin,
                Email = nEmail,
                Phone = nPhone,
                PasswordHash = passwordHash,
                Firstname = FirstName.Trim(),
                Lastname = LastName.Trim(),
                Address = string.IsNullOrWhiteSpace(Address) ? null : Address.Trim(),
                CreatedAt = DateTimeOffset.UtcNow,
                IsBlocked = false,
                RoleId = (int)clientRoleId.Value
            };

            await repo.AddAsync(user);
            await repo.SaveChangesAsync();

            MessageBox.Show("Регистрация прошла успешно!", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);

            OpenLoginRequested?.Invoke();
            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при регистрации: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenLogin()
    {
        OpenLoginRequested?.Invoke();
        RequestClose?.Invoke();
    }

    private string? ValidateInput(string login, string firstName, string lastName,
                                 string email, string phone, string password, string confirmPassword)
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

        string cleanPhone = phone.Replace(" ", "");
        if (!Regex.IsMatch(cleanPhone, @"^\+?\d{10,15}$"))
            return "Введите корректный номер телефона (только цифры и '+' в начале).";

        if (password.Length < 8)
            return "Пароль должен содержать не менее 8 символов.";

        if (!Regex.IsMatch(password, @"\d"))
            return "Пароль должен содержать хотя бы одну цифру.";

        if (!Regex.IsMatch(password, @"[!@#$%^&*(),.?""':{}|<>_\-+=]"))
            return "Пароль должен содержать хотя бы один спецсимвол.";

        if (password != confirmPassword)
            return "Пароли не совпадают.";

        return null;
    }

    private string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
