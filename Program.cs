using FlaUI.UIA3;
using TeamsAccessibilityPoc.Debounce;
using TeamsAccessibilityPoc.Focus;
using TeamsAccessibilityPoc.Inspection;
using TeamsAccessibilityPoc.Logging;
using TeamsAccessibilityPoc.Suggestions;
using TeamsAccessibilityPoc.TextReading;
using TeamsAccessibilityPoc.UI;
using TeamsAccessibilityPoc.Watching;

var mode = args.Length > 0 ? args[0].ToLowerInvariant() : "";

if (mode is not ("read" or "watch"))
{
    Console.WriteLine("Uso: dotnet run -- read   (le o texto da proxima caixa em que voce clicar)");
    Console.WriteLine("     dotnet run -- watch  (monitora continuamente, trocando de caixa a cada clique)");
    return;
}

using var automation = new UIA3Automation();

if (mode == "read")
{
    Console.WriteLine("Clique na caixa de texto que voce quer ler...");

    using var gate = new ManualResetEventSlim(false);
    FlaUI.Core.AutomationElements.AutomationElement? captured = null;

    var handler = automation.RegisterFocusChangedEvent(focused =>
    {
        if (captured is not null || !EditableElementDetector.IsTextEditable(focused)) return;
        captured = focused;
        gate.Set();
    });

    gate.Wait();
    automation.UnregisterFocusChangedEvent(handler);

    ElementReport.Print(captured!);
    var text = ComposeTextReader.Read(captured!);
    Console.WriteLine($"Texto atual ({text.Length} caracteres):");
    Console.WriteLine(text);
    return;
}

// mode == "watch"
using var logger = new EventLogger();
using var uiHost = new SuggestionWindowHost();
uiHost.Start();

using var pokeClient = new PokeApiClient();
CancellationTokenSource? lookupCts = null;

async void LookupPokemon(string rawText)
{
    lookupCts?.Cancel();
    var cts = new CancellationTokenSource();
    lookupCts = cts;

    var query = rawText.Trim();
    if (query.Length == 0)
    {
        uiHost.UpdateApiStatus(string.Empty);
        uiHost.UpdateApiResult(string.Empty, null);
        return;
    }

    uiHost.UpdateApiStatus($"Buscando \"{query}\" na PokeAPI...");
    try
    {
        var pokemon = await pokeClient.GetPokemonAsync(query, cts.Token);
        if (cts.IsCancellationRequested) return;

        if (pokemon is null)
        {
            uiHost.UpdateApiStatus($"\"{query}\" nao encontrado.");
            uiHost.UpdateApiResult(string.Empty, null);
            return;
        }

        uiHost.UpdateApiStatus($"#{pokemon.Id} {pokemon.Name}");
        uiHost.UpdateApiResult(
            $"Tipo: {string.Join(", ", pokemon.Types)}\nAltura: {pokemon.Height}  Peso: {pokemon.Weight}",
            pokemon.SpriteUrl);
    }
    catch (OperationCanceledException)
    {
        // superseded by a newer lookup, ignore
    }
    catch (Exception ex)
    {
        if (!cts.IsCancellationRequested)
            uiHost.UpdateApiStatus($"Erro na chamada: {ex.Message}");
    }
}

using var debouncer = new Debouncer(TimeSpan.FromMilliseconds(500), LookupPokemon);

ComposeBoxWatcher? current = null;

var focusHandler = automation.RegisterFocusChangedEvent(focused =>
{
    if (!EditableElementDetector.IsTextEditable(focused))
        return;

    if (current is not null && focused.Equals(current.Target))
        return;

    current?.Dispose();
    logger.Log("Focus", $"nova caixa: {focused.ControlType} \"{focused.Name}\" (AutomationId={focused.AutomationId})");
    uiHost.UpdateStatus($"{focused.ControlType} — \"{focused.Name}\"");
    uiHost.UpdateApiStatus(string.Empty);
    uiHost.UpdateApiResult(string.Empty, null);

    current = new ComposeBoxWatcher(automation, focused, logger, text =>
    {
        uiHost.UpdateText(text);
        debouncer.Notify(text);
    });
    current.Start();
});

Console.WriteLine("Clique em qualquer caixa de texto para comecar a monitorar.");
Console.WriteLine($"Log salvo em: {logger.LogFilePath}");
Console.WriteLine("500ms depois da ultima mudanca, o texto e enviado para a PokeAPI.");
Console.WriteLine("Pressione ENTER para encerrar.");
Console.ReadLine();

current?.Dispose();
automation.UnregisterFocusChangedEvent(focusHandler);
