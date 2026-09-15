using System.Windows.Threading;

namespace TeamsAccessibilityPoc.UI;

/// <summary>
/// Runs the SuggestionWindow on its own STA/dispatcher thread so slow UI work never
/// blocks the UI Automation event callbacks that feed it.
/// </summary>
public sealed class SuggestionWindowHost : IDisposable
{
    private readonly ManualResetEventSlim _ready = new(false);
    private SuggestionWindow? _window;

    public void Start()
    {
        var thread = new Thread(() =>
        {
            _window = new SuggestionWindow();
            _window.Show();
            _ready.Set();
            Dispatcher.Run();
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
        _ready.Wait();
    }

    public void UpdateStatus(string label)
    {
        var window = _window;
        window?.Dispatcher.InvokeAsync(() => window.SetBoxLabel(label));
    }

    public void UpdateText(string text)
    {
        var window = _window;
        window?.Dispatcher.InvokeAsync(() => window.SetText(text));
    }

    public void UpdateApiStatus(string status)
    {
        var window = _window;
        window?.Dispatcher.InvokeAsync(() => window.SetApiStatus(status));
    }

    public void UpdateApiResult(string summary, string? spriteUrl)
    {
        var window = _window;
        window?.Dispatcher.InvokeAsync(() => window.SetApiResult(summary, spriteUrl));
    }

    public void Dispose()
    {
        var window = _window;
        if (window is null) return;
        window.Dispatcher.Invoke(() =>
        {
            window.Close();
            Dispatcher.CurrentDispatcher.InvokeShutdown();
        });
    }
}
