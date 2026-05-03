using System;

namespace SlowToxVisualized;

internal static class UxColorMath
{
    public static double[] StrokeRgbaFromFillRgba(double[] fillRgba)
    {
        double r = fillRgba[0];
        double g = fillRgba[1];
        double b = fillRgba[2];
        RgbToHsl(r, g, b, out double h, out double s, out double l);

        // Keep hue readable: previous L band (~0.04–0.3) looked black on all UX stops.
        double lOut = Math.Clamp(l * 0.5, 0.14, 0.42);
        if (l > 0.58)
        {
            lOut = Math.Clamp(0.2 + (l - 0.58) * 0.55, 0.18, 0.45);
        }

        double sOut = Math.Clamp(s * 1.12 + 0.18, 0.48, 1.0);
        HslToRgb(h, sOut, lOut, out r, out g, out b);
        return new[] { r, g, b, 1.0 };
    }

    private static void RgbToHsl(double r, double g, double b, out double h, out double s, out double l)
    {
        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        l = (max + min) / 2.0;

        if (Math.Abs(max - min) < 1e-9)
        {
            h = 0;
            s = 0;
            return;
        }

        double d = max - min;
        s = l > 0.5 ? d / (2.0 - max - min) : d / (max + min);

        if (Math.Abs(max - r) < 1e-9)
        {
            h = (g - b) / d + (g < b ? 6.0 : 0);
        }
        else if (Math.Abs(max - g) < 1e-9)
        {
            h = (b - r) / d + 2.0;
        }
        else
        {
            h = (r - g) / d + 4.0;
        }

        h /= 6.0;
    }

    private static void HslToRgb(double h, double s, double l, out double r, out double g, out double b)
    {
        if (s < 1e-9)
        {
            r = g = b = l;
            return;
        }

        double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        double p = 2.0 * l - q;
        r = HueToRgb(p, q, h + 1.0 / 3.0);
        g = HueToRgb(p, q, h);
        b = HueToRgb(p, q, h - 1.0 / 3.0);
    }

    private static double HueToRgb(double p, double q, double t)
    {
        if (t < 0)
        {
            t += 1;
        }

        if (t > 1)
        {
            t -= 1;
        }

        if (t < 1.0 / 6.0)
        {
            return p + (q - p) * 6.0 * t;
        }

        if (t < 0.5)
        {
            return q;
        }

        if (t < 2.0 / 3.0)
        {
            return p + (q - p) * (2.0 / 3.0 - t) * 6.0;
        }

        return p;
    }
}
