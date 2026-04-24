using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Serialises game state to PlayerPrefs as JSON.
/// Auto-saves every 30 seconds and on application pause/quit.
/// Applies offline income on load (capped at 8 hours).
/// </summary>
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    private const string SaveKey           = "ScrachyMons_Save_v1";
    private const float  AutoSaveInterval  = 30f;
    private const float  MaxOfflineSeconds = 8f * 3600f;

    private float _autoSaveTimer;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        _autoSaveTimer += Time.deltaTime;
        if (_autoSaveTimer >= AutoSaveInterval)
        {
            _autoSaveTimer = 0f;
            SaveGame();
        }
    }

    // ── Save ─────────────────────────────────────────────────────────────

    public void SaveGame()
    {
        try
        {
            var data = new SaveData
            {
                prestigeLevel     = PrestigeSystem.Instance?.GetPrestigeLevel()  ?? 0,
                permanentGenes    = PrestigeSystem.Instance?.GetPermanentGenes() ?? 0,
                lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            };

            var allDNA = DNAController.Instance?.GetAllDNA();
            if (allDNA != null)
                foreach (var pair in allDNA)
                    data.dnaStorage.Add(new SaveData.SerializedDNA { type = pair.Key, amount = pair.Value });

            data.creatures = CreatureManager.Instance?.GetAllCreatures() ?? new List<Creature>();

            foreach (ToolType type in Enum.GetValues(typeof(ToolType)))
                data.toolStates.Add(new SaveData.SerializedTool
                {
                    toolType   = type,
                    isUnlocked = ToolSystem.Instance?.IsUnlocked(type) ?? false,
                    isActive   = ToolSystem.Instance?.IsActive(type)   ?? false,
                });

            PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data, true));
            PlayerPrefs.Save();
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Save failed: {e.Message}");
        }
    }

    // ── Load ─────────────────────────────────────────────────────────────

    public bool LoadGame()
    {
        if (!PlayerPrefs.HasKey(SaveKey)) return false;

        try
        {
            var data = JsonUtility.FromJson<SaveData>(PlayerPrefs.GetString(SaveKey));

            PrestigeSystem.Instance?.LoadFromSave(data.prestigeLevel, data.permanentGenes);

            var dnaDict = new Dictionary<DNAType, float>();
            foreach (var d in data.dnaStorage) dnaDict[d.type] = d.amount;
            DNAController.Instance?.LoadFromSave(dnaDict);

            CreatureManager.Instance?.LoadFromSave(data.creatures);

            var unlocked = new Dictionary<ToolType, bool>();
            var active   = new Dictionary<ToolType, bool>();
            foreach (var t in data.toolStates)
            {
                unlocked[t.toolType] = t.isUnlocked;
                active[t.toolType]   = t.isActive;
            }
            ToolSystem.Instance?.LoadFromSave(unlocked, active);

            ApplyOfflineIncome(data.lastSaveTimestamp);

            Debug.Log("[SaveSystem] Game loaded successfully.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Load failed: {e.Message}");
            return false;
        }
    }

    public void DeleteSave() => PlayerPrefs.DeleteKey(SaveKey);

    // ── Offline income ───────────────────────────────────────────────────

    private void ApplyOfflineIncome(long lastSaveTimestamp)
    {
        float elapsed = Mathf.Clamp(
            (float)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - lastSaveTimestamp),
            0f, MaxOfflineSeconds);

        if (elapsed > 0f)
        {
            DNAController.Instance?.ApplyPassiveGeneration(elapsed);
            Debug.Log($"[SaveSystem] Applied {elapsed:F0}s of offline income.");
        }
    }
}
