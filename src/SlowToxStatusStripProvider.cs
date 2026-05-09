using System.Collections.Generic;
using System.Globalization;
using PlayerStatusStrip;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace SlowToxVisualized;

internal sealed class SlowToxStatusStripProvider : IStatusStripProvider
{
    private readonly ICoreClientAPI _capi;
    private readonly HudLayoutConfig _layout;
    private readonly List<SlowToxHudEffectKind> _activeEffects = new(SlowToxHudEffectKindMeta.KindCount);

    internal SlowToxStatusStripProvider(ICoreClientAPI capi)
    {
        _capi = capi;
        _layout = HudLayoutConfig.LoadOrCreate(capi);
    }

    internal void ReloadLayout()
    {
        HudLayoutConfig latest = HudLayoutConfig.Reload(_capi);
        CopyConfig(latest, _layout);
    }

    public void Collect(ICoreClientAPI capi, float deltaTime, List<StatusDescriptor> dest)
    {
        IClientPlayer? player = capi.World.Player;
        if (player?.Entity == null)
        {
            return;
        }

        float raw = IntoxicationResolve.GetRaw(player.Entity, _layout);
        if (raw > 0f)
        {
            string intoxTooltip = string.Format(
                CultureInfo.InvariantCulture,
                "{0}\n<font color=\"#cfcfcf\"><b>Current intoxication:</b></font> {1:0.##}%",
                Lang.Get("slowtoxvisualized:tooltip-jug-fmt"),
                raw * 100f);
            dest.Add(new StatusDescriptor(
                "slowtoxvisualized:intoxication",
                SlowToxHudEffectIcons.Intoxication,
                0,
                intoxTooltip,
                raw,
                StatusAffectKind.Neutral));
        }

        SlowToxEffectProbe.CollectActiveKinds(player.Entity, capi, raw, _activeEffects);
        foreach (SlowToxHudEffectKind kind in _activeEffects)
        {
            dest.Add(new StatusDescriptor(
                StableIdFor(kind),
                IconFor(kind),
                SortOrderFor(kind),
                SlowToxStatusTooltipContent.BuildVtml(kind, player.Entity, capi, _layout),
                SlowToxStatusTooltipContent.GetPulseMetric(kind, player.Entity, capi, _layout),
                AffectKindFor(kind)));
        }
    }

    private static string StableIdFor(SlowToxHudEffectKind kind)
    {
        return kind switch
        {
            SlowToxHudEffectKind.DamageReductionBuff => "slowtoxvisualized:damage-reduction",
            SlowToxHudEffectKind.HealthRegenBuff => "slowtoxvisualized:health-regen",
            SlowToxHudEffectKind.TemporalRecoveryBuff => "slowtoxvisualized:temporal-recovery",
            SlowToxHudEffectKind.MeleeDamageBuff => "slowtoxvisualized:melee-damage",
            SlowToxHudEffectKind.MiningSpeedBuff => "slowtoxvisualized:mining-speed",
            SlowToxHudEffectKind.SlowDebuff => "slowtoxvisualized:slow",
            SlowToxHudEffectKind.PoisonDebuff => "slowtoxvisualized:poison",
            _ => "slowtoxvisualized:unknown"
        };
    }

    private static AssetLocation IconFor(SlowToxHudEffectKind kind)
    {
        return SlowToxHudEffectIcons.Resolve(kind, SlowToxHudEffectIcons.Intoxication);
    }

    private static int SortOrderFor(SlowToxHudEffectKind kind)
    {
        return kind switch
        {
            SlowToxHudEffectKind.DamageReductionBuff => 10,
            SlowToxHudEffectKind.HealthRegenBuff => 20,
            SlowToxHudEffectKind.TemporalRecoveryBuff => 30,
            SlowToxHudEffectKind.MeleeDamageBuff => 40,
            SlowToxHudEffectKind.MiningSpeedBuff => 50,
            SlowToxHudEffectKind.SlowDebuff => 60,
            SlowToxHudEffectKind.PoisonDebuff => 70,
            _ => 90
        };
    }

    private static StatusAffectKind AffectKindFor(SlowToxHudEffectKind kind)
    {
        return kind switch
        {
            SlowToxHudEffectKind.SlowDebuff or SlowToxHudEffectKind.PoisonDebuff => StatusAffectKind.Negative,
            _ => StatusAffectKind.Positive
        };
    }

    private static void CopyConfig(HudLayoutConfig src, HudLayoutConfig dst)
    {
        dst.DialogArea = src.DialogArea;
        dst.DialogOffsetX = src.DialogOffsetX;
        dst.DialogOffsetY = src.DialogOffsetY;
        dst.DialogWidth = src.DialogWidth;
        dst.DialogHeight = src.DialogHeight;
        dst.StatusTooltipMaxWidth = src.StatusTooltipMaxWidth;
        dst.StatusTooltipZ = src.StatusTooltipZ;
        dst.StatusStripOffsetX = src.StatusStripOffsetX;
        dst.StatusStripOffsetY = src.StatusStripOffsetY;
        dst.StatusIconGapPx = src.StatusIconGapPx;
        dst.StatusIconSize = src.StatusIconSize;
        dst.StatusStripAnchorMode = src.StatusStripAnchorMode;
        dst.StatusStripVerticalAlign = src.StatusStripVerticalAlign;
        dst.StatusStripSide = src.StatusStripSide;
        dst.StatusIconAnimEnabled = src.StatusIconAnimEnabled;
        dst.StatusIconsWaveEnabled = src.StatusIconsWaveEnabled;
        dst.StatusIconsWaveAmplitudePx = src.StatusIconsWaveAmplitudePx;
        dst.StatusIconsWavePeriodSec = src.StatusIconsWavePeriodSec;
        dst.StatusIconsWaveStaggerSec = src.StatusIconsWaveStaggerSec;
        dst.UseMockIntoxicationOverride = src.UseMockIntoxicationOverride;
        dst.MockIntoxicationRaw = src.MockIntoxicationRaw;
        dst.EnsureDefaults();
    }
}
