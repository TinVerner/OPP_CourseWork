using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.SupportClasses;

namespace ООП_Курсовой.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private string _login = "";
    public string Login
    {
        get => _login;
        set => SetProperty(ref _login, value);
    }

    private string _password = "";
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand GoToRegistrationCommand { get; }

    public Action? OpenMainRequested { get; set; }
    public Action? OpenAdminPanelRequested { get; set; }
    public Action? OpenRegistrationRequested { get; set; }
    public Action? RequestClose { get; set; }

    public LoginViewModel()
    {
        LoginCommand = new AsyncCommand(LoginAsync);
        GoToRegistrationCommand = new Command(OpenRegistration);
    }

    private async Task LoginAsync()
    {
        string raw = Login?.Trim() ?? "";
        string pwd = Password ?? "";

        if (string.IsNullOrWhiteSpace(raw) || string.IsNullOrWhiteSpace(pwd))
        {
            MessageBox.Show("Введите логин и пароль.");
            return;
        }

        string normalized = raw.ToLowerInvariant();

        try
        {
            using var repo = new UserRepository();

            var allUsers = await repo.GetAllAsync();

            var user = allUsers.FirstOrDefault(u =>
                u.Login.ToLower() == normalized ||
                u.Email.ToLower() == normalized);

            if (user == null)
            {
                MessageBox.Show("Неверный логин или пароль.");
                return;
            }

            if (!VerifyPassword(pwd, user.PasswordHash))
            {
                MessageBox.Show("Неверный логин или пароль.");
                return;
            }

            if (user.IsBlocked)
            {
                MessageBox.Show(
                    "Ваш аккаунт заблокирован.\n\n" +
                    "Если вы считаете, что это произошло по ошибке, пожалуйста, свяжитесь с администрацией:\n" +
                    "• Email: kaorimau@gmail.com\n" +
                    "• Телефон: +375 (33) 17-45-333\n" +
                    "• Время работы поддержки: Пн-Сб, 10:00-18:00\n\n" +
                    "Мы рассмотрим ваше обращение в кратчайшие сроки.",
                    "Аккаунт заблокирован",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var userFull = await repo.GetWithRoleByIdAsync(user.Id);
            if (userFull == null)
            {
                MessageBox.Show("Ошибка загрузки профиля.");
                return;
            }

            CurrentUser.Id = userFull.Id;
            CurrentUser.Login = userFull.Login;
            CurrentUser.Email = userFull.Email;
            CurrentUser.Phone = userFull.Phone;
            CurrentUser.Firstname = userFull.Firstname;
            CurrentUser.Lastname = userFull.Lastname;
            CurrentUser.Address = userFull.Address;
            CurrentUser.RoleId = userFull.RoleId;
            CurrentUser.RoleName = userFull.Role?.Name ?? "";
            CurrentUser.CreatedAt = userFull.CreatedAt;
            CurrentUser.IsBlocked = userFull.IsBlocked;

            MessageBox.Show("Вход успешен!");

            if (CurrentUser.RoleId == 1)
            {
                OpenAdminPanelRequested?.Invoke();
            }
            else
            {
                OpenMainRequested?.Invoke();
            }
            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка входа: {ex.Message}");
        }
    }

    private void OpenRegistration()
    {
        OpenRegistrationRequested?.Invoke();
        RequestClose?.Invoke();
    }

    private bool VerifyPassword(string password, string stored)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        string computed = Convert.ToBase64String(hash);
        return computed == stored;
    }
}
