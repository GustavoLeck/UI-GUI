namespace TeamsAccessibilityPoc.Debounce;

/// <summary>
/// Fires <see cref="_onSettled"/> only after <see cref="_delay"/> has passed since the
/// last call to <see cref="Notify"/> without a newer call cancelling it.
/// </summary>
public sealed class Debouncer : IDisposable
{
    private readonly TimeSpan _delay;
    private readonly Action<string> _onSettled;
    private CancellationTokenSource? _pending;

    public Debouncer(TimeSpan delay, Action<string> onSettled)
    {
        _delay = delay;
        _onSettled = onSettled;
    }

    public void Notify(string value)
    {
        _pending?.Cancel();
        _pending?.Dispose();

        var cts = new CancellationTokenSource();
        _pending = cts;
        _ = RunAsync(value, cts.Token);
    }

    private async Task RunAsync(string value, CancellationToken token)
    {
        try
        {
            await Task.Delay(_delay, token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        if (!token.IsCancellationRequested)
            _onSettled(value);
    }

    public void Dispose() => _pending?.Cancel();
}
