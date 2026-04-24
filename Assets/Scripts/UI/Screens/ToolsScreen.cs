using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Displays all tools with their unlock/active state.
/// Panels are built once on enable and refreshed on tool state changes.
/// </summary>
public class ToolsScreen : MonoBehaviour
{
    [SerializeField] private Transform  _container;
    [SerializeField] private GameObject _toolPanelPrefab;

    private readonly List<ToolPanel> _panels = new();

    private void OnEnable()
    {
        GameEvents.OnToolUnlocked  += OnToolStateChanged;
        GameEvents.OnToolActivated += OnToolStateChanged;
        BuildPanels();
    }

    private void OnDisable()
    {
        GameEvents.OnToolUnlocked  -= OnToolStateChanged;
        GameEvents.OnToolActivated -= OnToolStateChanged;
    }

    private void OnToolStateChanged(ToolType _) => RefreshPanels();

    // ── Build ────────────────────────────────────────────────────────────

    private void BuildPanels()
    {
        if (_container == null || _toolPanelPrefab == null) return;

        foreach (Transform child in _container) Destroy(child.gameObject);
        _panels.Clear();

        foreach (ToolType type in System.Enum.GetValues(typeof(ToolType)))
        {
            var go = Instantiate(_toolPanelPrefab, _container);
            var panel = go.GetComponent<ToolPanel>();
            if (panel == null) continue;

            ToolData data = ToolSystem.Instance?.GetToolData(type);
            panel.Setup(type, data);
            _panels.Add(panel);
        }
    }

    private void RefreshPanels()
    {
        foreach (var p in _panels) p.Refresh();
    }
}
