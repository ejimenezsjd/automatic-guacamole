using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ScrachyMons → Build Bootstrap Scene
/// Creates Assets/Scenes/Bootstrap.unity with all systems + full UI.
/// Prerequisites: run "Generate Starter Data" and "Build UI Prefabs" first.
/// </summary>
public class SceneBuilder : EditorWindow
{
    [MenuItem("ScrachyMons/Build Bootstrap Scene")]
    public static void ShowWindow()
        => GetWindow<SceneBuilder>("Scene Builder");

    private void OnGUI()
    {
        GUILayout.Label("Bootstrap Scene Builder", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Builds Assets/Scenes/Bootstrap.unity from scratch.\n\n" +
            "Run in this order:\n" +
            "  1. ScrachyMons → Generate Starter Data\n" +
            "  2. ScrachyMons → Build UI Prefabs\n" +
            "  3. ScrachyMons → Build Bootstrap Scene  ←  this button",
            MessageType.Info);

        EditorGUILayout.Space();

        if (GUILayout.Button("▶  FULL SETUP (SOs + Prefabs + Scene)", GUILayout.Height(40)))
            FullSetup();

        EditorGUILayout.Space();
        GUILayout.Label("Or step by step:", EditorStyles.miniLabel);
        if (GUILayout.Button("  Build Bootstrap Scene only"))
            BuildScene();
    }

    // ── Full one-click setup ─────────────────────────────────────────────

    private static void FullSetup()
    {
        if (!EditorUtility.DisplayDialog("Full Setup",
            "This will:\n1. Generate all ScriptableObject assets\n2. Create UI prefabs\n3. Build Bootstrap.unity\n\nContinue?",
            "Yes", "Cancel"))
            return;

        StarterDataGenerator.GenerateAll();
        UIPrefabBuilder.BuildAll();
        BuildScene();
    }

    // ── Scene construction ───────────────────────────────────────────────

    public static void BuildScene()
    {
        if (!EditorUtility.DisplayDialog("Build Bootstrap Scene",
            "Creates/replaces Assets/Scenes/Bootstrap.unity\n\nContinue?",
            "Build", "Cancel"))
            return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        ConfigureCamera();
        EnsureEventSystem();

        var systemsGO = BuildSystems();
        var (canvasGO, screenMap) = BuildCanvas();

        WireUIManager(canvasGO, screenMap);
        WireScreenPrefabs(screenMap);

        string dir  = "Assets/Scenes";
        string path = $"{dir}/Bootstrap.unity";
        Directory.CreateDirectory(dir);
        EditorSceneManager.SaveScene(scene, path);
        AssetDatabase.Refresh();

        PrintChecklist(systemsGO);

        EditorUtility.DisplayDialog("Done!",
            "Bootstrap.unity created in Assets/Scenes/\n\n" +
            "Check the Console for the setup checklist.", "OK");
    }

    // ── Camera ───────────────────────────────────────────────────────────

    private static void ConfigureCamera()
    {
        var cam = Camera.main;
        if (cam == null) return;
        cam.backgroundColor = new Color(0.07f, 0.07f, 0.11f);
        cam.clearFlags      = CameraClearFlags.SolidColor;
        cam.orthographic    = true;
    }

    private static void EnsureEventSystem()
    {
        if (Object.FindFirstObjectByType<EventSystem>() != null) return;
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<StandaloneInputModule>();
    }

    // ── Systems ──────────────────────────────────────────────────────────

    private static GameObject BuildSystems()
    {
        var go = new GameObject("[Systems]");

        var gm  = go.AddComponent<GameManager>();
               go.AddComponent<DNAController>();
               go.AddComponent<CreatureDatabase>();
        var cg  = go.AddComponent<CreatureGenerator>();
               go.AddComponent<CreatureManager>();
               go.AddComponent<ProductionManager>();
        var evo = go.AddComponent<EvolutionSystem>();
               go.AddComponent<FusionSystem>();
        var ts  = go.AddComponent<ToolSystem>();
               go.AddComponent<PrestigeSystem>();
               go.AddComponent<SaveSystem>();
               go.AddComponent<DynamicEventSystem>();

        WireSystemSOs(gm, cg, evo, ts);
        return go;
    }

