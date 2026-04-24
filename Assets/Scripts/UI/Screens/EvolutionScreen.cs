using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Lists all owned creatures with their evolution status and an Evolve button
/// that is enabled only when requirements are satisfied.
/// </summary>
public class EvolutionScreen : MonoBehaviour
{
    [SerializeField] private Transform  _container;
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private TMP_Text   _statusText;

    private void OnEnable()
    {
        GameEvents.OnCreatureCreated += OnCreatureChanged;
        GameEvents.OnCreatureEvolved += OnCreatureChanged;
        RefreshList();
    }

    private void OnDisable()
    {
        GameEvents.OnCreatureCreated -= OnCreatureChanged;
        GameEvents.OnCreatureEvolved -= OnCreatureChanged;
    }

    private void OnCreatureChanged(Creature _) => RefreshList();

    private void RefreshList()
    {
        if (_container == null) return;
        foreach (Transform child in _container) Destroy(child.gameObject);

        var creatures = CreatureManager.Instance?.GetAllCreatures();
        if (creatures == null) return;

        foreach (var c in creatures)
            BuildCard(c);
    }

    private void BuildCard(Creature creature)
    {
        var card   = Instantiate(_cardPrefab, _container);
        var labels = card.GetComponentsInChildren<TMP_Text>();
        var btn    = card.GetComponentInChildren<Button>();

        bool canEvolve = EvolutionSystem.Instance?.CanEvolve(creature) ?? false;

        if (labels.Length > 0) labels[0].text = creature.name;
        if (labels.Length > 1)
        {
            var (dupReq, specType, specAmt) = EvolutionSystem.Instance != null
                ? EvolutionSystem.Instance.GetRequirements(creature)
                : (0, DNAType.Normal, 0f);

            string req = creature.evolutionLevel < 3
                ? $"Needs: {dupReq} dupes" + (specAmt > 0f ? $" + {specAmt:F0} {specType} DNA" : "")
                : "MAX LEVEL";
            labels[1].text = $"Lv.{creature.evolutionLevel} | Dupes: {creature.duplicates} | {req}";
        }

        if (btn != null)
        {
            btn.interactable = canEvolve;
            var captured = creature;
            btn.onClick.AddListener(() => TryEvolve(captured));

            var btnLabel = btn.GetComponentInChildren<TMP_Text>();
            if (btnLabel != null)
                btnLabel.text = creature.evolutionLevel >= 3 ? "MAX" : canEvolve ? "EVOLVE" : "Not ready";
        }
    }

    private void TryEvolve(Creature creature)
    {
        bool ok = EvolutionSystem.Instance?.TryEvolve(creature) ?? false;
        if (_statusText != null)
            _statusText.text = ok
                ? $"{creature.name} evolved to Lv.{creature.evolutionLevel}!"
                : "Evolution failed.";
    }
}
