using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

using SmartMealService.Domain.Models;
using SmartMealService.Infrastructure;
using SmartMealService.Services;
using SmartMealService.Services.Interfaces;

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", false, true)
    .Build();


Log.Logger = new LoggerConfiguration()
    .ReadFrom.KeyValuePairs(configuration.AsEnumerable())
    .CreateLogger();

var cancellationSource = new CancellationTokenSource();

try
{
    Log.Information("Запуск SmartMealService...");

    var authConfig = configuration.AsEnumerable().FirstOrDefault(c => c.Key == "AuthData");
    var uriConfig = configuration.AsEnumerable().FirstOrDefault(configuration => configuration.Key == "ServerUri");

    var authData = new AuthData(authConfig.Value, authConfig.Key);

    var serviceProvider = new ServiceCollection()
        .AddSingleton<IConfiguration>(configuration)
        .AddSingleton<ILogger>(Log.Logger)
        .AddSingleton<HttpClient>()
        .AddSingleton(authData)
        .AddSingleton(new Uri(uriConfig.Value))
        .AddDbContext<MenuDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")))
        .AddScoped<IMenuRepository, MenuRepository>()
        .AddTransient<IOrderService, HttpOrderService>()
        .BuildServiceProvider();

    using (var scope = serviceProvider.CreateScope())
    {
        var menuRepository = scope.ServiceProvider.GetRequiredService<IMenuRepository>();
        await menuRepository.InitializeDatabaseAsync(cancellationSource.Token);
    }

    var orderService = serviceProvider.GetRequiredService<IOrderService>();

    var menuItems = await orderService.GetMenuAsync(true, cancellationSource.Token);

    var repository = serviceProvider.GetRequiredService<IMenuRepository>();

    await repository.SaveMenuItemsAsync(menuItems, cancellationSource.Token);

    var input = Console.ReadLine().Split(";");

    var orderingItems = new List<SmsOrderingItem>();

    foreach (string s in input)
    {
        var values = s.Split(":");
        var orderingItem = new SmsOrderingItem { MenuItemId = values[0], Quantity = values[1] };
        orderingItems.Add(orderingItem);
    }

    var order = new SmsOrder { Id = Guid.NewGuid(), Items = orderingItems };

    orderService.SendOrderAsync(order, cancellationSource.Token);
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение аварийно завершило работу.");
}
finally
{
    await Log.CloseAndFlushAsync();
}