using Microsoft.EntityFrameworkCore;

using Serilog;

using SmartMealService.Domain.Models;
using SmartMealService.Services.Interfaces;

namespace SmartMealService.Infrastructure;

public sealed class MenuRepository : IMenuRepository
{
    private const int ChunkSize = 100; // Оптимальный размер пакета для Enterprise-систем
    private readonly MenuDbContext _context;
    private readonly ILogger _logger;

    public MenuRepository(MenuDbContext context, ILogger logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task SaveMenuItemsAsync(List<SmsMenuItem>? menuItems, CancellationToken cancellationToken)
    {
        if (menuItems is null || menuItems.Count == 0)
        {
            _logger.Warning("Передан пустой список блюд.");
            return;
        }

        try
        {
            _logger.Information("Получено для обработки: {Count} шт.", menuItems.Count);

            int updatedCount = 0;
            int insertedCount = 0;
            int skippedCount = 0;

            //  UPD: выгребаем данные чанками, чтобы не материализовать полностью все коллекцию в память
            foreach (var chunk in menuItems.Chunk(ChunkSize))
            {
                var incomingArticles = chunk.Select(i => i.Article).ToList();

                var existingItemsDict = await _context.MenuItems
                    .Where(m => incomingArticles.Contains(m.Article))
                    .ToDictionaryAsync(m => m.Article ?? "(пустой код)", cancellationToken);

                foreach (var incomingItem in chunk)
                {
                    if (existingItemsDict.TryGetValue(incomingItem.Article, out var existingItem))
                    {
                        if (existingItem.Name != incomingItem.Name ||
                            existingItem.Price != incomingItem.Price)
                        {
                            existingItem.Name = incomingItem.Name;
                            existingItem.Price = incomingItem.Price;

                            _context.MenuItems.Update(existingItem);
                            updatedCount++;
                        }
                        else
                        {
                            skippedCount++;
                        }
                    }
                    else
                    {
                        await _context.MenuItems.AddAsync(incomingItem, cancellationToken);
                        insertedCount++;
                    }
                }

                if (insertedCount > 0 || updatedCount > 0)
                {
                    await _context.SaveChangesAsync(cancellationToken);
                }
            }

            _logger.Information(
                "Обработка завершена. Добавлено: {Inserted}, Обновлено: {Updated}, Без изменений: {Skipped}.",
                insertedCount, updatedCount, skippedCount);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при выполнении пакетной операции для меню.");
            throw;
        }
    }

    public async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.Information("Инициализация базы данных.");
            await _context.Database.EnsureCreatedAsync(cancellationToken);
            _logger.Information("База данных успешно инициализирована.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Ошибка при инициализации базы данных.");
            throw;
        }
    }
}