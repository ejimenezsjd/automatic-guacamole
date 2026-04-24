using UnityEngine;
using TMPro;

/// <summary>
/// HUD widget that shows the currently active bonus event.
/// Attach to a panel that is hidden when no event is running.
/// </summary>
public class ActiveEventWidget : MonoBehaviour
{
    [SerializeField] private GameObject _panel;
    [SerializeField] private TMP_Text   _nameText;
    [SerializeField] private TMP_Text   _descText;
    [SerializeField] private TMP_Text   _timerText;

    private void OnEnable()
    {
        if (DynamicEventSystem.Instance != null)
        {
            DynamicEventSystem.Instance.OnEventStarted += OnEventStarted;
            DynamicEventSystem.Instance.OnEventEnded   += OnEventEnded;
        }

        GameEvents.OnTickProcessed += RefreshTimer;
        RefreshImmediate();
    }

    private void OnDisable()
    {
        if (DynamicEventSystem.Instance != null)
        {
            DynamicEventSystem.Instance.OnEventStarted -= OnEventStarted;
            DynamicEventSystem.Instance.OnEventEnded   -= OnEventEnded;
        }

        GameEvents.OnTickProcessed -= RefreshTimer;
    }

    private void OnEventStarted(BonusEvent e) => RefreshImmediate();
    private void OnEventEnded(BonusEvent e)   => SetVisible(false);

    private void RefreshImmediate()
    {
        var e = DynamicEventSystem.Instance?.GetActiveEvent();
        if (e == null) { SetVisible(false); return; }

        SetVisible(true);
        if (_nameText != null) _nameText.text = e.Name;
        if (_descText != null) _descText.text = e.Description;
        RefreshTimer();
    }

    private void RefreshTimer()
    {
        var e = DynamicEventSystem.Instance?.GetActiveEvent();
        if (e == null) return;
        if (_timerText != null)
            _timerText.text = NumberFormatter.FormatTime(e.RemainingSeconds);
    }

    private void SetVisible(bool show)
    {
        if (_panel != null) _panel.SetActive(show);
    }
}
