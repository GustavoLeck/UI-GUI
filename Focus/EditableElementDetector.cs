using FlaUI.Core.AutomationElements;

namespace TeamsAccessibilityPoc.Focus;

/// <summary>
/// Heuristic to decide whether a focused element is worth treating as a text box:
/// it must expose ValuePattern or TextPattern, since those are what we read from.
/// </summary>
public static class EditableElementDetector
{
    public static bool IsTextEditable(AutomationElement element)
    {
        return element.Patterns.Value.IsSupported || element.Patterns.Text.IsSupported;
    }
}
