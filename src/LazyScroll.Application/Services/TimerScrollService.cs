
using LazyScroll.Core.Interfaces;
using LazyScroll.Core.Models;

namespace LazyScroll.Application.Services;

public sealed class TimerScrollService : IAsyncDisposable
{
    private readonly IScrollExecutor _executor;

    private CancellationTokenSource? _cts;
    private Task? _worker;

    public bool IsRunning =>
        _cts is { IsCancellationRequested: false };

    public TimerScrollService(IScrollExecutor executor)
    {
        _executor = executor;
    }

    public void Start(ScrollOptions options)
    {
        options.Validate();

        if (_cts is not null)
            throw new InvalidOperationException(
                "Timer is already active.");

        _cts = new CancellationTokenSource();

        _worker = RunAsync(options, _cts.Token);
    }

    private async Task RunAsync(
        ScrollOptions options,
        CancellationToken token)
    {
        using var timer = new PeriodicTimer(options.Interval);

        try
        {
            while (await timer.WaitForNextTickAsync(token)
                              .ConfigureAwait(false))
            {
                token.ThrowIfCancellationRequested();
                _executor.Scroll(options.WheelDelta);
            }
        }
        catch (OperationCanceledException)
            when (token.IsCancellationRequested)
        {
            // Expected when Stop is called.
        }
    }

    public async Task StopAsync()
    {
        var cts = _cts;

        if (cts is null)
            return;

        cts.Cancel();

        try
        {
            if (_worker is not null)
                await _worker.ConfigureAwait(false);
        }
        finally
        {
            cts.Dispose();
            _cts = null;
            _worker = null;
        }
    }

    public ValueTask DisposeAsync() =>
        new(StopAsync());
}
