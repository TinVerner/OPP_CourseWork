using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ООП_Курсовой.SupportClasses;
using ООП_Курсовой.Tables;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace KaoriMau.Tests;

public class CartTests : IDisposable
{
    // Выполняется перед каждым тестом.
    public CartTests()
    {
        Cart.Clear();
    }

    // Выполняется после каждого теста, даже если проверка провалилась.
    public void Dispose()
    {
        Cart.Clear();
    }

    // Вспомогательный метод для создания тестовых товаров.
    private static MenuItem CreateProduct(
        long id = 1,
        decimal price = 12.50m,
        string type = "roll")
    {
        return new MenuItem
        {
            Id = id,
            Name = "Филадельфия",
            Price = price,
            ItemType = type,
            Description = "Исходное описание"
        };
    }

    // 1. Пустая корзина.
    [Fact]
    public void GetTotal_EmptyCart_ReturnsZero()
    {
        // Arrange — корзина очищена в конструкторе.

        // Act
        decimal result = Cart.GetTotal();

        // Assert
        Assert.Equal(0m, result);
    }

    // 2. Добавление нового товара.
    [Fact]
    public async Task AddAsync_NewProduct_AddsOneItem()
    {
        // Arrange
        var product = CreateProduct();

        // Act
        await Cart.AddAsync(product);

        // Assert
        var item = Assert.Single(Cart.Items);

        Assert.Equal(product.Id, item.MenuItemId);
        Assert.Equal(product.Name, item.Name);
        Assert.Equal(product.Price, item.Price);
        Assert.Equal(product.Description, item.Description);
        Assert.Equal(1, item.Quantity);
    }

    // 3. Повторное добавление товара.
    [Fact]
    public async Task AddAsync_SameProductTwice_IncreasesQuantity()
    {
        // Arrange
        var product = CreateProduct();
        await Cart.AddAsync(product);

        // Act
        await Cart.AddAsync(product);

        // Assert
        var item = Assert.Single(Cart.Items);

        Assert.Equal(2, item.Quantity);
        Assert.Equal(25m, item.Total);
    }

    // 4. Увеличение количества.
    [Fact]
    public async Task ChangeQuantity_PositiveDelta_IncreasesQuantity()
    {
        // Arrange
        await Cart.AddAsync(CreateProduct());

        // Act
        Cart.ChangeQuantity(1, 3);

        // Assert
        var item = Assert.Single(Cart.Items);

        Assert.Equal(4, item.Quantity);
        Assert.Equal(50m, item.Total);
    }

    // 5. Уменьшение количества до нуля и ниже.
    [Fact]
    public async Task ChangeQuantity_ZeroOrNegativeQuantity_RemovesProducts()
    {
        // Arrange
        await Cart.AddAsync(CreateProduct(id: 1));
        await Cart.AddAsync(CreateProduct(id: 2));
        await Cart.AddAsync(CreateProduct(id: 3));

        // Act
        Cart.ChangeQuantity(1, -1);
        Cart.ChangeQuantity(2, -5);

        // Assert — третий товар должен сохраниться.
        var remainingItem = Assert.Single(Cart.Items);

        Assert.Equal(3L, remainingItem.MenuItemId);
        Assert.Equal(1, remainingItem.Quantity);
    }

    // 6. Изменение количества отсутствующего товара.
    [Fact]
    public async Task ChangeQuantity_UnknownId_DoesNotChangeCart()
    {
        // Arrange
        await Cart.AddAsync(CreateProduct());

        // Act
        Cart.ChangeQuantity(999, 5);

        // Assert
        var item = Assert.Single(Cart.Items);

        Assert.Equal(1L, item.MenuItemId);
        Assert.Equal(1, item.Quantity);
        Assert.Equal(12.50m, Cart.GetTotal());
    }

