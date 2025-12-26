using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.SupportClasses;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.ViewModels;

public class MyOrdersWindowViewModel : BaseViewModel
{
    private ObservableCollection<OrderViewModel> _orders = new();
    public ObservableCollection<OrderViewModel> Orders
    {
        get => _orders;
        set => SetProperty(ref _orders, value);
    }

    public ICommand LoadDataCommand { get; }
    public ICommand ViewDetailsCommand { get; }
    public ICommand CancelOrderCommand { get; }
    public Action? OpenProfileRequested { get; set; }
    public Action? OpenHomeRequested { get; set; }
    public Action? OpenMenuRollsRequested { get; set; }
    public Action? OpenMenuSetsRequested { get; set; }
    public Action? OpenCustomRollsRequested { get; set; }
    public Action? OpenCustomSetsRequested { get; set; }
    public Action? OpenCartRequested { get; set; }
    public Action? OpenAdminPanelRequested { get; set; }
    public Action<long>? OpenOrderDetailsRequested { get; set; }

    public MyOrdersWindowViewModel()
    {
        LoadDataCommand = new AsyncCommand(LoadDataAsync);
        ViewDetailsCommand = new Command<long>(ViewDetails);
        CancelOrderCommand = new AsyncCommand<long>(CancelOrderAsync);
    }

    public async Task LoadDataAsync()
    {
        if (!CurrentUser.IsAuthenticated)
        {
            Orders.Clear();
            return;
        }

        try
        {
            using var orderRepo = new OrderRepository();
            using var orderItemRepo = new OrderItemRepository();
            var orders = await orderRepo.GetByUserIdAsync(CurrentUser.Id);

            var orderViewModels = new List<OrderViewModel>();
            
            foreach (var order in orders)
            {
                var orderItems = await orderItemRepo.GetByOrderIdAsync(order.Id);
                var description = BuildOrderDescription(orderItems);
                
                orderViewModels.Add(new OrderViewModel
                {
                    Id = order.Id,
                    Status = GetStatusText(order.Status),
                    Description = description,
                    TotalPrice = order.TotalPrice,
                    CanCancel = order.Status == OrderStatus.pending
                });
            }
            
            Orders = new ObservableCollection<OrderViewModel>(orderViewModels);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string BuildOrderDescription(List<OrderItem> orderItems)
    {
        if (orderItems == null || !orderItems.Any())
            return "Нет товаров";

        var items = orderItems
            .Where(oi => oi.MenuItem != null)
            .Select(oi => oi.Qty > 1 
                ? $"{oi.MenuItem.Name} × {oi.Qty}" 
                : oi.MenuItem.Name)
            .ToList();

        return string.Join(", ", items);
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

    private void ViewDetails(long orderId)
    {
        OpenOrderDetailsRequested?.Invoke(orderId);
    }

    private async Task CancelOrderAsync(long orderId)
    {
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
            var order = await orderRepo.GetByIdAsync(orderId);
            
            if (order == null)
            {
                MessageBox.Show("Заказ не найден.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (order.Status != OrderStatus.pending)
            {
                MessageBox.Show("Можно отменить только заказы со статусом 'В ожидании'.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                await LoadDataAsync(); // Обновляем список
                return;
            }

            order.Status = OrderStatus.canceled;
            orderRepo.Update(order);
            await orderRepo.SaveChangesAsync();

            MessageBox.Show("Заказ успешно отменен.", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);

            await LoadDataAsync();
        }
        catch (Exception ex)
        {
                MessageBox.Show($"Ошибка при отмене заказа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

    public class OrderViewModel
    {
        public long Id { get; set; }
        public string Status { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal TotalPrice { get; set; }
        public bool CanCancel { get; set; }
    }
}

