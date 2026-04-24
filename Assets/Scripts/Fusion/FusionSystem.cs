using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Core differentiator system — combines two creatures into a new, more powerful hybrid.
/// Both parents must be at least level 2 and a DNA cost is charged.
/// </summary>
public class FusionSystem : MonoBehaviour
{
    public static FusionSystem Instance { get; private set; }

    private const int   MinEvolutionLevel   = 2;
    private const float FusionProductionBonus = 1.4f;
    private const int   MaxDNATypes          = 3;

    [SerializeField] private float _fusionCostPerLevel = 50f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Public API ───────────────────────────────────────────────────────

    public bool CanFuse(Creature a, Creature b)
    {
        if (a == null || b == null) return false;
        if (a.id == b.id)          return false;
        if (a.evolutionLevel < MinEvolutionLevel) return false;
        if (b.evolutionLevel < MinEvolutionLevel) return false;
        return true;
    }

    /// <summary>
    /// Fuses creatures a and b into a new hybrid.
    /// Removes both parents and registers the result.
    /// Returns null if requirements are not met or DNA cost cannot be paid.
    /// </summary>
    public Creature Fuse(Creature a, Creature b)
    {
        if (!CanFuse(a, b)) return null;

        var cost = CalculateFusionCost(a, b);
        if (!DNAController.Instance.ConsumeDNA(cost)) return null;

        var combinedTypes = MergeDNATypes(a.dnaTypes, b.dnaTypes);
        float newProduction = (a.productionRate + b.productionRate) * FusionProductionBonus;
        RarityType newRarity = DetermineOutputRarity(a.rarity, b.rarity);
        string newName = GenerateFusedName(a.name, b.name);

        var fused = new Creature(newName, combinedTypes, newProduction, newRarity);

        CreatureManager.Instance.RemoveCreature(a.id);
        CreatureManager.Instance.RemoveCreature(b.id);
        CreatureManager.Instance.AddCreature(fused);

        GameEvents.TriggerCreaturesFused(a, b, fused);
        return fused;
    }

    public Dictionary<DNAType, float> CalculateFusionCost(Creature a, Creature b)
    {
        var allTypes = MergeDNATypes(a.dnaTypes, b.dnaTypes);
        float baseCost = _fusionCostPerLevel * ((a.evolutionLevel + b.evolutionLevel) * 0.5f);

        var cost = new Dictionary<DNAType, float>();
        foreach (var type in allTypes)
            cost[type] = baseCost;
        return cost;
    }

    // ── Internal helpers ─────────────────────────────────────────────────

    private static List<DNAType> MergeDNATypes(List<DNAType> a, List<DNAType> b)
    {
        var merged = new List<DNAType>(a);
        foreach (var t in b)
            if (!merged.Contains(t)) merged.Add(t);

        if (merged.Count > MaxDNATypes)
            merged = merged.GetRange(0, MaxDNATypes);

        return merged;
    }

    private static RarityType DetermineOutputRarity(RarityType a, RarityType b)
    {
        var highest = (RarityType)Mathf.Max((int)a, (int)b);
        // 20 % chance to upgrade one tier
        if (highest < RarityType.Epic && Random.value < 0.20f)
            return highest + 1;
        return highest;
    }

    private static string GenerateFusedName(string nameA, string nameB)
    {
        int take = Mathf.CeilToInt(nameA.Length * 0.5f);
        string prefix = nameA[..Mathf.Min(take, nameA.Length)];
        string suffix  = nameB[Mathf.Max(0, nameB.Length / 2)..];
        return prefix + suffix;
    }
}
