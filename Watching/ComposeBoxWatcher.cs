using FlaUI.Core;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Definitions;
using FlaUI.Core.EventHandlers;
using TeamsAccessibilityPoc.Logging;
using TeamsAccessibilityPoc.TextReading;

namespace TeamsAccessibilityPoc.Watching;

/// <summary>
/// Registers text-change event handlers on a single element (Fase 3) and logs
/// every change with a timestamp (Fase 4 measurements). Focus tracking (deciding
/// which element to attach to) is the caller's responsibility.
/// </summary>
public sealed class ComposeBoxWatcher : IDisposable
{
    private readonly AutomationBase _automation;
    private readonly AutomationElement _target;
    private readonly EventLogger _logger;

    private readonly Action<string>? _onTextChanged;

    private TextEditTextChangedEventHandlerBase? _textEditHandler;
    private PropertyChangedEventHandlerBase? _valueChangedHandler;

    public AutomationElement Target => _target;

    public ComposeBoxWatcher(AutomationBase automation, AutomationElement target, EventLogger logger, Action<string>? onTextChanged = null)
    {
        _automation = automation;
        _target = target;
        _logger = logger;
        _onTextChanged = onTextChanged;
    }

    public void Start()
    {
        if (_target.Patterns.Text2.IsSupported)
        {
            _textEditHandler = _target.RegisterTextEditTextChangedEventHandler(
                TreeScope.Element,
                TextEditChangeType.None,
                (_, changeType, changedText) =>
                {
                    _logger.Log("TextEditTextChanged", $"{changeType}: {string.Join(" | ", changedText)}");
                    _onTextChanged?.Invoke(ComposeTextReader.Read(_target));
                });
        }
        else
        {
            _logger.Log("Info", "TextPattern2 indisponivel, TextEditTextChanged nao sera usado.");
        }

        if (_target.Patterns.Value.IsSupported)
        {
            var valueProperty = _automation.PropertyLibrary.Value.Value;
            _valueChangedHandler = _target.RegisterPropertyChangedEvent(
                TreeScope.Element,
                (_, _, newValue) =>
                {
                    var text = newValue?.ToString() ?? string.Empty;
                    _logger.Log("ValueChanged", $"len={text.Length} preview=\"{Preview(text)}\"");
                    _onTextChanged?.Invoke(text);
                },
                valueProperty);
        }
        else
        {
            _logger.Log("Info", "ValuePattern indisponivel, fallback por PropertyChanged nao sera usado.");
        }

        var initialText = ComposeTextReader.Read(_target);
        _logger.Log("Info", $"Texto inicial: len={initialText.Length} preview=\"{Preview(initialText)}\"");
        _onTextChanged?.Invoke(initialText);
    }

    private static string Preview(string text)
    {
        const int maxLength = 60;
        var singleLine = text.Replace('\n', ' ').Replace('\r', ' ');
        return singleLine.Length <= maxLength ? singleLine : singleLine[..maxLength] + "...";
    }

    public void Dispose()
    {
        if (_textEditHandler is not null) _target.FrameworkAutomationElement.UnregisterTextEditTextChangedEventHandler(_textEditHandler);
        if (_valueChangedHandler is not null) _target.FrameworkAutomationElement.UnregisterPropertyChangedEventHandler(_valueChangedHandler);
    }
}
