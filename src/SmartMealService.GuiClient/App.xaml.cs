using System.Diagnostics;
using System.Windows;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

using SmartMealService.GuiClient.Application;
using SmartMealService.GuiClient.Infrastructure;
using SmartMealService.GuiClient.Models;
using SmartMealService.GuiClient.ViewModels;
using SmartMealService.GuiClient.Views;

namespace SmartMealService.GuiClient;

public partial class App
{
    private const string MutexName = nameof(GuiClient);
    private Mutex? _mutex;
    private IServiceProvider? _serviceProvider;

    protected override void OnExit(ExitEventArgs e)
    {
        _mutex?.ReleaseMutex();
        _mutex?.Dispose();
        base.OnExit(e);
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        _mutex = new Mutex(true, MutexName, out bool createdNew);

        if (!createdNew)
        {
            MessageBox.Show("Приложение уже запущено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        ServiceCollection services = new();

        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();

        InitializeEnvironmentVariablesAsync(_serviceProvider);

        DispatcherUnhandledException += (_, args) => { Debug.WriteLine(args.Exception.Message); };

        MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        var mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

        mainWindow.DataContext = mainWindowViewModel;
        mainWindow.Show();

        base.OnStartup(e);
    }

    private static void InitializeEnvironmentVariablesAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<VariablesDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger>();

        context.Database.EnsureCreated();

        var requiredVariables = configuration.GetSection("_environmentVariables").Get<string[]>();

        if (requiredVariables is not null)
        {
            foreach (var key in requiredVariables)
            {
                var existsInDb = context.EnvironmentVariables.Any(v => v.Key == key);

                if (!existsInDb)
                {
                    string currentOsValue = Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.User)
                                            ?? "По умолчанию";

                    var defaultVariable = new CustomEnvironmentVariable
                    {
                        Key = key,
                        Value = currentOsValue,
                        Comment = "Инициализировано автоматически при первом старте"
                    };

                    Environment.SetEnvironmentVariable(key, currentOsValue, EnvironmentVariableTarget.User);

                    context.EnvironmentVariables.Add(defaultVariable);

                    logger.Information("Выполнена инициализация по умолчанию для переменной: {Key}", key);
                }
            }

            context.SaveChanges();
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(Log.Logger);
        services.AddDbContext<VariablesDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IVariablesRepository, VariablesRepository>();
        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();
    }
}