namespace SixSlime.AxiomKeydGen;

public record Layer
{
    public required IReadOnlyCollection<EKeyMod> KeyMods { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyDictionary<string, EKeyMapping> Mappings { get; init; }
}