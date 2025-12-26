using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.SupportClasses;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.ViewModels;

public class CheckoutWindowViewModel : BaseViewModel
{
    private string _customerName = "";
    public string CustomerName
    {
        get => _customerName;
        set => SetProperty(ref _customerName, value);
    }

    private string _customerPhone = "";
    public string CustomerPhone
    {
        get => _customerPhone;
        set => SetProperty(ref _customerPhone, value);
    }

    private DeliveryType _deliveryType = DeliveryType.pickup;
    public DeliveryType DeliveryType
    {
        get => _deliveryType;
        set
        {
            if (SetProperty(ref _deliveryType, value))
            {
                OnPropertyChanged(nameof(IsDeliveryAddressVisible));
                if (value == DeliveryType.courier && CurrentUser.IsAuthenticated)
                {
                    LoadUserAddressAsync();
                }
            }
        }
    }

    private string _customerAddress = "";
    public string CustomerAddress
    {
        get => _customerAddress;
        set => SetProperty(ref _customerAddress, value);
    }

    private DateTime? _deliveryTime;
    public DateTime? DeliveryTime
    {
        get => _deliveryTime;
        set => SetProperty(ref _deliveryTime, value);
    }

    private DateTime? _deliveryDate;
    public DateTime? DeliveryDate
    {
        get => _deliveryDate;
        set
        {
            if (SetProperty(ref _deliveryDate, value))
            {
                UpdateDeliveryTime();
            }
        }
    }

    private string? _selectedDeliveryTime;
    public string? SelectedDeliveryTime
    {
        get => _selectedDeliveryTime;
        set
        {
            if (SetProperty(ref _selectedDeliveryTime, value))
            {
                UpdateDeliveryTime();
            }
        }
    }

    public ObservableCollection<string> AvailableDeliveryTimes { get; } = new ObservableCollection<string>();

    public DateTime MinDeliveryDate => DateTime.Today;
    
    public DateTime MaxDeliveryDate => DateTime.Today.AddDays(3);

    private string _comment = "";
    public string Comment
    {
        get => _comment;
        set => SetProperty(ref _comment, value);
    }

    public bool IsDeliveryAddressVisible => DeliveryType == DeliveryType.courier;

    public ObservableCollection<Cart.CartItem> Items { get; }

    private decimal _totalPrice;
    public decimal TotalPrice
    {
        get => _totalPrice;
        set => SetProperty(ref _totalPrice, value);
    }

    public ICommand SubmitOrderCommand { get; }
    public ICommand CancelCommand { get; }

    public Action? RequestClose { get; set; }

