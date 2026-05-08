using System;
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

    private readonly struct EffectContext
    {
        internal readonly float Intox;
        internal readonly bool Composed;
        internal readonly float BenefitMult;
        internal readonly float OverMult;

        internal EffectContext(float intox, bool composed, float benefitMult, float overMult)
        {
            Intox = intox;
            Composed = composed;
            BenefitMult = benefitMult;
            OverMult = overMult;
        }
    }

    internal static string BuildVtml(
        SlowToxHudEffectKind kind,
        Entity entity,
        ICoreClientAPI capi,
        HudLayoutConfig layout)
    {
        EffectContext ctx = ReadContext(entity, capi, layout);
        return Lang.Get(LangKeyForKind(kind), ComputeEffectScalar(kind, entity, ctx));
    }

    /// <summary>Single scalar aligned with tooltip numbers — for detecting value-driven pulse.</summary>
    internal static float GetPulseMetric(
        SlowToxHudEffectKind kind,
        Entity entity,
        ICoreClientAPI capi,
        HudLayoutConfig layout)
    {
        EffectContext ctx = ReadContext(entity, capi, layout);
        return ComputeEffectScalar(kind, entity, ctx);
    }

    private static EffectContext ReadContext(Entity entity, ICoreClientAPI capi, HudLayoutConfig layout)
    {
        return new EffectContext(
            SlowToxEffectProbe.ResolveIntoxicationForLogic(entity, layout),
            SlowToxEffectProbe.HasComposedTrait(entity, capi),
            entity.Stats.GetBlended("slowtox:benefitMult"),
            entity.Stats.GetBlended("slowtox:overintoxicationDamageMult"));
    }

    private static string LangKeyForKind(SlowToxHudEffectKind kind)
    {
        switch (kind)
        {
            case SlowToxHudEffectKind.DamageReductionBuff:
                return LangFmtDr;

            case SlowToxHudEffectKind.HealthRegenBuff:
                return LangFmtHp;

            case SlowToxHudEffectKind.TemporalRecoveryBuff:
                return LangFmtTemporal;

            case SlowToxHudEffectKind.MeleeDamageBuff:
                return LangFmtMelee;

            case SlowToxHudEffectKind.MiningSpeedBuff:
                return LangFmtMining;

            case SlowToxHudEffectKind.SlowDebuff:
                return LangFmtSlow;

            case SlowToxHudEffectKind.PoisonDebuff:
                return LangFmtPoison;

            default:
                return LangFmtDr;
        }
    }

    private static float ComputeEffectScalar(SlowToxHudEffectKind kind, Entity entity, EffectContext ctx)
    {
        switch (kind)
        {
            case SlowToxHudEffectKind.DamageReductionBuff:
                return SlowToxEffectMath.CalculateBenefit(
                    ctx.BenefitMult,
                    SlowToxHudDefaults.DamageReductionMax,
                    SlowToxHudDefaults.DamageReductionIntoxRangeBottom,
                    SlowToxHudDefaults.DamageReductionIntoxRangeTop,
                    ctx.Intox,
                    ctx.Composed);

            case SlowToxHudEffectKind.HealthRegenBuff:
                return SlowToxEffectMath.CalculateBenefit(
                    ctx.BenefitMult,
                    SlowToxHudDefaults.HealthRegenRateMax,
                    SlowToxHudDefaults.HealthRegenIntoxRangeBottom,
                    SlowToxHudDefaults.HealthRegenIntoxRangeTop,
                    ctx.Intox,
                    ctx.Composed);

            case SlowToxHudEffectKind.TemporalRecoveryBuff:
                return SlowToxEffectMath.CalculateBenefit(
                    ctx.BenefitMult,
                    SlowToxHudDefaults.StabilityRegenRateMax,
                    SlowToxHudDefaults.StabilityRegenIntoxRangeBottom,
                    SlowToxHudDefaults.StabilityRegenIntoxRangeTop,
                    ctx.Intox,
                    ctx.Composed);

            case SlowToxHudEffectKind.MeleeDamageBuff:
            {
                float meleeStat = SlowToxEffectProbe.ReadKeyedStat(entity, "meleeWeaponsDamage", "intoxicated");
                float strengthCalc = SlowToxEffectMath.CalculateBenefit(
                    ctx.BenefitMult,
                    SlowToxHudDefaults.StrengthBonusMultiplierMax,
                    SlowToxHudDefaults.StrengthBonusMultiplierIntoxRangeBottom,
                    SlowToxHudDefaults.StrengthBonusMultiplierIntoxRangeTop,
                    ctx.Intox,
                    ctx.Composed);
                return GameMath.Max(meleeStat, strengthCalc) * 100f;
            }

            case SlowToxHudEffectKind.MiningSpeedBuff:
            {
                float miningStat = SlowToxEffectProbe.ReadKeyedStat(entity, "miningSpeedMul", "intoxicated");
                float strengthCalc = SlowToxEffectMath.CalculateBenefit(
                    ctx.BenefitMult,
                    SlowToxHudDefaults.StrengthBonusMultiplierMax,
                    SlowToxHudDefaults.StrengthBonusMultiplierIntoxRangeBottom,
                    SlowToxHudDefaults.StrengthBonusMultiplierIntoxRangeTop,
                    ctx.Intox,
                    ctx.Composed);
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
                    ctx.Intox);
                return GameMath.Max(penaltyFromStat, penaltyCalc) * 100f;
            }

            case SlowToxHudEffectKind.PoisonDebuff:
            {
                return SlowToxHudDefaults.PoisonDamagePerSecond(ctx.Intox, ctx.OverMult);
            }

            default:
                return 0f;
        }
    }
}
