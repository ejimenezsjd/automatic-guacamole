using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Entry point. Initialises the game on Start, hands off to SaveSystem,
/// and seeds a new game if no save exists. Lives in the bootstrap scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GameBalanceConfig _balance;

    private bool _initialised = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => Initialise();

    private void Initialise()
    {
        if (_initialised) return;
        _initialised = true;

        ApplyBalanceConfig();

        bool hasSave = SaveSystem.Instance?.LoadGame() ?? false;
        if (!hasSave) SeedNewGame();

        Debug.Log("[GameManager] Initialised.");
    }

    private void ApplyBalanceConfig()
    {
        if (_balance == null) return;

        ProductionManager.Instance?.SetTickInterval(_balance.tickInterval);
        DynamicEventSystem.Instance?.Configure(_balance.eventCheckInterval, _balance.eventTriggerChance);
    }

    private void SeedNewGame()
    {
        float normal = _balance != null ? _balance.starterNormalDNA : 100f;
        float fire   = _balance != null ? _balance.starterFireDNA   : 50f;
        float water  = _balance != null ? _balance.starterWaterDNA  : 50f;

        DNAController.Instance?.AddDNA(DNAType.Normal, normal);
        DNAController.Instance?.AddDNA(DNAType.Fuego,  fire);
        DNAController.Instance?.AddDNA(DNAType.Agua,   water);
        ToolSystem.Instance?.TryUnlockTool(ToolType.AutoExtractor);
    }

    // ── Application lifecycle hooks ──────────────────────────────────────

    private void OnApplicationPause(bool paused)
    {
        if (paused) SaveSystem.Instance?.SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveSystem.Instance?.SaveGame();
    }

    // ── Global reset ─────────────────────────────────────────────────────

    public void ResetGame()
    {
        SaveSystem.Instance?.DeleteSave();
        SceneManager.LoadScene(0);
    }
}
