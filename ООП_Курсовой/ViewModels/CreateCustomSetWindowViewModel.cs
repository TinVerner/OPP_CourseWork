using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.SupportClasses;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.ViewModels;

public class CreateCustomSetWindowViewModel : BaseViewModel
{
    private const decimal BasePrice = 30.00m;
    private const decimal PricePerRoll = 6.99m;

    private ObservableCollection<SelectableRoll> _rollOptions = new();

    public ObservableCollection<SelectableRoll> RollOptions
    {
        get => _rollOptions;
        set => SetProperty(ref _rollOptions, value);
    }

    private string _setName = string.Empty;
    public string SetName
    {
        get => _setName;
        set => SetProperty(ref _setName, value);
    }

    private string _selectedRollsText = "Роллы пока не выбраны.";
    public string SelectedRollsText
    {
        get => _selectedRollsText;
        set => SetProperty(ref _selectedRollsText, value);
    }

    private string _macroSummaryText = "Б/Ж/У и калории появятся после выбора роллов.";
    public string MacroSummaryText
    {
        get => _macroSummaryText;
        set => SetProperty(ref _macroSummaryText, value);
    }

    private string _priceText = $"Цена: {BasePrice:0.00} руб.";
    public string PriceText
    {
        get => _priceText;
        set => SetProperty(ref _priceText, value);
    }

    private decimal _totalPrice = BasePrice;
    public decimal TotalPrice
    {
        get => _totalPrice;
        set => SetProperty(ref _totalPrice, value);
    }

    private int _selectedCount;
    public int SelectedCount
    {
        get => _selectedCount;
        set => SetProperty(ref _selectedCount, value);
    }

    private string _imageUrl = "";
    public string ImageUrl
    {
        get => _imageUrl;
        set
        {
            if (SetProperty(ref _imageUrl, value))
            {
                OnPropertyChanged(nameof(DisplayImageUrl));
            }
        }
    }

    public string DisplayImageUrl => string.IsNullOrWhiteSpace(ImageUrl) ? DefaultImagePath : ImageUrl;

    private const string DefaultImagePath = "D:\\Yes\\Уник\\3 курс\\5 семестр\\Курсовая\\Приложение\\ООП_Курсовой\\src\\Sets\\set.png";

    private ObservableCollection<Category> _categories = new();
    public ObservableCollection<Category> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    private long? _selectedCategoryId;
    public long? SelectedCategoryId
    {
        get => _selectedCategoryId;
        set => SetProperty(ref _selectedCategoryId, value);
    }

    private Visibility _adminPanelVisibility = Visibility.Collapsed;
    public Visibility AdminPanelVisibility
    {
        get => _adminPanelVisibility;
        set => SetProperty(ref _adminPanelVisibility, value);
    }

    public ICommand LoadDataCommand { get; }
    public ICommand LoadCategoriesCommand { get; }
    public ICommand CategoryChangedCommand { get; }
    public ICommand ResetSelectionCommand { get; }
    public ICommand CreateSetOnlyCommand { get; }
    public ICommand CreateSetAndAddToCartCommand { get; }
    public ICommand BrowseImageCommand { get; }
    // Клиентские команды
    public ICommand HomeCommand { get; }
    public ICommand MenuCommand { get; }
    public ICommand CartCommand { get; }
    public ICommand CustomRollsCommand { get; }
    public ICommand CustomSetsCommand { get; }
    public ICommand ProfileCommand { get; }
    // Админские команды
    public ICommand AdminPanelCommand { get; }
    public ICommand CreateRollNavCommand { get; }
    public ICommand CreateSetNavCommand { get; }
    public ICommand ViewUsersCommand { get; }
    public ICommand ViewMenuItemsCommand { get; }
    public ICommand ProfileNavCommand { get; }
    public ICommand LogoutCommand { get; }

