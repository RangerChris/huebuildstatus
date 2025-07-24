namespace HueBuildStatus.Core;

public static class ColorConverter
{
    // Converts hex color to CIE xy color space (approximate)
    public static (double x, double y) HexToCIE(string hex)
    {
        if (string.IsNullOrWhiteSpace(hex) || !hex.StartsWith("#") || hex.Length != 7)
            throw new ArgumentException("Invalid hex color format. Expected #RRGGBB.");
        var r = Convert.ToInt32(hex.Substring(1, 2), 16);
        var g = Convert.ToInt32(hex.Substring(3, 2), 16);
        var b = Convert.ToInt32(hex.Substring(5, 2), 16);
        // Convert RGB to XYZ
        double[] rgb = { r / 255.0, g / 255.0, b / 255.0 };
        for (var i = 0; i < 3; i++)
            rgb[i] = rgb[i] > 0.04045 ? Math.Pow((rgb[i] + 0.055) / 1.055, 2.4) : rgb[i] / 12.92;
        var X = rgb[0] * 0.4124 + rgb[1] * 0.3576 + rgb[2] * 0.1805;
        var Y = rgb[0] * 0.2126 + rgb[1] * 0.7152 + rgb[2] * 0.0722;
        var Z = rgb[0] * 0.0193 + rgb[1] * 0.1192 + rgb[2] * 0.9505;
        var sum = X + Y + Z;
        var x = sum == 0 ? 0 : X / sum;
        var y = sum == 0 ? 0 : Y / sum;
        return (x, y);
    }
}