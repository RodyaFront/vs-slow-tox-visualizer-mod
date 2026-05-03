using System;

namespace SlowToxVisualized;

public enum SlowToxHudEffectKind
{
    DamageReductionBuff = 0,
    HealthRegenBuff = 1,
    TemporalRecoveryBuff = 2,
    MeleeDamageBuff = 3,
    MiningSpeedBuff = 4,
    SlowDebuff = 5,
    PoisonDebuff = 6
}

internal static class SlowToxHudEffectKindMeta
{
    internal static readonly int KindCount = Enum.GetValues<SlowToxHudEffectKind>().Length;
}
