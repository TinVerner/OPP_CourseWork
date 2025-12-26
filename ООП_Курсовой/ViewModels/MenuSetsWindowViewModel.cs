using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.SupportClasses;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.ViewModels;

public class MenuSetsWindowViewModel : BaseViewModel
{
    private ObservableCollection<RollViewModel> _allSets = new();
    private ObservableCollection<RollViewModel> _filteredSets = new();
    private ObservableCollection<Category> _allCategories = new();
    private long? _selectedCategoryId;

    private string _searchText = string.Empty;
    private string _sortMode = "none";

    public ObservableCollection<RollViewModel> FilteredSets
    {
        get => _filteredSets;
        set => SetProperty(ref _filteredSets, value);
    }

    public ObservableCollection<Category> Categories
    {
        get => _allCategories;
        set => SetProperty(ref _allCategories, value);
    }

    public long? SelectedCategoryId
    {
        get => _selectedCategoryId;
        set
        {
            if (SetProperty(ref _selectedCategoryId, value))
                ApplyFilters();
        }
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
    public ICommand CategoryFilterChangedCommand { get; }
    public ICommand ResetFiltersCommand { get; }
    public ICommand DetailsCommand { get; }
    public ICommand AddToCartCommand { get; }
    public Action? OpenAdminPanelRequested { get; set; }
    public Action? OpenProfileRequested { get; set; }
    public Action? OpenHomeRequested { get; set; }
    public Action? OpenCartRequested { get; set; }
    public Action? OpenCustomRollsRequested { get; set; }
    public Action? OpenCustomSetsRequested { get; set; }
    public Action? OpenMenuRollsRequested { get; set; }
    public Action? OpenMenuSetsRequested { get; set; }
    public Action? OpenOrdersRequested { get; set; }
    public Action<RollViewModel>? OpenDetailsRequested { get; set; }

    public MenuSetsWindowViewModel()
    {
        LoadDataCommand = new AsyncCommand(LoadDataAsync);
        CategoryFilterChangedCommand = new Command<object>(categoryObj => 
        { 
            if (categoryObj is long id)
                SelectedCategoryId = id;
            else
                SelectedCategoryId = null;
        });
        ResetFiltersCommand = new Command(ResetFilters);
        DetailsCommand = new Command<RollViewModel>(Details);
        AddToCartCommand = new AsyncCommand<long>(AddToCartAsync);

        LoadDataCommand.Execute(null);
    }

    public async Task LoadDataAsync()
    {
        try
        {
            using var categoryRepo = new CategoryRepository();
            using var menuItemRepo = new MenuItemRepository();

            var categories = await categoryRepo.GetAllOrderedByNameAsync();

            Categories = new ObservableCollection<Category>(categories);

            var sets = await menuItemRepo.GetByItemTypeAsync("set");

            _allSets.Clear();
            foreach (var s in sets)
            {
                _allSets.Add(new RollViewModel
                {
                    Id = s.Id,
                    Name = s.Name,
                    Price = s.Price,
                    ImageUrl = s.ImageUrl,
                    CategoryId = s.CategoryId,
                    IngredientNames = string.IsNullOrWhiteSpace(s.Description)
                        ? new List<string>()
                        : s.Description
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(p => p.Trim())
                            .ToList(),
                    Description = s.Description,
                    ProteinG = s.ProteinG,
                    FatG = s.FatG,
                    CarbsG = s.CarbsG,
                    CaloriesKcal = s.CaloriesKcal,
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

        if (_selectedCategoryId.HasValue)
        {
            result = result.Where(s =>
                s.CategoryId.HasValue &&
                s.CategoryId.Value == _selectedCategoryId.Value);
        }

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
        SelectedCategoryId = null;
        SortMode = "none";
        SearchText = string.Empty;
        OnPropertyChanged(nameof(SelectedCategoryId));
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
}

