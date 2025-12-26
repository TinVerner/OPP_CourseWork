using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.Views;

public partial class StatusSelectionWindow : Window
{
    public OrderStatus? SelectedStatus { get; private set; }

    private readonly Dictionary<string, OrderStatus> _statusMap = new()
    {
        { "Принят", OrderStatus.accepted },
        { "В процессе приготовления", OrderStatus.preparing },
        { "На доставке", OrderStatus.delivering },
        { "Готов к выдаче", OrderStatus.ready_for_pickup },
        { "Завершено", OrderStatus.done },
        { "Отменено", OrderStatus.canceled }
    };

    public StatusSelectionWindow(List<OrderStatus> availableStatuses)
    {
        InitializeComponent();

        var statusItems = availableStatuses.Select(status =>
        {
            var text = status switch
            {
                OrderStatus.accepted => "Принят",
                OrderStatus.preparing => "В процессе приготовления",
                OrderStatus.delivering => "На доставке",
                OrderStatus.ready_for_pickup => "Готов к выдаче",
                OrderStatus.done => "Завершено",
                OrderStatus.canceled => "Отменено",
                _ => status.ToString()
            };
            return new { Status = status, Text = text };
        }).ToList();

        StatusListBox.ItemsSource = statusItems;
        StatusListBox.DisplayMemberPath = "Text";
        StatusListBox.SelectedValuePath = "Status";
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        if (StatusListBox.SelectedValue is OrderStatus status)
        {
            SelectedStatus = status;
            DialogResult = true;
            Close();
        }
        else
        {
            MessageBox.Show("Выберите статус.", "Внимание",
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