    // 7. Удаление товара и повторная попытка удаления.
    [Fact]
    public async Task Remove_Product_RemovesOnlySelectedProduct()
    {
        // Arrange
        await Cart.AddAsync(CreateProduct(id: 1));
        await Cart.AddAsync(CreateProduct(id: 2));

        // Act
        Cart.Remove(1);
        Cart.Remove(1);

        // Assert
        var remainingItem = Assert.Single(Cart.Items);

        Assert.Equal(2L, remainingItem.MenuItemId);
    }

    // 8. Подсчёт суммы нескольких товаров.
    [Fact]
    public async Task GetTotal_DifferentProducts_ReturnsCorrectSum()
    {
        // Arrange
        await Cart.AddAsync(CreateProduct(id: 1, price: 12.50m));
        await Cart.AddAsync(CreateProduct(id: 2, price: 7.25m));

        Cart.ChangeQuantity(1, 1); // Всего 2 товара.
        Cart.ChangeQuantity(2, 2); // Всего 3 товара.

        // Act
        decimal result = Cart.GetTotal();

        // Assert — 12.50 × 2 + 7.25 × 3.
        Assert.Equal(46.75m, result);
    }

    // 9. Очистка заполненной корзины.
    [Fact]
    public async Task Clear_PopulatedCart_RemovesAllProducts()
    {
        // Arrange
        await Cart.AddAsync(CreateProduct(id: 1));
        await Cart.AddAsync(CreateProduct(id: 2));

        // Act
        Cart.Clear();

        // Assert
        Assert.Empty(Cart.Items);
        Assert.Equal(0m, Cart.GetTotal());
    }

    // 10. Формирование описания сета через заглушку.
    [Fact]
    public async Task AddAsync_Set_UsesUniqueNonEmptyComponentNames()
    {
        // Arrange
        var product = CreateProduct(id: 42, type: "custom_set");

        var loader = new SetComponentsDouble
        {
            Components = new List<MenuSetComponent>
            {
                new MenuSetComponent
                {
                    Item = new MenuItem { Name = "Филадельфия" }
                },
                new MenuSetComponent
                {
                    Item = new MenuItem { Name = "Филадельфия" }
                },
                new MenuSetComponent
                {
                    Item = new MenuItem { Name = " " }
                },
                new MenuSetComponent
                {
                    Item = null!
                },
                new MenuSetComponent
                {
                    Item = new MenuItem { Name = "Калифорния" }
                }
            }
        };

        // Act
        await Cart.AddAsync(product, loader.LoadAsync);

        // Assert
        var item = Assert.Single(Cart.Items);

        Assert.Equal("Филадельфия, Калифорния", item.Description);
        Assert.Equal(42L, Assert.Single(loader.RequestedIds));
    }

    // 11. Пустой состав сета.
    [Fact]
    public async Task AddAsync_EmptySetComponents_KeepsOriginalDescription()
    {
        // Arrange
        var product = CreateProduct(type: "set");
        var loader = new SetComponentsDouble();

        // Act
        await Cart.AddAsync(product, loader.LoadAsync);

        // Assert
        var item = Assert.Single(Cart.Items);

        Assert.Equal(product.Description, item.Description);
        Assert.Equal(product.Id, Assert.Single(loader.RequestedIds));
    }

    // 12. Ошибка при загрузке состава сета.
    [Fact]
    public async Task AddAsync_LoaderThrows_AddsProductWithOriginalDescription()
    {
        // Arrange
        var product = CreateProduct(type: "custom_set");

        var loader = new SetComponentsDouble
        {
            Error = new InvalidOperationException("Тестовая ошибка загрузки")
        };

        // Act — исключение загрузчика должно обработаться внутри корзины.
        await Cart.AddAsync(product, loader.LoadAsync);

        // Assert
        var item = Assert.Single(Cart.Items);

        Assert.Equal(product.Id, item.MenuItemId);
        Assert.Equal(product.Description, item.Description);
        Assert.Equal(1, item.Quantity);
        Assert.Equal(product.Id, Assert.Single(loader.RequestedIds));
    }
}