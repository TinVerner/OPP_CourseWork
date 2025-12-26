using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using MvvmHelpers;
using MvvmHelpers.Commands;
using ООП_Курсовой.Repository;
using ООП_Курсовой.SupportClasses;
using ООП_Курсовой.Tables;
using SelectableRoll = ООП_Курсовой.ViewModels.CreateCustomSetWindowViewModel.SelectableRoll;

namespace ООП_Курсовой.ViewModels;

public class EditMenuItemWindowViewModel : BaseViewModel
{
    private long _menuItemId;
    private MenuItem? _originalItem;

    private ObservableCollection<SelectableIngredient> _ingredientOptions = new();
    private ObservableCollection<SelectableRoll> _rollOptions = new();
    private ObservableCollection<Category> _categories = new();

    private string _name = "";
    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    private string _description = "";
    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
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
        set
        {
            if (SetProperty(ref _priceText, value))
            {
                if (decimal.TryParse(value, out var parsed))
                    Price = parsed;
            }
        }
    }

    private decimal? _proteinG;
    public decimal? ProteinG
    {
        get => _proteinG;
        set => SetProperty(ref _proteinG, value);
    }

    private string _proteinText = "";
    public string ProteinText
    {
        get => _proteinText;
        set
        {
            if (SetProperty(ref _proteinText, value))
            {
                if (string.IsNullOrWhiteSpace(value))
                    ProteinG = null;
                else if (decimal.TryParse(value, out var parsed))
                    ProteinG = parsed;
            }
        }
    }

    private decimal? _fatG;
    public decimal? FatG
    {
        get => _fatG;
        set => SetProperty(ref _fatG, value);
    }

    private string _fatText = "";
    public string FatText
    {
        get => _fatText;
        set
        {
            if (SetProperty(ref _fatText, value))
            {
                if (string.IsNullOrWhiteSpace(value))
                    FatG = null;
                else if (decimal.TryParse(value, out var parsed))
                    FatG = parsed;
            }
        }
    }

    private decimal? _carbsG;
    public decimal? CarbsG
    {
        get => _carbsG;
        set => SetProperty(ref _carbsG, value);
    }

    private string _carbsText = "";
    public string CarbsText
    {
        get => _carbsText;
        set
        {
            if (SetProperty(ref _carbsText, value))
            {
                if (string.IsNullOrWhiteSpace(value))
                    CarbsG = null;
                else if (decimal.TryParse(value, out var parsed))
                    CarbsG = parsed;
            }
        }
    }

    private decimal? _caloriesKcal;
    public decimal? CaloriesKcal
    {
        get => _caloriesKcal;
        set => SetProperty(ref _caloriesKcal, value);
    }

    private string _caloriesText = "";
    public string CaloriesText
    {
        get => _caloriesText;
        set
        {
            if (SetProperty(ref _caloriesText, value))
            {
                if (string.IsNullOrWhiteSpace(value))
                    CaloriesKcal = null;
                else if (decimal.TryParse(value, out var parsed))
                    CaloriesKcal = parsed;
            }
        }
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

    private string _itemType = "";
    public string ItemType
    {
        get => _itemType;
        set => SetProperty(ref _itemType, value);
    }

    public string ItemTypeDisplay => ItemType switch
    {
        "roll" => "Ролл",
        "set" => "Сет",
        "custom_roll" => "Кастомный ролл",
        "custom_set" => "Кастомный сет",
        _ => ItemType
    };

    public ObservableCollection<SelectableIngredient> IngredientOptions
    {
        get => _ingredientOptions;
        set => SetProperty(ref _ingredientOptions, value);
    }

    public ObservableCollection<SelectableRoll> RollOptions
    {
        get => _rollOptions;
        set => SetProperty(ref _rollOptions, value);
    }

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

    public bool IsRoll => ItemType == "roll" || ItemType == "custom_roll";
    public bool IsSet => ItemType == "set" || ItemType == "custom_set";
    public bool IsAdminItem => ItemType == "roll" || ItemType == "set";

    public ICommand LoadDataCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand BrowseImageCommand { get; }
    public ICommand CategoryChangedCommand { get; }

    public Action? RequestClose { get; set; }
    public Action? RequestRefresh { get; set; }

    public EditMenuItemWindowViewModel(long menuItemId)
    {
        _menuItemId = menuItemId;

        LoadDataCommand = new AsyncCommand(LoadDataAsync);
        SaveCommand = new AsyncCommand(SaveAsync);
        CancelCommand = new Command(() => RequestClose?.Invoke());
        BrowseImageCommand = new Command(BrowseImage);
        CategoryChangedCommand = new Command<object>(categoryObj =>
        {
            if (categoryObj is long id)
                SelectedCategoryId = id;
            else
                SelectedCategoryId = null;
        });

        LoadDataCommand.Execute(null);
    }

    private async Task LoadDataAsync()
    {
        try
        {
            using var menuItemRepo = new MenuItemRepository();
            using var ingredientRepo = new IngredientRepository();
            using var categoryRepo = new CategoryRepository();
            using var setComponentRepo = new MenuSetComponentRepository();

            _originalItem = await menuItemRepo.GetByIdWithIngredientsAsync(_menuItemId);

            if (_originalItem == null)
            {
                MessageBox.Show("Позиция меню не найдена.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                RequestClose?.Invoke();
                return;
            }

            ItemType = _originalItem.ItemType;
            Name = _originalItem.Name;
            Description = _originalItem.Description ?? "";
            Price = _originalItem.Price;
            PriceText = _originalItem.Price.ToString("F2");
            ProteinG = _originalItem.ProteinG;
            ProteinText = _originalItem.ProteinG?.ToString("F2") ?? "";
            FatG = _originalItem.FatG;
            FatText = _originalItem.FatG?.ToString("F2") ?? "";
            CarbsG = _originalItem.CarbsG;
            CarbsText = _originalItem.CarbsG?.ToString("F2") ?? "";
            CaloriesKcal = _originalItem.CaloriesKcal;
            CaloriesText = _originalItem.CaloriesKcal?.ToString("F2") ?? "";
            ImageUrl = _originalItem.ImageUrl ?? "";
            SelectedCategoryId = _originalItem.CategoryId;

            if (IsRoll)
            {
                var ingredients = await ingredientRepo.GetAllOrderedByNameAsync();
                IngredientOptions.Clear();
                foreach (var ing in ingredients)
                {
                    var selectable = new SelectableIngredient(ing);
                    if (_originalItem.IngredientRolls != null)
                    {
                        selectable.IsSelected = _originalItem.IngredientRolls.Any(ir => ir.IngredientId == ing.Id);
                    }
                    IngredientOptions.Add(selectable);
                }
            }

            if (IsSet)
            {
                var categories = await categoryRepo.GetAllOrderedByNameAsync();
                Categories = new ObservableCollection<Category>(categories);

                var allRolls = await menuItemRepo.GetByItemTypeWithIngredientsAsync("roll");
                RollOptions.Clear();
                foreach (var roll in allRolls)
                {
                    var selectable = new SelectableRoll
                    {
                        Id = roll.Id,
                        Name = roll.Name,
                        Description = roll.Description,
                        ImageUrl = roll.ImageUrl,
                        Price = roll.Price,
                        ProteinG = roll.ProteinG,
                        FatG = roll.FatG,
                        CarbsG = roll.CarbsG,
                        CaloriesKcal = roll.CaloriesKcal
                    };
                    var components = await setComponentRepo.GetBySetIdAsync(_menuItemId);
                    if (components != null)
                    {
                        selectable.IsSelected = components.Any(c => c.ItemId == roll.Id);
                    }
                    RollOptions.Add(selectable);
                }
            }

            OnPropertyChanged(nameof(IsRoll));
            OnPropertyChanged(nameof(IsSet));
            OnPropertyChanged(nameof(IsAdminItem));
            OnPropertyChanged(nameof(ItemTypeDisplay));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка загрузки данных: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
            RequestClose?.Invoke();
        }
    }

    private void BrowseImage()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp|Все файлы|*.*",
            Title = "Выберите изображение"
        };

        if (dialog.ShowDialog() == true)
        {
            ImageUrl = dialog.FileName;
        }
    }

    private async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            MessageBox.Show("Введите название позиции.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (Price <= 0)
        {
            MessageBox.Show("Цена должна быть больше нуля.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (IsSet && IsAdminItem && !SelectedCategoryId.HasValue)
        {
            MessageBox.Show("Выберите категорию для сета.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var dbContext = new Tables.DatabaseContext();
            using var menuItemRepo = new MenuItemRepository(dbContext);
            using var ingredientRollRepo = new IngredientRollRepository(dbContext);
            using var setComponentRepo = new MenuSetComponentRepository(dbContext);

            var item = await menuItemRepo.GetByIdAsync(_menuItemId);
            if (item == null)
            {
                MessageBox.Show("Позиция меню не найдена.", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            item.Name = Name.Trim();
            item.Description = string.IsNullOrWhiteSpace(Description) ? null : Description.Trim();
            item.Price = Price;
            item.ProteinG = ProteinG;
            item.FatG = FatG;
            item.CarbsG = CarbsG;
            item.CaloriesKcal = CaloriesKcal;
            item.ImageUrl = string.IsNullOrWhiteSpace(ImageUrl) ? null : ImageUrl;

            if (IsSet && IsAdminItem)
            {
                item.CategoryId = SelectedCategoryId;
            }

            menuItemRepo.Update(item);

            if (IsRoll)
            {
                var existingIngredientRolls = await ingredientRollRepo.GetByRollIdAsync(_menuItemId);
                foreach (var existing in existingIngredientRolls)
                {
                    ingredientRollRepo.Remove(existing);
                }

                var selectedIngredients = IngredientOptions.Where(ing => ing.IsSelected).ToList();
                foreach (var selected in selectedIngredients)
                {
                    var ingredientRoll = new IngredientRoll
                    {
                        RollId = _menuItemId,
                        IngredientId = selected.Id
                    };
                    await ingredientRollRepo.AddAsync(ingredientRoll);
                }
            }

            if (IsSet)
            {
                var existingComponents = await setComponentRepo.GetBySetIdAsync(_menuItemId);
                foreach (var existing in existingComponents)
                {
                    setComponentRepo.Remove(existing);
                }

                var selectedRolls = RollOptions.Where(r => r.IsSelected).ToList();
                if (selectedRolls.Any())
                {
                    foreach (var selected in selectedRolls)
                    {
                        var component = new MenuSetComponent
                        {
                            SetId = _menuItemId,
                            ItemId = selected.Id,
                            Qty = 1
                        };
                        await setComponentRepo.AddAsync(component);
                    }
                }
            }

            await menuItemRepo.SaveChangesAsync();

            MessageBox.Show("Позиция меню успешно обновлена.", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);

            RequestRefresh?.Invoke();
            RequestClose?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}

