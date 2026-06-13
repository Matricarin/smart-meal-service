using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

using SmartMealService.Infrastructure;
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

    var serviceProvider = new ServiceCollection()
        .AddSingleton<IConfiguration>(configuration)
        .AddSingleton<ILogger>(Log.Logger)
        .AddDbContext<MenuDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")))
        .AddScoped<IMenuRepository, MenuRepository>()
        .BuildServiceProvider();

    using (var scope = serviceProvider.CreateScope())
    {
        var menuRepository = scope.ServiceProvider.GetRequiredService<IMenuRepository>();
        await menuRepository.InitializeDatabaseAsync(cancellationSource.Token);
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