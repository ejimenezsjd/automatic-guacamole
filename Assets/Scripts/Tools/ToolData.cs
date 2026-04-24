using UnityEngine;

/// <summary>
/// ScriptableObject definition for a lab tool.
/// Each tool gets one asset; behaviour is driven by ToolSystem reading these values.
/// </summary>
[CreateAssetMenu(fileName = "NewToolData", menuName = "ScrachyMons/Tool Data")]
public class ToolData : ScriptableObject
{
    [Header("Identity")]
    public ToolType toolType;
    public string toolName;
    [TextArea] public string description;
    public bool unlockedByDefault;

    [Header("Unlock Cost")]
    public UnlockCost[] unlockCosts;

    [Header("AutoExtractor Settings")]
    public DNAType extractorOutputType;
    public float extractorOutputPerSecond = 1f;

    [Header("Refiner Settings")]
    public DNAType refinerInputType;
    public DNAType refinerOutputType;
    public float refinerConversionRatio = 5f;   // 5 input → 1 output

    [Header("Cloner Settings")]
    public float clonerCooldownSeconds = 60f;

    [Header("Mutator Settings")]
    [Range(0f, 1f)] public float mutatorRarityBonus = 0.1f;

    [System.Serializable]
    public class UnlockCost
    {
        public DNAType dnaType;
        public float amount;
    }
}
