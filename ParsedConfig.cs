namespace SixSlime.AxiomKeydGen;

using Tomlyn.Model;
using Tomlyn;
public record ParsedConfig
{
    public IReadOnlyCollection<Layer> Layers { get; }
    public IReadOnlyDictionary<EKeyMod, string> ModifierAliases { get; }
    public ParsedConfig(string tomlText)
    {
        var model = TomlSerializer.Deserialize<TomlTable>(tomlText);
        if (model is null) throw new ProgramException("invalid toml?");
        if (!model.TryGetValue("main", out var mainLayer))
            throw new ProgramException("'main' key must be a table");
        var layers = new List<Layer>()
        {
            ParseLayer(mainLayer, "main")
        };
        if (model.TryGetValue("layer", out var layerMappingObj))
        {
            if (layerMappingObj is not TomlTable layerMapping)
                throw new ProgramException("'layer' key must be a table");
            foreach (var (layerKey, layerObj) in layerMapping)
            {
                layers.Add(ParseLayer(layerObj, layerKey));
            }
        }

        if (!model.TryGetValue("modifier_aliases", out var modAliasTableObj) || modAliasTableObj is not TomlTable modAliasTable)
            throw new ProgramException("key 'modifier_aliases' must be defined");

        var modAliases = new Dictionary<EKeyMod, string>();
        foreach (var (modString, aliasObj) in modAliasTable)
        {
            var alias = aliasObj as string ?? throw new ProgramException("mod alias must be a string");
            modAliases.Add(ParseModString(modString), alias);
        }

        Layers = layers.AsReadOnly();
        ModifierAliases = modAliases;
    }

    private static Layer ParseLayer(object? table, string keyName)
    {
        if (table is not TomlTable layer) throw new ProgramException($"layer '{keyName}' must be a table");
        var nameSplit = keyName.Split("--");
        if (nameSplit.Length > 2)
            throw new ProgramException("layer keys must only have one instance of '--' to indicate start of modifiers");
        var mods = new List<EKeyMod>();
        if (nameSplit.Length == 2)
            mods.AddRange(nameSplit[1].Split('+').Select(ParseModString));
        var layerName = nameSplit[0];
        var mappings = new Dictionary<string, EKeyMapping>();
        foreach (var (k, v) in layer)
        {
            mappings[k] = ParseKeyMapping(v);
        }

        return new()
        {
            Name = layerName,
            KeyMods = mods.AsReadOnly(),
            Mappings = mappings,
        };
    }

    private static EKeyMod ParseModString(string modString) => modString switch
    {
        "shift" => EKeyMod.Shift,
        "meta" => EKeyMod.Meta,
        "alt" => EKeyMod.Alt,
        "altgr" => EKeyMod.Altgr,
        "control" => EKeyMod.Control,
        _ => throw new ProgramException($"invalid modifier '{modString}'")
    };

    private static EKeyMapping ParseKeyMapping(object tomlValue)
    {
        switch (tomlValue)
        {
            case TomlTable v:
                if (v.TryGetValue("layer", out var layerName))
                    return new EKeyMapping.KeydAction()
                    {
                        KeydValue = $"layer({layerName})"
                    };
                if (v.TryGetValue("keyd", out var keydAction))
                    return new EKeyMapping.KeydAction()
                    {
                        KeydValue = keydAction as string ?? throw new ProgramException($"value for 'keyd' mapping type must be a string")
                    };
                throw new ProgramException("special mappings can only have keys 'layer' or 'keyd'");
            case string v:
                return new EKeyMapping.Char()
                {
                    CharacterCode = v,
                    ActionAlias = null
                };
            case TomlArray v:
                if (v.Count != 2) throw new ProgramException("array-type mappings must have length 2 [<char code>, <action alias>]");
                return new EKeyMapping.Char()
                {
                    CharacterCode = v[0] as string ?? throw new ProgramException("array-type mappings must have 2 strings"),
                    ActionAlias = v[1] as string ?? throw new ProgramException("array-type mappings must have 2 strings")
                };
            default:
                throw new ProgramException("invalid keymapping type encountered");
        }
    }
}