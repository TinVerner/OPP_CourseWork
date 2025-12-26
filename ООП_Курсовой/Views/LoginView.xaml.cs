using System.Windows;
using System.Windows.Input;
using ООП_Курсовой.ViewModels;

namespace ООП_Курсовой.Views;

public partial class LoginView : Window
{
    public LoginView()
    {
        InitializeComponent();

        var vm = new LoginViewModel();
        DataContext = vm;

        vm.OpenMainRequested = () => new MainWindowView().Show();
        vm.OpenAdminPanelRequested = () => new AdminWindowView().Show();
        vm.OpenRegistrationRequested = () => new RegistrationView().Show();
        vm.RequestClose = () => this.Close();

        PasswordBox.PasswordChanged += (_, __) =>
        {
            vm.Password = PasswordBox.Password;
        };
    }

    private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            if (DataContext is LoginViewModel vm)
                vm.LoginCommand.Execute(null);
        }
    }
}

