using Xunit;

namespace PlayerStatusStrip.Tests;

public sealed class StatusAnimMathTests
{
    [Fact]
    public void EnterScale_Neutral_StartsBelowOne()
    {
        float s = StatusAnimMath.EnterScale(StatusAffectKind.Neutral, 0.1f);
        Assert.True(s < 1f);
    }

    [Fact]
    public void ResolveEffectiveKind_FallsBackToNeutral_WhenNegativeDisabled()
    {
        StatusAffectKind k = StatusAnimMath.ResolveEffectiveKind(
            StatusAffectKind.Negative,
            requestedEnabled: false,
            neutralEnabled: true);
        Assert.Equal(StatusAffectKind.Neutral, k);
    }

    [Fact]
    public void ResolveEffectiveEnabled_DisablesAll_WhenNeutralDisabled()
    {
        bool enabled = StatusAnimMath.ResolveEffectiveEnabled(
            StatusAffectKind.Positive,
            requestedEnabled: false,
            neutralEnabled: false);
        Assert.False(enabled);
    }

    [Fact]
    public void ComputePulseScale_MidCurve_IsAboveOne()
    {
        float s = StatusAnimMath.ComputePulseScale(0.5f, 0.11f);
        Assert.True(s > 1f);
    }

    [Fact]
    public void EnterScale_Positive_IsDistinctFromNeutral()
    {
        float pos = StatusAnimMath.EnterScale(StatusAffectKind.Positive, 0.25f);
        float neutral = StatusAnimMath.EnterScale(StatusAffectKind.Neutral, 0.25f);
        Assert.NotEqual(neutral, pos);
    }

    [Fact]
    public void NegativeShake_HasHorizontalOffsetNearStart()
    {
        float dx = StatusAnimMath.HorizontalShakeOffset(StatusAffectKind.Negative, 0.2f, 6f);
        Assert.NotEqual(0f, dx);
    }

    [Fact]
    public void NegativeUpdateShake_IsNonZeroMidPulse()
    {
        float dx = StatusAnimMath.UpdateHorizontalShakeOffset(0.5f, 6f);
        Assert.NotEqual(0f, dx);
    }

    [Fact]
    public void NegativeUpdateShake_StartsAndEndsNearZero()
    {
        float start = StatusAnimMath.UpdateHorizontalShakeOffset(0f, 6f);
        float end = StatusAnimMath.UpdateHorizontalShakeOffset(1f, 6f);
        Assert.True(Math.Abs(start) < 1e-4f);
        Assert.True(Math.Abs(end) < 1e-4f);
    }

    [Fact]
    public void PositiveUpdateSlide_IsUpward()
    {
        float dy = StatusAnimMath.UpdateVerticalOffset(StatusAffectKind.Positive, 0.1f, 5f);
        Assert.True(dy < 0f);
    }
    [Fact]
    public void ExitAlpha_DecreasesToZero()
    {
        float start = StatusAnimMath.ExitAlpha(0f);
        float end = StatusAnimMath.ExitAlpha(1f);
        Assert.True(start > end);
        Assert.True(end <= 0f + 1e-6f);
    }

    [Fact]
    public void ExitScale_Negative_IsSlightlySharper()
    {
        float neg = StatusAnimMath.ExitScale(StatusAffectKind.Negative, 0.5f);
        float neutral = StatusAnimMath.ExitScale(StatusAffectKind.Neutral, 0.5f);
        Assert.True(neg < neutral);
    }
}
