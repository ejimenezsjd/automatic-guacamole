using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates creature instances from weighted CreatureData pools.
/// Consumes the required DNA from DNAController automatically.
/// </summary>
public class CreatureGenerator : MonoBehaviour
{
    public static CreatureGenerator Instance { get; private set; }

    [SerializeField] private CreatureData[] _availableCreatures;
    [SerializeField] private RarityConfig   _rarityConfig;

    private float _rarityBonus = 0f;   // added by Mutator tool

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Public API ───────────────────────────────────────────────────────

    /// <summary>
    /// Selects a random creature template, validates affordability,
    /// consumes DNA, and returns a ready-to-use Creature instance.
    /// Returns null if unaffordable or no templates are registered.
    /// </summary>
    public Creature GenerateCreature()
    {
        CreatureData data = SelectCreatureData();
        if (data == null)
        {
            Debug.LogWarning("[CreatureGenerator] No creature templates available.");
            return null;
        }

        if (!CanAfford(data))
        {
            Debug.Log($"[CreatureGenerator] Not enough DNA to create {data.creatureName}.");
            return null;
        }

        var costs = BuildCostDict(data);
        if (!DNAController.Instance.ConsumeDNA(costs)) return null;

        return BuildCreature(data);
    }

    /// <summary>Creates a creature from a specific template without consuming DNA.</summary>
    public Creature GenerateCreatureFromData(CreatureData data)
    {
        if (data == null) return null;
        return BuildCreature(data);
    }

    public DNAType GetRandomDNAType()
    {
        var values = System.Enum.GetValues(typeof(DNAType));
        return (DNAType)values.GetValue(Random.Range(0, values.Length));
    }

    public float CalculateProduction(CreatureData data)
    {
        float rarityMult = _rarityConfig != null
            ? _rarityConfig.GetProductionMultiplier(data.rarity)
            : 1f;

        float typeSynergyBonus = 1f + (data.dnaTypes.Count - 1) * 0.15f;
        float variation        = Random.Range(0.9f, 1.1f);

        return data.baseProductionRate * rarityMult * typeSynergyBonus * variation;
    }

    /// <summary>Replaces the live creature pool (e.g. after loading additional data at runtime).</summary>
    public void RegisterCreatureData(CreatureData[] creatures)
        => _availableCreatures = creatures;

    /// <summary>Applied by the Mutator tool to shift spawn weights toward rarer entries.</summary>
    public void SetRarityBonus(float bonus) => _rarityBonus = bonus;

    // ── Internal helpers ─────────────────────────────────────────────────

    private CreatureData SelectCreatureData()
    {
        if (_availableCreatures == null || _availableCreatures.Length == 0) return null;

        float totalWeight = 0f;
        foreach (var d in _availableCreatures)
            totalWeight += GetAdjustedWeight(d);

        float roll       = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        foreach (var d in _availableCreatures)
        {
            cumulative += GetAdjustedWeight(d);
            if (roll <= cumulative) return d;
        }
        return _availableCreatures[^1];
    }

    private float GetAdjustedWeight(CreatureData data)
    {
        float rarityPenalty = (int)data.rarity * 0.1f;  // higher rarity = naturally lower
        return Mathf.Max(0.001f, data.spawnWeight - rarityPenalty + _rarityBonus * (int)data.rarity);
    }

    private Creature BuildCreature(CreatureData data)
    {
        var creature = new Creature(data)
        {
            productionRate = CalculateProduction(data)
        };
        return creature;
    }

    private Dictionary<DNAType, float> BuildCostDict(CreatureData data)
    {
        var costs = new Dictionary<DNAType, float>();
        foreach (var cost in data.creationCosts)
        {
            if (costs.ContainsKey(cost.type)) costs[cost.type] += cost.amount;
            else costs[cost.type] = cost.amount;
        }
        return costs;
    }

    private bool CanAfford(CreatureData data)
    {
        foreach (var cost in data.creationCosts)
            if (DNAController.Instance.GetDNA(cost.type) < cost.amount) return false;
        return true;
    }
}
