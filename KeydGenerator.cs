namespace SixSlime.AxiomKeydGen;

using System.Text;
public static class KeydGenerator
{
    public static string Generate(ParsedConfig config)
    {
        var builder = new StringBuilder();
        builder
            .AppendLine("[ids]")
            .AppendLine("*");
        AppendLayer(config.MainLayer, builder);
        foreach (var additionalLayer in config.AdditionalLayers)
            AppendLayer(additionalLayer, builder);
        return builder.ToString();
    }

    private static void AppendLayer(Layer layer, StringBuilder builder)
    {
        builder.Append($"[{layer.Name}");
        if (layer.KeyMods.Count > 0)
        {
            builder.Append(":");
            var modSetString = string.Join(",", layer.KeyMods.Select(GetModifierChar));
            builder.Append(modSetString);
        }
        builder.AppendLine("]");
        foreach (var (key, mapping) in layer.Mappings)
        {
            builder.Append($"{key} = ");
            var keydValue = mapping switch
            {
                EKeyMapping.Char v => v.CharacterCode,
                EKeyMapping.KeydAction v => v.KeydValue,
                _ => throw new NotSupportedException()
            };
            builder.AppendLine(keydValue);
        }
    }

    private static string GetModifierChar(EKeyMod mod) => mod switch
    {
        EKeyMod.Shift => "S",
        EKeyMod.Alt => "A",
        EKeyMod.Control => "C",
        EKeyMod.Altgr => "G",
        EKeyMod.Meta => "M",
        _ => throw new NotSupportedException()
    };
}