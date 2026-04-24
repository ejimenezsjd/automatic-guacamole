using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// ScrachyMons → Build UI Prefabs
/// Creates CreatureCard, DNAEntryRow and ToolPanel prefabs in Assets/Prefabs/UI/.
/// Run AFTER "Generate Starter Data" so SO references exist.
/// </summary>
public class UIPrefabBuilder : EditorWindow
{
    private const string Dir = "Assets/Prefabs/UI";

    [MenuItem("ScrachyMons/Build UI Prefabs")]
    public static void ShowWindow()
        => GetWindow<UIPrefabBuilder>("UI Prefab Builder");

    private void OnGUI()
    {
        GUILayout.Label("UI Prefab Builder", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Creates prefabs consumed by the screen components.\n" +
            "Run once before building the scene.",
            MessageType.Info);

        EditorGUILayout.Space();
        if (GUILayout.Button("▶  Build ALL Prefabs (recommended)")) BuildAll();

        EditorGUILayout.Space();
        GUILayout.Label("Individual:", EditorStyles.miniLabel);
        if (GUILayout.Button("CreatureCard.prefab"))  BuildCreatureCard();
        if (GUILayout.Button("DNAEntryRow.prefab"))   BuildDNAEntryRow();
        if (GUILayout.Button("ToolPanel.prefab"))     BuildToolPanel();
    }

    // ── Public entry point (called by SceneBuilder) ──────────────────────

    public static void BuildAll()
    {
        EnsureDir(Dir);
        BuildCreatureCard();
        BuildDNAEntryRow();
        BuildToolPanel();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[UIPrefabBuilder] All prefabs created in " + Dir);
    }

    // ── CreatureCard ─────────────────────────────────────────────────────

    public static GameObject BuildCreatureCard()
    {
        string path = $"{Dir}/CreatureCard.prefab";

        // Root panel
        var root    = MakeRect("CreatureCard");
        SetSize(root, 960, 220);
        var rootImg = root.AddComponent<Image>();
        rootImg.color = new Color(0.13f, 0.13f, 0.19f);

        // Left accent bar (rarity colour comes from CreatureCard.Refresh)
        var bar = MakeRect("RarityBar", root.transform);
        var barRT = bar.GetComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0f, 0f);
        barRT.anchorMax = new Vector2(0.015f, 1f);
        barRT.offsetMin = barRT.offsetMax = Vector2.zero;
        var barImg = bar.AddComponent<Image>();
        barImg.color = new Color(0.3f, 0.55f, 1f);  // placeholder — overridden at runtime

        // Name
        var nameText = MakeTMP("NameText", root.transform, "Creature Name", 28, FontStyles.Bold);
        AnchorRect(nameText, 0.03f, 0.62f, 0.72f, 0.95f);

        // Info line
        var infoText = MakeTMP("InfoText", root.transform, "Lv.1 | Common | Fuego | Dupes: 0", 17);
        AnchorRect(infoText, 0.03f, 0.32f, 0.72f, 0.62f);
        infoText.color = new Color(0.7f, 0.7f, 0.75f);

        // Production
        var prodText = MakeTMP("ProductionText", root.transform, "1.00/s", 22);
        AnchorRect(prodText, 0.03f, 0.05f, 0.72f, 0.32f);
        prodText.color = new Color(0.4f, 1f, 0.4f);

        // Action button
        var btnGO = MakeRect("ActionButton", root.transform);
        AnchorRectGO(btnGO, 0.74f, 0.15f, 0.97f, 0.85f);
        var btnImg2 = btnGO.AddComponent<Image>();
        btnImg2.color = new Color(0.2f, 0.48f, 0.9f);
        var btn = btnGO.AddComponent<Button>();
        ColorBlock cb = btn.colors;
        cb.highlightedColor = new Color(0.3f, 0.6f, 1f);
        btn.colors = cb;

        var btnLabel = MakeTMP("Label", btnGO.transform, "SELECCIONAR", 18, FontStyles.Bold);
        StretchFill(btnLabel.GetComponent<RectTransform>());
        btnLabel.alignment = TextAlignmentOptions.Center;

