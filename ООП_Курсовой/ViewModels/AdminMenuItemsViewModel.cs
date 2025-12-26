using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.SupportClasses;

namespace ООП_Курсовой.ViewModels;

public class AdminMenuItemsViewModel : BaseViewModel
{
    private ObservableCollection<RollViewModel> _allItems = new();
    private ObservableCollection<RollViewModel> _filteredItems = new();

    private string _searchText = string.Empty;
    private string _selectedItemType = "all";
    private string _sortMode = "priceAsc";

    public ObservableCollection<RollViewModel> FilteredItems
    {
        get => _filteredItems;
        set => SetProperty(ref _filteredItems, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
                ApplyFilters();
        }
    }

    public string SelectedItemType
    {
        get => _selectedItemType;
        set
        {
            if (SetProperty(ref _selectedItemType, value))
                ApplyFilters();
        }
    }

    public string SortMode
    {
        get => _sortMode;
        set
        {
            if (SetProperty(ref _sortMode, value))
                ApplyFilters();
        }
    }

    public ICommand LoadDataCommand { get; }
    public ICommand DetailsCommand { get; }
    public ICommand EditCommand { get; }
    public ICommand DeleteCommand { get; }
    public ICommand CreateRollCommand { get; }
    public ICommand CreateSetCommand { get; }
    public ICommand ResetFiltersCommand { get; }

    public Action<long>? OpenDetailsRequested { get; set; }
    public Action<long>? OpenEditRequested { get; set; }
    public Action? OpenCreateRollRequested { get; set; }
    public Action? OpenCreateSetRequested { get; set; }
    public Action? OpenAdminPanelRequested { get; set; }
    public Action? OpenProfileRequested { get; set; }
    public Action? OpenUsersRequested { get; set; }
    public Action? RefreshDataRequested { get; set; }

    public AdminMenuItemsViewModel()
    {
        LoadDataCommand = new AsyncCommand(LoadDataAsync);
        DetailsCommand = new Command<long>(id => OpenDetailsRequested?.Invoke(id));
        EditCommand = new Command<long>(id => OpenEditRequested?.Invoke(id));
        DeleteCommand = new AsyncCommand<long>(DeleteAsync);
        CreateRollCommand = new Command(() => OpenCreateRollRequested?.Invoke());
        CreateSetCommand = new Command(() => OpenCreateSetRequested?.Invoke());
        ResetFiltersCommand = new Command(ResetFilters);

        LoadDataCommand.Execute(null);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            using var repo = new MenuItemRepository();
            var list = await repo.GetAllAsync();

            _allItems.Clear();
            foreach (var item in list)
            {
                var rollVm = new RollViewModel
                {
                    Id = item.Id,
                    Name = item.Name,
                    Price = item.Price,
                    ImageUrl = item.ImageUrl,
                    CategoryId = item.CategoryId,
                    Description = item.Description,
                    ProteinG = item.ProteinG,
                    FatG = item.FatG,
                    CarbsG = item.CarbsG,
                    CaloriesKcal = item.CaloriesKcal,
                    ItemType = item.ItemType
                };

                if (item.ItemType == "roll" || item.ItemType == "custom_roll")
                {
                    var itemWithIngredients = await repo.GetByIdWithIngredientsAsync(item.Id);
                    if (itemWithIngredients?.IngredientRolls != null)
                    {
                        rollVm.IngredientIds = itemWithIngredients.IngredientRolls
                            .Select(ir => ir.IngredientId)
                            .ToList();
                        rollVm.IngredientNames = itemWithIngredients.IngredientRolls
                            .Select(ir => ir.Ingredient?.Name ?? "")
                            .ToList();
                    }
                }

                rollVm.ItemTypeDisplay = GetItemTypeDisplay(item.ItemType);

                _allItems.Add(rollVm);
            }

            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки позиций меню: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string GetItemTypeDisplay(string? itemType)
    {
        return itemType switch
        {
            "roll" => "Ролл",
            "set" => "Сет",
            "custom_roll" => "Кастомный ролл",
            "custom_set" => "Кастомный сет",
            _ => itemType ?? "Неизвестно"
        };
    }

    private void ApplyFilters()
    {
        IEnumerable<RollViewModel> result = _allItems;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var searchLower = SearchText.Trim().ToLower();
            result = result.Where(i =>
                !string.IsNullOrEmpty(i.Name) &&
                i.Name.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(SelectedItemType) && SelectedItemType != "all")
        {
            result = result.Where(i =>
                string.Equals(i.ItemType, SelectedItemType, StringComparison.OrdinalIgnoreCase));
        }

        result = SortMode switch
        {
            "priceDesc" => result.OrderByDescending(i => i.Price),
            _ => result.OrderBy(i => i.Price)
        };

        FilteredItems = new ObservableCollection<RollViewModel>(result);
    }

    private void ResetFilters()
    {
        SelectedItemType = "all";
        SearchText = string.Empty;
        SortMode = "priceAsc";
        OnPropertyChanged(nameof(SelectedItemType));
    }

    private async Task DeleteAsync(long id)
    {
        var result = MessageBox.Show(
            "Удалить эту позицию меню?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            using var repo = new MenuItemRepository();
            var item = await repo.GetByIdAsync(id);

            if (item == null)
            {
                MessageBox.Show("Позиция не найдена.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            repo.Remove(item);
            await repo.SaveChangesAsync();

            MessageBox.Show("Позиция удалена.", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);

            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка удаления: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}