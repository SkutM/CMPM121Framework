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

public Spell Build(string spellName, SpellCaster owner)
{
    if (!spellDefinitions.ContainsKey(spellName))
    {
        Debug.LogWarning($"Spell '{spellName}' not found! Defaulting to arcane_bolt.");
        spellName = "arcane_bolt";
    }

    JObject def = spellDefinitions[spellName];
    Spell spell;

    switch (spellName)
    {
        case "arcane_bolt":
            spell = new BaseSpell(owner);
            break;
        case "arcane_spray":
            spell = new ArcaneSpraySpell(owner);
            break;
        case "magic_missile":
            spell = new MagicMissileSpell(owner);
            break;
        case "arcane_explosion":
            spell = new ArcaneExplosionSpell(owner);
            break;
        case "chaining_lightning":
            spell = new ChainingLightningSpell(owner);
            break;
        case "fireball":
            spell = new FireballSpell(owner);
            break;

        // === Modifiers ===
        case "splitter":
            spell = new SplitterSpell(Build(def["inner"].ToString(), owner));
            break;
        case "doubler":
            spell = new DoublerSpell(Build(def["inner"].ToString(), owner));
            break;
        case "damage_magnifier":
            spell = new DamageMagnifierSpell(Build(def["inner"].ToString(), owner));
            break;
        case "speed_modifier":
            spell = new SpeedModifierSpell(Build(def["inner"].ToString(), owner));
            break;
        case "chaos_modifier":
            spell = new ChaosModifierSpell(Build(def["inner"].ToString(), owner));
            break;
        case "homing_modifier":
            spell = new HomingModifierSpell(Build(def["inner"].ToString(), owner));
            break;
        case "slow_on_hit":
            spell = new SlowOnHitModifierSpell(Build(def["inner"].ToString(), owner));
            break;
        case "knockback_on_hit":
            spell = new KnockbackModifierSpell(Build(def["inner"].ToString(), owner));
            break;

        default:
            Debug.LogWarning($"Unhandled spell type '{spellName}', using base spell.");
            spell = new BaseSpell(owner);
            break;
    }

    // Only call SetAttributes if it's a BaseSpell or subclass
    if (spell is BaseSpell baseSpell)
    {
        baseSpell.SetAttributes(def);
    }

    return spell;
}

public Spell BuildRandomSpell(SpellCaster owner)
{
    // 1️⃣ Pick a random base spell
    string[] baseSpells = { "arcane_bolt", "arcane_spray", "magic_missile", "arcane_explosion", "chaining_lightning", "fireball" };
    string baseSpellName = baseSpells[UnityEngine.Random.Range(0, baseSpells.Length)];
    Spell spell = Build(baseSpellName, owner);

    // 2️⃣ Decide how many modifiers to add (0–3)
    int modifierCount = UnityEngine.Random.Range(0, 4);

    string[] modifiers = { "splitter", "doubler", "damage_magnifier", "speed_modifier", "chaos_modifier", "homing_modifier", "slow_on_hit", "knockback_on_hit" };

    for (int i = 0; i < modifierCount; i++)
    {
        string mod = modifiers[UnityEngine.Random.Range(0, modifiers.Length)];

        switch (mod)
        {
            case "splitter":
                spell = new SplitterSpell(spell);
                break;
            case "doubler":
                spell = new DoublerSpell(spell);
                break;
            case "damage_magnifier":
                spell = new DamageMagnifierSpell(spell);
                break;
            case "speed_modifier":
                spell = new SpeedModifierSpell(spell);
                break;
            case "chaos_modifier":
                spell = new ChaosModifierSpell(spell);
                break;
            case "homing_modifier":
                spell = new HomingModifierSpell(spell);
                break;
            case "slow_on_hit":
                spell = new SlowOnHitModifierSpell(spell);
                break;
            case "knockback_on_hit":
                spell = new KnockbackModifierSpell(spell);
                break;
        }
    }

    return spell;
}





}
