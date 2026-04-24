using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Unity Editor tool — ScrachyMons/Generate Starter Data
/// Creates all default ScriptableObject assets in Assets/Resources/
/// so the game is playable out of the box without manual SO authoring.
/// </summary>
public class StarterDataGenerator : EditorWindow
{
    [MenuItem("ScrachyMons/Generate Starter Data")]
    public static void ShowWindow()
        => GetWindow<StarterDataGenerator>("ScrachyMons Starter Data");

    private void OnGUI()
    {
        GUILayout.Label("Generate default ScriptableObject assets", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        if (GUILayout.Button("▶  Generate ALL (recommended)"))
            GenerateAll();

        EditorGUILayout.Space();
        GUILayout.Label("Individual generators:", EditorStyles.miniLabel);
        if (GUILayout.Button("Creatures  (Resources/Creatures/)"))  GenerateCreatures();
        if (GUILayout.Button("Tools      (Resources/Tools/)"))      GenerateTools();
        if (GUILayout.Button("RarityConfig"))                       GenerateRarityConfig();
        if (GUILayout.Button("EvolutionConfig"))                    GenerateEvolutionConfig();
        if (GUILayout.Button("GameBalanceConfig"))                  GenerateBalanceConfig();
    }

    public static void GenerateAll()
    {
        GenerateCreatures();
        GenerateTools();
        GenerateRarityConfig();
        GenerateEvolutionConfig();
        GenerateBalanceConfig();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[StarterDataGenerator] All starter assets generated.");
    }

    // ── Creatures ────────────────────────────────────────────────────────

    private static void GenerateCreatures()
    {
        EnsureDir("Assets/Resources/Creatures");

        // (id, displayName, types[], rarity, baseRate, spawnWeight, costPerType)
        var defs = new (string id, string cname, DNAType[] types, RarityType rarity, float rate, float weight, float cost)[]
        {
            ("ignicore",  "Ignicore",   new[]{DNAType.Fuego},                       RarityType.Common, 1.0f, 0.70f, 25f),
            ("aquamite",  "Aquamite",   new[]{DNAType.Agua},                        RarityType.Common, 1.2f, 0.65f, 25f),
            ("voltix",    "Voltix",     new[]{DNAType.Electrico},                   RarityType.Common, 1.5f, 0.60f, 30f),
            ("terrath",   "Terrath",    new[]{DNAType.Tierra},                      RarityType.Common, 1.1f, 0.65f, 20f),
            ("frostbite", "Frostbite",  new[]{DNAType.Hielo},                       RarityType.Common, 1.3f, 0.60f, 28f),
            ("galewind",  "Galewind",   new[]{DNAType.Volador},                     RarityType.Common, 1.4f, 0.55f, 30f),
            ("spectrax",  "Spectrax",   new[]{DNAType.Fantasma},                    RarityType.Rare,   3.0f, 0.25f, 75f),
            ("dracorel",  "Dracorel",   new[]{DNAType.Dragon},                      RarityType.Rare,   4.0f, 0.20f,100f),
            ("faelight",  "Faelight",   new[]{DNAType.Hada},                        RarityType.Rare,   3.5f, 0.22f, 90f),
            ("psicore",   "Psicore",    new[]{DNAType.Psiquico},                    RarityType.Rare,   3.8f, 0.20f, 95f),
            ("cosmovoid", "Cosmovoid",  new[]{DNAType.Dragon, DNAType.Fantasma},    RarityType.Epic,   8.0f, 0.05f,250f),
            ("omegalith", "Omegalith",  new[]{DNAType.Roca,   DNAType.Acero},       RarityType.Epic,   7.5f, 0.05f,230f),
            ("primordex", "Primordex",  new[]{DNAType.Normal, DNAType.Psiquico, DNAType.Hada}, RarityType.Epic, 10f, 0.03f, 300f),
        };

        foreach (var d in defs)
            CreateCreature(d.id, d.cname, d.types, d.rarity, d.rate, d.weight, d.cost);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[StarterDataGenerator] {defs.Length} creature assets generated.");
    }

    private static void CreateCreature(string id, string cname, DNAType[] types,
        RarityType rarity, float rate, float weight, float costPerType)
    {
        string path = $"Assets/Resources/Creatures/{cname}.asset";
        if (File.Exists(ToAbsolute(path))) return;

        var data             = CreateInstance<CreatureData>();
        data.creatureId      = id;
        data.creatureName    = cname;
        data.dnaTypes        = new List<DNAType>(types);
        data.rarity          = rarity;
        data.baseProductionRate = rate;
        data.spawnWeight     = weight;

        data.creationCosts = new List<CreatureData.DNACost>();
        foreach (var t in types)
            data.creationCosts.Add(new CreatureData.DNACost { type = t, amount = costPerType });

        AssetDatabase.CreateAsset(data, path);
    }

    // ── Tools ────────────────────────────────────────────────────────────

    private static void GenerateTools()
    {
        EnsureDir("Assets/Resources/Tools");

        CreateTool(ToolType.AutoExtractor, "AutoExtractor",
            "Generates Normal DNA automatically.",
            unlockedByDefault: true,
            extractOut: DNAType.Normal, extractRate: 2f);

        CreateTool(ToolType.Refiner, "Refiner",
            "Converts 8 Normal DNA into 1 Fuego DNA.",
            unlockedByDefault: false,
            refinIn: DNAType.Normal, refinOut: DNAType.Fuego, refinRatio: 8f,
            costs: new[] { (DNAType.Normal, 500f) });

        CreateTool(ToolType.Cloner, "Cloner",
            "Every 2 minutes, adds a duplicate to a random creature.",
            unlockedByDefault: false,
            clonerCooldown: 120f,
            costs: new[] { (DNAType.Dragon, 200f) });

        CreateTool(ToolType.Mutator, "Mutator",
            "Permanently shifts creature spawns toward rarer tiers.",
            unlockedByDefault: false,
            mutatorBonus: 0.10f,
            costs: new[] { (DNAType.Psiquico, 300f), (DNAType.Hada, 300f) });

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[StarterDataGenerator] Tool assets generated.");
    }

    private static void CreateTool(ToolType type, string assetName, string desc,
        bool unlockedByDefault  = false,
        DNAType extractOut      = DNAType.Normal, float extractRate = 0f,
        DNAType refinIn         = DNAType.Normal,
        DNAType refinOut        = DNAType.Normal, float refinRatio  = 1f,
        float clonerCooldown    = 60f,
        float mutatorBonus      = 0f,
        (DNAType t, float amt)[] costs = null)
    {
        string path = $"Assets/Resources/Tools/{assetName}.asset";
        if (File.Exists(ToAbsolute(path))) return;

        var data = CreateInstance<ToolData>();
        data.toolType                 = type;
        data.toolName                 = assetName;
        data.description              = desc;
        data.unlockedByDefault        = unlockedByDefault;
        data.extractorOutputType      = extractOut;
        data.extractorOutputPerSecond = extractRate;
        data.refinerInputType         = refinIn;
        data.refinerOutputType        = refinOut;
        data.refinerConversionRatio   = refinRatio;
        data.clonerCooldownSeconds    = clonerCooldown;
        data.mutatorRarityBonus       = mutatorBonus;

        if (costs != null)
        {
            data.unlockCosts = new ToolData.UnlockCost[costs.Length];
            for (int i = 0; i < costs.Length; i++)
                data.unlockCosts[i] = new ToolData.UnlockCost { dnaType = costs[i].t, amount = costs[i].amt };
        }
        else
        {
            data.unlockCosts = new ToolData.UnlockCost[0];
        }

        AssetDatabase.CreateAsset(data, path);
    }

    // ── Config SOs ───────────────────────────────────────────────────────

    private static void GenerateRarityConfig()
    {
        string path = "Assets/Resources/RarityConfig.asset";
        if (File.Exists(ToAbsolute(path))) return;
        var so = CreateInstance<RarityConfig>();
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        Debug.Log("[StarterDataGenerator] RarityConfig generated.");
    }

    private static void GenerateEvolutionConfig()
    {
        string path = "Assets/Resources/EvolutionConfig.asset";
        if (File.Exists(ToAbsolute(path))) return;
        var so = CreateInstance<EvolutionConfig>();
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        Debug.Log("[StarterDataGenerator] EvolutionConfig generated.");
    }

    private static void GenerateBalanceConfig()
    {
        string path = "Assets/Resources/GameBalanceConfig.asset";
        if (File.Exists(ToAbsolute(path))) return;
        var so = CreateInstance<GameBalanceConfig>();
        AssetDatabase.CreateAsset(so, path);
        AssetDatabase.SaveAssets();
        Debug.Log("[StarterDataGenerator] GameBalanceConfig generated.");
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static void EnsureDir(string path)
    {
        string abs = ToAbsolute(path);
        if (!Directory.Exists(abs)) Directory.CreateDirectory(abs);
    }

    private static string ToAbsolute(string assetPath)
        => Path.GetFullPath(assetPath);
}
