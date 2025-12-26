using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ООП_Курсовой.Tables;
using ООП_Курсовой.Repository;

namespace ООП_Курсовой.SupportClasses;

public static class Cart
{
    public class CartItem : INotifyPropertyChanged
    {
        public long MenuItemId { get; set; }

        public string Name { get; set; } = "";

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public decimal Price { get; set; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value) return;
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Total));
            }
        }

        public decimal Total => Price * Quantity;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public static ObservableCollection<CartItem> Items { get; } = new();

    public static async Task AddAsync(MenuItem item)
    {
        var existing = Items.FirstOrDefault(i => i.MenuItemId == item.Id);
        if (existing != null)
        {
            existing.Quantity++;
        }
        else
        {
            string? description = item.Description;
            
            if (item.ItemType == "set" || item.ItemType == "custom_set")
            {
                try
                {
                    using var setComponentRepo = new MenuSetComponentRepository();
                    var components = await setComponentRepo.GetBySetIdAsync(item.Id);
                    
                    if (components != null && components.Any())
                    {
                        var rollNames = components
                            .Where(c => c.Item != null)
                            .Select(c => c.Item.Name)
                            .Where(n => !string.IsNullOrWhiteSpace(n))
                            .Distinct()
                            .ToList();
                        
                        if (rollNames.Any())
                        {
                            description = string.Join(", ", rollNames);
                        }
                    }
                }
                catch
                {
                    description = item.Description;
                }
            }
            
            Items.Add(new CartItem
            {
                MenuItemId = item.Id,
                Name = item.Name,
                Description = description,
                ImageUrl = item.ImageUrl,
                Price = item.Price,
                Quantity = 1
            });
        }
    }

    public static void Add(MenuItem item)
    {
        AddAsync(item).GetAwaiter().GetResult();
    }

    public static void ChangeQuantity(long menuItemId, int delta)
    {
        var existing = Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
        if (existing == null) return;

        existing.Quantity += delta;
        if (existing.Quantity <= 0)
            Items.Remove(existing);
    }

    public static void Remove(long menuItemId)
    {
        var existing = Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
        if (existing != null)
            Items.Remove(existing);
    }

    public static decimal GetTotal() => Items.Sum(i => i.Total);

    public static void Clear() => Items.Clear();
}
