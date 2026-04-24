using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages tool unlock state and drives per-tick tool effects.
/// Extend by adding entries to ToolType enum and handling them in ProcessTick.
/// </summary>
public class ToolSystem : MonoBehaviour
{
    public static ToolSystem Instance { get; private set; }

    [SerializeField] private ToolData[] _toolDataAssets;

    private readonly Dictionary<ToolType, bool>     _unlocked  = new();
    private readonly Dictionary<ToolType, bool>     _active    = new();
    private readonly Dictionary<ToolType, float>    _cooldowns = new();
    private readonly Dictionary<ToolType, ToolData> _dataMap   = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InitializeTools();
    }

    // ── Initialisation ───────────────────────────────────────────────────

    private void InitializeTools()
    {
        foreach (ToolType type in System.Enum.GetValues(typeof(ToolType)))
        {
            _unlocked[type]  = false;
            _active[type]    = false;
            _cooldowns[type] = 0f;
        }

        foreach (var data in _toolDataAssets)
        {
            _dataMap[data.toolType] = data;
            if (data.unlockedByDefault) _unlocked[data.toolType] = true;
        }
    }

    // ── State queries ────────────────────────────────────────────────────

    public bool IsUnlocked(ToolType type) => _unlocked.TryGetValue(type, out bool v) && v;
    public bool IsActive(ToolType type)   => _active.TryGetValue(type, out bool v)   && v;

    // ── Unlock / Activate ────────────────────────────────────────────────

    public bool TryUnlockTool(ToolType type)
    {
        if (IsUnlocked(type)) return true;
        if (!_dataMap.TryGetValue(type, out var data)) return false;

        var costs = new Dictionary<DNAType, float>();
        foreach (var c in data.unlockCosts) costs[c.dnaType] = c.amount;

        if (costs.Count > 0 && !DNAController.Instance.ConsumeDNA(costs)) return false;

        _unlocked[type] = true;
        ApplyPassiveEffect(type);
        GameEvents.TriggerToolUnlocked(type);
        return true;
    }

    public void SetActive(ToolType type, bool active)
    {
        if (!IsUnlocked(type)) return;
        _active[type] = active;
        if (active) GameEvents.TriggerToolActivated(type);
    }

    // ── Tick processing ──────────────────────────────────────────────────

    /// <summary>Called every production tick by ProductionManager.</summary>
    public void ProcessTick(float deltaTime)
    {
        TickAutoExtractor(deltaTime);
        TickRefiner(deltaTime);
        TickCloner(deltaTime);
    }

    private void TickAutoExtractor(float deltaTime)
    {
        if (!IsActive(ToolType.AutoExtractor)) return;
        if (!_dataMap.TryGetValue(ToolType.AutoExtractor, out var data)) return;

        DNAController.Instance.AddDNA(data.extractorOutputType,
            data.extractorOutputPerSecond * deltaTime);
    }

    private void TickRefiner(float deltaTime)
    {
        if (!IsActive(ToolType.Refiner)) return;
        if (!_dataMap.TryGetValue(ToolType.Refiner, out var data)) return;

        float available = DNAController.Instance.GetDNA(data.refinerInputType);
        float toConvert = Mathf.Min(available, data.refinerConversionRatio * deltaTime * 10f);
        if (toConvert <= 0f) return;

        DNAController.Instance.ConsumeDNA(data.refinerInputType, toConvert);
        DNAController.Instance.AddDNA(data.refinerOutputType, toConvert / data.refinerConversionRatio);
    }

    private void TickCloner(float deltaTime)
    {
        if (!IsActive(ToolType.Cloner)) return;
        if (!_dataMap.TryGetValue(ToolType.Cloner, out var data)) return;

        _cooldowns[ToolType.Cloner] -= deltaTime;
        if (_cooldowns[ToolType.Cloner] > 0f) return;

        _cooldowns[ToolType.Cloner] = data.clonerCooldownSeconds;

        var creatures = CreatureManager.Instance.GetAllCreatures();
        if (creatures.Count == 0) return;

        Creature target = creatures[Random.Range(0, creatures.Count)];
        target.duplicates++;
        Debug.Log($"[Cloner] Duplicated {target.name}. Dupes: {target.duplicates}");
    }

    // ── Passive effects (applied once on unlock) ─────────────────────────

    private void ApplyPassiveEffect(ToolType type)
    {
        if (type == ToolType.Mutator && _dataMap.TryGetValue(type, out var data))
            CreatureGenerator.Instance?.SetRarityBonus(data.mutatorRarityBonus);
    }

    // ── Data access ──────────────────────────────────────────────────────

    public ToolData GetToolData(ToolType type)
        => _dataMap.TryGetValue(type, out var d) ? d : null;

    // ── Save / Load ──────────────────────────────────────────────────────

    public void LoadFromSave(Dictionary<ToolType, bool> unlockedState,
                             Dictionary<ToolType, bool> activeState)
    {
        foreach (var pair in unlockedState) _unlocked[pair.Key] = pair.Value;
        foreach (var pair in activeState)   _active[pair.Key]   = pair.Value;

        // Re-apply passive effects for unlocked tools
        foreach (var pair in _unlocked)
            if (pair.Value) ApplyPassiveEffect(pair.Key);
    }
}
