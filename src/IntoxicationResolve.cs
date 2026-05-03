using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;

namespace SlowToxVisualized;

internal static class IntoxicationResolve
{
    internal static float GetRaw(Entity? entity, HudLayoutConfig cfg)
    {
        cfg.EnsureDefaults();

        if (cfg.UseMockIntoxicationOverride)
        {
            return GameMath.Clamp((float)cfg.MockIntoxicationRaw, 0f, 10f);
        }

        float real = entity?.WatchedAttributes.GetFloat("intoxication", 0f) ?? 0f;
        return GameMath.Clamp(real, 0f, 10f);
    }
}
