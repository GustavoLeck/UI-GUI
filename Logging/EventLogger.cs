using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace TeamsAccessibilityPoc.Logging;

/// <summary>
/// Logs timestamped events to the console and appends them to a CSV file,
/// tracking elapsed time since the previous event (proxy for latency/frequency measurements in Fase 4).
/// </summary>
public sealed class EventLogger : IDisposable
{
    private readonly StreamWriter _csvWriter;
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();
    private long _lastEventMs;

    public string LogFilePath { get; }

    public EventLogger(string? logDirectory = null)
    {
        logDirectory ??= Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logDirectory);

        LogFilePath = Path.Combine(logDirectory, $"session-{DateTime.Now:yyyyMMdd-HHmmss}.csv");
        _csvWriter = new StreamWriter(LogFilePath, append: false) { AutoFlush = true };
        _csvWriter.WriteLine("TimestampIso,ElapsedMsSincePrevious,Source,Detail");
    }

    public void Log(string source, string detail)
    {
        var nowMs = _stopwatch.ElapsedMilliseconds;
        var elapsed = nowMs - _lastEventMs;
        _lastEventMs = nowMs;

        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff", CultureInfo.InvariantCulture);
        Console.WriteLine($"[{timestamp}] (+{elapsed}ms) {source}: {detail}");
        _csvWriter.WriteLine($"{DateTime.Now:O},{elapsed},{CsvEscape(source)},{CsvEscape(detail)}");
    }

    private static string CsvEscape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    public void Dispose() => _csvWriter.Dispose();
}
