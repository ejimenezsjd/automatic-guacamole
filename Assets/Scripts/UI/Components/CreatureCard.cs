using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Self-contained creature card. Call Setup() to populate from a Creature instance.
/// Used by Collection, Fusion, and Evolution screens.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class CreatureCard : MonoBehaviour
{
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _infoText;
    [SerializeField] private TMP_Text _productionText;
    [SerializeField] private Image    _background;
    [SerializeField] private Button   _actionButton;
    [SerializeField] private TMP_Text _buttonLabel;

    private static readonly Color ColorCommon = new(0.70f, 0.70f, 0.75f);
    private static readonly Color ColorRare   = new(0.30f, 0.55f, 1.00f);
    private static readonly Color ColorEpic   = new(0.75f, 0.20f, 1.00f);

    private Creature _creature;

    public Creature Creature => _creature;

    // ── Setup ────────────────────────────────────────────────────────────

    public void Setup(Creature creature, string buttonText = "", Action<Creature> onAction = null)
    {
        _creature = creature;
        Refresh();

        if (_buttonLabel != null) _buttonLabel.text = buttonText;

        if (_actionButton != null)
        {
            _actionButton.onClick.RemoveAllListeners();
            bool visible = onAction != null;
            _actionButton.gameObject.SetActive(visible);
            if (visible)
            {
                var captured = creature;
                _actionButton.onClick.AddListener(() => onAction(captured));
            }
        }
    }

    public void SetButtonInteractable(bool interactable)
    {
        if (_actionButton != null) _actionButton.interactable = interactable;
    }

    // ── Refresh ──────────────────────────────────────────────────────────

    public void Refresh()
    {
        if (_creature == null) return;

        float prestige = PrestigeSystem.Instance?.GetProductionMultiplier() ?? 1f;

        if (_nameText       != null) _nameText.text       = _creature.name;
        if (_infoText       != null) _infoText.text       = BuildInfo();
        if (_productionText != null) _productionText.text = NumberFormatter.FormatRate(_creature.GetTotalProduction(prestige));
        if (_background     != null) _background.color    = RarityColor(_creature.rarity);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private string BuildInfo()
    {
        string types = string.Join("/", _creature.dnaTypes);
        return $"Lv.{_creature.evolutionLevel} | {_creature.rarity} | {types} | Dupes: {_creature.duplicates}";
    }

    private static Color RarityColor(RarityType r) => r switch
    {
        RarityType.Rare => ColorRare,
        RarityType.Epic => ColorEpic,
        _               => ColorCommon,
    };
}