    public Action? RequestClose { get; set; }
    public Action? OpenHomeRequested { get; set; }
    public Action? OpenMenuRequested { get; set; }
    public Action? OpenCartRequested { get; set; }
    public Action? OpenProfileRequested { get; set; }
    public Action? OpenLoginRequested { get; set; }
    public Action? OpenCustomRollsRequested { get; set; }
    public Action? OpenCustomSetsRequested { get; set; }
    public Action? OpenAdminPanelRequested { get; set; }
    public Action? OpenCreateRollRequested { get; set; }
    public Action? OpenCreateSetRequested { get; set; }
    public Action? OpenUsersRequested { get; set; }
    public Action? OpenMenuItemsRequested { get; set; }

    public CreateCustomSetWindowViewModel()
    {
        LoadDataCommand = new AsyncCommand(LoadDataAsync);
        LoadCategoriesCommand = new AsyncCommand(LoadCategoriesAsync);
        CategoryChangedCommand = new Command<object>(categoryObj =>
        {
            if (categoryObj is long id)
                SelectedCategoryId = id;
            else
                SelectedCategoryId = null;
        });
        ResetSelectionCommand = new Command(ResetSelection);
        CreateSetOnlyCommand = new AsyncCommand(CreateSetOnlyAsync);
        CreateSetAndAddToCartCommand = new AsyncCommand(CreateSetAndAddToCartAsync);
        BrowseImageCommand = new Command(BrowseImage);

        LoadDataCommand.Execute(null);
        
        if (CurrentUser.RoleId == 1)
        {
            LoadCategoriesCommand.Execute(null);
        }
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            using var categoryRepo = new CategoryRepository();
            var cats = await categoryRepo.GetAllOrderedByNameAsync();
            Categories = new ObservableCollection<Category>(cats);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void BrowseImage()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Изображения|*.png;*.jpg;*.jpeg;*.webp;*.bmp|Все файлы|*.*"
        };

