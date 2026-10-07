
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

using LazyScroll.App.ViewModels;
using LazyScroll.App.Views;
using LazyScroll.Application.Services;
using LazyScroll.Infrastructure.Windows;

namespace LazyScroll.App;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly WindowsMiddleMouseHook _mouseHook;

    private ModeMenuWindow? _modeMenu;
    private bool _closeApproved;

    public MainWindow()
    {
        InitializeComponent();

        // Existing Timer MVP components.
        var executor = new Win32ScrollExecutor();
        var timer = new TimerScrollService(executor);

        _viewModel = new MainViewModel(timer);
        DataContext = _viewModel;

        // New mouse hook component.
        _mouseHook = new WindowsMiddleMouseHook(
            new DispatcherSynchronizationContext(
                Dispatcher));

        _mouseHook.MenuRequested +=
            OnMenuRequested;

        Loaded += OnWindowLoaded;
    }

    private void OnWindowLoaded(
        object sender, RoutedEventArgs e)
    {
        try
        {
            _mouseHook.Start();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Cannot start mouse hook: {ex.Message}",
                "LazyScroll",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }

    private void OnMenuRequested(
        object? sender, EventArgs e)
    {
        ShowModeMenu();
    }

    private void ShowModeMenu()
    {
        if (_closeApproved)
            return;

        if (_modeMenu is { IsVisible: true })
        {
            _modeMenu.Activate();
            return;
        }

        var menu = new ModeMenuWindow();
        _modeMenu = menu;

        // Normal mode stops automatic scrolling.
        menu.NormalSelected += () =>
        {
            if (_viewModel.StopCommand.CanExecute(null))
                _viewModel.StopCommand.Execute(null);

            menu.Close();
        };

        // Start Timer using existing settings.
        menu.TimerSelected += () =>
        {
            menu.Close();

            if (_viewModel.StartCommand.CanExecute(null))
                _viewModel.StartCommand.Execute(null);
        };

        // Stop active Timer.
        menu.StopSelected += () =>
        {
            if (_viewModel.StopCommand.CanExecute(null))
                _viewModel.StopCommand.Execute(null);

            menu.Close();
        };

        menu.Closed += (_, _) =>
        {
            if (ReferenceEquals(_modeMenu, menu))
                _modeMenu = null;
        };

        menu.Show();
        menu.Activate();
    }

    protected override async void OnClosing(
        CancelEventArgs e)
    {
        if (_closeApproved)
        {
            base.OnClosing(e);
            return;
        }

        // Wait for all services to stop first.
        e.Cancel = true;

        try
        {
            _mouseHook.Dispose();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Mouse hook cleanup error: {ex.Message}");
        }

        try
        {
            await _viewModel.ShutdownAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Timer cleanup error: {ex.Message}");
        }
        finally
        {
            _modeMenu?.Close();

            _closeApproved = true;
            Close();
        }
    }
}
