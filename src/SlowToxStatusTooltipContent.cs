using System;
using System.Globalization;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace SlowToxVisualized;

internal static class SlowToxStatusTooltipContent
{
    internal const string LangFmtDr = "slowtoxvisualized:tooltip-dr-fmt";
    internal const string LangFmtHp = "slowtoxvisualized:tooltip-hp-fmt";
    internal const string LangFmtTemporal = "slowtoxvisualized:tooltip-temporal-fmt";
    internal const string LangFmtMelee = "slowtoxvisualized:tooltip-melee-fmt";
    internal const string LangFmtMining = "slowtoxvisualized:tooltip-mining-fmt";
    internal const string LangFmtSlow = "slowtoxvisualized:tooltip-slow-fmt";
    internal const string LangFmtPoison = "slowtoxvisualized:tooltip-poison-fmt";

    internal static string BuildVtml(
        SlowToxHudEffectKind kind,
        Entity entity,
        ICoreClientAPI capi,
        HudLayoutConfig layout)
    {
        IFormatProvider inv = CultureInfo.InvariantCulture;
        float intox = SlowToxEffectProbe.ResolveIntoxicationForLogic(entity, layout);
        bool composed = SlowToxEffectProbe.HasComposedTrait(entity, capi);
        float benefitMult = entity.Stats.GetBlended("slowtox:benefitMult");
        float overMult = entity.Stats.GetBlended("slowtox:overintoxicationDamageMult");

        switch (kind)
        {
            case SlowToxHudEffectKind.DamageReductionBuff:
            {
                float dr = SlowToxEffectMath.CalculateBenefit(
                    benefitMult,
                    SlowToxHudDefaults.DamageReductionMax,
                    SlowToxHudDefaults.DamageReductionIntoxRangeBottom,
                    SlowToxHudDefaults.DamageReductionIntoxRangeTop,
                    intox,
                    composed);
                return string.Format(inv, Lang.Get(LangFmtDr), dr);
            }

            case SlowToxHudEffectKind.HealthRegenBuff:
            {
                float hpPerSec = SlowToxEffectMath.CalculateBenefit(
                    benefitMult,
                    SlowToxHudDefaults.HealthRegenRateMax,
                    SlowToxHudDefaults.HealthRegenIntoxRangeBottom,
                    SlowToxHudDefaults.HealthRegenIntoxRangeTop,
                    intox,
                    composed);
                return string.Format(inv, Lang.Get(LangFmtHp), hpPerSec);
            }

            case SlowToxHudEffectKind.TemporalRecoveryBuff:
            {
                float stabPerSec = SlowToxEffectMath.CalculateBenefit(
                    benefitMult,
                    SlowToxHudDefaults.StabilityRegenRateMax,
                    SlowToxHudDefaults.StabilityRegenIntoxRangeBottom,
                    SlowToxHudDefaults.StabilityRegenIntoxRangeTop,
                    intox,
                    composed);
                return string.Format(inv, Lang.Get(LangFmtTemporal), stabPerSec);
            }

            case SlowToxHudEffectKind.MeleeDamageBuff:
            {
                float meleeStat = SlowToxEffectProbe.ReadKeyedStat(entity, "meleeWeaponsDamage", "intoxicated");
                float strengthCalc = SlowToxEffectMath.CalculateBenefit(
                    benefitMult,
                    SlowToxHudDefaults.StrengthBonusMultiplierMax,
                    SlowToxHudDefaults.StrengthBonusMultiplierIntoxRangeBottom,
                    SlowToxHudDefaults.StrengthBonusMultiplierIntoxRangeTop,
                    intox,
                    composed);
                float melee = GameMath.Max(meleeStat, strengthCalc);
                float pct = melee * 100f;
                return string.Format(inv, Lang.Get(LangFmtMelee), pct);
            }

            case SlowToxHudEffectKind.MiningSpeedBuff:
            {
                float miningStat = SlowToxEffectProbe.ReadKeyedStat(entity, "miningSpeedMul", "intoxicated");
                float strengthCalc = SlowToxEffectMath.CalculateBenefit(
                    benefitMult,
                    SlowToxHudDefaults.StrengthBonusMultiplierMax,
                    SlowToxHudDefaults.StrengthBonusMultiplierIntoxRangeBottom,
                    SlowToxHudDefaults.StrengthBonusMultiplierIntoxRangeTop,
                    intox,
                    composed);
                float mining = GameMath.Max(miningStat, strengthCalc);
                float pct = mining * 100f;
                return string.Format(inv, Lang.Get(LangFmtMining), pct);
            }

            case SlowToxHudEffectKind.SlowDebuff:
            {
                float? walk = SlowToxEffectProbe.ReadKeyedStatNullable(entity, "walkspeed", "intoxicated");
                float penaltyFromStat = walk.HasValue ? GameMath.Max(0f, -walk.Value) : 0f;
                float penaltyCalc = SlowToxEffectMath.CalculatePenalty(
                    SlowToxHudDefaults.WalkSpeedPenaltyMax,
                    SlowToxHudDefaults.WalkSpeedPenaltyIntoxBeginApply,
                    SlowToxHudDefaults.WalkSpeedPenaltyIntoxFullApply,
                    intox);
                float penalty = GameMath.Max(penaltyFromStat, penaltyCalc);
                float pct = penalty * 100f;
                return string.Format(inv, Lang.Get(LangFmtSlow), pct);
            }

            case SlowToxHudEffectKind.PoisonDebuff:
            {
                float dmg = (0.1f + 0.4f * (intox - SlowToxHudDefaults.OverintoxicationThreshold)) * overMult;
                const float tickPeriodSec = 6f;
                float dps = dmg / tickPeriodSec;
                return string.Format(inv, Lang.Get(LangFmtPoison), dps);
            }

            default:
                return string.Format(inv, Lang.Get(LangFmtDr), 0f);
        }
    }

