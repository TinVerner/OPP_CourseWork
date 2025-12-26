using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;

namespace ООП_Курсовой.ViewModels;

public class EditUserWindowViewModel : BaseViewModel
{
    private long _userId;

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

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public Action? RequestClose { get; set; }
    public Action? OnUserUpdated { get; set; }

    public EditUserWindowViewModel(long userId)
    {
        _userId = userId;
        SaveCommand = new AsyncCommand(SaveUserAsync);
        CancelCommand = new Command(() => RequestClose?.Invoke());
        
        LoadUserAsync();
    }

    private async Task LoadUserAsync()
    {
        try
        {
            using var repo = new UserRepository();
            var user = await repo.GetWithRoleByIdAsync(_userId);
            
            if (user == null)
            {
                MessageBox.Show("Пользователь не найден.", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                RequestClose?.Invoke();
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
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки пользователя: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
            RequestClose?.Invoke();
        }
    }

    private async Task SaveUserAsync()
    {
        var login = Login.Trim();
        var firstName = FirstName.Trim();
        var lastName = LastName.Trim();
        var email = Email.Trim();
        var phone = Phone.Trim();
        var address = Address.Trim();

        var err = ValidateUserInput(login, firstName, lastName, email, phone);
        if (err != null)
        {
            MessageBox.Show(err, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        try
        {
            using var repo = new UserRepository();
            var user = await repo.GetByIdAsync(_userId);
            
            if (user == null)
            {
                MessageBox.Show("Пользователь не найден.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string nLogin = login.ToLowerInvariant();
            string nEmail = email.ToLowerInvariant();
            string nPhone = phone;

            var uniqueError = await repo.ValidateUpdateUserAsync(_userId, nLogin, nEmail, nPhone);
            if (uniqueError != null)
            {
                MessageBox.Show(uniqueError, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            user.Login = login;
            user.Firstname = firstName;
            user.Lastname = lastName;
            user.Email = email;
            user.Phone = phone;
            user.Address = string.IsNullOrWhiteSpace(address) ? null : address;

            repo.Update(user);
            await repo.SaveChangesAsync();

            MessageBox.Show("Данные пользователя успешно обновлены.", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);
            
            OnUserUpdated?.Invoke();
            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string? ValidateUserInput(string login, string firstName, string lastName,
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
}

