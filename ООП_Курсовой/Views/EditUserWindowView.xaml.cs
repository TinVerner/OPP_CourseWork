using System.Windows;
using ООП_Курсовой.ViewModels;

namespace ООП_Курсовой.Views;

public partial class EditUserWindowView : Window
{
    public EditUserWindowView(long userId)
    {
        InitializeComponent();

        var vm = new EditUserWindowViewModel(userId);
        DataContext = vm;

        vm.RequestClose = () => this.Close();
        vm.OnUserUpdated = () =>
        {
            // Обновление списка пользователей происходит автоматически через навигацию
            // Если окно открыто из страницы UsersAdminPage, она обновит данные при возврате
        };
    }
}

