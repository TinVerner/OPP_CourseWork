using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.ViewModels;

public class OrderDetailsWindowViewModel : BaseViewModel
{
    private long _orderId;
    private OrderStatus _currentStatus;
    private string _orderNumber = "";
    private string _status = "";
    private string _customerName = "";
    private string _customerPhone = "";
    private string _deliveryType = "";
    private string _customerAddress = "";
    private string _deliveryTime = "";
    private string _comment = "";
    private decimal _totalPrice;
    private string _createdAt = "";
    private string _deliveredAt = "";

    public string OrderNumber
    {
        get => _orderNumber;
        set => SetProperty(ref _orderNumber, value);
    }

    public string Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public bool IsCancelable => _currentStatus == OrderStatus.pending;

    public string CustomerName
    {
        get => _customerName;
        set => SetProperty(ref _customerName, value);
    }

    public string CustomerPhone
    {
        get => _customerPhone;
        set => SetProperty(ref _customerPhone, value);
    }

    public string DeliveryType
    {
        get => _deliveryType;
        set => SetProperty(ref _deliveryType, value);
    }

    public string CustomerAddress
    {
        get => _customerAddress;
        set => SetProperty(ref _customerAddress, value);
    }

    public string DeliveryTime
    {
        get => _deliveryTime;
        set => SetProperty(ref _deliveryTime, value);
    }

    public string Comment
    {
        get => _comment;
        set => SetProperty(ref _comment, value);
    }

    public decimal TotalPrice
    {
        get => _totalPrice;
        set => SetProperty(ref _totalPrice, value);
    }

    public string CreatedAt
    {
        get => _createdAt;
        set => SetProperty(ref _createdAt, value);
    }

    public string DeliveredAt
    {
        get => _deliveredAt;
        set => SetProperty(ref _deliveredAt, value);
    }

    private ObservableCollection<OrderItemViewModel> _items = new();
    public ObservableCollection<OrderItemViewModel> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    public ICommand LoadDataCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand CancelOrderCommand { get; }

    public Action? RequestClose { get; set; }
    public Action? OnOrderCanceled { get; set; }

    public OrderDetailsWindowViewModel(long orderId)
    {
        _orderId = orderId;
        LoadDataCommand = new AsyncCommand(LoadDataAsync);
        CloseCommand = new Command(Close);
        CancelOrderCommand = new AsyncCommand(CancelOrderAsync, _ => IsCancelable);

        LoadDataCommand.Execute(null);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            using var orderRepo = new OrderRepository();
            using var orderItemRepo = new OrderItemRepository();

            var order = await orderRepo.GetByIdAsync(_orderId);
            if (order == null)
            {
                MessageBox.Show("Заказ не найден.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                RequestClose?.Invoke();
                return;
            }

            _currentStatus = order.Status;
            OrderNumber = $"Заказ №{order.Id}";
            Status = GetStatusText(order.Status);
            CustomerName = order.CustomerName ?? "Не указано";
            CustomerPhone = order.CustomerPhone ?? "Не указано";
            DeliveryType = GetDeliveryTypeText(order.DeliveryType);
            CustomerAddress = order.CustomerAddress ?? "Не указано";
            DeliveryTime = order.DeliveryTime?.ToString("dd.MM.yyyy HH:mm") ?? "Не указано";
            Comment = order.Comment ?? "Нет комментария";
            TotalPrice = order.TotalPrice;
            CreatedAt = order.CreatedAt.ToString("dd.MM.yyyy HH:mm");
            DeliveredAt = order.DeliveredAt?.ToString("dd.MM.yyyy HH:mm") ?? "—";
            
            OnPropertyChanged(nameof(IsCancelable));
            ((AsyncCommand)CancelOrderCommand).RaiseCanExecuteChanged();

            var orderItems = await orderItemRepo.GetByOrderIdAsync(order.Id);
            Items = new ObservableCollection<OrderItemViewModel>(
                orderItems.Select(oi => new OrderItemViewModel
                {
                    MenuItemName = oi.MenuItem?.Name ?? "Неизвестный товар",
                    Quantity = oi.Qty,
                    Price = oi.Price,
                    Total = oi.Qty * oi.Price
                })
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных заказа: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
            RequestClose?.Invoke();
        }
    }

    private string GetStatusText(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.pending => "В ожидании",
            OrderStatus.accepted => "Принят",
            OrderStatus.preparing => "В процессе приготовления",
            OrderStatus.delivering => "На доставке",
            OrderStatus.ready_for_pickup => "Готов к выдаче",
            OrderStatus.done => "Завершено",
            OrderStatus.canceled => "Отменено",
            _ => status.ToString()
        };
    }

    private string GetDeliveryTypeText(Tables.DeliveryType deliveryType)
    {
        return deliveryType switch
        {
            Tables.DeliveryType.pickup => "Самовывоз",
            Tables.DeliveryType.courier => "Доставка курьером",
            _ => deliveryType.ToString()
        };
    }

    private async Task CancelOrderAsync()
    {
        if (_currentStatus != OrderStatus.pending)
        {
            MessageBox.Show("Можно отменить только заказы со статусом 'В ожидании'.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirm = MessageBox.Show(
            "Вы действительно хотите отменить этот заказ?",
            "Подтверждение отмены",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
            return;

        try
        {
            using var orderRepo = new OrderRepository();
            var order = await orderRepo.GetByIdAsync(_orderId);
            
            if (order == null)
            {
                MessageBox.Show("Заказ не найден.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (order.Status != OrderStatus.pending)
            {
                MessageBox.Show("Статус заказа изменился. Отмена невозможна.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                await LoadDataAsync();
                return;
            }

            order.Status = OrderStatus.canceled;
            orderRepo.Update(order);
            await orderRepo.SaveChangesAsync();

            _currentStatus = OrderStatus.canceled;
            Status = GetStatusText(OrderStatus.canceled);
            OnPropertyChanged(nameof(IsCancelable));
            ((AsyncCommand)CancelOrderCommand).RaiseCanExecuteChanged();

            MessageBox.Show("Заказ успешно отменен.", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);

            OnOrderCanceled?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при отмене заказа: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Close()
    {
        RequestClose?.Invoke();
    }

    public class OrderItemViewModel
    {
        public string MenuItemName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total { get; set; }
    }
}