    private static void WireSystemSOs(GameManager gm, CreatureGenerator cg,
                                      EvolutionSystem evo, ToolSystem ts)
    {
        var balance  = Resources.Load<GameBalanceConfig>("GameBalanceConfig");
        var rarity   = Resources.Load<RarityConfig>("RarityConfig");
        var evoConf  = Resources.Load<EvolutionConfig>("EvolutionConfig");
        var tools    = Resources.LoadAll<ToolData>("Tools");

        bool missing = balance == null || rarity == null || evoConf == null || tools.Length == 0;
        if (missing)
            Debug.LogWarning("[SceneBuilder] Some SOs missing — run 'Generate Starter Data' first.\n" +
                             "You can assign them manually in the Inspector afterwards.");

        if (balance != null) SetField(gm, "_balance", balance);
        if (rarity  != null) SetField(cg, "_rarityConfig", rarity);
        if (evoConf != null) SetField(evo, "_config", evoConf);

        if (tools.Length > 0)
        {
            var so  = new SerializedObject(ts);
            var arr = so.FindProperty("_toolDataAssets");
            arr.arraySize = tools.Length;
            for (int i = 0; i < tools.Length; i++)
                arr.GetArrayElementAtIndex(i).objectReferenceValue = tools[i];
            so.ApplyModifiedProperties();
        }
    }

    // ── Canvas ───────────────────────────────────────────────────────────

    private static (GameObject canvas, Dictionary<ScreenType, GameObject> screens) BuildCanvas()
    {
        var canvasGO = new GameObject("Canvas");

        var canvas             = canvasGO.AddComponent<Canvas>();
        canvas.renderMode      = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder    = 0;

        var scaler                  = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode          = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution  = new Vector2(1080, 1920);  // portrait mobile
        scaler.matchWidthOrHeight   = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Dark background
        var bgImg   = canvasGO.AddComponent<Image>();
        bgImg.color = new Color(0.07f, 0.07f, 0.11f);

        // Navigation bar (bottom 8 %)
        var navBar = BuildNavigationBar(canvasGO.transform);

        // Screen container (above nav bar)
        var container = MakeRectGO("Screens", canvasGO.transform);
        AnchorFill(container, 0f, 0.08f, 1f, 1f);

        // Build each screen
        var screens = new Dictionary<ScreenType, GameObject>
        {
            { ScreenType.Main,       BuildScreen("LAB",         container.transform, typeof(MainScreen))       },
            { ScreenType.Collection, BuildScreen("COLECCIÓN",   container.transform, typeof(CollectionScreen))  },
            { ScreenType.Fusion,     BuildScreen("FUSIÓN",      container.transform, typeof(FusionScreen))      },
            { ScreenType.Evolution,  BuildScreen("EVOLUCIÓN",   container.transform, typeof(EvolutionScreen))   },
            { ScreenType.Tools,      BuildScreen("HERRAMIENTAS",container.transform, typeof(ToolsScreen))       },
        };

        // Wire nav buttons to UIManager (done after UIManager is added to canvas)
        WireNavButtons(navBar, canvasGO);

        return (canvasGO, screens);
    }

    private static GameObject BuildNavigationBar(Transform parent)
    {
        var navGO = MakeRectGO("NavigationBar", parent);
        AnchorFill(navGO, 0f, 0f, 1f, 0.08f);

        navGO.AddComponent<Image>().color = new Color(0.05f, 0.05f, 0.09f);

        var layout                     = navGO.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment          = TextAnchor.MiddleCenter;
        layout.childControlWidth       = true;
        layout.childControlHeight      = true;
        layout.childForceExpandWidth   = true;
        layout.spacing                 = 2;
        layout.padding                 = new RectOffset(4, 4, 4, 4);

        string[] labels = { "LAB", "COLECCIÓN", "FUSIÓN", "EVOLUCIÓN", "TOOLS" };
        foreach (var label in labels)
        {
            var btnGO = new GameObject($"Btn_{label}");
            btnGO.transform.SetParent(navGO.transform, false);

            var img   = btnGO.AddComponent<Image>();
            img.color = new Color(0.14f, 0.14f, 0.20f);
            btnGO.AddComponent<Button>();

            var lbl       = MakeTMP("Label", btnGO.transform, label, 17, FontStyles.Bold);
            StretchFill(lbl.GetComponent<RectTransform>());
            lbl.alignment = TextAlignmentOptions.Center;
        }

        return navGO;
    }

    private static void WireNavButtons(GameObject navBar, GameObject canvasGO)
    {
        var buttons = navBar.GetComponentsInChildren<Button>();
        string[] methods = { "ShowMain", "ShowCollection", "ShowFusion", "ShowEvolution", "ShowTools" };

        for (int i = 0; i < buttons.Length && i < methods.Length; i++)
        {
            var so     = new SerializedObject(buttons[i]);
            var onClick = so.FindProperty("m_OnClick.m_PersistentCalls.m_Calls");
            onClick.arraySize = 1;
            var call = onClick.GetArrayElementAtIndex(0);
            call.FindPropertyRelative("m_Target").objectReferenceValue    = canvasGO;
            call.FindPropertyRelative("m_MethodName").stringValue         = methods[i];
            call.FindPropertyRelative("m_Mode").intValue                  = (int)UnityEngine.Events.PersistentListenerMode.Void;
            call.FindPropertyRelative("m_CallState").intValue             = (int)UnityEngine.Events.UnityEventCallState.RuntimeOnly;
            so.ApplyModifiedProperties();
        }
    }

