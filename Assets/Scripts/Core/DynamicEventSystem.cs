using System;
using UnityEngine;

/// <summary>
/// Fires random time-limited bonus events (e.g. "DNA Surge — production x2 for 5 min").
/// Implements ITickable so it participates in the global tick without using Update().
/// Call GetProductionMultiplier() from CreatureManager to apply the active bonus.
/// </summary>
public class DynamicEventSystem : MonoBehaviour, ITickable
{
    public static DynamicEventSystem Instance { get; private set; }

    // ── Public event notifications ────────────────────────────────────────
    public event Action<BonusEvent> OnEventStarted;
    public event Action<BonusEvent> OnEventEnded;

    [SerializeField] private float _checkInterval   = 300f;
    [SerializeField] private float _triggerChance   = 0.15f;

    private BonusEvent _active;
    private float      _nextCheckIn;

    // ── Event pool ────────────────────────────────────────────────────────

    private static readonly BonusEventDefinition[] EventPool =
    {
        new("DNA Surge",      "All DNA production ×2 for 5 minutes!",  2.0f, 300f),
        new("Mutation Wave",  "Production ×1.5 for 10 minutes!",        1.5f, 600f),
        new("Lab Accident",   "Production ×3 for 2 minutes!",           3.0f, 120f),
        new("Gene Cascade",   "Production ×1.5 for 15 minutes!",        1.5f, 900f),
        new("Primal Burst",   "Production ×4 for 1 minute!",            4.0f, 60f),
    };

    // ── Lifecycle ────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()  => ProductionManager.Instance?.Register(this);
    private void OnDisable() => ProductionManager.Instance?.Unregister(this);

    private void Start() => _nextCheckIn = _checkInterval;

    // ── ITickable ────────────────────────────────────────────────────────

    public void OnTick(float deltaTime)
    {
        if (_active != null)
        {
            _active.RemainingSeconds -= deltaTime;
            if (_active.RemainingSeconds <= 0f) EndCurrentEvent();
        }
        else
        {
            _nextCheckIn -= deltaTime;
            if (_nextCheckIn <= 0f)
            {
                _nextCheckIn = _checkInterval;
                TryTrigger();
            }
        }
    }

    // ── Query API ────────────────────────────────────────────────────────

    public float      GetProductionMultiplier() => _active != null ? _active.ProductionMultiplier : 1f;
    public BonusEvent GetActiveEvent()          => _active;
    public bool       HasActiveEvent()          => _active != null;

    // ── Config injection (from GameManager / GameBalanceConfig) ──────────

    public void Configure(float checkInterval, float triggerChance)
    {
        _checkInterval  = checkInterval;
        _triggerChance  = triggerChance;
        _nextCheckIn    = _checkInterval;
    }

    // ── Internal ─────────────────────────────────────────────────────────

    private void TryTrigger()
    {
        if (UnityEngine.Random.value > _triggerChance) return;

        var def = EventPool[UnityEngine.Random.Range(0, EventPool.Length)];
        _active = new BonusEvent(def);

        CreatureManager.Instance?.UpdatePassiveRates();
        OnEventStarted?.Invoke(_active);

        Debug.Log($"[DynamicEvent] Started — {_active.Name} ({NumberFormatter.FormatTime(def.DurationSeconds)})");
    }

    private void EndCurrentEvent()
    {
        Debug.Log($"[DynamicEvent] Ended — {_active.Name}");
        var ended = _active;
        _active = null;

        CreatureManager.Instance?.UpdatePassiveRates();
        OnEventEnded?.Invoke(ended);
    }
}

// ── Data classes (not MonoBehaviours) ────────────────────────────────────────

public sealed class BonusEventDefinition
{
    public readonly string Name;
    public readonly string Description;
    public readonly float  ProductionMultiplier;
    public readonly float  DurationSeconds;

    public BonusEventDefinition(string name, string desc, float mult, float duration)
    {
        Name                 = name;
        Description          = desc;
        ProductionMultiplier = mult;
        DurationSeconds      = duration;
    }
}

public sealed class BonusEvent
{
    public string Name;
    public string Description;
    public float  ProductionMultiplier;
    public float  RemainingSeconds;

    public BonusEvent(BonusEventDefinition def)
    {
        Name                 = def.Name;
        Description          = def.Description;
        ProductionMultiplier = def.ProductionMultiplier;
        RemainingSeconds     = def.DurationSeconds;
    }
}
