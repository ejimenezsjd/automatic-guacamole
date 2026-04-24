using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Owns the list of all active creatures and keeps passive DNA rates in sync.
/// All creature mutations (add, remove, evolve) should go through this class.
/// </summary>
public class CreatureManager : MonoBehaviour
{
    public static CreatureManager Instance { get; private set; }

    private readonly List<Creature> _creatures = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Collection API ───────────────────────────────────────────────────

    /// <summary>
    /// Adds a creature. If one with the same sourceDataId already exists,
    /// increments its duplicate count instead of adding a second entry.
    /// </summary>
    public void AddCreature(Creature creature)
    {
        if (creature == null) return;

        Creature existing = _creatures.Find(c => c.sourceDataId == creature.sourceDataId);
        if (existing != null)
        {
            existing.duplicates++;
            UpdatePassiveRates();
            GameEvents.TriggerCreatureCreated(existing);
            return;
        }

        _creatures.Add(creature);
        UpdatePassiveRates();
        GameEvents.TriggerCreatureCreated(creature);
    }

    public void RemoveCreature(string id)
    {
        _creatures.RemoveAll(c => c.id == id);
        UpdatePassiveRates();
    }

    public Creature GetCreature(string id)
        => _creatures.Find(c => c.id == id);

    public List<Creature> GetAllCreatures()
        => new(_creatures);

    public int GetTotalCount()
        => _creatures.Count;

    // ── Production sync ──────────────────────────────────────────────────

    /// <summary>
    /// Recomputes all passive DNA rates from scratch.
    /// Called whenever the creature list or evolution levels change.
    /// </summary>
    public void UpdatePassiveRates()
    {
        foreach (DNAType type in System.Enum.GetValues(typeof(DNAType)))
            DNAController.Instance.SetPassiveRate(type, 0f);

        float prestigeMult = PrestigeSystem.Instance != null
            ? PrestigeSystem.Instance.GetProductionMultiplier()
            : 1f;

        foreach (var creature in _creatures)
        {
            float production = creature.GetTotalProduction(prestigeMult);
            float perType    = production / Mathf.Max(1, creature.dnaTypes.Count);

            foreach (var dnaType in creature.dnaTypes)
                DNAController.Instance.AddPassiveRate(dnaType, perType);
        }
    }

    // ── Save / Load ──────────────────────────────────────────────────────

    public void LoadFromSave(List<Creature> saved)
    {
        _creatures.Clear();
        if (saved != null) _creatures.AddRange(saved);
        UpdatePassiveRates();
    }
}
