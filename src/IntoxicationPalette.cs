using System;

namespace SlowToxVisualized;

internal static class IntoxicationPalette
{
    private readonly record struct RgbStop(float T, float R, float G, float B);

    // Canonical palette (validated in-game). SlowTox default IntoxicationConfig semantics; warm spectrum.
    private static readonly RgbStop[] Stops =
    {
        new(0.00f, 0.40f, 0.72f, 0.56f),
        new(0.15f, 0.30f, 0.78f, 0.44f),
        new(0.30f, 0.26f, 0.84f, 0.34f),
        new(0.45f, 0.38f, 0.86f, 0.28f),
        new(0.55f, 0.58f, 0.82f, 0.22f),
        new(0.60f, 0.90f, 0.62f, 0.14f),
        new(0.75f, 0.94f, 0.46f, 0.12f),
        new(0.90f, 0.94f, 0.30f, 0.12f),
        new(1.00f, 0.90f, 0.22f, 0.12f),
        new(1.10f, 0.86f, 0.16f, 0.13f),
        new(1.20f, 0.80f, 0.10f, 0.12f),
        new(1.45f, 0.68f, 0.07f, 0.09f),
        new(1.70f, 0.58f, 0.05f, 0.07f),
        new(2.00f, 0.48f, 0.04f, 0.06f),
    };

    public static double[] FillRgba(double rawIntoxication)
    {
        float t = (float)Math.Max(0.0, rawIntoxication);
        if (t <= Stops[0].T)
        {
            return ToRgba(Stops[0]);
        }

        int last = Stops.Length - 1;
        if (t >= Stops[last].T)
        {
            return ToRgba(Stops[last]);
        }

        for (int i = 0; i < last; i++)
        {
            if (t > Stops[i + 1].T + 1e-6f)
            {
                continue;
            }

            float t0 = Stops[i].T;
            float t1 = Stops[i + 1].T;
            float span = t1 - t0;
            float u = span > 1e-6f ? (t - t0) / span : 0f;
            float r = Stops[i].R + u * (Stops[i + 1].R - Stops[i].R);
            float g = Stops[i].G + u * (Stops[i + 1].G - Stops[i].G);
            float b = Stops[i].B + u * (Stops[i + 1].B - Stops[i].B);
            return new[] { r, g, b, 1.0 };
        }

        return ToRgba(Stops[last]);
    }

    private static double[] ToRgba(RgbStop s)
    {
        return new[] { s.R, s.G, s.B, 1.0 };
    }
}
