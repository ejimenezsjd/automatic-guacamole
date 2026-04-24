using UnityEngine;
using TMPro;

/// <summary>
/// Single row in the DNA panel. Shows type name, current amount, and passive rate.
/// Prefab: HorizontalLayout > TypeLabel | AmountLabel | RateLabel
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class DNAEntryRow : MonoBehaviour
{
    [SerializeField] private TMP_Text _typeLabel;
    [SerializeField] private TMP_Text _amountLabel;
    [SerializeField] private TMP_Text _rateLabel;

    private DNAType _type;

    public DNAType Type => _type;

    public void Setup(DNAType type, float amount, float ratePerSecond = 0f)
    {
        _type = type;

        if (_typeLabel != null)
            _typeLabel.text = type.ToString();

        Refresh(amount, ratePerSecond);
    }

    public void Refresh(float amount, float ratePerSecond = 0f)
    {
        if (_amountLabel != null)
            _amountLabel.text = NumberFormatter.Format(amount);

        if (_rateLabel != null)
        {
            bool hasRate = ratePerSecond > 0.001f;
            _rateLabel.gameObject.SetActive(hasRate);
            if (hasRate) _rateLabel.text = $"+{NumberFormatter.FormatRate(ratePerSecond)}";
        }
    }
}
