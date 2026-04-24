using UnityEngine;

/// <summary>
/// Controls which screen is visible. Subscribe to navigation buttons from each screen.
/// Assign screen root GameObjects in the Inspector.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screen Roots")]
    [SerializeField] private GameObject _mainScreen;
    [SerializeField] private GameObject _collectionScreen;
    [SerializeField] private GameObject _fusionScreen;
    [SerializeField] private GameObject _evolutionScreen;

    private GameObject _current;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start() => ShowScreen(ScreenType.Main);

    public void ShowScreen(ScreenType type)
    {
        _current?.SetActive(false);

        _current = type switch
        {
            ScreenType.Main       => _mainScreen,
            ScreenType.Collection => _collectionScreen,
            ScreenType.Fusion     => _fusionScreen,
            ScreenType.Evolution  => _evolutionScreen,
            _                     => _mainScreen,
        };

        _current?.SetActive(true);
    }

    // Convenience wrappers for UnityEvent bindings on buttons
    public void ShowMain()       => ShowScreen(ScreenType.Main);
    public void ShowCollection() => ShowScreen(ScreenType.Collection);
    public void ShowFusion()     => ShowScreen(ScreenType.Fusion);
    public void ShowEvolution()  => ShowScreen(ScreenType.Evolution);
}
