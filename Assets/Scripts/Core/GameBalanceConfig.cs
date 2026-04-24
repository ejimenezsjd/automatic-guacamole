using UnityEngine;

/// <summary>
/// Single ScriptableObject for all numeric game balance values.
/// Systems read from this SO on Start so tuning never requires code changes.
/// Create one asset via ScrachyMons/Balance Config and assign it to GameManager.
/// </summary>
[CreateAssetMenu(fileName = "GameBalanceConfig", menuName = "ScrachyMons/Balance Config")]
public class GameBalanceConfig : ScriptableObject
{
    [Header("Production Tick")]
    public float tickInterval          = 1f;
    public float maxOfflineSeconds     = 28800f;    // 8 hours

    [Header("Generation")]
    public float starterNormalDNA      = 100f;
    public float starterFireDNA        = 50f;
    public float starterWaterDNA       = 50f;

    [Header("Evolution")]
    [Tooltip("Duplicates required to go from level N (index) to N+1")]
    public int[] duplicatesPerLevel    = { 3, 10 };
    public float[] evoProductionBoost  = { 2.5f, 2.4f };

    [Header("Fusion")]
    public int   minFusionLevel        = 2;
    public float fusionProductionBonus = 1.4f;
    [Range(0f, 1f)]
    public float rarityUpgradeChance   = 0.20f;
    public float fusionCostPerLevel    = 50f;
    public int   maxDNATypesPerCreature = 3;

    [Header("Prestige")]
    public float productionMultPerPrestige = 0.25f;
    public float rarityBonusPerPrestige    = 0.05f;
    public int   genesPerPrestige          = 10;

    [Header("Save")]
    public float autoSaveInterval      = 30f;

    [Header("Dynamic Events")]
    public float eventCheckInterval    = 300f;   // every 5 minutes
    [Range(0f, 1f)]
    public float eventTriggerChance    = 0.15f;
}
