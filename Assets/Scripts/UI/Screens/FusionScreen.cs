using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Lets the player select two creatures and attempt fusion.
/// Selection is stored locally; the Fuse button becomes active only when both slots are filled.
/// </summary>
public class FusionScreen : MonoBehaviour
{
    [SerializeField] private Transform  _container;
    [SerializeField] private GameObject _cardPrefab;
    [SerializeField] private TMP_Text   _statusText;
    [SerializeField] private Button     _fuseButton;
    [SerializeField] private TMP_Text   _slotAText;
    [SerializeField] private TMP_Text   _slotBText;

    private Creature _slotA;
    private Creature _slotB;

    private void OnEnable()
    {
        GameEvents.OnCreaturesFused += OnFusionDone;
        _fuseButton?.onClick.AddListener(OnFuseClicked);
        RefreshList();
        UpdateFuseButton();
    }

    private void OnDisable()
    {
        GameEvents.OnCreaturesFused -= OnFusionDone;
        _fuseButton?.onClick.RemoveAllListeners();
    }

    // ── Button handlers ──────────────────────────────────────────────────

    private void OnFuseClicked()
    {
        if (_slotA == null || _slotB == null)
        {
            SetStatus("Select two creatures first.");
            return;
        }

        var result = FusionSystem.Instance?.Fuse(_slotA, _slotB);
        if (result == null)
            SetStatus("Fusion failed — not enough DNA or level requirements not met.");
    }

    private void OnFusionDone(Creature a, Creature b, Creature result)
    {
        _slotA = null;
        _slotB = null;
        SetStatus($"Success! Created: {result.name} ({result.rarity})");
        RefreshList();
        UpdateSlotLabels();
        UpdateFuseButton();
    }

    private void SelectCreature(Creature c)
    {
        if (_slotA == null) { _slotA = c; }
        else if (_slotB == null && c.id != _slotA.id) { _slotB = c; }
        else { _slotA = c; _slotB = null; }   // restart selection

        UpdateSlotLabels();
        UpdateFuseButton();
    }

    // ── List refresh ─────────────────────────────────────────────────────

    private void RefreshList()
    {
        if (_container == null) return;
        foreach (Transform child in _container) Destroy(child.gameObject);

        var creatures = CreatureManager.Instance?.GetAllCreatures();
        if (creatures == null) return;

        foreach (var c in creatures)
        {
            var card  = Instantiate(_cardPrefab, _container);
            var label = card.GetComponentInChildren<TMP_Text>();
            var btn   = card.GetComponentInChildren<Button>();

            bool eligible = c.evolutionLevel >= 2;
            if (label != null) label.text = $"{c.name} (Lv.{c.evolutionLevel}) {(eligible ? "" : "[needs Lv2]")}";
            if (btn   != null)
            {
                btn.interactable = eligible;
                var captured = c;
                btn.onClick.AddListener(() => SelectCreature(captured));
            }
        }
    }

    // ── UI helpers ───────────────────────────────────────────────────────

    private void UpdateSlotLabels()
    {
        if (_slotAText != null) _slotAText.text = _slotA != null ? _slotA.name : "— empty —";
        if (_slotBText != null) _slotBText.text = _slotB != null ? _slotB.name : "— empty —";
    }

    private void UpdateFuseButton()
    {
        if (_fuseButton != null)
            _fuseButton.interactable = _slotA != null && _slotB != null;
    }

    private void SetStatus(string msg)
    {
        if (_statusText != null) _statusText.text = msg;
    }
}
