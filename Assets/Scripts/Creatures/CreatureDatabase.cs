using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central registry for all CreatureData ScriptableObjects.
/// Loads from Resources/Creatures/ automatically; can also be populated via Inspector.
/// Registers the catalogue with CreatureGenerator after loading.
/// </summary>
public class CreatureDatabase : MonoBehaviour
{
    public static CreatureDatabase Instance { get; private set; }

    [SerializeField] private CreatureData[] _creatures;

    private readonly Dictionary<string, CreatureData> _byId    = new();
    private readonly Dictionary<RarityType, List<CreatureData>> _byRarity = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (_creatures == null || _creatures.Length == 0)
            _creatures = Resources.LoadAll<CreatureData>("Creatures");

        if (_creatures.Length == 0)
            Debug.LogWarning("[CreatureDatabase] No CreatureData assets found. " +
                             "Place them in Resources/Creatures/ or assign them in the Inspector.");

        BuildIndices();
        CreatureGenerator.Instance?.RegisterCreatureData(_creatures);
    }

    // ── Index building ───────────────────────────────────────────────────

    private void BuildIndices()
    {
        _byId.Clear();
        _byRarity.Clear();

        foreach (RarityType r in System.Enum.GetValues(typeof(RarityType)))
            _byRarity[r] = new List<CreatureData>();

        foreach (var data in _creatures)
        {
            if (string.IsNullOrEmpty(data.creatureId))
            {
                Debug.LogWarning($"[CreatureDatabase] {data.name} has no creatureId — skipped.");
                continue;
            }

            if (_byId.ContainsKey(data.creatureId))
            {
                Debug.LogWarning($"[CreatureDatabase] Duplicate creatureId '{data.creatureId}' — skipped.");
                continue;
            }

            _byId[data.creatureId] = data;
            _byRarity[data.rarity].Add(data);
        }

        Debug.Log($"[CreatureDatabase] Loaded {_byId.Count} creature templates.");
    }

    // ── Query API ────────────────────────────────────────────────────────

    public CreatureData GetById(string id)
        => _byId.TryGetValue(id, out var d) ? d : null;

    public CreatureData[] GetAll()
        => _creatures;

    public List<CreatureData> GetByRarity(RarityType rarity)
        => _byRarity.TryGetValue(rarity, out var list) ? list : new List<CreatureData>();

    public int TotalCount => _creatures?.Length ?? 0;

    /// <summary>
    /// Merges additional creature data at runtime (e.g. from DLC or procedural generation).
    /// </summary>
    public void RegisterAdditional(CreatureData[] additional)
    {
        var combined = new CreatureData[(_creatures?.Length ?? 0) + additional.Length];
        _creatures?.CopyTo(combined, 0);
        additional.CopyTo(combined, _creatures?.Length ?? 0);
        _creatures = combined;
        BuildIndices();
        CreatureGenerator.Instance?.RegisterCreatureData(_creatures);
    }
}
