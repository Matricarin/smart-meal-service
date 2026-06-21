using System.Net.Http.Headers;
using System.Text;

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
    .ReadFrom.Configuration(configuration)
    .CreateLogger();

var cancellationSource = new CancellationTokenSource();

try
{
    Log.Information("Запуск SmartMealService...");

    var authSection = configuration.GetSection("AuthData").GetChildren().FirstOrDefault();
    var authData = authSection != null
        ? new AuthData(authSection.Value, authSection.Key)
        : new AuthData("default_password", "default_user");

    var uriString = configuration.GetSection("ServerUri")["BaseAddress"];
    if (string.IsNullOrWhiteSpace(uriString))
    {
        throw new InvalidOperationException("Не найден параметр ServerUri в конфигурации.");
    }

    //  INFO: регистрация HttpClient через специальный метод расширения помогает решить проблему с выбором времени жизни HttpClient
    var serviceProvider = new ServiceCollection()
        .AddSingleton<IConfiguration>(configuration)
        .AddSingleton(Log.Logger)
        .AddDbContext<MenuDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")))
        .AddScoped<IMenuRepository, MenuRepository>()
        .AddHttpClient<IOrderService, HttpOrderService>(client =>
        {
            client.BaseAddress = new Uri(uriString);
            var authToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(authData.ToString()));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", authToken);
        })
        .Services
        .BuildServiceProvider();

    using (var scope = serviceProvider.CreateScope())
    {
        var menuRepository = scope.ServiceProvider.GetRequiredService<IMenuRepository>();
        await menuRepository.InitializeDatabaseAsync(cancellationSource.Token);
    }

    var orderService = serviceProvider.GetRequiredService<IOrderService>();
    var repository = serviceProvider.GetRequiredService<IMenuRepository>();

    var menuItems = await orderService.GetMenuAsync(true, cancellationSource.Token);
    await repository.SaveMenuItemsAsync(menuItems, cancellationSource.Token);

    Log.Information("МЕНЮ БЛЮД:");

    foreach (var item in menuItems)
    {
        Log.Information($"{item.Name} – {item.Article} – {item.Price}");
    }

    Log.Information("------------------\n");

    var orderingItems = new List<SmsOrderingItem>();

    while (true)
    {
        Log.Information("Введите заказ в формате Код1:Количество1;Код2:Количество2 (или пустую строку для отмены):");

        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            Log.Warning("Ввод пуст. Заказ отменен.");
            return;
        }

        var pairs = input.Split(";", StringSplitOptions.RemoveEmptyEntries);

        bool isValid = true;

        orderingItems.Clear();

        foreach (var pair in pairs)
        {
            var values = pair.Split(":");

            if (values.Length != 2)
            {
                Log.Error("Ошибка формата. Ожидается формат Код:Количество.");
                isValid = false;
                break;
            }

            var article = values[0].Trim();

            if (!double.TryParse(values[1], out var quantity) || quantity <= 0)
            {
                Log.Error($"Ошибка: Количество для кода '{article}' должно быть числом больше нуля.");
                isValid = false;
                break;
            }

            var menuItem = menuItems.FirstOrDefault(m => m.Article == article);

            if (menuItem == null)
            {
                Log.Error($"Ошибка: Блюдо с кодом '{article}' не найдено в меню.");
                isValid = false;
                break;
            }

            orderingItems.Add(new SmsOrderingItem { MenuItemId = menuItem.Id, Quantity = quantity });
        }

        if (isValid)
        {
            break;
        }

        Log.Warning("Попробуйте ввести заказ заново.\n");
    }

    var order = new SmsOrder { Id = Guid.NewGuid(), Items = orderingItems };

    Log.Information("Отправка заказа {OrderId} на сервер...", order.Id);

    bool isSuccess = await orderService.SendOrderAsync(order, cancellationSource.Token);

    if (isSuccess)
    {
        Log.Information("УСПЕХ");
    }
    else
    {
        Log.Error("ОШИБКА: Сервер отклонил заказ или произошла внутренняя ошибка.");
    }
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение аварийно завершило работу.");
}
finally
{
    await Log.CloseAndFlushAsync();
}