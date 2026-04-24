using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject template for a creature species.
/// One asset per species — no hardcoded data in code.
/// </summary>
[CreateAssetMenu(fileName = "NewCreatureData", menuName = "ScrachyMons/Creature Data")]
public class CreatureData : ScriptableObject
{
    [Header("Identity")]
    public string creatureId;
    public string creatureName;

    [Header("DNA")]
    public List<DNAType> dnaTypes = new();

    [Header("Stats")]
    public RarityType rarity;
    public float baseProductionRate = 1f;
    [Range(0f, 1f)] public float spawnWeight = 0.5f;

    [Header("Visuals")]
    public Sprite icon;
    public GameObject prefab3D;

    [Header("Creation Cost")]
    public List<DNACost> creationCosts = new();

    [System.Serializable]
    public class DNACost
    {
        public DNAType type;
        public float amount;
    }
}
