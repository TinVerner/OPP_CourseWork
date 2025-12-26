using System.Windows;
using ООП_Курсовой.ViewModels;

namespace ООП_Курсовой.Views;

public partial class OrderDetailsWindowView : Window
{
    public OrderDetailsWindowView(long orderId)
    {
        InitializeComponent();

        var vm = new OrderDetailsWindowViewModel(orderId);
        DataContext = vm;

        vm.RequestClose = () => this.Close();
        vm.OnOrderCanceled = () =>
        {
            vm.LoadDataCommand.Execute(null);
        };
    }
}

