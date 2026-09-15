using FlaUI.Core.AutomationElements;

namespace TeamsAccessibilityPoc.TextReading;

/// <summary>
/// Reads the current text of an element, preferring TextPattern (full document text)
/// and falling back to ValuePattern, then to the raw Name property.
/// </summary>
public static class ComposeTextReader
{
    public static string Read(AutomationElement element)
    {
        if (element.Patterns.Text.IsSupported)
            return element.Patterns.Text.Pattern.DocumentRange.GetText(-1);

        if (element.Patterns.Value.IsSupported)
            return element.Patterns.Value.Pattern.Value.Value;

        return element.Name ?? string.Empty;
    }
}
