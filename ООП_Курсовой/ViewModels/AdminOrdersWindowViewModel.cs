using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.Tables;
using ООП_Курсовой.Views;

namespace ООП_Курсовой.ViewModels;

public class AdminOrdersWindowViewModel : BaseViewModel
{
    private ObservableCollection<AdminOrderViewModel> _allOrders = new();
    private ObservableCollection<AdminOrderViewModel> _filteredOrders = new();
    private OrderStatus? _selectedStatusFilter;

    public ObservableCollection<AdminOrderViewModel> FilteredOrders
    {
        get => _filteredOrders;
        set => SetProperty(ref _filteredOrders, value);
    }

    public List<OrderStatusFilter> StatusFilters { get; }

    private OrderStatusFilter? _selectedFilter;
    public OrderStatusFilter? SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            if (SetProperty(ref _selectedFilter, value))
            {
                _selectedStatusFilter = value?.Status;
                ApplyFilter();
            }
        }
    }

    public ICommand LoadDataCommand { get; }
    public ICommand ChangeStatusCommand { get; }
    public ICommand ViewDetailsCommand { get; }

    public Action? OpenAdminPanelRequested { get; set; }
    public Action<long>? OpenOrderDetailsRequested { get; set; }

    public AdminOrdersWindowViewModel()
    {
        StatusFilters = new List<OrderStatusFilter>
        {
            new OrderStatusFilter { Status = null, DisplayName = "Все заказы" },
            new OrderStatusFilter { Status = OrderStatus.pending, DisplayName = "В ожидании" },
            new OrderStatusFilter { Status = OrderStatus.accepted, DisplayName = "Принят" },
            new OrderStatusFilter { Status = OrderStatus.preparing, DisplayName = "В процессе приготовления" },
            new OrderStatusFilter { Status = OrderStatus.delivering, DisplayName = "На доставке" },
            new OrderStatusFilter { Status = OrderStatus.ready_for_pickup, DisplayName = "Готов к выдаче" },
            new OrderStatusFilter { Status = OrderStatus.done, DisplayName = "Завершено" },
            new OrderStatusFilter { Status = OrderStatus.canceled, DisplayName = "Отменено" }
        };

        SelectedFilter = StatusFilters[0];

        LoadDataCommand = new AsyncCommand(LoadDataAsync);
        ChangeStatusCommand = new AsyncCommand<AdminOrderViewModel>(ChangeStatusAsync);
        ViewDetailsCommand = new Command<long>(ViewDetails);

        LoadDataCommand.Execute(null);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            using var orderRepo = new OrderRepository();
            using var orderItemRepo = new OrderItemRepository();
            
            var allOrders = await orderRepo.GetAllAsync();
            var orders = allOrders.OrderByDescending(o => o.CreatedAt).ToList();
            
            using var userRepo = new UserRepository();
            foreach (var order in orders)
            {
                if (order.UserId.HasValue && order.User == null)
                {
                    order.User = await userRepo.GetByIdAsync(order.UserId.Value);
                }
            }

            var orderViewModels = new List<AdminOrderViewModel>();
            
            foreach (var order in orders)
            {
                var orderItems = await orderItemRepo.GetByOrderIdAsync(order.Id);
                var description = BuildOrderDescription(orderItems);
                
                orderViewModels.Add(new AdminOrderViewModel
                {
                    Id = order.Id,
                    Status = order.Status,
                    StatusText = GetStatusText(order.Status),
                    Description = description,
                    TotalPrice = order.TotalPrice,
                    CustomerName = order.CustomerName ?? order.User?.Firstname + " " + order.User?.Lastname ?? "Гость",
                    CustomerPhone = order.CustomerPhone ?? order.User?.Phone ?? "",
                    DeliveryType = order.DeliveryType,
                    DeliveryTypeText = GetDeliveryTypeText(order.DeliveryType),
                    CreatedAt = order.CreatedAt,
                    DeliveryTime = order.DeliveryTime,
                    AvailableStatuses = GetAvailableStatuses(order.Status, order.DeliveryType)
                });
            }
            
            _allOrders = new ObservableCollection<AdminOrderViewModel>(orderViewModels);
            ApplyFilter();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void ApplyFilter()
    {
        if (_selectedStatusFilter == null)
        {
            FilteredOrders = new ObservableCollection<AdminOrderViewModel>(_allOrders);
        }
        else
        {
            var filtered = _allOrders
                .Where(o => o.Status == _selectedStatusFilter.Value)
                .ToList();
            FilteredOrders = new ObservableCollection<AdminOrderViewModel>(filtered);
        }
    }

    private async Task ChangeStatusAsync(AdminOrderViewModel orderVm)
    {
        if (orderVm == null) return;

        try
        {
            using var orderRepo = new OrderRepository();
            var order = await orderRepo.GetByIdAsync(orderVm.Id);
            
            if (order == null)
            {
                MessageBox.Show("Заказ не найден.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var availableStatuses = orderVm.AvailableStatuses;
            if (!availableStatuses.Any())
            {
                MessageBox.Show("Нет доступных статусов для изменения.", "Информация",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var statusWindow = new StatusSelectionWindow(availableStatuses);
            if (statusWindow.ShowDialog() == true && statusWindow.SelectedStatus.HasValue)
            {
                var newStatus = statusWindow.SelectedStatus.Value;
                
                if (order.Status == newStatus)
                {
                    return;
                }

                order.Status = newStatus;
                
                if ((newStatus == OrderStatus.done || newStatus == OrderStatus.ready_for_pickup) && order.DeliveredAt == null)
                {
                    order.DeliveredAt = DateTimeOffset.UtcNow;
                }

                orderRepo.Update(order);
                await orderRepo.SaveChangesAsync();

                orderVm.Status = newStatus;
                orderVm.StatusText = GetStatusText(newStatus);
                orderVm.AvailableStatuses = GetAvailableStatuses(newStatus, order.DeliveryType);

                MessageBox.Show("Статус заказа успешно изменён.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadDataAsync();
            }
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException dbEx)
        {
            var innerMessage = dbEx.InnerException?.Message ?? dbEx.Message;
            var fullMessage = $"Ошибка базы данных при изменении статуса:\n{innerMessage}";
            
            if (dbEx.InnerException != null)
            {
                fullMessage += $"\n\nДетали: {dbEx.InnerException.GetType().Name}";
                if (dbEx.InnerException.InnerException != null)
                {
                    fullMessage += $"\nВнутренняя ошибка: {dbEx.InnerException.InnerException.Message}";
                }
            }

            MessageBox.Show(fullMessage, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            var errorMessage = $"Ошибка изменения статуса: {ex.Message}";
            if (ex.InnerException != null)
            {
                errorMessage += $"\n\nДетали: {ex.InnerException.Message}";
                if (ex.InnerException.InnerException != null)
                {
                    errorMessage += $"\nВнутренняя ошибка: {ex.InnerException.InnerException.Message}";
                }
            }

            MessageBox.Show(errorMessage, "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private List<OrderStatus> GetAvailableStatuses(OrderStatus currentStatus, DeliveryType deliveryType)
    {
        return currentStatus switch
        {
            OrderStatus.pending => new List<OrderStatus> { OrderStatus.accepted, OrderStatus.canceled },
            OrderStatus.accepted => new List<OrderStatus> { OrderStatus.preparing, OrderStatus.canceled },
            OrderStatus.preparing => deliveryType == DeliveryType.pickup 
                ? new List<OrderStatus> { OrderStatus.ready_for_pickup, OrderStatus.canceled }
                : new List<OrderStatus> { OrderStatus.delivering, OrderStatus.canceled },
            OrderStatus.delivering => new List<OrderStatus> { OrderStatus.done, OrderStatus.canceled },
            OrderStatus.ready_for_pickup => new List<OrderStatus> { OrderStatus.done, OrderStatus.canceled },
            OrderStatus.done => new List<OrderStatus>(),
            OrderStatus.canceled => new List<OrderStatus>(),
            _ => new List<OrderStatus>()
        };
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

    private string GetDeliveryTypeText(DeliveryType deliveryType)
    {
        return deliveryType switch
        {
            DeliveryType.pickup => "Самовывоз",
            DeliveryType.courier => "Доставка курьером",
            _ => deliveryType.ToString()
        };
    }

    private void ViewDetails(long orderId)
    {
        OpenOrderDetailsRequested?.Invoke(orderId);
    }

    public class OrderStatusFilter
    {
        public OrderStatus? Status { get; set; }
        public string DisplayName { get; set; } = "";
    }

    public class AdminOrderViewModel
    {
        public long Id { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusText { get; set; } = "";
        public string Description { get; set; } = "";
        public decimal TotalPrice { get; set; }
        public string CustomerName { get; set; } = "";
        public string CustomerPhone { get; set; } = "";
        public DeliveryType DeliveryType { get; set; }
        public string DeliveryTypeText { get; set; } = "";
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DeliveryTime { get; set; }
        public List<OrderStatus> AvailableStatuses { get; set; } = new();
    }
}

