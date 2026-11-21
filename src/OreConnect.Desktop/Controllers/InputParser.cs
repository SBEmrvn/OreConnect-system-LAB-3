using System.Globalization;

namespace OreConnect.Desktop.Controllers;

internal static class InputParser
{
    /// <summary>Parses a number typed with either "." or "," as decimal separator.</summary>
    public static bool TryDecimal(string text, out decimal value)
    {
        var t = (text ?? "").Trim().Replace(',', '.');
        return decimal.TryParse(t, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }
}
