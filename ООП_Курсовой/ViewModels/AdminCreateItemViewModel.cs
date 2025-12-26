using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using MvvmHelpers;
using MvvmHelpers.Commands;
using Microsoft.Win32;
using ООП_Курсовой.Repository;
using ООП_Курсовой.SupportClasses;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.ViewModels;

public class AdminCreateItemViewModel : BaseViewModel
{
    private ObservableCollection<Category> _categories = new();

    private string _itemType = "";
    private long? _categoryId;
    private string _name = "";
    private string _description = "";
    private string _price = "";
    private string _protein = "";
    private string _fat = "";
    private string _carbs = "";
    private string _calories = "";
    private string _imageUrl = "";

    public ObservableCollection<Category> Categories
    {
        get => _categories;
        set => SetProperty(ref _categories, value);
    }

    public string ItemType
    {
        get => _itemType;
        set => SetProperty(ref _itemType, value);
    }

    public long? CategoryId
    {
        get => _categoryId;
        set => SetProperty(ref _categoryId, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }

    public string Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }

    public string Protein
    {
        get => _protein;
        set => SetProperty(ref _protein, value);
    }

    public string Fat
    {
        get => _fat;
        set => SetProperty(ref _fat, value);
    }

    public string Carbs
    {
        get => _carbs;
        set => SetProperty(ref _carbs, value);
    }

    public string Calories
    {
        get => _calories;
        set => SetProperty(ref _calories, value);
    }

    public string ImageUrl
    {
        get => _imageUrl;
        set => SetProperty(ref _imageUrl, value);
    }

    private Visibility _adminPanelVisibility = Visibility.Collapsed;
    public Visibility AdminPanelVisibility
    {
        get => _adminPanelVisibility;
        set => SetProperty(ref _adminPanelVisibility, value);
    }

    public ICommand LoadCategoriesCommand { get; }
    public ICommand BrowseImageCommand { get; }
    public ICommand CreateItemCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand HomeCommand { get; }
    public ICommand AdminPanelCommand { get; }

    public Action? RequestClose { get; set; }
    public Action? OpenLoginRequested { get; set; }
    public Action? OpenHomeRequested { get; set; }
    public Action? OpenAdminPanelRequested { get; set; }

    public AdminCreateItemViewModel()
    {
        _adminPanelVisibility = CurrentUser.RoleId == 1 ? Visibility.Visible : Visibility.Collapsed;

        LoadCategoriesCommand = new AsyncCommand(LoadCategoriesAsync);
        BrowseImageCommand = new Command(BrowseImage);
        CreateItemCommand = new AsyncCommand(CreateItemAsync);
        LogoutCommand = new Command(Logout);
        HomeCommand = new Command(OpenHome);
        AdminPanelCommand = new Command(OpenAdminPanel);

        LoadCategoriesCommand.Execute(null);
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
            MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
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

    private async Task CreateItemAsync()
    {
        if (string.IsNullOrWhiteSpace(ItemType))
        {
            MessageBox.Show("Выберите тип позиции (ролл или сет).",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (CategoryId == null)
        {
            MessageBox.Show("Выберите категорию.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var name = Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            MessageBox.Show("Введите название позиции.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!TryParseRequiredDecimal(Price, "Цена", out var price))
            return;

        if (!TryParseOptionalDecimal(Protein, "Белки", out var protein))
            return;

        if (!TryParseOptionalDecimal(Fat, "Жиры", out var fat))
            return;

        if (!TryParseOptionalDecimal(Carbs, "Углеводы", out var carbs))
            return;

        if (!TryParseOptionalDecimal(Calories, "Ккал", out var kcal))
            return;

        var imageUrl = ImageUrl.Trim();
        var description = Description.Trim();

        try
        {
            using var menuItemRepo = new MenuItemRepository();

            var item = new MenuItem
            {
                CategoryId = CategoryId.Value,
                ItemType = ItemType,
                Name = name,
                Description = string.IsNullOrWhiteSpace(description) ? null : description,
                Price = price,
                ProteinG = protein,
                FatG = fat,
                CarbsG = carbs,
                CaloriesKcal = kcal,
                ImageUrl = string.IsNullOrWhiteSpace(imageUrl) ? null : imageUrl,
                UserId = CurrentUser.IsAuthenticated ? CurrentUser.Id : null,
                CreatedAt = DateTimeOffset.UtcNow
            };

            await menuItemRepo.AddAsync(item);
            await menuItemRepo.SaveChangesAsync();

            MessageBox.Show("Позиция меню успешно создана!", "Успех",
                MessageBoxButton.OK, MessageBoxImage.Information);

            Name = "";
            Description = "";
            Price = "";
            Protein = "";
            Fat = "";
            Carbs = "";
            Calories = "";
            ImageUrl = "";
            CategoryId = null;
            ItemType = "";

            OpenAdminPanelRequested?.Invoke();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка сохранения: {ex.Message}");
        }
    }

    private bool TryParseRequiredDecimal(string text, string fieldName, out decimal value)
    {
        text = (text ?? string.Empty).Trim().Replace(',', '.');

        if (!decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out value))
        {
            MessageBox.Show($"Поле \"{fieldName}\" должно содержать число.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (value < 0)
        {
            MessageBox.Show($"Поле \"{fieldName}\" не может быть отрицательным.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        return true;
    }

    private bool TryParseOptionalDecimal(string text, string fieldName, out decimal? value)
    {
        text = (text ?? string.Empty).Trim();

        if (string.IsNullOrEmpty(text))
        {
            value = null;
            return true;
        }

        text = text.Replace(',', '.');

        if (!decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var tmp))
        {
            MessageBox.Show($"Поле \"{fieldName}\" должно содержать число.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            value = null;
            return false;
        }

        if (tmp < 0)
        {
            MessageBox.Show($"Поле \"{fieldName}\" не может быть отрицательным.",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            value = null;
            return false;
        }

        value = tmp;
        return true;
    }

    private void Logout()
    {
        CurrentUser.Logout();
        OpenLoginRequested?.Invoke();
        RequestClose?.Invoke();
    }

    private void OpenHome()
    {
        OpenHomeRequested?.Invoke();
        RequestClose?.Invoke();
    }

    private void OpenAdminPanel()
    {
        OpenAdminPanelRequested?.Invoke();
        RequestClose?.Invoke();
    }
}