using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Displays a single tool's status, cost, unlock button, and active toggle.
/// Call Setup() once; call Refresh() when state changes.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class ToolPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text   _nameText;
    [SerializeField] private TMP_Text   _descText;
    [SerializeField] private TMP_Text   _costText;
    [SerializeField] private TMP_Text   _statusText;
    [SerializeField] private Button     _unlockButton;
    [SerializeField] private Toggle     _activeToggle;
    [SerializeField] private GameObject _lockedOverlay;

    private ToolType _toolType;
    private ToolData _data;

    // ── Setup ────────────────────────────────────────────────────────────

    public void Setup(ToolType type, ToolData data)
    {
        _toolType = type;
        _data     = data;

        if (_nameText != null) _nameText.text = data != null ? data.toolName    : type.ToString();
        if (_descText != null) _descText.text = data != null ? data.description : string.Empty;

        _unlockButton?.onClick.AddListener(OnUnlockClicked);
        _activeToggle?.onValueChanged.AddListener(OnToggleChanged);

        Refresh();
    }

    // ── Refresh ──────────────────────────────────────────────────────────

    public void Refresh()
    {
        bool isUnlocked = ToolSystem.Instance?.IsUnlocked(_toolType) ?? false;
        bool isActive   = ToolSystem.Instance?.IsActive(_toolType)   ?? false;

        if (_lockedOverlay != null) _lockedOverlay.SetActive(!isUnlocked);
        if (_unlockButton  != null) _unlockButton.gameObject.SetActive(!isUnlocked);

        if (_activeToggle  != null)
        {
            _activeToggle.gameObject.SetActive(isUnlocked);
            _activeToggle.SetIsOnWithoutNotify(isActive);
        }

        if (_statusText != null)
            _statusText.text = !isUnlocked ? "Bloqueado" : isActive ? "ACTIVO" : "Inactivo";

        if (_costText != null)
        {
            _costText.gameObject.SetActive(!isUnlocked && _data != null);
            if (!isUnlocked && _data != null)
                _costText.text = BuildCostText();
        }
    }

    // ── Handlers ─────────────────────────────────────────────────────────

    private void OnUnlockClicked()
    {
        ToolSystem.Instance?.TryUnlockTool(_toolType);
        Refresh();
    }

    private void OnToggleChanged(bool value)
    {
        ToolSystem.Instance?.SetActive(_toolType, value);
        Refresh();
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private string BuildCostText()
    {
        var parts = new List<string>();
        foreach (var c in _data.unlockCosts)
            parts.Add($"{NumberFormatter.Format(c.amount)} {c.dnaType}");
        return parts.Count > 0 ? "Coste: " + string.Join(", ", parts) : string.Empty;
    }
}
