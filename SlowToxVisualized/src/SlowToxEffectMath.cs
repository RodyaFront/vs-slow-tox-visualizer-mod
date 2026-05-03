using System;
using Vintagestory.API.MathTools;

namespace SlowToxVisualized;

internal static class SlowToxEffectMath
{
    internal static float CalculatePenalty(float penaltyMax, float rangeBottom, float rangeTop, float intoxication)
    {
        if (penaltyMax <= 0f || intoxication <= 0f || rangeTop <= 0f || rangeTop <= rangeBottom)
        {
            return 0f;
        }

        if (intoxication < rangeBottom)
        {
            return 0f;
        }

        return GameMath.Min(
            penaltyMax,
            penaltyMax * ((intoxication - rangeBottom) / (rangeTop - rangeBottom)));
    }

    internal static float CalculateBenefit(
        float entityBenefitMult,
        float benefitMax,
        float rangeBottom,
        float rangeTop,
        float intoxication,
        bool falloffDisabled)
    {
        if (benefitMax <= 0f)
        {
            return 0f;
        }

        float range = rangeTop - rangeBottom;
        if (range <= 0f)
        {
            return 0f;
        }

        float peak = (rangeBottom + rangeTop) / 2f;
        float peakRange = range * (1f / 6f);
        float distanceFromPeak = Math.Abs(intoxication - peak);
        if (falloffDisabled && intoxication >= peak)
        {
            distanceFromPeak = 0f;
        }

        if (distanceFromPeak <= peakRange)
        {
            return benefitMax * entityBenefitMult;
        }

        float mult = GameMath.Max(0f, 1f - ((distanceFromPeak - peakRange) / (2f * peakRange)));
        return benefitMax * mult * entityBenefitMult;
    }
}
