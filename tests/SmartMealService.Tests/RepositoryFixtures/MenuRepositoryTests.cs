using FluentAssertions;

using Microsoft.EntityFrameworkCore;

using NSubstitute;

using Serilog;

using SmartMealService.Domain.Models;
using SmartMealService.Infrastructure;

namespace SmartMealService.Tests.RepositoryFixtures;

public sealed class MenuRepositoryTests : IDisposable
{
    private readonly MenuDbContext _context;
    private readonly ILogger _loggerMock = Substitute.For<ILogger>();
    private readonly MenuRepository _repository;

    public MenuRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<MenuDbContext>()
            .UseInMemoryDatabase($"SmartMeal_MenuTest_{Guid.NewGuid()}")
            .Options;

        _context = new MenuDbContext(options);
        _repository = new MenuRepository(_context, _loggerMock);
    }

    [Fact]
    public async Task SaveMenuItemsAsync_WhenItemsAreNew_ShouldInsertThemIntoDatabase()
    {
        var newItems = new List<SmsMenuItem>
        {
            new()
            {
                Id = 1,
                Article = "a001",
                Name = "Борщ",
                Price = 150.00m,
                IsWeighted = false,
                FullPath = string.Empty,
                Barcodes = []
            },
            new()
            {
                Id = 2,
                Article = "a002",
                Name = "Пюре с котлетой",
                Price = 250.00m,
                IsWeighted = false,
                FullPath = string.Empty,
                Barcodes = []
            }
        };

        await _repository.SaveMenuItemsAsync(newItems, CancellationToken.None);

        var savedItems = await _context.MenuItems.ToListAsync();
        savedItems.Should().HaveCount(2);
        savedItems.Should().ContainEquivalentOf(newItems[0]);
        savedItems.Should().ContainEquivalentOf(newItems[1]);
    }

    [Fact]
    public async Task SaveMenuItemsAsync_WhenArticleExistsButFieldsChanged_ShouldUpdateExistingRecords()
    {
        var existingItem = new SmsMenuItem
        {
            Id = 1,
            Article = "a001",
            Name = "Старое название",
            Price = 100.00m,
            IsWeighted = false,
            FullPath = string.Empty,
            Barcodes = []
        };

        _context.MenuItems.Add(existingItem);

        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        var incomingItem = new SmsMenuItem
        {
            Id = 1,
            Article = "a001",
            Name = "Новое название",
            Price = 130.00m,
            IsWeighted = false,
            FullPath = string.Empty,
            Barcodes = []
        };

        await _repository.SaveMenuItemsAsync([incomingItem], CancellationToken.None);

        var updatedItem = await _context.MenuItems.SingleAsync(e => e.Article == "a001");
        updatedItem.Name.Should().Be("Новое название");
        updatedItem.Price.Should().Be(130.00m);
    }

    [Fact]
    public async Task SaveMenuItemsAsync_WhenDataHasNotChanged_ShouldNotModifyDatabaseEntity()
    {
        var initialItem = new SmsMenuItem
        {
            Id = 5,
            Name = "Салат",
            Price = 90.00m,
            Article = "a1",
            IsWeighted = false,
            FullPath = string.Empty,
            Barcodes = []
        };
        _context.MenuItems.Add(initialItem);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var incomingItems = new List<SmsMenuItem>
        {
            new()
            {
                Article = "a1",
                Name = "Салат",
                Price = 90.00m,
                Id = 5,
                IsWeighted = false,
                FullPath = string.Empty,
                Barcodes = []
            }
        };

        await _repository.SaveMenuItemsAsync(incomingItems, CancellationToken.None);

        var result = await _context.MenuItems.SingleAsync(e => e.Article == "a1");
        result.Name.Should().Be("Салат");
        result.Price.Should().Be(90.00m);
    }

    [Fact]
    public async Task SaveMenuItemsAsync_WhenMixOfNewUpdateAndSameItems_ShouldProcessCorrectly()
    {
        var itemToUpdate = new SmsMenuItem
        {
            Id = 10,
            Article = "a-MIX-UPD",
            Name = "Старый суп",
            Price = 100.00m,
            IsWeighted = false,
            FullPath = string.Empty,
            Barcodes = []
        };

        var itemToKeep = new SmsMenuItem
        {
            Id = 11,
            Article = "a-MIX-SAME",
            Name = "Котлета",
            Price = 120.00m,
            IsWeighted = false,
            FullPath = string.Empty,
            Barcodes = []
        };

        _context.MenuItems.AddRange(itemToUpdate, itemToKeep);

        await _context.SaveChangesAsync();

        _context.ChangeTracker.Clear();

        var incomingItems = new List<SmsMenuItem>
        {
            new()
            {
                Article = "a-MIX-UPD",
                Name = "Обновленный суп",
                Price = 115.00m,
                Id = 10,
                IsWeighted = false,
                FullPath = string.Empty,
                Barcodes = []
            },
            new()
            {
                Article = "a-MIX-SAME",
                Name = "Котлета",
                Price = 120.00m,
                Id = 11,
                IsWeighted = false,
                FullPath = string.Empty,
                Barcodes = []
            },
            new()
            {
                Article = "a-MIX-NEW",
                Name = "Компот",
                Price = 40.00m,
                Id = 12,
                IsWeighted = false,
                FullPath = string.Empty,
                Barcodes = []
            }
        };

        await _repository.SaveMenuItemsAsync(incomingItems, CancellationToken.None);

        _context.MenuItems.Should().HaveCount(3);

        var updated = await _context.MenuItems.SingleAsync(e => e.Article == "a-MIX-UPD");
        updated.Name.Should().Be("Обновленный суп");
        updated.Price.Should().Be(115.00m);

        var added = await _context.MenuItems.SingleAsync(e => e.Article == "a-MIX-NEW");
        added.Name.Should().Be("Компот");

        var kept = await _context.MenuItems.SingleAsync(e => e.Article == "a-MIX-SAME");
        _context.Entry(kept).State.Should().Be(EntityState.Unchanged);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}