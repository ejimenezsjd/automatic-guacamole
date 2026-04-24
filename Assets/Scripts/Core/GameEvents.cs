using System;
using System.Collections.Generic;

/// <summary>
/// Static event bus — decouples all systems from each other.
/// Subscribe in OnEnable, unsubscribe in OnDisable.
/// </summary>
public static class GameEvents
{
    // ── DNA ──────────────────────────────────────────────────────────────
    public static event Action<DNAType, float>                OnDNAAdded;
    public static event Action<DNAType, float>                OnDNAConsumed;
    public static event Action<Dictionary<DNAType, float>>    OnDNAUpdated;

    // ── Creatures ────────────────────────────────────────────────────────
    public static event Action<Creature>                      OnCreatureCreated;
    public static event Action<Creature>                      OnCreatureEvolved;
    public static event Action<Creature, Creature, Creature>  OnCreaturesFused;

    // ── Tools ────────────────────────────────────────────────────────────
    public static event Action<ToolType>  OnToolUnlocked;
    public static event Action<ToolType>  OnToolActivated;

    // ── Prestige ─────────────────────────────────────────────────────────
    public static event Action<int>  OnPrestige;

    // ── Production ───────────────────────────────────────────────────────
    public static event Action  OnTickProcessed;

    // ── Triggers ─────────────────────────────────────────────────────────
    public static void TriggerDNAAdded(DNAType type, float amount)
        => OnDNAAdded?.Invoke(type, amount);

    public static void TriggerDNAConsumed(DNAType type, float amount)
        => OnDNAConsumed?.Invoke(type, amount);

    public static void TriggerDNAUpdated(Dictionary<DNAType, float> dna)
        => OnDNAUpdated?.Invoke(dna);

    public static void TriggerCreatureCreated(Creature c)
        => OnCreatureCreated?.Invoke(c);

    public static void TriggerCreatureEvolved(Creature c)
        => OnCreatureEvolved?.Invoke(c);

    public static void TriggerCreaturesFused(Creature a, Creature b, Creature result)
        => OnCreaturesFused?.Invoke(a, b, result);

    public static void TriggerToolUnlocked(ToolType t)
        => OnToolUnlocked?.Invoke(t);

    public static void TriggerToolActivated(ToolType t)
        => OnToolActivated?.Invoke(t);

    public static void TriggerPrestige(int level)
        => OnPrestige?.Invoke(level);

    public static void TriggerTickProcessed()
        => OnTickProcessed?.Invoke();
}