    // ── Screens ──────────────────────────────────────────────────────────

    private static GameObject BuildScreen(string title, Transform parent, System.Type screenType)
    {
        var go = MakeRectGO($"[Screen] {title}", parent);
        AnchorFill(go, 0f, 0f, 1f, 1f);
        go.AddComponent<Image>().color = new Color(0.09f, 0.09f, 0.13f, 0.95f);

        // Header
        var header = MakeRectGO("Header", go.transform);
        AnchorFill(header, 0f, 0.91f, 1f, 1f);
        header.AddComponent<Image>().color = new Color(0.06f, 0.06f, 0.10f);

        var titleTMP = MakeTMP("TitleLabel", header.transform, title, 32, FontStyles.Bold);
        StretchFill(titleTMP.GetComponent<RectTransform>(), 16);
        titleTMP.alignment = TextAlignmentOptions.MidlineLeft;

        // Content area (scrollable)
        var contentArea = BuildScrollView(go.transform);
        AnchorFill(contentArea, 0f, 0f, 1f, 0.91f);

        // Attach screen component
        go.AddComponent(screenType);

        // Start hidden (Main is activated by UIManager.Start())
        go.SetActive(false);

        return go;
    }

    private static GameObject BuildScrollView(Transform parent)
    {
        var svGO     = MakeRectGO("ScrollView", parent);
        var sv       = svGO.AddComponent<ScrollRect>();
        sv.horizontal = false;
        sv.vertical   = true;
        svGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

        var viewportGO = MakeRectGO("Viewport", svGO.transform);
        AnchorFill(viewportGO, 0f, 0f, 1f, 1f);
        viewportGO.AddComponent<Image>().color  = new Color(0f, 0f, 0f, 0f);
        viewportGO.AddComponent<RectMask2D>();

        var contentGO = MakeRectGO("Content", viewportGO.transform);
        var contentRT = contentGO.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f, 1f);
        contentRT.anchorMax = new Vector2(1f, 1f);
        contentRT.pivot     = new Vector2(0.5f, 1f);
        contentRT.sizeDelta = new Vector2(0f, 0f);

        var layout                   = contentGO.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment        = TextAnchor.UpperCenter;
        layout.childControlWidth     = true;
        layout.childControlHeight    = false;
        layout.childForceExpandWidth = true;
        layout.spacing               = 8;
        layout.padding               = new RectOffset(16, 16, 8, 8);

        var csf  = contentGO.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        sv.content  = contentGO.GetComponent<RectTransform>();
        sv.viewport = viewportGO.GetComponent<RectTransform>();

