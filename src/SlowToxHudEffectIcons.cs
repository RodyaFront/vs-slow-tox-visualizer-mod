using Vintagestory.API.Common;

namespace SlowToxVisualized;

internal static class SlowToxHudEffectIcons
{
    internal static readonly AssetLocation Intoxication = new("slowtoxvisualized", "textures/icons/jug.png");
    internal static readonly AssetLocation DamageReduction = new("slowtoxvisualized", "textures/icons/durable.png");
    internal static readonly AssetLocation HealthRegen = new("slowtoxvisualized", "textures/icons/regeneration.png");
    internal static readonly AssetLocation TemporalRecovery = new("slowtoxvisualized", "textures/icons/temporal_recovery.png");
    internal static readonly AssetLocation MeleeDamage = new("slowtoxvisualized", "textures/icons/strength.png");
    internal static readonly AssetLocation MiningSpeed = new("slowtoxvisualized", "textures/icons/mining_speed.png");
    internal static readonly AssetLocation SlowDebuff = new("slowtoxvisualized", "textures/icons/slow.png");
    internal static readonly AssetLocation PoisonDebuff = new("slowtoxvisualized", "textures/icons/poison.png");

    internal static AssetLocation Resolve(SlowToxHudEffectKind kind, AssetLocation fallback)
    {
        return kind switch
        {
            SlowToxHudEffectKind.DamageReductionBuff => DamageReduction,
            SlowToxHudEffectKind.HealthRegenBuff => HealthRegen,
            SlowToxHudEffectKind.TemporalRecoveryBuff => TemporalRecovery,
            SlowToxHudEffectKind.MeleeDamageBuff => MeleeDamage,
            SlowToxHudEffectKind.MiningSpeedBuff => MiningSpeed,
            SlowToxHudEffectKind.SlowDebuff => SlowDebuff,
            SlowToxHudEffectKind.PoisonDebuff => PoisonDebuff,
            _ => fallback
        };
    }
}