    /// <summary>Single scalar aligned with tooltip numbers — for detecting value-driven pulse.</summary>
    internal static float GetPulseMetric(
        SlowToxHudEffectKind kind,
        Entity entity,
        ICoreClientAPI capi,
        HudLayoutConfig layout)
    {
        float intox = SlowToxEffectProbe.ResolveIntoxicationForLogic(entity, layout);
        bool composed = SlowToxEffectProbe.HasComposedTrait(entity, capi);
        float benefitMult = entity.Stats.GetBlended("slowtox:benefitMult");
        float overMult = entity.Stats.GetBlended("slowtox:overintoxicationDamageMult");

        switch (kind)
        {
            case SlowToxHudEffectKind.DamageReductionBuff:
                return SlowToxEffectMath.CalculateBenefit(
                    benefitMult,
                    SlowToxHudDefaults.DamageReductionMax,
                    SlowToxHudDefaults.DamageReductionIntoxRangeBottom,
                    SlowToxHudDefaults.DamageReductionIntoxRangeTop,
                    intox,
                    composed);

            case SlowToxHudEffectKind.HealthRegenBuff:
                return SlowToxEffectMath.CalculateBenefit(
                    benefitMult,
                    SlowToxHudDefaults.HealthRegenRateMax,
                    SlowToxHudDefaults.HealthRegenIntoxRangeBottom,
                    SlowToxHudDefaults.HealthRegenIntoxRangeTop,
                    intox,
                    composed);

            case SlowToxHudEffectKind.TemporalRecoveryBuff:
                return SlowToxEffectMath.CalculateBenefit(
                    benefitMult,
                    SlowToxHudDefaults.StabilityRegenRateMax,
                    SlowToxHudDefaults.StabilityRegenIntoxRangeBottom,
                    SlowToxHudDefaults.StabilityRegenIntoxRangeTop,
                    intox,
                    composed);

            case SlowToxHudEffectKind.MeleeDamageBuff:
            {
                float meleeStat = SlowToxEffectProbe.ReadKeyedStat(entity, "meleeWeaponsDamage", "intoxicated");
                float strengthCalc = SlowToxEffectMath.CalculateBenefit(
                    benefitMult,
                    SlowToxHudDefaults.StrengthBonusMultiplierMax,
                    SlowToxHudDefaults.StrengthBonusMultiplierIntoxRangeBottom,
                    SlowToxHudDefaults.StrengthBonusMultiplierIntoxRangeTop,
                    intox,
                    composed);
                return GameMath.Max(meleeStat, strengthCalc) * 100f;
            }

            case SlowToxHudEffectKind.MiningSpeedBuff:
            {
                float miningStat = SlowToxEffectProbe.ReadKeyedStat(entity, "miningSpeedMul", "intoxicated");
                float strengthCalc = SlowToxEffectMath.CalculateBenefit(
                    benefitMult,
                    SlowToxHudDefaults.StrengthBonusMultiplierMax,
                    SlowToxHudDefaults.StrengthBonusMultiplierIntoxRangeBottom,
                    SlowToxHudDefaults.StrengthBonusMultiplierIntoxRangeTop,
                    intox,
                    composed);
                return GameMath.Max(miningStat, strengthCalc) * 100f;
            }

            case SlowToxHudEffectKind.SlowDebuff:
            {
                float? walk = SlowToxEffectProbe.ReadKeyedStatNullable(entity, "walkspeed", "intoxicated");
                float penaltyFromStat = walk.HasValue ? GameMath.Max(0f, -walk.Value) : 0f;
                float penaltyCalc = SlowToxEffectMath.CalculatePenalty(
                    SlowToxHudDefaults.WalkSpeedPenaltyMax,
                    SlowToxHudDefaults.WalkSpeedPenaltyIntoxBeginApply,
                    SlowToxHudDefaults.WalkSpeedPenaltyIntoxFullApply,
                    intox);
                return GameMath.Max(penaltyFromStat, penaltyCalc) * 100f;
            }

            case SlowToxHudEffectKind.PoisonDebuff:
            {
                float dmg = (0.1f + 0.4f * (intox - SlowToxHudDefaults.OverintoxicationThreshold)) * overMult;
                const float tickPeriodSec = 6f;
                return dmg / tickPeriodSec;
            }

            default:
                return 0f;
        }
    }
}
