using System.Windows;
using System.Windows.Controls;
using ООП_Курсовой.ViewModels;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.Views;

public partial class CheckoutWindowView : Window
{
    public CheckoutWindowView()
    {
        InitializeComponent();

        var vm = new CheckoutWindowViewModel();
        DataContext = vm;

        vm.RequestClose = () => 
        {
            this.DialogResult = true;
            this.Close();
        };
    }

    private void DeliveryType_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is RadioButton radioButton && DataContext is CheckoutWindowViewModel vm)
        {
            if (radioButton.Content.ToString() == "Самовывоз")
            {
                vm.DeliveryType = DeliveryType.pickup;
            }
            else if (radioButton.Content.ToString() == "Доставка курьером")
            {
                vm.DeliveryType = DeliveryType.courier;
            }
        }
    }
}

