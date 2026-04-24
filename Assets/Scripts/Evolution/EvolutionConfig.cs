using UnityEngine;

/// <summary>
/// ScriptableObject that defines what each evolution level requires.
/// Edit in the Inspector — no code changes needed to tune values.
/// </summary>
[CreateAssetMenu(fileName = "EvolutionConfig", menuName = "ScrachyMons/Evolution Config")]
public class EvolutionConfig : ScriptableObject
{
    [System.Serializable]
    public class EvolutionRequirement
    {
        public int fromLevel;
        public int duplicatesRequired;
        [Header("Special resource (leave amount = 0 to skip)")]
        public DNAType specialResourceType;
        public float specialResourceAmount;
        public float productionBoostMultiplier = 2.5f;
    }

    public EvolutionRequirement[] requirements = new EvolutionRequirement[]
    {
        new EvolutionRequirement { fromLevel = 1, duplicatesRequired = 3,  specialResourceAmount = 0f,   productionBoostMultiplier = 2.5f },
        new EvolutionRequirement { fromLevel = 2, duplicatesRequired = 10, specialResourceType = DNAType.Dragon, specialResourceAmount = 500f, productionBoostMultiplier = 2.4f },
    };

    public EvolutionRequirement GetRequirement(int fromLevel)
    {
        foreach (var req in requirements)
            if (req.fromLevel == fromLevel) return req;
        return null;
    }
}