        if (dlg.ShowDialog() == true)
        {
            ImageUrl = dlg.FileName;
        }
    }

    private async Task LoadDataAsync()
    {
        try
        {
            using var menuItemRepo = new MenuItemRepository();
            var allItems = await menuItemRepo.GetAllAsync();

            var baseRolls = allItems
                .Where(m => m.UserId == null && m.ItemType == "roll")
                .OrderBy(m => m.Name)
                .ToList();

            RollOptions.Clear();

            foreach (var roll in baseRolls)
            {
                var selectable = new SelectableRoll
                {
                    Id = roll.Id,
                    Name = roll.Name,
                    Description = roll.Description,
                    Price = roll.Price,
                    ImageUrl = roll.ImageUrl,
                    ProteinG = roll.ProteinG,
                    FatG = roll.FatG,
                    CarbsG = roll.CarbsG,
                    CaloriesKcal = roll.CaloriesKcal
                };

                selectable.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(SelectableRoll.IsSelected))
                        UpdateSelectionSummary();
                };

                RollOptions.Add(selectable);
            }

            UpdateSelectionSummary();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки роллов: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateSelectionSummary()
    {
        var selected = RollOptions.Where(r => r.IsSelected).ToList();
        SelectedCount = selected.Count;

        if (!selected.Any())
        {
            SelectedRollsText = "Роллы пока не выбраны.";
            MacroSummaryText = "Б/Ж/У и калории появятся после выбора роллов.";
            TotalPrice = BasePrice;
            PriceText = $"Цена: {TotalPrice:0.00} руб.";
            return;
        }

        SelectedRollsText = string.Join(", ", selected.Select(r => r.Name));

        var totalProtein = selected.Sum(r => r.ProteinG ?? 0m);
        var totalFat = selected.Sum(r => r.FatG ?? 0m);
        var totalCarbs = selected.Sum(r => r.CarbsG ?? 0m);
        var totalCalories = selected.Sum(r => r.CaloriesKcal ?? 0m);

        MacroSummaryText =
            $"Белки: {totalProtein:0.##} г · Жиры: {totalFat:0.##} г · Углеводы: {totalCarbs:0.##} г · Калории: {totalCalories:0.##} ккал";

        TotalPrice = BasePrice + PricePerRoll * selected.Count;
        PriceText = $"Цена: {TotalPrice:0.00} руб.";
    }

    private void ResetSelection()
    {
        foreach (var roll in RollOptions)
            roll.IsSelected = false;

        SetName = string.Empty;
        ImageUrl = string.Empty;
        SelectedCategoryId = null;
        UpdateSelectionSummary();
    }

    private async Task<MenuItem?> CreateSetAsync(bool addToCart)
    {
        var selected = RollOptions.Where(r => r.IsSelected).ToList();

        if (selected.Count < 2)
        {
            MessageBox.Show("Выберите как минимум два ролла.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        if (!CurrentUser.IsAuthenticated)
        {
            MessageBox.Show("Необходимо войти в систему, чтобы создавать сеты.",
                "Авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
            return null;
        }

        var name = string.IsNullOrWhiteSpace(SetName)
            ? $"Мой сет {DateTime.Now:HH:mm}"
            : SetName.Trim();

        var rollsCount = selected.Count;
        var piecesPerRoll = 8;
        var totalPieces = rollsCount * piecesPerRoll;

        var rollsLines = selected.Select(r =>
        {
            var baseDesc = string.IsNullOrWhiteSpace(r.Description)
                ? "описание отсутствует"
                : r.Description!.Trim();
            return $"{r.Name} — {baseDesc} ({piecesPerRoll} шт.)";
        });

        var description =
            $"Сет из {rollsCount} роллов, всего {totalPieces} шт.\n" +
            string.Join("\n", rollsLines);
        var totalProtein = selected.Sum(r => r.ProteinG ?? 0m);
        var totalFat = selected.Sum(r => r.FatG ?? 0m);
        var totalCarbs = selected.Sum(r => r.CarbsG ?? 0m);
        var totalCalories = selected.Sum(r => r.CaloriesKcal ?? 0m);
        var price = BasePrice + PricePerRoll * selected.Count;

        bool isAdmin = CurrentUser.RoleId == 1;
        string itemType = isAdmin ? "set" : "custom_set";
        long? userId = isAdmin ? null : CurrentUser.Id;
        
        if (isAdmin && !SelectedCategoryId.HasValue)
        {
            MessageBox.Show("Выберите категорию для сета.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        try
        {
            using var menuRepo = new MenuItemRepository();
            using var componentRepo = new MenuSetComponentRepository();

            var imageUrl = string.IsNullOrWhiteSpace(ImageUrl) ? DefaultImagePath : ImageUrl;

            var menuItem = new MenuItem
            {
                ItemType = itemType,
                Name = name,
                Description = description,
                Price = price,
                CategoryId = isAdmin ? SelectedCategoryId : null,
                ImageUrl = imageUrl,
                UserId = userId,
                ProteinG = totalProtein,
                FatG = totalFat,
                CarbsG = totalCarbs,
                CaloriesKcal = totalCalories,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await menuRepo.AddAsync(menuItem);
            await menuRepo.SaveChangesAsync();

            foreach (var roll in selected)
            {
                var component = new MenuSetComponent
                {
                    SetId = menuItem.Id,
                    ItemId = roll.Id,
                    Qty = 1
                };
                await componentRepo.AddAsync(component);
            }
            await componentRepo.SaveChangesAsync();

            if (addToCart)
                await Cart.AddAsync(menuItem);

            return menuItem;
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.Message ?? "";
            MessageBox.Show($"Ошибка сохранения сета: {ex.Message}\n{inner}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return null;
        }
    }

    private async Task CreateSetOnlyAsync()
    {
        var set = await CreateSetAsync(addToCart: false);
        if (set != null)
        {
            MessageBox.Show($"Сет «{set.Name}» успешно создан!",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            
            OpenCustomSetsRequested?.Invoke();
            ResetSelection();
            RequestClose?.Invoke();
        }
    }

    private async Task CreateSetAndAddToCartAsync()
    {
        var set = await CreateSetAsync(addToCart: true);
        if (set != null)
        {
            MessageBox.Show($"Сет «{set.Name}» создан и добавлен в корзину!",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            OpenCustomSetsRequested?.Invoke();
            ResetSelection();
            RequestClose?.Invoke();
        }
    }

    public class SelectableRoll : ObservableObject
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public decimal Price { get; set; }
        public decimal? ProteinG { get; set; }
        public decimal? FatG { get; set; }
        public decimal? CarbsG { get; set; }
        public decimal? CaloriesKcal { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }
    }
}