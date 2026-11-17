using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Tasvir.App.Views;
using Tasvir.Core.Services;
using Tasvir.Services;
using Tasvir.ViewModels;

namespace Tasvir.App;

public partial class App : Application
{
    private static IServiceProvider _services = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        var settings = new SettingsService();
        settings.Load();

        var collection = new ServiceCollection();
        collection.AddSingleton<ISettingsService>(settings);
        collection.AddSingleton<ILocalizationService, LocalizationService>();
        collection.AddSingleton<IProcessRunner, ProcessRunner>();
        collection.AddSingleton<IAdbPathProvider, AdbPathProvider>();
        collection.AddSingleton<IAdbService, AdbService>();
        collection.AddSingleton<IDeviceMonitor, DeviceMonitor>();
        collection.AddSingleton<IProcessLauncher, ProcessLauncher>();
        collection.AddSingleton<IScrcpyPathProvider, ScrcpyPathProvider>();
        collection.AddSingleton<IScrcpyService, ScrcpyService>();
        collection.AddTransient<MirrorViewModel>();
        collection.AddTransient<SettingsViewModel>();
        collection.AddTransient<AboutViewModel>();
        collection.AddTransient<WirelessConnectViewModel>();
        collection.AddTransient<ShellViewModel>();
        _services = collection.BuildServiceProvider();

        _ = _services.GetRequiredService<ILocalizationService>();

        var shell = _services.GetRequiredService<ShellViewModel>();
        shell.Mirror.RequestWirelessConnection += (_, _) => ShowWirelessDialog();
        shell.Mirror.RequestFullscreenToggle += (_, _) => ToggleFullscreen();

        var window = new MainWindow { DataContext = shell };
        MainWindow = window;
        window.Show();

        _services.GetRequiredService<IDeviceMonitor>().StartMonitoring();
        shell.Mirror.RefreshCommand.Execute(null);

        base.OnStartup(e);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_services is not null)
        {
            _services.GetRequiredService<IDeviceMonitor>().StopMonitoring();
            (_services as IDisposable)?.Dispose();
        }

        base.OnExit(e);
    }

    private static void ToggleFullscreen()
    {
        if (Current.MainWindow is not Window window)
        {
            return;
        }

        if (window.WindowStyle == WindowStyle.None)
        {
            window.WindowStyle = WindowStyle.SingleBorderWindow;
            window.WindowState = WindowState.Normal;
        }
        else
        {
            window.WindowStyle = WindowStyle.None;
            window.WindowState = WindowState.Maximized;
        }
    }

    private static void ShowWirelessDialog()
    {
        var model = _services.GetRequiredService<WirelessConnectViewModel>();
        var dialog = new WirelessConnectWindow
        {
            DataContext = model,
            Owner = Current.MainWindow
        };
        model.RequestClose += (_, _) => dialog.Close();
        dialog.Closed += (_, _) => model.Dispose();
        dialog.ShowDialog();
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogError("UI", e.Exception);
        MessageBox.Show(e.Exception.Message, "Tasvir", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            LogError("AppDomain", exception);
        }
    }

    private static void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogError("Task", e.Exception);
        e.SetObserved();
    }

    private static void LogError(string source, Exception exception)
    {
        try
        {
            var directory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Tasvir");
            Directory.CreateDirectory(directory);
            File.AppendAllText(
                Path.Combine(directory, "error.log"),
                $"[{DateTime.Now:O}] {source}: {exception}{Environment.NewLine}");
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
