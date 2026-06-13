using System.Diagnostics;
using System.IO;
using System.Windows;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Serilog;

using SmartMealService.GuiClient.ViewModels;
using SmartMealService.GuiClient.Views;

namespace SmartMealService.GuiClient;

public partial class App : System.Windows.Application
{
    private const string MutexName = nameof(SmartMealService.GuiClient);
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
        bool createdNew;
        _mutex = new Mutex(true, MutexName, out createdNew);

        if (!createdNew)
        {
            MessageBox.Show("Приложение уже запущено.", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
            Shutdown();
            return;
        }

        ServiceCollection services = new();

        ConfigureServices(services);

        _serviceProvider = services.BuildServiceProvider();

        DispatcherUnhandledException += (s, args) => { Debug.WriteLine(args.Exception.Message); };

        MainWindow mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        var mainWindowViewModel = _serviceProvider.GetRequiredService<MainWindowViewModel>();

        mainWindow.DataContext = mainWindowViewModel;
        mainWindow.Show();

        base.OnStartup(e);
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


        services.AddSingleton<MainWindowViewModel>();
        services.AddSingleton<MainWindow>();
    }
}