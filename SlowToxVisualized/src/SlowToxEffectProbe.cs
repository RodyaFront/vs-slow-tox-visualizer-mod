using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace SlowToxVisualized;

internal static class SlowToxEffectProbe
{
    internal const float Epsilon = 0.0005f;

    internal static float ResolveIntoxicationForLogic(Entity? entity, HudLayoutConfig layout)
    {
        return IntoxicationResolve.GetRaw(entity, layout);
    }

    internal static void CollectActiveKinds(
        Entity? entity,
        ICoreClientAPI capi,
        float intoxForLogic,
        List<SlowToxHudEffectKind> dest)
    {
        dest.Clear();

        if (entity == null)
        {
            return;
        }

        bool composed = HasComposedTrait(entity, capi);
        float benefitMult = entity.Stats.GetBlended("slowtox:benefitMult");
        float overMult = entity.Stats.GetBlended("slowtox:overintoxicationDamageMult");

        float dr = SlowToxEffectMath.CalculateBenefit(
            benefitMult,
            SlowToxHudDefaults.DamageReductionMax,
            SlowToxHudDefaults.DamageReductionIntoxRangeBottom,
            SlowToxHudDefaults.DamageReductionIntoxRangeTop,
            intoxForLogic,
            composed);

        if (dr > Epsilon)
        {
            dest.Add(SlowToxHudEffectKind.DamageReductionBuff);
        }

        bool healBranch = intoxForLogic < SlowToxHudDefaults.OverintoxicationThreshold
            || (overMult <= 0f && composed);

        if (healBranch)
        {
            float hp = SlowToxEffectMath.CalculateBenefit(
                benefitMult,
                SlowToxHudDefaults.HealthRegenRateMax,
                SlowToxHudDefaults.HealthRegenIntoxRangeBottom,
                SlowToxHudDefaults.HealthRegenIntoxRangeTop,
                intoxForLogic,
                composed);

            if (hp > Epsilon)
            {
                dest.Add(SlowToxHudEffectKind.HealthRegenBuff);
            }
        }

        if (entity.WatchedAttributes.HasAttribute("temporalStability"))
        {
            float stabPerSec = SlowToxEffectMath.CalculateBenefit(
                benefitMult,
                SlowToxHudDefaults.StabilityRegenRateMax,
                SlowToxHudDefaults.StabilityRegenIntoxRangeBottom,
                SlowToxHudDefaults.StabilityRegenIntoxRangeTop,
                intoxForLogic,
                composed);

            if (stabPerSec > Epsilon)
            {
                dest.Add(SlowToxHudEffectKind.TemporalRecoveryBuff);
            }
        }

        float meleeStat = ReadKeyedStat(entity, "meleeWeaponsDamage", "intoxicated");
        float miningStat = ReadKeyedStat(entity, "miningSpeedMul", "intoxicated");
        float strengthCalc = SlowToxEffectMath.CalculateBenefit(
            benefitMult,
            SlowToxHudDefaults.StrengthBonusMultiplierMax,
            SlowToxHudDefaults.StrengthBonusMultiplierIntoxRangeBottom,
            SlowToxHudDefaults.StrengthBonusMultiplierIntoxRangeTop,
            intoxForLogic,
            composed);

        if (GameMath.Max(meleeStat, strengthCalc) > Epsilon)
        {
            dest.Add(SlowToxHudEffectKind.MeleeDamageBuff);
        }

        if (GameMath.Max(miningStat, strengthCalc) > Epsilon)
        {
            dest.Add(SlowToxHudEffectKind.MiningSpeedBuff);
        }

        float? walk = ReadKeyedStatNullable(entity, "walkspeed", "intoxicated");
        float penaltyFromStat = walk.HasValue ? GameMath.Max(0f, -walk.Value) : 0f;
        float penaltyCalc = SlowToxEffectMath.CalculatePenalty(
            SlowToxHudDefaults.WalkSpeedPenaltyMax,
            SlowToxHudDefaults.WalkSpeedPenaltyIntoxBeginApply,
            SlowToxHudDefaults.WalkSpeedPenaltyIntoxFullApply,
            intoxForLogic);

        if (GameMath.Max(penaltyFromStat, penaltyCalc) > Epsilon)
        {
            dest.Add(SlowToxHudEffectKind.SlowDebuff);
        }

        if (intoxForLogic >= SlowToxHudDefaults.OverintoxicationThreshold)
        {
            float dmg = (0.1f + 0.4f * (intoxForLogic - SlowToxHudDefaults.OverintoxicationThreshold))
                * overMult;
            if (dmg >= 0.1f - 1e-4f)
            {
                dest.Add(SlowToxHudEffectKind.PoisonDebuff);
            }
        }
    }

    internal static float ReadKeyedStat(Entity entity, string statCode, string key)
    {
        return entity.Stats[statCode].ValuesByKey.TryGetValue(key, out EntityStat<float> mod)
            ? mod.Value
            : 0f;
    }

    internal static float? ReadKeyedStatNullable(Entity entity, string statCode, string key)
    {
        return entity.Stats[statCode].ValuesByKey.TryGetValue(key, out EntityStat<float> mod)
            ? mod.Value
            : null;
    }

    internal static bool HasComposedTrait(Entity entity, ICoreClientAPI capi)
    {
        if (entity is not EntityPlayer ep)
        {
            return false;
        }

        try
        {
            CharacterSystem? cs = capi.ModLoader.GetModSystem<CharacterSystem>();
            return cs != null && cs.HasTrait(ep.Player, "composed");
        }
        catch
        {
            return false;
        }
    }
}

internal static class SlowToxHudDefaults
{
    internal const float OverintoxicationThreshold = 1.2f;

    internal const float HealthRegenIntoxRangeBottom = 0.0f;
    internal const float HealthRegenIntoxRangeTop = 0.6f;
    internal const float HealthRegenRateMax = 0.02f;

    internal const float StrengthBonusMultiplierIntoxRangeBottom = 0.2f;
    internal const float StrengthBonusMultiplierIntoxRangeTop = 1f;
    internal const float StrengthBonusMultiplierMax = 0.2f;

    internal const float DamageReductionIntoxRangeBottom = 0.2f;
    internal const float DamageReductionIntoxRangeTop = 1f;
    internal const float DamageReductionMax = 1f;

    internal const float WalkSpeedPenaltyIntoxBeginApply = 0.6f;
    internal const float WalkSpeedPenaltyIntoxFullApply = 3f;
    internal const float WalkSpeedPenaltyMax = 0.6f;

    internal const float StabilityRegenIntoxRangeBottom = 0.0f;
    internal const float StabilityRegenIntoxRangeTop = 0.6f;
    internal const float StabilityRegenRateMax = 0.00125f;
}
