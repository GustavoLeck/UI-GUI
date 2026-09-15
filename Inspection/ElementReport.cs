using FlaUI.Core.AutomationElements;

namespace TeamsAccessibilityPoc.Inspection;

/// <summary>
/// Prints the details needed for Fase 1 (manual inspection): identity of the element
/// and which text-related patterns it actually supports.
/// </summary>
public static class ElementReport
{
    public static void Print(AutomationElement element)
    {
        Console.WriteLine("--- Elemento capturado ---");
        Console.WriteLine($"Name           : {element.Name}");
        Console.WriteLine($"AutomationId   : {element.AutomationId}");
        Console.WriteLine($"ControlType    : {element.ControlType}");
        Console.WriteLine($"ClassName      : {element.ClassName}");
        Console.WriteLine($"FrameworkType  : {element.FrameworkType}");
        Console.WriteLine($"IsEnabled      : {element.IsEnabled}");
        Console.WriteLine($"ValuePattern   : {element.Patterns.Value.IsSupported}");
        Console.WriteLine($"TextPattern    : {element.Patterns.Text.IsSupported}");
        Console.WriteLine($"TextPattern2   : {element.Patterns.Text2.IsSupported}");
        Console.WriteLine("--------------------------");
    }
}
