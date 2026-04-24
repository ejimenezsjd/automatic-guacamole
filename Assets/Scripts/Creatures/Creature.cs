using System.Collections.Generic;

/// <summary>
/// Runtime instance of a creature. Serializable for JSON save/load.
/// Holds only data — all logic lives in manager classes.
/// </summary>
[System.Serializable]
public class Creature
{
    public string     id;
    public string     name;
    public List<DNAType> dnaTypes = new();
    public int        evolutionLevel;   // 1–3
    public int        duplicates;
    public float      productionRate;
    public RarityType rarity;
    public string     sourceDataId;     // links back to CreatureData.creatureId

    public Creature() { }

    /// <summary>Creates an instance from a ScriptableObject template.</summary>
    public Creature(CreatureData data)
    {
        sourceDataId   = data.creatureId;
        id             = System.Guid.NewGuid().ToString();
        name           = data.creatureName;
        dnaTypes       = new List<DNAType>(data.dnaTypes);
        evolutionLevel = 1;
        duplicates     = 0;
        productionRate = data.baseProductionRate;
        rarity         = data.rarity;
    }

    /// <summary>Constructor for dynamically fused creatures (no template).</summary>
    public Creature(string newName, List<DNAType> types, float production, RarityType rarityType)
    {
        id             = System.Guid.NewGuid().ToString();
        sourceDataId   = "fused_" + id;
        name           = newName;
        dnaTypes       = new List<DNAType>(types);
        evolutionLevel = 1;
        duplicates     = 0;
        productionRate = production;
        rarity         = rarityType;
    }

    /// <summary>
    /// Effective production after evolution scaling and prestige multiplier.
    /// Evolution multipliers: Lv1 = 1×, Lv2 = 2.5×, Lv3 = 6×.
    /// </summary>
    public float GetTotalProduction(float prestigeMultiplier = 1f)
    {
        float evoMult = evolutionLevel switch
        {
            2 => 2.5f,
            3 => 6.0f,
            _ => 1.0f
        };
        return productionRate * evoMult * prestigeMultiplier;
    }
}
