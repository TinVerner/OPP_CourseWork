using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.SupportClasses;

namespace ООП_Курсовой.ViewModels;

public class CustomRollsWindowViewModel : BaseViewModel
{
    private ObservableCollection<RollViewModel> _userRolls = [];
    private ObservableCollection<RollViewModel> _filteredRolls = [];
    private ObservableCollection<SelectableIngredient> _selectableIngredients = [];

    private string _searchText = string.Empty;
    private string _sortMode = "none";

    public ObservableCollection<RollViewModel> FilteredRolls
    {
        get => _filteredRolls;
        set => SetProperty(ref _filteredRolls, value);
    }

    public ObservableCollection<SelectableIngredient> SelectableIngredients
    {
        get => _selectableIngredients;
        set => SetProperty(ref _selectableIngredients, value);
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
    public ICommand IngredientFilterChangedCommand { get; }
    public ICommand ResetFiltersCommand { get; }
    public ICommand DetailsCommand { get; }
    public ICommand AddToCartCommand { get; }
    public ICommand DeleteRollCommand { get; }
    public ICommand CreateRollCommand { get; }
    public Action? OpenAdminPanelRequested { get; set; }
    public Action? OpenProfileRequested { get; set; }
    public Action? OpenHomeRequested { get; set; }
    public Action? OpenMenuRollsRequested { get; set; }
    public Action? OpenMenuSetsRequested { get; set; }
    public Action? OpenCustomSetsRequested { get; set; }
    public Action? OpenCartRequested { get; set; }
    public Action? OpenOrdersRequested { get; set; }
    public Action? OpenCreateRollRequested { get; set; }
    public Action<RollViewModel>? OpenDetailsRequested { get; set; }

    public CustomRollsWindowViewModel()
    {
        LoadDataCommand = new AsyncCommand(LoadDataAsync);
        IngredientFilterChangedCommand = new Command(ApplyFilters);
        ResetFiltersCommand = new Command(ResetFilters);
        DetailsCommand = new Command<RollViewModel>(Details);
        AddToCartCommand = new AsyncCommand<long>(AddToCartAsync);
        DeleteRollCommand = new AsyncCommand<long>(DeleteRollAsync);
        CreateRollCommand = new Command(OpenCreateRoll);

        LoadDataCommand.Execute(null);
    }

    public async Task LoadDataAsync()
    {
        try
        {
            using var ingredientRepo = new IngredientRepository();
            using var menuItemRepo = new MenuItemRepository();

            var ingredients = await ingredientRepo.GetAllOrderedByNameAsync();

            SelectableIngredients.Clear();
            foreach (var ing in ingredients)
            {
                var selectable = new SelectableIngredient(ing);
                selectable.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(SelectableIngredient.IsSelected))
                        ApplyFilters();
                };
                SelectableIngredients.Add(selectable);
            }

            var rolls = await menuItemRepo.GetCustomItemsByUserIdAsync(CurrentUser.Id);
            var customRolls = rolls.Where(r => r.ItemType == "custom_roll").ToList();

            _userRolls.Clear();
            foreach (var r in customRolls)
            {
                _userRolls.Add(new RollViewModel
                {
                    Id = r.Id,
                    Name = r.Name,
                    Price = r.Price,
                    ImageUrl = r.ImageUrl,
                    IngredientIds = r.IngredientRolls.Select(ir => ir.IngredientId).ToList(),
                    IngredientNames = r.IngredientRolls.Select(ir => ir.Ingredient.Name).ToList(),
                    Description = r.Description,
                    ProteinG = r.ProteinG ?? 0m,
                    FatG = r.FatG ?? 0m,
                    CarbsG = r.CarbsG ?? 0m,
                    CaloriesKcal = r.CaloriesKcal ?? 0m
                });
            }

            ApplyFilters();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки: {ex.Message}");
        }
    }

    private void ApplyFilters()
    {
        var selectedIds = SelectableIngredients
            .Where(si => si.IsSelected)
            .Select(si => si.Id)
            .ToList();

        IEnumerable<RollViewModel> result = _userRolls;

        if (selectedIds.Any())
            result = result.Where(r => selectedIds.All(id => r.IngredientIds.Contains(id)));

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var searchLower = _searchText.Trim().ToLower();
            result = result.Where(r =>
                !string.IsNullOrEmpty(r.Name) &&
                r.Name.ToLower().Contains(searchLower));
        }

        result = _sortMode switch
        {
            "priceAsc" => result.OrderBy(r => r.Price),
            "priceDesc" => result.OrderByDescending(r => r.Price),
            _ => result
        };

        FilteredRolls = new ObservableCollection<RollViewModel>(result);
    }

    private void ResetFilters()
    {
        foreach (var si in SelectableIngredients)
            si.IsSelected = false;

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
            "Вы действительно хотите удалить этот ролл?",
            "Подтверждение удаления",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirm != MessageBoxResult.Yes)
            return;

        try
        {
            using var menuItemRepo = new MenuItemRepository();

            var roll = await menuItemRepo.GetByIdAsync(id);

            if (roll == null || roll.ItemType != "custom_roll")
            {
                MessageBox.Show("Ролл не найден в базе данных.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (CurrentUser.RoleId != 1 && roll.UserId != CurrentUser.Id)
            {
                MessageBox.Show("У вас нет прав на удаление этого ролла.", "Доступ запрещён",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            menuItemRepo.Remove(roll);
            await menuItemRepo.SaveChangesAsync();

            var toRemove = _userRolls.FirstOrDefault(r => r.Id == id);
            if (toRemove != null)
                _userRolls.Remove(toRemove);

            ApplyFilters();

            MessageBox.Show("Ролл успешно удалён.", "Готово",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при удалении ролла: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenCreateRoll()
    {
        OpenCreateRollRequested?.Invoke();
    }
}

