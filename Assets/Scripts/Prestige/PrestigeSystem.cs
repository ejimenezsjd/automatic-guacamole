using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Soft-reset system. Requires at least one Lv3 creature.
/// Awards Permanent Genes that boost production across all future runs.
/// </summary>
public class PrestigeSystem : MonoBehaviour
{
    public static PrestigeSystem Instance { get; private set; }

    [SerializeField] private float _productionMultiplierPerPrestige = 0.25f;
    [SerializeField] private float _rarityBonusPerPrestige          = 0.05f;
    [SerializeField] private int   _genesAwardedPerPrestige         = 10;

    private int _prestigeLevel   = 0;
    private int _permanentGenes  = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Public API ───────────────────────────────────────────────────────

    public bool CanPrestige()
    {
        var creatures = CreatureManager.Instance?.GetAllCreatures();
        return creatures != null && creatures.Exists(c => c.evolutionLevel >= 3);
    }

    public void Prestige()
    {
        if (!CanPrestige()) return;

        _prestigeLevel++;
        _permanentGenes += _genesAwardedPerPrestige * _prestigeLevel;

        ResetRunProgress();
        ApplyPrestigeBonuses();

        GameEvents.TriggerPrestige(_prestigeLevel);
        SaveSystem.Instance?.SaveGame();

        Debug.Log($"[Prestige] Level {_prestigeLevel} reached. Genes: {_permanentGenes}");
    }

    // ── Getters used by other systems ────────────────────────────────────

    /// <summary>
    /// Global production multiplier applied to every creature's output.
    /// 1.0 at prestige 0, grows linearly + permanent gene bonus.
    /// </summary>
    public float GetProductionMultiplier()
        => 1f
         + (_productionMultiplierPerPrestige * _prestigeLevel)
         + (_permanentGenes * 0.001f);

    public float GetRarityBonus()   => _rarityBonusPerPrestige * _prestigeLevel;
    public int   GetPrestigeLevel() => _prestigeLevel;
    public int   GetPermanentGenes() => _permanentGenes;

    // ── Internal ─────────────────────────────────────────────────────────

    private void ResetRunProgress()
    {
        CreatureManager.Instance?.LoadFromSave(new List<Creature>());
        DNAController.Instance?.ResetAllDNA();
    }

    private void ApplyPrestigeBonuses()
    {
        CreatureGenerator.Instance?.SetRarityBonus(GetRarityBonus());
        CreatureManager.Instance?.UpdatePassiveRates();
    }

    // ── Save / Load ──────────────────────────────────────────────────────

    public void LoadFromSave(int prestigeLevel, int permanentGenes)
    {
        _prestigeLevel  = prestigeLevel;
        _permanentGenes = permanentGenes;
        ApplyPrestigeBonuses();
    }
}
