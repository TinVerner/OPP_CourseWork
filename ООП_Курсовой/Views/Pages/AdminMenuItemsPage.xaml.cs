using System.Windows;
using ООП_Курсовой.Repository;
using ООП_Курсовой.SupportClasses;
using ООП_Курсовой.ViewModels;

namespace ООП_Курсовой.Views.Pages;

public partial class AdminMenuItemsPage : System.Windows.Controls.UserControl
{
    private AdminMenuItemsViewModel? _viewModel;

    public AdminMenuItemsPage()
    {
        InitializeComponent();
        Loaded += AdminMenuItemsPage_Loaded;
    }

    private void AdminMenuItemsPage_Loaded(object sender, RoutedEventArgs e)
    {
        if (_viewModel == null)
        {
            _viewModel = new AdminMenuItemsViewModel();
            
            var adminWindow = Application.Current.MainWindow as AdminWindowView;
            if (adminWindow != null && adminWindow.DataContext is AdminWindowViewModel adminVm)
            {
                _viewModel.OpenAdminPanelRequested = () => adminVm.AdminPanelCommand.Execute(null);
                _viewModel.OpenCreateRollRequested = () => adminVm.CreateRollCommand.Execute(null);
                _viewModel.OpenCreateSetRequested = () => adminVm.CreateSetCommand.Execute(null);
                _viewModel.OpenUsersRequested = () => adminVm.ViewUsersCommand.Execute(null);
                _viewModel.OpenProfileRequested = () => adminVm.ProfileCommand.Execute(null);
            }
            
            _viewModel.OpenDetailsRequested = async id =>
            {
                try
                {
                    using var repo = new MenuItemRepository();
                    var item = await repo.GetByIdWithIngredientsAsync(id);

                    if (item == null)
                    {
                        MessageBox.Show("Товар не найден.", "Ошибка",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    var model = new RollViewModel
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Price = item.Price,
                        ImageUrl = item.ImageUrl,
                        CategoryId = item.CategoryId,
                        IngredientIds = item.IngredientRolls?.Select(ir => ir.IngredientId).ToList() ?? new(),
                        IngredientNames = item.IngredientRolls?.Select(ir => ir.Ingredient?.Name ?? "").ToList() ?? new(),
                        Description = item.Description,
                        ProteinG = item.ProteinG,
                        FatG = item.FatG,
                        CarbsG = item.CarbsG,
                        CaloriesKcal = item.CaloriesKcal,
                        ItemType = item.ItemType
                    };

                    var w = new MenuDetailsWindowView(model) { Owner = Application.Current.MainWindow };
                    w.ShowDialog();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка загрузки товара: {ex.Message}",
                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };

            _viewModel.OpenEditRequested = id =>
            {
                var editWindow = new EditMenuItemWindowView(id, () => _viewModel.LoadDataCommand.Execute(null)) { Owner = Application.Current.MainWindow };
                editWindow.ShowDialog();
            };
            
            _viewModel.RefreshDataRequested = () => _viewModel.LoadDataCommand.Execute(null);
            
            DataContext = _viewModel;
        }
        else
        {
            _viewModel.LoadDataCommand.Execute(null);
        }
    }
}