        // Attach component and wire fields via SerializedObject
        var card = root.AddComponent<CreatureCard>();
        var so   = new SerializedObject(card);
        so.FindProperty("_nameText").objectReferenceValue       = nameText;
        so.FindProperty("_infoText").objectReferenceValue       = infoText;
        so.FindProperty("_productionText").objectReferenceValue = prodText;
        so.FindProperty("_background").objectReferenceValue     = rootImg;
        so.FindProperty("_actionButton").objectReferenceValue   = btn;
        so.FindProperty("_buttonLabel").objectReferenceValue    = btnLabel;
        so.ApplyModifiedProperties();

        return SavePrefab(root, path);
    }

    // ── DNAEntryRow ──────────────────────────────────────────────────────

    public static GameObject BuildDNAEntryRow()
    {
        string path = $"{Dir}/DNAEntryRow.prefab";

        var root   = MakeRect("DNAEntryRow");
        SetSize(root, 960, 64);
        var bg     = root.AddComponent<Image>();
        bg.color   = new Color(0.11f, 0.11f, 0.17f, 0.85f);

        var layout = root.AddComponent<HorizontalLayoutGroup>();
        layout.childAlignment        = TextAnchor.MiddleLeft;
        layout.childControlWidth     = false;
        layout.childControlHeight    = true;
        layout.childForceExpandWidth = false;
        layout.spacing               = 12;
        layout.padding               = new RectOffset(20, 20, 0, 0);

        // Type name (fixed width)
        var typeText = MakeTMP("TypeLabel", root.transform, "Normal", 21);
        typeText.alignment = TextAlignmentOptions.MidlineLeft;
        FixedWidth(typeText.gameObject, 190);

        // Amount
        var amtText = MakeTMP("AmountLabel", root.transform, "0", 21);
        amtText.alignment = TextAlignmentOptions.MidlineRight;
        FixedWidth(amtText.gameObject, 210);

        // Rate (hidden when zero)
        var rateText = MakeTMP("RateLabel", root.transform, "+0/s", 17);
        rateText.alignment = TextAlignmentOptions.MidlineLeft;
        rateText.color     = new Color(0.35f, 0.9f, 0.35f);
        FixedWidth(rateText.gameObject, 200);

        var row = root.AddComponent<DNAEntryRow>();
        var so  = new SerializedObject(row);
        so.FindProperty("_typeLabel").objectReferenceValue   = typeText;
        so.FindProperty("_amountLabel").objectReferenceValue = amtText;
        so.FindProperty("_rateLabel").objectReferenceValue   = rateText;
        so.ApplyModifiedProperties();

        return SavePrefab(root, path);
    }

    // ── ToolPanel ────────────────────────────────────────────────────────

    public static GameObject BuildToolPanel()
    {
        string path = $"{Dir}/ToolPanel.prefab";

        var root   = MakeRect("ToolPanel");
        SetSize(root, 960, 250);
        var bg     = root.AddComponent<Image>();
        bg.color   = new Color(0.13f, 0.13f, 0.19f);

        // Name
        var nameText = MakeTMP("NameText", root.transform, "Tool Name", 28, FontStyles.Bold);
        AnchorRect(nameText, 0.03f, 0.75f, 0.68f, 1f);

        // Description
        var descText = MakeTMP("DescText", root.transform, "Description...", 17);
        AnchorRect(descText, 0.03f, 0.48f, 0.68f, 0.75f);
        descText.color = new Color(0.65f, 0.65f, 0.7f);

        // Cost
        var costText = MakeTMP("CostText", root.transform, "Coste: 500 Normal", 17);
        AnchorRect(costText, 0.03f, 0.24f, 0.68f, 0.48f);
        costText.color = new Color(1f, 0.78f, 0.2f);

        // Status
        var statusText = MakeTMP("StatusText", root.transform, "Bloqueado", 19, FontStyles.Italic);
        AnchorRect(statusText, 0.03f, 0.03f, 0.45f, 0.24f);
        statusText.color = new Color(0.55f, 0.55f, 0.6f);

        // Unlock button (visible when locked)
        var unlockGO = MakeRect("UnlockButton", root.transform);
        AnchorRectGO(unlockGO, 0.71f, 0.55f, 0.97f, 0.93f);
        unlockGO.AddComponent<Image>().color = new Color(0.15f, 0.62f, 0.25f);
        var unlockBtn = unlockGO.AddComponent<Button>();
        var unlockLbl = MakeTMP("Label", unlockGO.transform, "DESBLOQUEAR", 18, FontStyles.Bold);
        StretchFill(unlockLbl.GetComponent<RectTransform>());
        unlockLbl.alignment = TextAlignmentOptions.Center;

        // Active toggle (visible when unlocked)
        var toggleGO = MakeRect("ActiveToggle", root.transform);
        AnchorRectGO(toggleGO, 0.71f, 0.08f, 0.97f, 0.52f);
        toggleGO.AddComponent<Image>().color = new Color(0.18f, 0.36f, 0.65f);
        var toggle = toggleGO.AddComponent<Toggle>();

        var toggleLbl = MakeTMP("Label", toggleGO.transform, "ACTIVAR", 18, FontStyles.Bold);
        StretchFill(toggleLbl.GetComponent<RectTransform>());
        toggleLbl.alignment = TextAlignmentOptions.Center;

        // Locked dimming overlay
        var overlayGO = MakeRect("LockedOverlay", root.transform);
        StretchFill(overlayGO.GetComponent<RectTransform>());
        overlayGO.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0.55f);

        var panel = root.AddComponent<ToolPanel>();
        var so    = new SerializedObject(panel);
        so.FindProperty("_nameText").objectReferenceValue    = nameText;
        so.FindProperty("_descText").objectReferenceValue    = descText;
        so.FindProperty("_costText").objectReferenceValue    = costText;
        so.FindProperty("_statusText").objectReferenceValue  = statusText;
        so.FindProperty("_unlockButton").objectReferenceValue = unlockBtn;
        so.FindProperty("_activeToggle").objectReferenceValue = toggle;
        so.FindProperty("_lockedOverlay").objectReferenceValue = overlayGO;
        so.ApplyModifiedProperties();

        return SavePrefab(root, path);
    }

    // ── Shared helpers ───────────────────────────────────────────────────

    private static GameObject MakeRect(string name, Transform parent = null)
    {
        var go = new GameObject(name);
        if (parent != null) go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        return go;
    }

    private static TextMeshProUGUI MakeTMP(string name, Transform parent, string text,
        float size = 24, FontStyles style = FontStyles.Normal)
    {
        var go  = MakeRect(name, parent);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text      = text;
        tmp.fontSize  = size;
        tmp.fontStyle = style;
        tmp.color     = Color.white;
        return tmp;
    }

    private static void SetSize(GameObject go, float w, float h)
    {
        var rt      = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(w, h);
    }

    // Anchor by normalized rect (no pixel offsets)
    private static void AnchorRect(Component c, float xMin, float yMin, float xMax, float yMax)
        => AnchorRectGO(c.gameObject, xMin, yMin, xMax, yMax);

    private static void AnchorRectGO(GameObject go, float xMin, float yMin, float xMax, float yMax)
    {
        var rt         = go.GetComponent<RectTransform>();
        rt.anchorMin   = new Vector2(xMin, yMin);
        rt.anchorMax   = new Vector2(xMax, yMax);
        rt.offsetMin   = Vector2.zero;
        rt.offsetMax   = Vector2.zero;
    }

    private static void StretchFill(RectTransform rt, float pad = 0f)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(pad, pad);
        rt.offsetMax = new Vector2(-pad, -pad);
    }

    private static void FixedWidth(GameObject go, float width)
    {
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = width;
        le.minWidth       = width;
    }

    private static GameObject SavePrefab(GameObject root, string path)
    {
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
        DestroyImmediate(root);
        Debug.Log($"[UIPrefabBuilder] Saved {path}");
        return prefab;
    }

    private static void EnsureDir(string path)
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }
}
