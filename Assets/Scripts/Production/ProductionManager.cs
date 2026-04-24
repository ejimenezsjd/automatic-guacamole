using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global tick driver. Keeps heavy per-frame work out of individual Update() calls.
/// Any system can register as ITickable to receive ticks without touching Update().
/// </summary>
public class ProductionManager : MonoBehaviour
{
    public static ProductionManager Instance { get; private set; }

    [SerializeField] private float _tickInterval = 1f;

    private readonly List<ITickable> _tickables = new();
    private float _timer;
    private bool  _running = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (!_running) return;

        _timer += Time.deltaTime;
        if (_timer < _tickInterval) return;

        float delta = _timer;
        _timer = 0f;
        ProcessTick(delta);
    }

    private void ProcessTick(float deltaTime)
    {
        DNAController.Instance?.ApplyPassiveGeneration(deltaTime);
        ToolSystem.Instance?.ProcessTick(deltaTime);

        foreach (var tickable in _tickables)
            tickable.OnTick(deltaTime);

        GameEvents.TriggerTickProcessed();
    }

    // ── Registration ─────────────────────────────────────────────────────

    public void Register(ITickable tickable)
    {
        if (!_tickables.Contains(tickable)) _tickables.Add(tickable);
    }

    public void Unregister(ITickable tickable)
        => _tickables.Remove(tickable);

    // ── Control ──────────────────────────────────────────────────────────

    public void SetRunning(bool running) => _running = running;

    public void SetTickInterval(float interval)
        => _tickInterval = Mathf.Max(0.1f, interval);
}
