using System.Windows;
using ООП_Курсовой.ViewModels;

namespace ООП_Курсовой.Views;

public partial class RegistrationView : Window
{
    public RegistrationView()
    {
        InitializeComponent();

        var vm = new RegistrationViewModel();
        DataContext = vm;

        vm.RequestClose = () => this.Close();
        vm.OpenLoginRequested = () => new LoginView().Show();

        PasswordBox.PasswordChanged += (_, __) =>
        {
            vm.Password = PasswordBox.Password;
        };

        ConfirmPasswordBox.PasswordChanged += (_, __) =>
        {
            vm.ConfirmPassword = ConfirmPasswordBox.Password;
        };
    }
}

