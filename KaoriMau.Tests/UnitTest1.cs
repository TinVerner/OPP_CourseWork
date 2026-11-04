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
    public CartTests()
    {
        Cart.Clear();
    }

    public void Dispose()
    {
        Cart.Clear();
    }

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

    [Fact]
    public void GetTotal_EmptyCart_ReturnsZero()
    {
        decimal result = Cart.GetTotal();
        Assert.Equal(0m, result);
    }

    [Fact]
    public async Task AddAsync_NewProduct_AddsOneItem()
    {
        var product = CreateProduct();
        await Cart.AddAsync(product);
        var item = Assert.Single(Cart.Items);

        Assert.Equal(product.Id, item.MenuItemId);
        Assert.Equal(product.Name, item.Name);
        Assert.Equal(product.Price, item.Price);
        Assert.Equal(product.Description, item.Description);
        Assert.Equal(1, item.Quantity);
    }

    [Fact]
    public async Task AddAsync_SameProductTwice_IncreasesQuantity()
    {
        var product = CreateProduct();
        await Cart.AddAsync(product);

        await Cart.AddAsync(product);

        var item = Assert.Single(Cart.Items);

        Assert.Equal(2, item.Quantity);
        Assert.Equal(25m, item.Total);
    }

    [Fact]
    public async Task ChangeQuantity_PositiveDelta_IncreasesQuantity()
    {
        await Cart.AddAsync(CreateProduct());

        Cart.ChangeQuantity(1, 3);

        var item = Assert.Single(Cart.Items);

        Assert.Equal(4, item.Quantity);
        Assert.Equal(50m, item.Total);
    }

    [Fact]
    public async Task ChangeQuantity_ZeroOrNegativeQuantity_RemovesProducts()
    {
        await Cart.AddAsync(CreateProduct(id: 1));
        await Cart.AddAsync(CreateProduct(id: 2));
        await Cart.AddAsync(CreateProduct(id: 3));

        Cart.ChangeQuantity(1, -1);
        Cart.ChangeQuantity(2, -5);

        var remainingItem = Assert.Single(Cart.Items);

        Assert.Equal(3L, remainingItem.MenuItemId);
        Assert.Equal(1, remainingItem.Quantity);
    }

    [Fact]
    public async Task ChangeQuantity_UnknownId_DoesNotChangeCart()
    {
        await Cart.AddAsync(CreateProduct());

        Cart.ChangeQuantity(999, 5);

        var item = Assert.Single(Cart.Items);

        Assert.Equal(1L, item.MenuItemId);
        Assert.Equal(1, item.Quantity);
        Assert.Equal(12.50m, Cart.GetTotal());
    }

    [Fact]
    public async Task Remove_Product_RemovesOnlySelectedProduct()
    {
        await Cart.AddAsync(CreateProduct(id: 1));
        await Cart.AddAsync(CreateProduct(id: 2));

        Cart.Remove(1);
        Cart.Remove(1);

        var remainingItem = Assert.Single(Cart.Items);

        Assert.Equal(2L, remainingItem.MenuItemId);
    }

    [Fact]
    public async Task GetTotal_DifferentProducts_ReturnsCorrectSum()
    {
        await Cart.AddAsync(CreateProduct(id: 1, price: 12.50m));
        await Cart.AddAsync(CreateProduct(id: 2, price: 7.25m));

        Cart.ChangeQuantity(1, 1);
        Cart.ChangeQuantity(2, 2);

        decimal result = Cart.GetTotal();

        Assert.Equal(46.75m, result);
    }

    [Fact]
    public async Task Clear_PopulatedCart_RemovesAllProducts()
    {
        await Cart.AddAsync(CreateProduct(id: 1));
        await Cart.AddAsync(CreateProduct(id: 2));

        Cart.Clear();

        Assert.Empty(Cart.Items);
        Assert.Equal(0m, Cart.GetTotal());
    }

    [Fact]
    public async Task AddAsync_Set_UsesUniqueNonEmptyComponentNames()
    {
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

        await Cart.AddAsync(product, loader.LoadAsync);

        var item = Assert.Single(Cart.Items);

        Assert.Equal("Филадельфия, Калифорния", item.Description);
        Assert.Equal(42L, Assert.Single(loader.RequestedIds));
    }

    [Fact]
    public async Task AddAsync_EmptySetComponents_KeepsOriginalDescription()
    {
        var product = CreateProduct(type: "set");
        var loader = new SetComponentsDouble();

        await Cart.AddAsync(product, loader.LoadAsync);

        var item = Assert.Single(Cart.Items);

        Assert.Equal(product.Description, item.Description);
        Assert.Equal(product.Id, Assert.Single(loader.RequestedIds));
    }

    [Fact]
    public async Task AddAsync_LoaderThrows_AddsProductWithOriginalDescription()
    {
        var product = CreateProduct(type: "custom_set");

        var loader = new SetComponentsDouble
        {
            Error = new InvalidOperationException("Тестовая ошибка загрузки")
        };

        await Cart.AddAsync(product, loader.LoadAsync);

        var item = Assert.Single(Cart.Items);

        Assert.Equal(product.Id, item.MenuItemId);
        Assert.Equal(product.Description, item.Description);
        Assert.Equal(1, item.Quantity);
        Assert.Equal(product.Id, Assert.Single(loader.RequestedIds));
    }
}