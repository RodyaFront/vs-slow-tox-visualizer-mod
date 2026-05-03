using System;
using System.IO;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SlowToxVisualized;

public class HudLayoutConfig
{
    public const string LayoutConfigFileName = "slowtoxvisualized-hudlayout.json";
    private const string LegacyLayoutConfigFileName = "hudlayout.json";
    public string DialogArea { get; set; } = "RightBottom";

    public double DialogOffsetX { get; set; } = -8;
    public double DialogOffsetY { get; set; } = -2;
    public double DialogWidth { get; set; } = 32;
    public double DialogHeight { get; set; } = 54;

    public double ColumnWidth { get; set; } = 32;
    public double TextOffsetX { get; set; } = -2;
    public double TextY { get; set; } = 30;
    public double TextHeight { get; set; } = 18;

    public int MugSize { get; set; } = 32;
    public int GearDrawSize { get; set; } = 64;
    public double GearCornerInsetFactor { get; set; } = 0.05;
    public double GearOffsetX { get; set; } = -2;
    public double GearOffsetY { get; set; } = -2;

    public float ZBeer { get; set; } = 440f;
    public float ZGear { get; set; } = 560f;
    public float ZStatusIcons { get; set; } = 570f;

    public int StatusTooltipMaxWidth { get; set; } = 260;
    public float StatusTooltipZ { get; set; } = 600f;

    public double StatusStripOffsetX { get; set; } = 4;
    public double StatusStripOffsetY { get; set; } = 2;
    public int StatusIconGapPx { get; set; } = 4;
    public int StatusIconSize { get; set; } = 0;
    public string StatusStripAnchorMode { get; set; } = "Max";
    public string StatusStripVerticalAlign { get; set; } = "Top";

    /// <summary>Where the status icon strip grows from the HUD block: "Right" (icons to the right of the anchor) or "Left" (icons to the left of the mug/dialog).</summary>
    public string StatusStripSide { get; set; } = "Left";

    public bool StatusIconAnimEnabled { get; set; } = true;

    public bool StatusIconsWaveEnabled { get; set; } = true;
    public double StatusIconsWaveAmplitudePx { get; set; } = 1;
    public double StatusIconsWavePeriodSec { get; set; } = 2.5;
    public double StatusIconsWaveStaggerSec { get; set; } = 0.2;

    public float GearRotateDegPerSec { get; set; } = 45f;

    public int FontSize { get; set; } = 14;
    public int StrokeWidth { get; set; } = 1;

    public bool TextStrokeMatchFillHue { get; set; } = true;

    public double[] TextStrokeColor { get; set; } = { 0, 0, 0, 1 };
    public double GearStrokeWidth { get; set; } = 6;
    public double[] GearStrokeColor { get; set; } = { 0, 0, 0, 1 };

    public bool UseMockIntoxicationOverride { get; set; } = false;

    public double MockIntoxicationRaw { get; set; } = 0.5;

    public static HudLayoutConfig CreateDefaults()
    {
        HudLayoutConfig c = new HudLayoutConfig();
        c.EnsureDefaults();
        return c;
    }

    public void EnsureDefaults()
    {
        TextStrokeColor ??= new double[] { 0, 0, 0, 1 };
        GearStrokeColor ??= new double[] { 0, 0, 0, 1 };
        StatusStripAnchorMode ??= "Max";
        StatusStripVerticalAlign ??= "Top";
        if (string.IsNullOrWhiteSpace(StatusStripSide))
        {
            StatusStripSide = "Left";
        }

        if (StatusTooltipMaxWidth <= 0)
        {
            StatusTooltipMaxWidth = 260;
        }
    }

    public static HudLayoutConfig LoadOrCreate(ICoreClientAPI capi)
    {
        string modConfigDir = capi.GetOrCreateDataPath("ModConfig");

        HudLayoutConfig? loaded = capi.LoadModConfig<HudLayoutConfig>(LayoutConfigFileName);
        if (loaded != null)
        {
            loaded.EnsureDefaults();
            capi.Logger.Notification(
                "[SlowTox Visualized] HUD layout: {0}",
                Path.Combine(modConfigDir, LayoutConfigFileName));
            return loaded;
        }

        loaded = capi.LoadModConfig<HudLayoutConfig>(LegacyLayoutConfigFileName);
        if (loaded != null)
        {
            loaded.EnsureDefaults();
            capi.Logger.Notification(
                "[SlowTox Visualized] Migrated layout from {0} -> {1}",
                LegacyLayoutConfigFileName,
                LayoutConfigFileName);
            capi.StoreModConfig(loaded, LayoutConfigFileName);
            capi.Logger.Notification(
                "[SlowTox Visualized] HUD layout: {0}",
                Path.Combine(modConfigDir, LayoutConfigFileName));
            return loaded;
        }

        HudLayoutConfig defaults = CreateDefaults();
        capi.StoreModConfig(defaults, LayoutConfigFileName);
        capi.Logger.Notification(
            "[SlowTox Visualized] Created default HUD layout: {0} (F9 to reload after edits)",
            Path.Combine(modConfigDir, LayoutConfigFileName));
        return defaults;
    }

    public static HudLayoutConfig Reload(ICoreClientAPI capi)
    {
        string dir = capi.GetOrCreateDataPath("ModConfig");
        string primaryPath = Path.Combine(dir, LayoutConfigFileName);
        string legacyPath = Path.Combine(dir, LegacyLayoutConfigFileName);

        bool primaryExists = File.Exists(primaryPath);
        bool legacyExists = File.Exists(legacyPath);

        if (primaryExists && legacyExists)
        {
            bool preferPrimary =
                File.GetLastWriteTimeUtc(primaryPath) >= File.GetLastWriteTimeUtc(legacyPath);
            HudLayoutConfig? cfg = preferPrimary
                ? capi.LoadModConfig<HudLayoutConfig>(LayoutConfigFileName)
                : capi.LoadModConfig<HudLayoutConfig>(LegacyLayoutConfigFileName);
            cfg ??= CreateDefaults();
            cfg.EnsureDefaults();
            return cfg;
        }

        if (primaryExists)
        {
            HudLayoutConfig? cfg = capi.LoadModConfig<HudLayoutConfig>(LayoutConfigFileName) ?? CreateDefaults();
            cfg.EnsureDefaults();
            return cfg;
        }

        if (legacyExists)
        {
            HudLayoutConfig? cfg = capi.LoadModConfig<HudLayoutConfig>(LegacyLayoutConfigFileName) ?? CreateDefaults();
            cfg.EnsureDefaults();
            return cfg;
        }

        HudLayoutConfig fallback = CreateDefaults();
        fallback.EnsureDefaults();
        return fallback;
    }

    public ElementBounds DialogBounds()
    {
        return ElementBounds.Fixed(ParseDialogArea(DialogArea), DialogOffsetX, DialogOffsetY, DialogWidth, DialogHeight);
    }

    public ElementBounds InnerBounds()
    {
        return ElementBounds.Fixed(0, 0, ColumnWidth, DialogHeight);
    }

    public ElementBounds TextBounds()
    {
        return ElementBounds.Fixed(TextOffsetX, TextY, ColumnWidth, TextHeight);
    }

    private static EnumDialogArea ParseDialogArea(string code)
    {
        return code?.Trim() switch
        {
            "CenterBottom" => EnumDialogArea.CenterBottom,
            "LeftBottom" => EnumDialogArea.LeftBottom,
            "RightBottom" => EnumDialogArea.RightBottom,
            "CenterTop" => EnumDialogArea.CenterTop,
            _ => EnumDialogArea.CenterBottom
        };
    }
}
