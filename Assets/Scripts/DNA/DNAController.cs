using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central authority for all DNA storage, consumption, and passive generation.
/// Holds the full type-weakness table as static data.
/// </summary>
public class DNAController : MonoBehaviour
{
    public static DNAController Instance { get; private set; }

    private readonly Dictionary<DNAType, float> _storage           = new();
    private readonly Dictionary<DNAType, float> _passiveRates      = new();

    // ── Weakness table ───────────────────────────────────────────────────
    // Key: defender type  |  Value: types that deal double damage to it
    private static readonly Dictionary<DNAType, List<DNAType>> Weaknesses = new()
    {
        { DNAType.Acero,     new List<DNAType> { DNAType.Lucha, DNAType.Fuego, DNAType.Tierra } },
        { DNAType.Agua,      new List<DNAType> { DNAType.Planta, DNAType.Electrico } },
        { DNAType.Bicho,     new List<DNAType> { DNAType.Volador, DNAType.Fuego, DNAType.Roca } },
        { DNAType.Dragon,    new List<DNAType> { DNAType.Hada, DNAType.Hielo, DNAType.Dragon } },
        { DNAType.Electrico, new List<DNAType> { DNAType.Tierra } },
        { DNAType.Fantasma,  new List<DNAType> { DNAType.Fantasma, DNAType.Siniestro } },
        { DNAType.Fuego,     new List<DNAType> { DNAType.Tierra, DNAType.Agua, DNAType.Roca } },
        { DNAType.Hada,      new List<DNAType> { DNAType.Acero, DNAType.Veneno } },
        { DNAType.Hielo,     new List<DNAType> { DNAType.Lucha, DNAType.Acero, DNAType.Roca, DNAType.Fuego } },
        { DNAType.Lucha,     new List<DNAType> { DNAType.Psiquico, DNAType.Volador, DNAType.Hielo } },
        { DNAType.Normal,    new List<DNAType> { DNAType.Lucha } },
        { DNAType.Planta,    new List<DNAType> { DNAType.Volador, DNAType.Bicho, DNAType.Veneno, DNAType.Hielo, DNAType.Fuego } },
        { DNAType.Psiquico,  new List<DNAType> { DNAType.Bicho, DNAType.Fantasma, DNAType.Siniestro } },
        { DNAType.Roca,      new List<DNAType> { DNAType.Lucha, DNAType.Tierra, DNAType.Acero, DNAType.Agua, DNAType.Planta } },
        { DNAType.Siniestro, new List<DNAType> { DNAType.Lucha, DNAType.Hada, DNAType.Bicho } },
        { DNAType.Tierra,    new List<DNAType> { DNAType.Agua, DNAType.Planta, DNAType.Hielo } },
        { DNAType.Veneno,    new List<DNAType> { DNAType.Tierra, DNAType.Psiquico } },
        { DNAType.Volador,   new List<DNAType> { DNAType.Roca, DNAType.Hielo, DNAType.Electrico } },
    };

    // ── Lifecycle ────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        InitializeStorage();
    }

    private void InitializeStorage()
    {
        foreach (DNAType type in System.Enum.GetValues(typeof(DNAType)))
        {
            _storage[type]      = 0f;
            _passiveRates[type] = 0f;
        }
    }

    // ── Storage API ──────────────────────────────────────────────────────

    public void AddDNA(DNAType type, float amount)
    {
        if (amount <= 0f) return;
        _storage[type] += amount;
        GameEvents.TriggerDNAAdded(type, amount);
        GameEvents.TriggerDNAUpdated(_storage);
    }

    /// <summary>Returns true and deducts if affordable; false otherwise.</summary>
    public bool ConsumeDNA(DNAType type, float amount)
    {
        if (_storage[type] < amount) return false;
        _storage[type] -= amount;
        GameEvents.TriggerDNAConsumed(type, amount);
        GameEvents.TriggerDNAUpdated(_storage);
        return true;
    }

    /// <summary>Atomic multi-type consumption — all or nothing.</summary>
    public bool ConsumeDNA(Dictionary<DNAType, float> costs)
    {
        foreach (var pair in costs)
            if (_storage[pair.Key] < pair.Value) return false;

        foreach (var pair in costs)
        {
            _storage[pair.Key] -= pair.Value;
            GameEvents.TriggerDNAConsumed(pair.Key, pair.Value);
        }
        GameEvents.TriggerDNAUpdated(_storage);
        return true;
    }

    public float GetDNA(DNAType type)
        => _storage.TryGetValue(type, out float v) ? v : 0f;

    public Dictionary<DNAType, float> GetAllDNA()
        => new(_storage);

    // ── Passive generation ───────────────────────────────────────────────

    public void SetPassiveRate(DNAType type, float rate)
        => _passiveRates[type] = Mathf.Max(0f, rate);

    public void AddPassiveRate(DNAType type, float delta)
        => _passiveRates[type] = Mathf.Max(0f, _passiveRates[type] + delta);

    /// <summary>Called every production tick; drives idle income.</summary>
    public void ApplyPassiveGeneration(float deltaTime)
    {
        bool changed = false;
        foreach (var pair in _passiveRates)
        {
            if (pair.Value <= 0f) continue;
            _storage[pair.Key] += pair.Value * deltaTime;
            changed = true;
        }
        if (changed) GameEvents.TriggerDNAUpdated(_storage);
    }

    public void ResetAllDNA()
    {
        foreach (DNAType type in System.Enum.GetValues(typeof(DNAType)))
            _storage[type] = 0f;
        GameEvents.TriggerDNAUpdated(_storage);
    }

    // ── Static queries ───────────────────────────────────────────────────

    public static List<DNAType> GetWeaknesses(DNAType type)
        => Weaknesses.TryGetValue(type, out var list) ? list : new List<DNAType>();

    public static bool IsWeakTo(DNAType defender, DNAType attacker)
        => Weaknesses.TryGetValue(defender, out var list) && list.Contains(attacker);

    // ── Save / Load ──────────────────────────────────────────────────────

    public void LoadFromSave(Dictionary<DNAType, float> saved)
    {
        foreach (var pair in saved)
            if (_storage.ContainsKey(pair.Key)) _storage[pair.Key] = pair.Value;
        GameEvents.TriggerDNAUpdated(_storage);
    }
}
