using UnityEngine;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

public class SpellBuilder
{
    private Dictionary<string, JObject> spellDefinitions;

    public SpellBuilder(TextAsset spellsJson)
    {
        spellDefinitions = new Dictionary<string, JObject>();
        LoadSpells(spellsJson);
    }

    void LoadSpells(TextAsset spellsJson)
    {
        var data = JObject.Parse(spellsJson.text);
        foreach (var prop in data.Properties())
        {
            spellDefinitions[prop.Name] = (JObject)prop.Value;
            Debug.Log($"Loaded spell: {prop.Name}");
        }
    }

    public Spell Build(SpellCaster owner)
{
    JObject def = spellDefinitions["arcane_bolt"];
    
    var baseSpell = new BaseSpell(owner);
    baseSpell.SetAttributes(def);
    return baseSpell;
}

}
