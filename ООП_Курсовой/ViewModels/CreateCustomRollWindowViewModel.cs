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

public class CreateCustomRollWindowViewModel : BaseViewModel
{
    private const int MaxIngredients = 5;
    private ObservableCollection<SelectableIngredient> _ingredientOptions = new();

    private string _rollName = "";

    public ObservableCollection<SelectableIngredient> IngredientOptions
    {
        get => _ingredientOptions;
        set => SetProperty(ref _ingredientOptions, value);
    }

    public string RollName
    {
        get => _rollName;
        set => SetProperty(ref _rollName, value);
    }

    private int _selectedCount;
    public int SelectedCount
    {
        get => _selectedCount;
        set => SetProperty(ref _selectedCount, value);
    }

    private string _selectedCountText = "";
    public string SelectedCountText
    {
        get => _selectedCountText;
        set => SetProperty(ref _selectedCountText, value);
    }

    private string _selectedIngredientsText = "";
    public string SelectedIngredientsText
    {
        get => _selectedIngredientsText;
        set => SetProperty(ref _selectedIngredientsText, value);
    }

    private decimal _price;
    public decimal Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    private string _priceText = "";
    public string PriceText
    {
        get => _priceText;
        set => SetProperty(ref _priceText, value);
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

    private const string DefaultImagePath = "D:\\Yes\\Уник\\3 курс\\5 семестр\\Курсовая\\Приложение\\ООП_Курсовой\\src\\Rolls\\classic.png";

    public ICommand LoadDataCommand { get; }
    public ICommand ResetSelectionCommand { get; }
    public ICommand CreateRollOnlyCommand { get; }
    public ICommand CreateRollAndAddToCartCommand { get; }
    public ICommand BrowseImageCommand { get; }
    // Клиентские команды
    public ICommand HomeCommand { get; }
    public ICommand MenuCommand { get; }
    public ICommand CartCommand { get; }
    public ICommand OrdersCommand { get; }
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

    private Visibility _adminPanelVisibility = Visibility.Collapsed;
    public Visibility AdminPanelVisibility
    {
        get => _adminPanelVisibility;
        set => SetProperty(ref _adminPanelVisibility, value);
    }

    public Action? RequestClose { get; set; }
    public Action? OpenHomeRequested { get; set; }
    public Action? OpenMenuRequested { get; set; }
    public Action? OpenCartRequested { get; set; }
    public Action? OpenOrdersRequested { get; set; }
    public Action? OpenProfileRequested { get; set; }
    public Action? OpenLoginRequested { get; set; }
    public Action? OpenCustomRollsRequested { get; set; }
    public Action? OpenCustomSetsRequested { get; set; }
    public Action? OpenAdminPanelRequested { get; set; }
    public Action? OpenCreateRollRequested { get; set; }
    public Action? OpenCreateSetRequested { get; set; }
    public Action? OpenUsersRequested { get; set; }
    public Action? OpenMenuItemsRequested { get; set; }

    public CreateCustomRollWindowViewModel()
    {
        LoadDataCommand = new AsyncCommand(LoadDataAsync);
        ResetSelectionCommand = new Command(ResetSelection);
        CreateRollOnlyCommand = new AsyncCommand(CreateRollOnlyAsync);
        CreateRollAndAddToCartCommand = new AsyncCommand(CreateRollAndAddToCartAsync);
        BrowseImageCommand = new Command(BrowseImage);

        LoadDataCommand.Execute(null);
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
            using var ingredientRepo = new IngredientRepository();
            var ingredients = await ingredientRepo.GetAllOrderedByNameAsync();

            IngredientOptions.Clear();

            foreach (var ing in ingredients)
            {
                var selectable = new SelectableIngredient(ing);
                selectable.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(SelectableIngredient.IsSelected))
                    {
                        var selectedCount = IngredientOptions.Count(i => i.IsSelected);
                        if (selectedCount > MaxIngredients && selectable.IsSelected)
                        {
                            selectable.IsSelected = false;
                            MessageBox.Show(
                                $"Можно выбрать не более {MaxIngredients} ингредиентов.",
                                "Ограничение",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information
                            );
                            return;
                        }
                        UpdateSelectionSummary();
                    }
                };
                IngredientOptions.Add(selectable);
            }

