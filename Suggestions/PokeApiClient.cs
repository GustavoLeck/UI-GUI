using System.Linq;
using System.Net.Http;
using System.Text.Json;

namespace TeamsAccessibilityPoc.Suggestions;

public sealed record PokemonInfo(int Id, string Name, int Height, int Weight, string[] Types, string? SpriteUrl);

/// <summary>
/// Stand-in for the real suggestion backend: GETs https://pokeapi.co/api/v2/pokemon/{value}
/// to prove the capture-to-external-call pipeline works end to end.
/// </summary>
public sealed class PokeApiClient : IDisposable
{
    private readonly HttpClient _http = new() { Timeout = TimeSpan.FromSeconds(10) };

    public async Task<PokemonInfo?> GetPokemonAsync(string query, CancellationToken ct = default)
    {
        var normalized = query.Trim().ToLowerInvariant();
        if (normalized.Length == 0)
            return null;

        var url = $"https://pokeapi.co/api/v2/pokemon/{Uri.EscapeDataString(normalized)}";

        using var response = await _http.GetAsync(url, ct);
        if (!response.IsSuccessStatusCode)
            return null;

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
        var root = doc.RootElement;

        var types = root.GetProperty("types")
            .EnumerateArray()
            .Select(t => t.GetProperty("type").GetProperty("name").GetString() ?? "")
            .Where(s => s.Length > 0)
            .ToArray();

        var sprite = root.GetProperty("sprites").GetProperty("front_default").GetString();

        return new PokemonInfo(
            root.GetProperty("id").GetInt32(),
            root.GetProperty("name").GetString() ?? normalized,
            root.GetProperty("height").GetInt32(),
            root.GetProperty("weight").GetInt32(),
            types,
            sprite);
    }

    public void Dispose() => _http.Dispose();
}
