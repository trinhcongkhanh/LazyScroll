
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LazyScroll.Application.Services;
using LazyScroll.Core.Models;

namespace LazyScroll.App.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly TimerScrollService _timer;

    [ObservableProperty]
    private string intervalSeconds = "120";

    [ObservableProperty]
    private string notches = "3";

    [ObservableProperty]
    private bool scrollUp = true;

    [ObservableProperty]
    private bool isRunning;

    [ObservableProperty]
    private string status = "Stopped";

    public MainViewModel(TimerScrollService timer)
    {
        _timer = timer;
    }

    private bool CanStart() => !IsRunning;

    private bool CanStop() => IsRunning;

    [RelayCommand(CanExecute = nameof(CanStart))]
    private void Start()
    {
        if (!int.TryParse(IntervalSeconds, out int seconds) ||
            seconds < 1 || seconds > 3600 ||
            !int.TryParse(Notches, out int wheelNotches) ||
            wheelNotches < 1 || wheelNotches > 20)
        {
            Status = "Invalid settings.";
            return;
        }

        try
        {
            var options = new ScrollOptions(
                TimeSpan.FromSeconds(seconds),
                wheelNotches,
                ScrollUp);

            _timer.Start(options);

            IsRunning = true;
            Status = "Running";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
    }

    [RelayCommand(CanExecute = nameof(CanStop))]
    private async Task StopAsync()
    {
        try
        {
            await _timer.StopAsync();
            Status = "Stopped";
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
        }
        finally
        {
            IsRunning = false;
        }
    }

    partial void OnIsRunningChanged(bool value)
    {
        StartCommand.NotifyCanExecuteChanged();
        StopCommand.NotifyCanExecuteChanged();
    }

    public async Task ShutdownAsync()
    {
        await _timer.StopAsync();
    }
}
