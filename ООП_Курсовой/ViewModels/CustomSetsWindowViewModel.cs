using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.SupportClasses;

namespace ООП_Курсовой.ViewModels;

public class CustomSetsWindowViewModel : BaseViewModel
{
    private ObservableCollection<RollViewModel> _allSets = new();
    private ObservableCollection<RollViewModel> _filteredSets = new();

    private string _searchText = string.Empty;
    private string _sortMode = "none";

    public ObservableCollection<RollViewModel> FilteredSets
    {
        get => _filteredSets;
        set => SetProperty(ref _filteredSets, value);
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
    public ICommand ResetFiltersCommand { get; }
    public ICommand DetailsCommand { get; }
    public ICommand AddToCartCommand { get; }
    public ICommand DeleteRollCommand { get; }
    public ICommand CreateSetCommand { get; }
    public Action? OpenAdminPanelRequested { get; set; }
    public Action? OpenProfileRequested { get; set; }
    public Action? OpenHomeRequested { get; set; }
    public Action? OpenMenuRollsRequested { get; set; }
    public Action? OpenMenuSetsRequested { get; set; }
    public Action? OpenCustomRollsRequested { get; set; }
    public Action? OpenCartRequested { get; set; }
    public Action? OpenOrdersRequested { get; set; }
    public Action? OpenCreateSetRequested { get; set; }
    public Action<RollViewModel>? OpenDetailsRequested { get; set; }

    public CustomSetsWindowViewModel()
    {
        LoadDataCommand = new AsyncCommand(LoadDataAsync);
        ResetFiltersCommand = new Command(ResetFilters);
        DetailsCommand = new Command<RollViewModel>(Details);
        AddToCartCommand = new AsyncCommand<long>(AddToCartAsync);
        DeleteRollCommand = new AsyncCommand<long>(DeleteRollAsync);
        CreateSetCommand = new Command(OpenCreateSet);
    }

    public async Task LoadDataAsync()
    {
        try
        {
            using var menuItemRepo = new MenuItemRepository();
            using var setComponentRepo = new MenuSetComponentRepository();

            var customItems = await menuItemRepo.GetCustomItemsByUserIdAsync(CurrentUser.Id);
            var sets = customItems.Where(s => s.ItemType == "custom_set").ToList();

            _allSets.Clear();
            foreach (var s in sets)
            {
                var components = await setComponentRepo.GetBySetIdAsync(s.Id);
                var rollNames = components
                    .Where(c => c.Item != null)
                    .Select(c => c.Item.Name)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct()
                    .ToList();

                _allSets.Add(new RollViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    ImageUrl = s.ImageUrl,
                    CategoryId = s.CategoryId,
                    Description = s.Description,
                    ProteinG = s.ProteinG,
                    FatG = s.FatG,
                    CarbsG = s.CarbsG,
                    CaloriesKcal = s.CaloriesKcal,
                    IngredientNames = rollNames,
                    ItemType = s.ItemType
                });
            }

            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки меню: {ex.Message}");
        }
    }

    private void ApplyFilters()
    {
        IEnumerable<RollViewModel> result = _allSets;

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var searchLower = _searchText.Trim().ToLower();
            result = result.Where(s =>
                !string.IsNullOrEmpty(s.Name) &&
                s.Name.ToLower().Contains(searchLower));
        }

        result = _sortMode switch
        {
            "priceAsc" => result.OrderBy(s => s.Price),
            "priceDesc" => result.OrderByDescending(s => s.Price),
            _ => result
        };

        FilteredSets = new ObservableCollection<RollViewModel>(result);
    }

    private void ResetFilters()
    {
        SearchText = string.Empty;
        SortMode = "none";
    }

    private void Details(RollViewModel? vm)
    {
        if (vm == null) return;
        OpenDetailsRequested?.Invoke(vm);
    }

    private async Task AddToCartAsync(long id)
    {
        try
        {
            using var menuItemRepo = new MenuItemRepository();
            var item = await menuItemRepo.GetByIdAsync(id);

            if (item == null)
            {
                MessageBox.Show("Позиция не найдена.");
                return;
            }

            await Cart.AddAsync(item);
            MessageBox.Show($"«{item.Name}» добавлен(а) в корзину.",
                "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка добавления в корзину: {ex.Message}");
        }
    }

    private async Task DeleteRollAsync(long id)
    {
        var confirm = MessageBox.Show(
            "Вы действительно хотите удалить этот сет?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
            return;

        try
        {
            using var menuItemRepo = new MenuItemRepository();

            var set = await menuItemRepo.GetByIdAsync(id);

            if (set == null || set.ItemType != "custom_set")
            {
                MessageBox.Show("Сет не найден в базе данных.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CurrentUser.RoleId != 1 && set.UserId != CurrentUser.Id)
            {
                MessageBox.Show("У вас нет прав на удаление этого сета.", "Доступ запрещён",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            menuItemRepo.Remove(set);
            await menuItemRepo.SaveChangesAsync();

            var toRemove = _allSets.FirstOrDefault(s => s.Id == id);
            if (toRemove != null)
                _allSets.Remove(toRemove);

            ApplyFilters();

            MessageBox.Show("Сет успешно удалён.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при удалении сета: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenCreateSet()
    {
        OpenCreateSetRequested?.Invoke();
    }
}

