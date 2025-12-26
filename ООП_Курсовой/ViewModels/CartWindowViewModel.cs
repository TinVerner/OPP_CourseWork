using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.SupportClasses;

namespace ООП_Курсовой.ViewModels;

public class CartWindowViewModel : BaseViewModel
{
    private ObservableCollection<Cart.CartItem> _items = new();

    public ObservableCollection<Cart.CartItem> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    private decimal _totalPrice;
    public decimal TotalPrice
    {
        get => _totalPrice;
        set => SetProperty(ref _totalPrice, value);
    }

    public ICommand LoadDataCommand { get; }
    public ICommand IncreaseQuantityCommand { get; }
    public ICommand DecreaseQuantityCommand { get; }
    public ICommand RemoveItemCommand { get; }
    public ICommand ClearCartCommand { get; }
    public ICommand CheckoutCommand { get; }

    public Action? OpenProfileRequested { get; set; }
    public Action? OpenHomeRequested { get; set; }
    public Action? OpenMenuRequested { get; set; }
    public Action? OpenCustomRollsRequested { get; set; }
    public Action? OpenCustomSetsRequested { get; set; }
    public Action? OpenAdminPanelRequested { get; set; }
    public Action? OpenOrdersRequested { get; set; }
    public Action? OpenCheckoutRequested { get; set; }

    public CartWindowViewModel()
    {
        LoadDataCommand = new Command(LoadData);
        IncreaseQuantityCommand = new Command<long>(IncreaseQuantity);
        DecreaseQuantityCommand = new Command<long>(DecreaseQuantity);
        RemoveItemCommand = new Command<long>(RemoveItem);
        ClearCartCommand = new Command(ClearCart);
        CheckoutCommand = new AsyncCommand(CheckoutAsync, _ => Items.Any());

        LoadData();
        Cart.Items.CollectionChanged += (s, e) =>
        {
            LoadData();
            foreach (Cart.CartItem item in Cart.Items)
            {
                item.PropertyChanged += (sender, args) =>
                {
                    if (args.PropertyName == nameof(Cart.CartItem.Quantity) || args.PropertyName == nameof(Cart.CartItem.Total))
                        TotalPrice = Items.Sum(i => i.Total);
                };
            }
        };
    }

    public void LoadData()
    {
        Items = new ObservableCollection<Cart.CartItem>(Cart.Items);
        TotalPrice = Items.Sum(i => i.Total);
        ((AsyncCommand)CheckoutCommand).RaiseCanExecuteChanged();
    }

    private void IncreaseQuantity(long menuItemId)
    {
        Cart.ChangeQuantity(menuItemId, 1);
        LoadData();
    }

    private void DecreaseQuantity(long menuItemId)
    {
        Cart.ChangeQuantity(menuItemId, -1);
        LoadData();
    }

    private void RemoveItem(long menuItemId)
    {
        Cart.Remove(menuItemId);
        LoadData();
    }

    private void ClearCart()
    {
        var confirm = MessageBox.Show(
            "Вы действительно хотите очистить корзину?",
            "Подтверждение",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm == MessageBoxResult.Yes)
        {
            Cart.Clear();
            LoadData();
        }
    }

    private async Task CheckoutAsync()
    {
        if (!Items.Any())
        {
            MessageBox.Show("Корзина пуста.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        OpenCheckoutRequested?.Invoke();
    }
}

