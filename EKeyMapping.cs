namespace SixSlime.AxiomKeydGen;

public abstract record EKeyMapping
{
    public sealed record Char : EKeyMapping
    {
        public required string CharacterCode { get; init; }
        public required string? ActionAlias { get; init; }
    }

    public sealed record KeydAction : EKeyMapping
    {
        public required string KeydValue { get; init; }
    }
}