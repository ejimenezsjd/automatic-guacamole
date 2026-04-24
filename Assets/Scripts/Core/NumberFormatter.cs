/// <summary>
/// Static utility for idle-game number formatting.
/// Converts large doubles to K / M / B / T / Q suffixes for display.
/// </summary>
public static class NumberFormatter
{
    private static readonly (double threshold, string suffix)[] Tiers =
    {
        (1e18, "Qi"),
        (1e15, "Qa"),
        (1e12, "T"),
        (1e9,  "B"),
        (1e6,  "M"),
        (1e3,  "K"),
    };

    /// <summary>Formats a number as "1.23M", "456.7K", "0.05", etc.</summary>
    public static string Format(double value)
    {
        if (value < 0d) return "-" + Format(-value);

        foreach (var (threshold, suffix) in Tiers)
        {
            if (value >= threshold)
                return $"{value / threshold:F2}{suffix}";
        }

        return value >= 10d ? $"{value:F1}" : $"{value:F2}";
    }

    /// <summary>Appends "/s" for rate display.</summary>
    public static string FormatRate(double perSecond)
        => $"{Format(perSecond)}/s";

    /// <summary>Formats duration in seconds as "2h 15m" / "45s".</summary>
    public static string FormatTime(float seconds)
    {
        if (seconds >= 3600f)
        {
            int h = (int)(seconds / 3600f);
            int m = (int)((seconds % 3600f) / 60f);
            return $"{h}h {m}m";
        }
        if (seconds >= 60f)
        {
            int m = (int)(seconds / 60f);
            int s = (int)(seconds % 60f);
            return $"{m}m {s}s";
        }
        return $"{(int)seconds}s";
    }

    /// <summary>DNA amount with type label: "1.23K Fuego DNA".</summary>
    public static string FormatDNA(double amount, DNAType type)
        => $"{Format(amount)} {type} DNA";
}
