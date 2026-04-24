using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Main laboratory screen.
/// Displays DNA balances, total production rate, and the creature-generation button.
/// </summary>
public class MainScreen : MonoBehaviour
{
    [Header("DNA Display")]
    [SerializeField] private Transform    _dnaContainer;
    [SerializeField] private GameObject   _dnaEntryPrefab;    // needs a TMP_Text child

    [Header("Stats")]
    [SerializeField] private TMP_Text _productionText;
    [SerializeField] private TMP_Text _prestigeLevelText;
    [SerializeField] private TMP_Text _permanentGenesText;

    [Header("Buttons")]
    [SerializeField] private Button _generateButton;
    [SerializeField] private Button _prestigeButton;

    // ── Lifecycle ────────────────────────────────────────────────────────

    private void OnEnable()
    {
        GameEvents.OnDNAUpdated    += RefreshDNA;
        GameEvents.OnTickProcessed += RefreshProduction;
        GameEvents.OnPrestige      += RefreshPrestigeInfo;
        RefreshAll();
    }

    private void OnDisable()
    {
        GameEvents.OnDNAUpdated    -= RefreshDNA;
        GameEvents.OnTickProcessed -= RefreshProduction;
        GameEvents.OnPrestige      -= RefreshPrestigeInfo;
    }

    private void Start()
    {
        _generateButton?.onClick.AddListener(OnGenerateClicked);
        _prestigeButton?.onClick.AddListener(OnPrestigeClicked);
    }

    // ── Button handlers ──────────────────────────────────────────────────

    private void OnGenerateClicked()
    {
        var creature = CreatureGenerator.Instance?.GenerateCreature();
        if (creature != null)
            CreatureManager.Instance?.AddCreature(creature);
    }

    private void OnPrestigeClicked()
    {
        if (PrestigeSystem.Instance?.CanPrestige() == true)
            PrestigeSystem.Instance.Prestige();
    }

    // ── Refresh helpers ──────────────────────────────────────────────────

    private void RefreshAll()
    {
        var dna = DNAController.Instance?.GetAllDNA();
        if (dna != null) RefreshDNA(dna);
        RefreshProduction();
        RefreshPrestigeInfo(PrestigeSystem.Instance?.GetPrestigeLevel() ?? 0);
    }

    private void RefreshDNA(Dictionary<DNAType, float> dna)
    {
        if (_dnaContainer == null) return;

        foreach (Transform child in _dnaContainer)
            Destroy(child.gameObject);

        foreach (var pair in dna)
        {
            if (pair.Value < 0.01f) continue;
            var entry = Instantiate(_dnaEntryPrefab, _dnaContainer);
            var label = entry.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = $"{pair.Key}: {pair.Value:F1}";
        }
    }

    private void RefreshProduction()
    {
        if (_productionText == null) return;

        float total    = 0f;
        float prestige = PrestigeSystem.Instance?.GetProductionMultiplier() ?? 1f;
        var   list     = CreatureManager.Instance?.GetAllCreatures();

        if (list != null)
            foreach (var c in list) total += c.GetTotalProduction(prestige);

        _productionText.text = $"Production: {total:F2}/s";
    }

    private void RefreshPrestigeInfo(int level)
    {
        if (_prestigeLevelText  != null) _prestigeLevelText.text  = $"Prestige: {level}";
        if (_permanentGenesText != null) _permanentGenesText.text = $"Genes: {PrestigeSystem.Instance?.GetPermanentGenes() ?? 0}";
    }
}