        return svGO;
    }

    // ── Wiring ───────────────────────────────────────────────────────────

    private static void WireUIManager(GameObject canvasGO, Dictionary<ScreenType, GameObject> screens)
    {
        var uim = canvasGO.AddComponent<UIManager>();
        var so  = new SerializedObject(uim);

        var map = new Dictionary<ScreenType, string>
        {
            { ScreenType.Main,       "_mainScreen"       },
            { ScreenType.Collection, "_collectionScreen" },
            { ScreenType.Fusion,     "_fusionScreen"     },
            { ScreenType.Evolution,  "_evolutionScreen"  },
            { ScreenType.Tools,      "_toolsScreen"      },
        };

        foreach (var pair in map)
            if (screens.TryGetValue(pair.Key, out var go))
                so.FindProperty(pair.Value).objectReferenceValue = go;

        so.ApplyModifiedProperties();
    }

    private static void WireScreenPrefabs(Dictionary<ScreenType, GameObject> screens)
    {
        var cardPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/CreatureCard.prefab");
        var dnaRow     = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/DNAEntryRow.prefab");
        var toolPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/UI/ToolPanel.prefab");

        if (cardPrefab != null)
        {
            TrySetPrefabField(screens, ScreenType.Collection, typeof(CollectionScreen), "_cardPrefab",  cardPrefab);
            TrySetPrefabField(screens, ScreenType.Fusion,     typeof(FusionScreen),     "_cardPrefab",  cardPrefab);
            TrySetPrefabField(screens, ScreenType.Evolution,  typeof(EvolutionScreen),  "_cardPrefab",  cardPrefab);
        }
        if (dnaRow != null)
            TrySetPrefabField(screens, ScreenType.Main, typeof(MainScreen), "_dnaEntryPrefab", dnaRow);

        if (toolPrefab != null)
            TrySetPrefabField(screens, ScreenType.Tools, typeof(ToolsScreen), "_toolPanelPrefab", toolPrefab);

        // Wire scroll content containers to screen _container fields
        foreach (var pair in screens)
        {
            var scrollContent = FindDescendant(pair.Value, "Content");
            if (scrollContent == null) continue;

            System.Type screenType = pair.Key switch
            {
                ScreenType.Main       => typeof(MainScreen),
                ScreenType.Collection => typeof(CollectionScreen),
                ScreenType.Fusion     => typeof(FusionScreen),
                ScreenType.Evolution  => typeof(EvolutionScreen),
                ScreenType.Tools      => typeof(ToolsScreen),
                _                     => null,
            };
            if (screenType == null) continue;

            var comp = pair.Value.GetComponent(screenType);
            if (comp == null) continue;

            var so   = new SerializedObject(comp);
            var prop = so.FindProperty("_container");
            if (prop != null)
            {
                prop.objectReferenceValue = scrollContent.transform;
                so.ApplyModifiedProperties();
            }
        }
    }

    private static void TrySetPrefabField(Dictionary<ScreenType, GameObject> screens,
        ScreenType screen, System.Type componentType, string fieldName, Object value)
    {
        if (!screens.TryGetValue(screen, out var go)) return;
        var comp = go.GetComponent(componentType);
        if (comp == null) return;
        SetField(comp, fieldName, value);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static void SetField(Object target, string fieldName, Object value)
    {
        var so   = new SerializedObject(target);
        var prop = so.FindProperty(fieldName);
        if (prop != null) { prop.objectReferenceValue = value; so.ApplyModifiedProperties(); }
    }

    private static GameObject MakeRectGO(string name, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static TextMeshProUGUI MakeTMP(string name, Transform parent,
        string text, float size = 24, FontStyles style = FontStyles.Normal)
    {
        var go  = MakeRectGO(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.fontStyle = style;
        tmp.color     = Color.white;
        return tmp;
    }

    private static void AnchorFill(GameObject go, float xMin, float yMin, float xMax, float yMax)
    {
        var rt       = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(xMin, yMin);
        rt.anchorMax = new Vector2(xMax, yMax);
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    private static void StretchFill(RectTransform rt, float pad = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(pad, pad);
        rt.offsetMax = new Vector2(-pad, -pad);
    }

    private static GameObject FindDescendant(GameObject root, string name)
    {
        foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            if (t.name == name) return t.gameObject;
        return null;
    }

    // ── Post-build checklist ─────────────────────────────────────────────

    private static void PrintChecklist(GameObject systemsGO)
    {
        Debug.Log(
            "╔══════════════════════════════════════════════════════╗\n" +
            "║        ScrachyMons — Setup Checklist                 ║\n" +
            "╠══════════════════════════════════════════════════════╣\n" +
            "║  AUTOMÁTICO (ya hecho por el SceneBuilder):          ║\n" +
            "║  ✓ [Systems] con todos los managers                  ║\n" +
            "║  ✓ Canvas + 5 pantallas + barra de navegación        ║\n" +
            "║  ✓ ScrollViews en cada pantalla                      ║\n" +
            "║  ✓ UIManager conectado a las 5 pantallas             ║\n" +
            "║  ✓ Prefabs de tarjeta y DNA asignados                ║\n" +
            "║  ✓ SO references (Balance, Rarity, Evolution, Tools) ║\n" +
            "╠══════════════════════════════════════════════════════╣\n" +
            "║  MANUAL (en Inspector):                              ║\n" +
            "║  □ MainScreen → _generateButton  (botón Generar)    ║\n" +
            "║  □ MainScreen → _prestigeButton  (botón Prestige)   ║\n" +
            "║  □ MainScreen → _productionText  (TMP producción)   ║\n" +
            "║  □ FusionScreen → _fuseButton, _slotAText, _slotBText║\n" +
            "║  □ FusionScreen → _statusText, _fusionStatusText     ║\n" +
            "║  □ EvolutionScreen → _statusText                    ║\n" +
            "║  □ Sprites de criaturas en CreatureData SOs          ║\n" +
            "╠══════════════════════════════════════════════════════╣\n" +
            "║  OPCIONAL:                                           ║\n" +
            "║  □ ActiveEventWidget en la pantalla Main             ║\n" +
            "║  □ Ajustar valores en GameBalanceConfig SO           ║\n" +
            "╚══════════════════════════════════════════════════════╝"
        );
    }
}
