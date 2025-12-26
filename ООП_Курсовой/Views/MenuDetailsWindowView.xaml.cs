using System.Windows;
using ООП_Курсовой.ViewModels;
using ООП_Курсовой.SupportClasses;

namespace ООП_Курсовой.Views;

public partial class MenuDetailsWindowView : Window
{
    public MenuDetailsWindowView(RollViewModel model)
    {
        InitializeComponent();

        var vm = new MenuDetailsWindowViewModel(model);
        DataContext = vm;

        vm.RequestClose = () => this.Close();
    }
}

