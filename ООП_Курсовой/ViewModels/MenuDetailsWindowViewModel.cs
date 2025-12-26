using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.SupportClasses;

namespace ООП_Курсовой.ViewModels;

public class MenuDetailsWindowViewModel : BaseViewModel
{
    private RollViewModel _model;

    public RollViewModel Model
    {
        get => _model;
        set => SetProperty(ref _model, value);
    }

    public ICommand CloseCommand { get; }
    public ICommand AddToCartCommand { get; }

    public Action? RequestClose { get; set; }

    public MenuDetailsWindowViewModel(RollViewModel model)
    {
        Model = model;
        CloseCommand = new Command(Close);
        AddToCartCommand = new AsyncCommand(AddToCartAsync);
        
        LoadSetComponentsAsync();
    }

    private async Task LoadSetComponentsAsync()
    {
        if (Model?.ItemType != "set" && Model?.ItemType != "custom_set")
            return;

        try
        {
            using var setComponentRepo = new MenuSetComponentRepository();
            var components = await setComponentRepo.GetBySetIdAsync(Model.Id);

            if (components != null && components.Any())
            {
                var rollNames = components
                    .Where(c => c.Item != null)
                    .Select(c => c.Item.Name)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct()
                    .ToList();

                Model.IngredientNames = rollNames;
                OnPropertyChanged(nameof(Model));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Ошибка загрузки компонентов сета: {ex.Message}");
        }
    }

    private void Close()
    {
        RequestClose?.Invoke();
    }

    private async Task AddToCartAsync()
    {
        if (Model == null)
        {
            RequestClose?.Invoke();
            return;
        }

        try
        {
            using var menuItemRepo = new MenuItemRepository();
            var item = await menuItemRepo.GetByIdAsync(Model.Id);

            if (item == null)
            {
                MessageBox.Show("Позиция не найдена.");
                return;
            }

            await Cart.AddAsync(item);
            MessageBox.Show($"«{item.Name}» добавлен(а) в корзину.",
                "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);

            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка добавления в корзину: {ex.Message}");
        }
    }
}

