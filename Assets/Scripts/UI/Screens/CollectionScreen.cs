using UnityEngine;
using TMPro;

/// <summary>
/// Shows all owned creatures with name, level, rarity, and production rate.
/// Refreshes automatically when creatures are added or evolved.
/// </summary>
public class CollectionScreen : MonoBehaviour
{
    [SerializeField] private Transform  _container;
    [SerializeField] private GameObject _cardPrefab;    // needs ≥3 TMP_Text children

    private void OnEnable()
    {
        GameEvents.OnCreatureCreated += _ => RefreshList();
        GameEvents.OnCreatureEvolved += _ => RefreshList();
        GameEvents.OnCreaturesFused  += (_, _, _) => RefreshList();
        RefreshList();
    }

    private void OnDisable()
    {
        GameEvents.OnCreatureCreated -= _ => RefreshList();
        GameEvents.OnCreatureEvolved -= _ => RefreshList();
        GameEvents.OnCreaturesFused  -= (_, _, _) => RefreshList();
    }

    private void RefreshList()
    {
        if (_container == null) return;

        foreach (Transform child in _container)
            Destroy(child.gameObject);

        var creatures = CreatureManager.Instance?.GetAllCreatures();
        if (creatures == null) return;

        float prestige = PrestigeSystem.Instance?.GetProductionMultiplier() ?? 1f;

        foreach (var c in creatures)
        {
            var card   = Instantiate(_cardPrefab, _container);
            var labels = card.GetComponentsInChildren<TMP_Text>();

            if (labels.Length > 0) labels[0].text = c.name;
            if (labels.Length > 1) labels[1].text = $"Lv.{c.evolutionLevel}  |  {c.rarity}  |  Dupes: {c.duplicates}";
            if (labels.Length > 2) labels[2].text = $"{c.GetTotalProduction(prestige):F2} DNA/s";
        }
    }
}