            UpdateSelectionSummary();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки ингредиентов: {ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateSelectionSummary()
    {
        var selected = IngredientOptions.Where(i => i.IsSelected).ToList();
        var count = selected.Count;

        SelectedCount = count;
        SelectedCountText = $"Выбрано: {count} из {MaxIngredients} ингредиентов";

        if (count == 0)
        {
            SelectedIngredientsText = "Ингредиенты пока не выбраны.";
            Price = 0;
            PriceText = "Цена: 0.00 руб.";
        }
        else
        {
            SelectedIngredientsText = string.Join(", ", selected.Select(s => s.Name));
            Price = CalculateCustomRollPrice(selected);
            PriceText = $"Цена: {Price:0.00} руб.";
        }
    }

    private void ResetSelection()
    {
        foreach (var ing in IngredientOptions)
            ing.IsSelected = false;

        RollName = string.Empty;
        ImageUrl = string.Empty;
        UpdateSelectionSummary();
    }

    private decimal CalculateCustomRollPrice(List<SelectableIngredient> selected)
    {
        decimal basePrice = 10.99m;
        decimal perIngredient = 1.99m;
        return basePrice + perIngredient * selected.Count;
    }

    private (decimal protein, decimal fat, decimal carbs, decimal calories) CalculateMacros(int ingredientCount)
    {
        var baseProtein = 4m;
        var baseFat = 3m;
        var baseCarbs = 10m;
        var baseCalories = 120m;

        var perIngredientProtein = 1.5m;
        var perIngredientFat = 1.2m;
        var perIngredientCarbs = 4m;
        var perIngredientCalories = 35m;

        var protein = baseProtein + perIngredientProtein * ingredientCount;
        var fat = baseFat + perIngredientFat * ingredientCount;
        var carbs = baseCarbs + perIngredientCarbs * ingredientCount;
        var calories = baseCalories + perIngredientCalories * ingredientCount;

        return (protein, fat, carbs, calories);
    }

    private async Task<MenuItem?> CreateRollInDatabaseAsync(bool addToCart)
    {
        var selected = IngredientOptions.Where(i => i.IsSelected).ToList();

        if (!selected.Any())
        {
            MessageBox.Show("Выберите хотя бы один ингредиент.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        if (selected.Count > MaxIngredients)
        {
            MessageBox.Show($"Можно выбрать не более {MaxIngredients} ингредиентов.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
            return null;
        }

        var name = RollName.Trim();
        if (string.IsNullOrEmpty(name))
        {
            name = CurrentUser.RoleId == 1 ? "Новый ролл" : "Мой ролл";
        }

        var price = CalculateCustomRollPrice(selected);
        var (protein, fat, carbs, calories) = CalculateMacros(selected.Count);

        bool isAdmin = CurrentUser.RoleId == 1;
        string itemType = isAdmin ? "roll" : "custom_roll";
        long? userId = isAdmin ? null : CurrentUser.Id;

        try
        {
            using var menuItemRepo = new MenuItemRepository();
            using var ingredientRollRepo = new IngredientRollRepository();

            var imageUrl = string.IsNullOrWhiteSpace(ImageUrl) ? DefaultImagePath : ImageUrl;

            var menuItem = new MenuItem
            {
                ItemType = itemType,
                Name = name,
                Description = "Ингредиенты: " + string.Join(", ", selected.Select(s => s.Name)),
                Price = price,
                ImageUrl = imageUrl,
                CategoryId = null,
                UserId = userId,
                ProteinG = protein,
                FatG = fat,
                CarbsG = carbs,
                CaloriesKcal = calories,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await menuItemRepo.AddAsync(menuItem);
            await menuItemRepo.SaveChangesAsync();

            var links = selected
                .Select(s => new IngredientRoll
                {
                    RollId = menuItem.Id,
                    IngredientId = s.Id
                })
                .ToList();

            foreach (var link in links)
            {
                await ingredientRollRepo.AddAsync(link);
            }
            await ingredientRollRepo.SaveChangesAsync();

            if (addToCart)
                await Cart.AddAsync(menuItem);

            return menuItem;
        }
        catch (Exception ex)
        {
            var inner = ex.InnerException?.Message ?? "";
            MessageBox.Show(
                $"Ошибка сохранения ролла: {ex.Message}\n\n{inner}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
            return null;
        }
    }

    private async Task CreateRollOnlyAsync()
    {
        var roll = await CreateRollInDatabaseAsync(addToCart: false);

        if (roll != null)
        {
            MessageBox.Show(
                $"Ролл «{roll.Name}» успешно создан!",
                "Успех",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
            
            OpenCustomRollsRequested?.Invoke();
            ResetSelection();
            RequestClose?.Invoke();
        }
    }

    private async Task CreateRollAndAddToCartAsync()
    {
        var roll = await CreateRollInDatabaseAsync(addToCart: true);

        if (roll != null)
        {
            MessageBox.Show(
                $"Ролл «{roll.Name}» создан и добавлен в корзину!",
                "Успех",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            OpenCustomRollsRequested?.Invoke();
            ResetSelection();
            RequestClose?.Invoke();
        }
    }
}