    public CheckoutWindowViewModel()
    {
        Items = new ObservableCollection<Cart.CartItem>(Cart.Items);
        TotalPrice = Cart.GetTotal();

        if (CurrentUser.IsAuthenticated)
        {
            var fullName = $"{CurrentUser.Firstname} {CurrentUser.Lastname}".Trim();
            CustomerName = string.IsNullOrWhiteSpace(fullName) ? CurrentUser.Login : fullName;
            CustomerPhone = CurrentUser.Phone ?? "";
        }

        InitializeDeliveryTimes();

        SubmitOrderCommand = new AsyncCommand(SubmitOrderAsync, _ => IsValid());
        CancelCommand = new Command(() => RequestClose?.Invoke());

        PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(CustomerName) || 
                e.PropertyName == nameof(CustomerPhone) || 
                e.PropertyName == nameof(CustomerAddress) ||
                e.PropertyName == nameof(DeliveryType) ||
                e.PropertyName == nameof(DeliveryDate) ||
                e.PropertyName == nameof(SelectedDeliveryTime) ||
                e.PropertyName == nameof(DeliveryTime))
            {
                ((AsyncCommand)SubmitOrderCommand).RaiseCanExecuteChanged();
            }
        };
    }

    private void InitializeDeliveryTimes()
    {
        AvailableDeliveryTimes.Clear();
        var startHour = 10;
        var endHour = 22;
        
        for (int hour = startHour; hour <= endHour; hour++)
        {
            AvailableDeliveryTimes.Add($"{hour:00}:00");
            if (hour < endHour)
            {
                AvailableDeliveryTimes.Add($"{hour:00}:30");
            }
        }
    }

    private void UpdateDeliveryTime()
    {
        if (DeliveryDate.HasValue && !string.IsNullOrWhiteSpace(SelectedDeliveryTime))
        {
            if (TimeSpan.TryParse(SelectedDeliveryTime, out var time))
            {
                var dateTime = DeliveryDate.Value.Date.Add(time);
                DeliveryTime = dateTime;
            }
        }
        else
        {
            DeliveryTime = null;
        }
    }

    private async void LoadUserAddressAsync()
    {
        if (!CurrentUser.IsAuthenticated)
            return;

        try
        {
            using var userRepo = new UserRepository();
            var user = await userRepo.GetByIdAsync(CurrentUser.Id);
            
            if (user != null && !string.IsNullOrWhiteSpace(user.Address))
            {
                CustomerAddress = user.Address;
            }
            else if (!string.IsNullOrWhiteSpace(CurrentUser.Address))
            {
                CustomerAddress = CurrentUser.Address;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки адреса: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(CustomerName))
            return false;

        if (string.IsNullOrWhiteSpace(CustomerPhone))
            return false;

        if (DeliveryType == DeliveryType.courier && string.IsNullOrWhiteSpace(CustomerAddress))
            return false;

        if (!Items.Any())
            return false;

        if (!DeliveryDate.HasValue)
            return false;

        if (string.IsNullOrWhiteSpace(SelectedDeliveryTime))
            return false;

        if (!DeliveryTime.HasValue)
            return false;

        var today = DateTime.Today;
        var maxDate = DateTime.Today.AddDays(3);
        if (DeliveryDate.Value.Date < today || DeliveryDate.Value.Date > maxDate)
            return false;

        var now = DateTime.Now;
        var minDeliveryTime = now.AddHours(2);
        
        if (DeliveryDate.Value.Date == today)
        {
            if (DeliveryTime.Value < minDeliveryTime)
                return false;
        }

        return true;
    }

    private async Task SubmitOrderAsync()
    {
        if (!IsValid())
        {
            string errorMessage = "Пожалуйста, заполните все обязательные поля.";
            
            if (!DeliveryDate.HasValue || string.IsNullOrWhiteSpace(SelectedDeliveryTime))
            {
                errorMessage = "Пожалуйста, выберите дату и время доставки.";
            }
            else if (DeliveryDate.HasValue && DeliveryTime.HasValue)
            {
                var today = DateTime.Today;
                var maxDate = DateTime.Today.AddDays(3);
                var now = DateTime.Now;
                var minDeliveryTime = now.AddHours(2);
                
                if (DeliveryDate.Value.Date < today)
                {
                    errorMessage = "Нельзя оформить заказ на прошедшую дату. Выберите сегодняшнюю дату или дату в будущем.";
                }
                else if (DeliveryDate.Value.Date > maxDate)
                {
                    errorMessage = "Нельзя оформить заказ более чем на 3 дня вперед. Выберите дату в пределах следующих 3 дней.";
                }
                else if (DeliveryDate.Value.Date == today && DeliveryTime.Value < minDeliveryTime)
                {
                    errorMessage = $"Время доставки должно быть минимум на 2 часа позже текущего времени.\n\n" +
                                   $"Текущее время: {now:HH:mm}\n" +
                                   $"Минимальное время доставки: {minDeliveryTime:HH:mm}";
                }
            }
            
            MessageBox.Show(errorMessage, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var dbContext = new Tables.DatabaseContext();
            using var orderRepo = new OrderRepository(dbContext);
            using var orderItemRepo = new OrderItemRepository(dbContext);
            using var menuItemRepo = new MenuItemRepository(dbContext);

            foreach (var cartItem in Items)
            {
                var menuItem = await menuItemRepo.GetByIdAsync(cartItem.MenuItemId);
                if (menuItem == null)
                {
                    MessageBox.Show(
                        $"Товар \"{cartItem.Name}\" больше не доступен в меню. Пожалуйста, обновите корзину.",
                        "Ошибка",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }

            var order = new Order
            {
                UserId = CurrentUser.IsAuthenticated ? CurrentUser.Id : null,
                Status = OrderStatus.pending,
                CustomerName = CustomerName.Trim(),
                CustomerPhone = CustomerPhone.Trim(),
                DeliveryType = DeliveryType,
                CustomerAddress = DeliveryType == DeliveryType.courier ? CustomerAddress.Trim() : null,
                DeliveryTime = DeliveryTime.HasValue ? new DateTimeOffset(DeliveryTime.Value).ToUniversalTime() : null,
                Comment = string.IsNullOrWhiteSpace(Comment) ? null : Comment.Trim(),
                TotalPrice = TotalPrice,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await orderRepo.AddAsync(order);
            await orderRepo.SaveChangesAsync();

            if (order.Id <= 0)
            {
                throw new Exception("Не удалось создать заказ. ID заказа не был присвоен.");
            }

            foreach (var cartItem in Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.Id,
                    MenuItemId = cartItem.MenuItemId,
                    Qty = cartItem.Quantity,
                    Price = cartItem.Price
                };

                await orderItemRepo.AddAsync(orderItem);
            }

            await orderItemRepo.SaveChangesAsync();

            Cart.Clear();

            MessageBox.Show(
                $"Заказ №{order.Id} успешно оформлен!\n\n" +
                $"Имя: {order.CustomerName}\n" +
                $"Телефон: {order.CustomerPhone}\n" +
                $"Тип доставки: {(order.DeliveryType == DeliveryType.pickup ? "Самовывоз" : "Доставка")}\n" +
                $"Сумма: {order.TotalPrice:F2} руб.",
                "Заказ оформлен",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            RequestClose?.Invoke();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
            var fullMessage = $"Ошибка базы данных: {innerMessage}";
            
            if (dbEx.InnerException != null)
            {
                fullMessage += $"\n\nДетали: {dbEx.InnerException.GetType().Name}";
            }

            MessageBox.Show(
                $"Ошибка при оформлении заказа:\n{fullMessage}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message;
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nДетали: {ex.InnerException.Message}";
            }

            MessageBox.Show(
                $"Ошибка при оформлении заказа: {errorMessage}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

}

