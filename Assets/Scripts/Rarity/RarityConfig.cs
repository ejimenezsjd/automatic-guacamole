using UnityEngine;

[CreateAssetMenu(fileName = "RarityConfig", menuName = "ScrachyMons/Rarity Config")]
public class RarityConfig : ScriptableObject
{
    [System.Serializable]
    public class RarityEntry
    {
        public RarityType rarityType;
        [Range(0f, 1f)] public float spawnWeight;
        public float productionMultiplier = 1f;
        public Color displayColor = Color.white;
    }

    public RarityEntry[] entries = new RarityEntry[]
    {
        new RarityEntry { rarityType = RarityType.Common, spawnWeight = 0.70f, productionMultiplier = 1.0f },
        new RarityEntry { rarityType = RarityType.Rare,   spawnWeight = 0.25f, productionMultiplier = 2.5f },
        new RarityEntry { rarityType = RarityType.Epic,   spawnWeight = 0.05f, productionMultiplier = 6.0f },
    };

    public RarityEntry GetEntry(RarityType type)
    {
        foreach (var e in entries)
            if (e.rarityType == type) return e;
        return null;
    }

    public float GetProductionMultiplier(RarityType type)
    {
        var entry = GetEntry(type);
        return entry != null ? entry.productionMultiplier : 1f;
    }

    public float GetSpawnWeight(RarityType type)
    {
        var entry = GetEntry(type);
        return entry != null ? entry.spawnWeight : 0f;
    }
}
