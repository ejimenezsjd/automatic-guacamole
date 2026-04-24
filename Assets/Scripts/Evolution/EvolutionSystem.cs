using UnityEngine;

/// <summary>
/// Handles all creature evolution logic.
/// Requirements and multipliers are driven by EvolutionConfig (ScriptableObject).
/// </summary>
public class EvolutionSystem : MonoBehaviour
{
    public static EvolutionSystem Instance { get; private set; }

    private const int MaxLevel = 3;

    [SerializeField] private EvolutionConfig _config;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Public API ───────────────────────────────────────────────────────

    public bool CanEvolve(Creature creature)
    {
        if (creature == null || creature.evolutionLevel >= MaxLevel) return false;

        var req = _config?.GetRequirement(creature.evolutionLevel);
        if (req == null) return false;

        if (creature.duplicates < req.duplicatesRequired) return false;

        if (req.specialResourceAmount > 0f &&
            DNAController.Instance.GetDNA(req.specialResourceType) < req.specialResourceAmount)
            return false;

        return true;
    }

    /// <summary>
    /// Attempts evolution. Returns true on success.
    /// Consumes duplicates and special resources, boosts production rate,
    /// and fires tool unlocks where applicable.
    /// </summary>
    public bool TryEvolve(Creature creature)
    {
        if (!CanEvolve(creature)) return false;

        var req = _config.GetRequirement(creature.evolutionLevel);

        creature.duplicates -= req.duplicatesRequired;

        if (req.specialResourceAmount > 0f)
            DNAController.Instance.ConsumeDNA(req.specialResourceType, req.specialResourceAmount);

        creature.evolutionLevel++;
        creature.productionRate *= req.productionBoostMultiplier;

        TryUnlockToolsForLevel(creature.evolutionLevel);

        CreatureManager.Instance.UpdatePassiveRates();
        GameEvents.TriggerCreatureEvolved(creature);

        return true;
    }

    public (int duplicates, DNAType specialType, float specialAmount) GetRequirements(Creature creature)
    {
        var req = _config?.GetRequirement(creature?.evolutionLevel ?? 0);
        return req == null
            ? (0, DNAType.Normal, 0f)
            : (req.duplicatesRequired, req.specialResourceType, req.specialResourceAmount);
    }

    // ── Internal ─────────────────────────────────────────────────────────

    private void TryUnlockToolsForLevel(int newLevel)
    {
        if (ToolSystem.Instance == null) return;

        if (newLevel >= 2) ToolSystem.Instance.TryUnlockTool(ToolType.Refiner);
        if (newLevel >= 3) ToolSystem.Instance.TryUnlockTool(ToolType.Cloner);
    }
}
