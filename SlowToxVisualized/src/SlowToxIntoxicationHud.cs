using System;
using System.Collections.Generic;
using Cairo;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace SlowToxVisualized;

public sealed class SlowToxIntoxicationHud : HudElement
{
    private static readonly AssetLocation BeerPath = new("slowtoxvisualized", "textures/icons/jug.png");
    private static readonly AssetLocation GearSvgPath = new("slowtoxvisualized", "textures/icons/gear.svg");

    private static readonly AssetLocation IconDurability =
        new("slowtoxvisualized", "textures/icons/durable.png");
    private static readonly AssetLocation IconHp =
        new("slowtoxvisualized", "textures/icons/regeneration.png");
    private static readonly AssetLocation IconTemporalRecovery =
        new("slowtoxvisualized", "textures/icons/temporal_recovery.png");
    private static readonly AssetLocation IconMelee =
        new("slowtoxvisualized", "textures/icons/strength.png");
    private static readonly AssetLocation IconMining =
        new("slowtoxvisualized", "textures/icons/mining_speed.png");
    private static readonly AssetLocation IconSlow = new("slowtoxvisualized", "textures/icons/slow.png");
    private static readonly AssetLocation IconPoison =
        new("slowtoxvisualized", "textures/icons/poison.png");

    private static readonly Vec4f WhiteTint = new(1f, 1f, 1f, 1f);

    private LoadedTexture? _gearSvg;
    private LoadedTexture? _beerTex;
    private readonly int[] _statusTextureIds = new int[7];
    private readonly List<SlowToxHudEffectKind> _activeEffects = new(7);
    private float _gearRotationDeg;
    private double _statusIconsWaveTimeSec;
    private int _lastComposedDisplayPercent = int.MinValue;
    private bool _warnedGearMissing;

    private GuiElementRichtext? _statusTooltipRich;
    private StatusTooltipPanelBackend? _statusTooltipPanel;
    private LoadedTexture? _statusTooltipBgTex;
    private string? _lastHudTooltipVtml;

    private const float StatusPopupDurationSec = 0.28f;
    private const float StatusPulseDurationSec = 0.38f;
    private const float StatusPulseAmplitude = 0.11f;
    private const float StatusPopoutDurationSec = 0.24f;
    /// <summary>Regen/stability DPS use small absolute numbers (~0…0.02); plain absolute epsilon missed e.g. 0.01→0.011 (Δ=0.001).</summary>
    private const float StatusPulseRelativeThreshold = 0.012f;

    private const float StatusPulseAbsoluteFloor = 0.001f;

    private readonly Dictionary<SlowToxHudEffectKind, KindRuntimeAnim> _kindAnim = new();
    private readonly List<PopoutAnim> _popoutAnims = new();
    private readonly HashSet<SlowToxHudEffectKind> _prevActiveKinds = new();
    private readonly HashSet<SlowToxHudEffectKind> _scratchCurrentKinds = new();
    private readonly Dictionary<SlowToxHudEffectKind, float> _prevPulseMetric = new();
    private readonly Dictionary<SlowToxHudEffectKind, SavedIconRect> _lastIconRect = new();
    private bool _statusAnimIntroSynced;
    private bool _intoxHudWasVisible;

    private struct KindRuntimeAnim
    {
        public float PopupElapsed;
        public float PulseElapsed;
        public bool Pulsing;
    }

    private struct SavedIconRect
    {
        public double Left;
        public double Top;
        public int Size;
    }

    private sealed class PopoutAnim
    {
        public SlowToxHudEffectKind Kind;
        public float Elapsed;
        public double CenterX;
        public double CenterY;
        public int BaselineSize;
    }

    private HudLayoutConfig _layout;

    public SlowToxIntoxicationHud(ICoreClientAPI capi)
        : base(capi)
    {
        _layout = HudLayoutConfig.LoadOrCreate(capi);
        ComposeHud();
    }

    public void ReloadLayoutFromDisk()
    {
        _layout = HudLayoutConfig.Reload(capi);
        _lastComposedDisplayPercent = int.MinValue;
        ComposeHud();
        TryLoadGearTexture();
        ReloadStatusIconTextures();

        _statusTooltipRich?.Dispose();
        _statusTooltipRich = null;
        _statusTooltipBgTex?.Dispose();
        _statusTooltipBgTex = null;
        _statusTooltipPanel = null;
        _lastHudTooltipVtml = null;

        ResetIntoxHudTransientState();
        _intoxHudWasVisible = false;

        if (capi.World is IClientWorldAccessor clientWorld && clientWorld.Player != null)
        {
            clientWorld.Player.ShowChatNotification(
                $"Slow Tox Visualized: layout reloaded (FontSize={_layout.FontSize}; status Δxy={_layout.StatusStripOffsetX},{_layout.StatusStripOffsetY}; anchor={_layout.StatusStripAnchorMode}; valign={_layout.StatusStripVerticalAlign}; statusSide={_layout.StatusStripSide}).");
        }
    }

    private static int DisplayPercent(float rawIntoxication)
    {
        int p = (int)Math.Round(rawIntoxication * 100f);
        return Math.Clamp(p, 0, 999);
    }

    /// <summary>Hide HUD when displayed percent would be 0 (same rounding as the big digit).</summary>
    private static bool ShouldRenderIntoxicationHud(int displayPercent)
    {
        return displayPercent >= 1;
    }

    private void ResetIntoxHudTransientState()
    {
        _kindAnim.Clear();
        _popoutAnims.Clear();
        _prevActiveKinds.Clear();
        _scratchCurrentKinds.Clear();
        _prevPulseMetric.Clear();
        _lastIconRect.Clear();
        _statusAnimIntroSynced = false;
        _lastHudTooltipVtml = null;
    }

    private static Vec4f RgbaToVec4f(double[] rgba)
    {
        return new Vec4f(
            (float)rgba[0],
            (float)rgba[1],
            (float)rgba[2],
            (float)rgba[3]);
    }

    private void ComposeHud()
    {
        GuiComposer? previous = SingleComposer;

        float rawHud = IntoxicationResolve.GetRaw(capi.World.Player?.Entity, _layout);
        int displayPercent = DisplayPercent(rawHud);
        string text = displayPercent.ToString();

        ElementBounds dialogBounds = _layout.DialogBounds();
        ElementBounds innerBounds = _layout.InnerBounds();
        ElementBounds textBounds = _layout.TextBounds();

        _layout.EnsureDefaults();

        double[] fillRgba = IntoxicationPalette.FillRgba(rawHud);
        double[] strokeRgba = _layout.TextStrokeMatchFillHue
            ? UxColorMath.StrokeRgbaFromFillRgba(fillRgba)
            : _layout.TextStrokeColor;

        FontConfig fc = new FontConfig
        {
            UnscaledFontsize = _layout.FontSize,
            Fontname = GuiStyle.StandardFontName,
            FontWeight = FontWeight.Bold,
            Color = fillRgba,
            StrokeColor = strokeRgba,
            StrokeWidth = _layout.StrokeWidth
        };

        CairoFont font = new CairoFont(fc);

        GuiComposer composer = capi.Gui.CreateCompo("slowtoxvisualized-intoxhud", dialogBounds);

        composer.BeginChildElements(innerBounds);
        GuiComposerHelpers.AddStaticText(
            composer,
            text,
            font,
            EnumTextOrientation.Center,
            textBounds,
            "uxvalue");
        composer.EndChildElements();

        SingleComposer = composer.Compose();
        previous?.Dispose();
    }

    public override void OnGuiOpened()
    {
        base.OnGuiOpened();

        _beerTex ??= new LoadedTexture(capi);
        capi.Render.GetOrLoadTexture(BeerPath, ref _beerTex);

        TryLoadGearTexture();
        ReloadStatusIconTextures();
    }

    private void TryLoadGearTexture()
    {
        int g = _layout.GearDrawSize;
        int raster = Math.Max(64, g * 4);

        _gearSvg?.Dispose();
        _gearSvg = capi.Gui.LoadSvg(
            GearSvgPath,
            raster,
            raster,
            g,
            g,
            ColorUtil.WhiteArgb);

        if (_gearSvg == null || _gearSvg.TextureId == 0)
        {
            if (!_warnedGearMissing)
            {
                _warnedGearMissing = true;
                capi.Logger.Warning(
                    "[Slow Tox Visualized] Gear SVG did not load (path {0}). Check mod assets.",
                    GearSvgPath);
            }

            return;
        }

        _warnedGearMissing = false;
    }

    private static AssetLocation PngPathForEffectKind(SlowToxHudEffectKind kind)
    {
        return kind switch
        {
            SlowToxHudEffectKind.DamageReductionBuff => IconDurability,
            SlowToxHudEffectKind.HealthRegenBuff => IconHp,
            SlowToxHudEffectKind.TemporalRecoveryBuff => IconTemporalRecovery,
            SlowToxHudEffectKind.MeleeDamageBuff => IconMelee,
            SlowToxHudEffectKind.MiningSpeedBuff => IconMining,
            SlowToxHudEffectKind.SlowDebuff => IconSlow,
            SlowToxHudEffectKind.PoisonDebuff => IconPoison,
            _ => IconDurability
        };
    }

    private void ReloadStatusIconTextures()
    {
        for (int i = 0; i < _statusTextureIds.Length; i++)
        {
            var kind = (SlowToxHudEffectKind)i;
            int id = capi.Render.GetOrLoadTexture(PngPathForEffectKind(kind));
            _statusTextureIds[i] = id > 0 ? id : 0;
        }
    }

    private void RefreshActiveStatusEffects()
    {
        IPlayer? player = capi.World.Player;
        Entity? entity = player?.Entity;
        float intox = SlowToxEffectProbe.ResolveIntoxicationForLogic(entity, _layout);
        SlowToxEffectProbe.CollectActiveKinds(entity, capi, intox, _activeEffects);
    }

    private static bool IsStatusStripLeft(string? side)
    {
        return string.Equals(side?.Trim(), "Left", StringComparison.OrdinalIgnoreCase);
    }

    private void GetStatusStripBaseLayout(
        ElementBounds root,
        int activeCount,
        out int mug,
        out int sz,
        out int gap,
        out double stripLeft,
        out double yBase)
    {
        _layout.EnsureDefaults();
        mug = _layout.MugSize;
        sz = _layout.StatusIconSize > 0 ? _layout.StatusIconSize : mug;
        gap = _layout.StatusIconGapPx;
        if (IsStatusStripLeft(_layout.StatusStripSide) && activeCount > 0)
        {
            double span = activeCount * sz + Math.Max(0, activeCount - 1) * gap;
            stripLeft = root.renderX - _layout.StatusStripOffsetX - span;
        }
        else
        {
            double anchorW = StatusStripAnchorWidthPx(root, mug);
            stripLeft = root.renderX + anchorW + _layout.StatusStripOffsetX;
        }

        yBase = StatusStripOriginYPx(root, mug, sz);
    }

    private bool TryPickStatusIcon(
        int mouseX,
        int mouseY,
        ElementBounds root,
        out int pickedIndex)
    {
        pickedIndex = -1;
        if (_activeEffects.Count == 0)
        {
            return false;
        }

        GetStatusStripBaseLayout(root, _activeEffects.Count, out _, out int sz, out int gap, out double stripLeft, out double yBase);

        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            int idx = (int)_activeEffects[i];
            if (_statusTextureIds[idx] <= 0)
            {
                continue;
            }

            double x = stripLeft + i * (sz + gap);
            double yDraw = yBase - StatusIconsWaveVerticalOffsetPx(i);
            if (mouseX >= x && mouseX < x + sz && mouseY >= yDraw && mouseY < yDraw + sz)
            {
                pickedIndex = i;
                return true;
            }
        }

        return false;
    }

    private static bool TryPickJug(int mouseX, int mouseY, ElementBounds root, int mug)
    {
        return mouseX >= root.renderX && mouseX < root.renderX + mug
            && mouseY >= root.renderY && mouseY < root.renderY + mug;
    }

    private static bool ShouldTriggerPulseMetric(float prevM, float nowM)
    {
        float d = Math.Abs(nowM - prevM);
        if (d < 1e-8f)
        {
            return false;
        }

        float scale = Math.Max(Math.Max(Math.Abs(prevM), Math.Abs(nowM)), 1e-9f);
        float relative = d / scale;

        if (relative >= StatusPulseRelativeThreshold)
        {
            return true;
        }

        return d >= StatusPulseAbsoluteFloor;
    }

    private void EnsureStatusTooltipElements(ElementBounds root)
    {
        _layout.EnsureDefaults();
        _statusTooltipPanel ??= new StatusTooltipPanelBackend(capi);
        if (_statusTooltipRich != null)
        {
            return;
        }

        ElementBounds richBounds = ElementBounds.Fixed(0, 0, _layout.StatusTooltipMaxWidth, 320);
        richBounds.WithFixedPadding(0);
        richBounds.IsDrawingSurface = true;
        richBounds.WithParent(root);
        _statusTooltipRich = new GuiElementRichtext(
            capi,
            VtmlUtil.Richtextify(capi, " ", CairoFont.WhiteDetailText()),
            richBounds);
    }

    private void RenderHudTooltips(float deltaTime, ElementBounds root)
    {
        int mx = capi.Input.MouseX;
        int my = capi.Input.MouseY;
        _layout.EnsureDefaults();
        int mug = _layout.MugSize;

        if (TryPickJug(mx, my, root, mug))
        {
            string vtml = Lang.Get("slowtoxvisualized:tooltip-jug-fmt");
            const double jugGapBelowPx = 4;
            RenderRichTooltipAt(
                deltaTime,
                root,
                vtml,
                root.renderX,
                root.renderY + mug + jugGapBelowPx);
            return;
        }

        if (_activeEffects.Count == 0)
        {
            _lastHudTooltipVtml = null;
            return;
        }

        if (!TryPickStatusIcon(mx, my, root, out int pickedIndex))
        {
            _lastHudTooltipVtml = null;
            return;
        }

        Entity? entity = capi.World.Player?.Entity;
        if (entity == null)
        {
            _lastHudTooltipVtml = null;
            return;
        }

        SlowToxHudEffectKind kind = _activeEffects[pickedIndex];
        string statusVtml = SlowToxStatusTooltipContent.BuildVtml(kind, entity, capi, _layout);
        GetStatusStripBaseLayout(root, _activeEffects.Count, out _, out int sz, out int gap, out double stripLeft, out double yBase);
        double ix = stripLeft + pickedIndex * (sz + gap);
        double yDraw = yBase - StatusIconsWaveVerticalOffsetPx(pickedIndex);
        const double gapBelowPx = 4;
        RenderRichTooltipAt(deltaTime, root, statusVtml, ix, yDraw + sz + gapBelowPx);
    }

    private void RenderRichTooltipAt(
        float deltaTime,
        ElementBounds root,
        string vtml,
        double desiredLeftScreenX,
        double desiredTopScreenY)
    {
        EnsureStatusTooltipElements(root);
        if (_statusTooltipRich == null || _statusTooltipPanel == null)
        {
            return;
        }

        int maxW = _layout.StatusTooltipMaxWidth;
        bool textDirty = vtml != _lastHudTooltipVtml;
        if (textDirty)
        {
            ElementBounds composeBounds = ElementBounds.Fixed(0, 0, maxW, 320);
            composeBounds.WithFixedPadding(0);
            composeBounds.IsDrawingSurface = true;
            composeBounds.WithParent(root);
            _statusTooltipRich.Bounds = composeBounds;
            _statusTooltipRich.SetNewText(vtml, CairoFont.WhiteDetailText());
            _lastHudTooltipVtml = vtml;
        }

        LoadedTexture rtex = _statusTooltipRich.richtTextTexture;
        if (rtex.TextureId <= 0 || rtex.Width <= 0 || rtex.Height <= 0)
        {
            return;
        }

        int tw = rtex.Width;
        int th = rtex.Height;
        double pad = GuiElement.scaled(5);
        int bw = (int)Math.Ceiling(tw + 2 * pad);
        int bh = (int)Math.Ceiling(th + 2 * pad);

        double guiScale = Math.Max(1e-6, RuntimeEnv.GUIScale);
        double textUnscaledW = tw / guiScale;
        double textUnscaledH = th / guiScale;

        if (textDirty || _statusTooltipBgTex == null || _statusTooltipBgTex.TextureId <= 0)
        {
            _statusTooltipPanel.FillPanelToTexture(bw, bh, ref _statusTooltipBgTex);
        }

        if (_statusTooltipBgTex == null || _statusTooltipBgTex.TextureId <= 0)
        {
            return;
        }

        double screenX = desiredLeftScreenX;
        double screenY = desiredTopScreenY;
        double totalW = bw;
        double totalH = bh;
        if (screenX + totalW > capi.Render.FrameWidth)
        {
            screenX = capi.Render.FrameWidth - totalW;
        }

        if (screenY + totalH > capi.Render.FrameHeight)
        {
            screenY = capi.Render.FrameHeight - totalH;
        }

        if (screenX < 0)
        {
            screenX = 0;
        }

        if (screenY < 0)
        {
            screenY = 0;
        }

        float zBg = _layout.StatusTooltipZ - 1f;
        float zTxt = _layout.StatusTooltipZ;

        capi.Render.Render2DTexture(
            _statusTooltipBgTex.TextureId,
            (float)screenX,
            (float)screenY,
            bw,
            bh,
            zBg,
            WhiteTint);

        double relTextX = (screenX - root.renderX + pad) / guiScale;
        double relTextY = (screenY - root.renderY + pad) / guiScale;
        ElementBounds tb = ElementBounds.Fixed(relTextX, relTextY, textUnscaledW, textUnscaledH);
        tb.WithFixedPadding(0);
        tb.WithParent(root);
        tb.CalcWorldBounds();
        _statusTooltipRich.Bounds = tb;
        _statusTooltipRich.zPos = zTxt;
        _statusTooltipRich.RenderInteractiveElements(deltaTime);
    }

    private static float EaseOutBack(float t)
    {
        float c1 = 1.70158f;
        float c3 = c1 + 1f;
        return 1f + c3 * (float)Math.Pow(t - 1f, 3f) + c1 * (float)Math.Pow(t - 1f, 2f);
    }

    private static float SmoothStep(float t)
    {
        return t * t * (3f - 2f * t);
    }

    private static float ComputePopupScale(float popupElapsed)
    {
        float t = Math.Clamp(popupElapsed / StatusPopupDurationSec, 0f, 1f);
        return EaseOutBack(t);
    }

    private static float ComputePulseScale(float pulseElapsed)
    {
        float t = Math.Clamp(pulseElapsed / StatusPulseDurationSec, 0f, 1f);
        return 1f + StatusPulseAmplitude * (float)Math.Sin(Math.PI * t);
    }

    private float CombineKindScale(KindRuntimeAnim a)
    {
        float s = 1f;
        if (a.PopupElapsed < StatusPopupDurationSec)
        {
            s *= ComputePopupScale(a.PopupElapsed);
        }

        if (a.Pulsing && a.PulseElapsed < StatusPulseDurationSec)
        {
            s *= ComputePulseScale(a.PulseElapsed);
        }

        return s;
    }

    private bool KindPopupSettled(SlowToxHudEffectKind kind)
    {
        return !_kindAnim.TryGetValue(kind, out KindRuntimeAnim a) || a.PopupElapsed >= StatusPopupDurationSec;
    }

    private void DrawStatusIconScaled(
        int texId,
        double centerX,
        double centerY,
        int baselineSize,
        float scale,
        float alpha,
        float z)
    {
        double draw = baselineSize * scale;
        double half = draw * 0.5;
        Vec4f tint = alpha >= 0.999f ? WhiteTint : new Vec4f(1f, 1f, 1f, alpha);
        capi.Render.Render2DTexture(
            texId,
            (float)(centerX - half),
            (float)(centerY - half),
            (float)draw,
            (float)draw,
            z,
            tint);
    }

    private Dictionary<SlowToxHudEffectKind, SavedIconRect> CollectIconRects(
        double stripLeft,
        int sz,
        int gap,
        double yBase)
    {
        Dictionary<SlowToxHudEffectKind, SavedIconRect> rects = new();
        for (int i = 0; i < _activeEffects.Count; i++)
        {
            SlowToxHudEffectKind kind = _activeEffects[i];
            int idx = (int)kind;
            if (_statusTextureIds[idx] <= 0)
            {
                continue;
            }

            double x = stripLeft + i * (sz + gap);
            double yDraw = yBase - StatusIconsWaveVerticalOffsetPx(i);
            rects[kind] = new SavedIconRect { Left = x, Top = yDraw, Size = sz };
        }

        return rects;
    }

    private void FillScratchCurrentKinds()
    {
        _scratchCurrentKinds.Clear();
        foreach (SlowToxHudEffectKind k in _activeEffects)
        {
            _scratchCurrentKinds.Add(k);
        }
    }

    private void CommitAnimSnapshot(
        HashSet<SlowToxHudEffectKind> currentKinds,
        Dictionary<SlowToxHudEffectKind, SavedIconRect> rects,
        Entity? entity)
    {
        _prevActiveKinds.Clear();
        foreach (SlowToxHudEffectKind k in currentKinds)
        {
            _prevActiveKinds.Add(k);
        }

        _prevPulseMetric.Clear();
        if (entity != null)
        {
            foreach (SlowToxHudEffectKind kind in currentKinds)
            {
                _prevPulseMetric[kind] = SlowToxStatusTooltipContent.GetPulseMetric(kind, entity, capi, _layout);
            }
        }

        _lastIconRect.Clear();
        foreach (KeyValuePair<SlowToxHudEffectKind, SavedIconRect> kv in rects)
        {
            _lastIconRect[kv.Key] = kv.Value;
        }
    }

    private void UpdateStatusIconAnimations(float deltaTime, ElementBounds root)
    {
        FillScratchCurrentKinds();
        GetStatusStripBaseLayout(root, _activeEffects.Count, out _, out int sz, out int gap, out double stripLeft, out double yBase);
        Dictionary<SlowToxHudEffectKind, SavedIconRect> rects = CollectIconRects(stripLeft, sz, gap, yBase);
        Entity? entity = capi.World.Player?.Entity;

        if (!_layout.StatusIconAnimEnabled)
        {
            _popoutAnims.Clear();
            _kindAnim.Clear();
            CommitAnimSnapshot(_scratchCurrentKinds, rects, entity);
            _statusAnimIntroSynced = true;
            return;
        }

        for (int i = _popoutAnims.Count - 1; i >= 0; i--)
        {
            PopoutAnim p = _popoutAnims[i];
            p.Elapsed += deltaTime;
            if (p.Elapsed >= StatusPopoutDurationSec)
            {
                _popoutAnims.RemoveAt(i);
            }
        }

        if (!_statusAnimIntroSynced)
        {
            CommitAnimSnapshot(_scratchCurrentKinds, rects, entity);
            _statusAnimIntroSynced = true;
            return;
        }

        foreach (SlowToxHudEffectKind kind in _prevActiveKinds)
        {
            if (_scratchCurrentKinds.Contains(kind))
            {
                continue;
            }

            _kindAnim.Remove(kind);
            if (_lastIconRect.TryGetValue(kind, out SavedIconRect r))
            {
                _popoutAnims.Add(new PopoutAnim
                {
                    Kind = kind,
                    Elapsed = 0f,
                    CenterX = r.Left + r.Size * 0.5,
                    CenterY = r.Top + r.Size * 0.5,
                    BaselineSize = r.Size
                });
            }
        }

        foreach (SlowToxHudEffectKind kind in _scratchCurrentKinds)
        {
            if (!_prevActiveKinds.Contains(kind))
            {
                _kindAnim[kind] = default;
            }
        }

        if (entity != null)
        {
            foreach (SlowToxHudEffectKind kind in _scratchCurrentKinds)
            {
                if (!_prevActiveKinds.Contains(kind))
                {
                    continue;
                }

                if (!_prevPulseMetric.TryGetValue(kind, out float prevM))
                {
                    continue;
                }

                float nowM = SlowToxStatusTooltipContent.GetPulseMetric(kind, entity, capi, _layout);
                if (ShouldTriggerPulseMetric(prevM, nowM) && KindPopupSettled(kind))
                {
                    KindRuntimeAnim a = _kindAnim.TryGetValue(kind, out KindRuntimeAnim x) ? x : default;
                    a.Pulsing = true;
                    a.PulseElapsed = 0f;
                    _kindAnim[kind] = a;
                }
            }
        }

        foreach (SlowToxHudEffectKind kind in _scratchCurrentKinds)
        {
            KindRuntimeAnim a = _kindAnim.TryGetValue(kind, out KindRuntimeAnim x) ? x : default;
            if (a.PopupElapsed < StatusPopupDurationSec)
            {
                a.PopupElapsed += deltaTime;
            }

            if (a.Pulsing)
            {
                a.PulseElapsed += deltaTime;
                if (a.PulseElapsed >= StatusPulseDurationSec)
                {
                    a.Pulsing = false;
                    a.PulseElapsed = 0f;
                }
            }

            _kindAnim[kind] = a;
        }

        CommitAnimSnapshot(_scratchCurrentKinds, rects, entity);
    }

    private void RenderActiveEffectIcons(ElementBounds root)
    {
        if (_activeEffects.Count == 0 && _popoutAnims.Count == 0)
        {
            return;
        }

        _layout.EnsureDefaults();

        GetStatusStripBaseLayout(root, _activeEffects.Count, out _, out int sz, out int gap, out double stripLeft, out double yBase);
        float z = _layout.ZStatusIcons;

        if (!_layout.StatusIconAnimEnabled)
        {
            for (int i = 0; i < _activeEffects.Count; i++)
            {
                SlowToxHudEffectKind kind = _activeEffects[i];
                int idx = (int)kind;
                int texId = _statusTextureIds[idx];
                if (texId <= 0)
                {
                    continue;
                }

                double x = stripLeft + i * (sz + gap);
                double yDraw = yBase - StatusIconsWaveVerticalOffsetPx(i);
                capi.Render.Render2DTexture(
                    texId,
                    (float)x,
                    (float)yDraw,
                    sz,
                    sz,
                    z,
                    WhiteTint);
            }

            return;
        }

        for (int i = 0; i < _activeEffects.Count; i++)
        {
            SlowToxHudEffectKind kind = _activeEffects[i];
            int idx = (int)kind;
            int texId = _statusTextureIds[idx];
            if (texId <= 0)
            {
                continue;
            }

            double x = stripLeft + i * (sz + gap);
            double yDraw = yBase - StatusIconsWaveVerticalOffsetPx(i);
            double cx = x + sz * 0.5;
            double cy = yDraw + sz * 0.5;
            _ = _kindAnim.TryGetValue(kind, out KindRuntimeAnim anim);
            float scale = CombineKindScale(anim);
            DrawStatusIconScaled(texId, cx, cy, sz, scale, 1f, z);
        }

        float zPop = z + 2f;
        for (int i = 0; i < _popoutAnims.Count; i++)
        {
            PopoutAnim p = _popoutAnims[i];
            int idx = (int)p.Kind;
            int texId = _statusTextureIds[idx];
            if (texId <= 0)
            {
                continue;
            }

            float t = Math.Clamp(p.Elapsed / StatusPopoutDurationSec, 0f, 1f);
            float ease = SmoothStep(t);
            float scale = 1f - ease;
            float alpha = 1f - ease;
            if (scale <= 0.01f || alpha <= 0.01f)
            {
                continue;
            }

            DrawStatusIconScaled(texId, p.CenterX, p.CenterY, p.BaselineSize, scale, alpha, zPop);
        }
    }

    private double StatusIconsWaveVerticalOffsetPx(int visibleIndex)
    {
        if (!_layout.StatusIconsWaveEnabled)
        {
            return 0;
        }

        double amp = _layout.StatusIconsWaveAmplitudePx;
        double period = _layout.StatusIconsWavePeriodSec;
        if (amp <= 1e-9 || period <= 1e-9)
        {
            return 0;
        }

        period = Math.Max(period, 1e-6);
        double stagger = _layout.StatusIconsWaveStaggerSec;
        double t = _statusIconsWaveTimeSec - visibleIndex * stagger;
        double phase = 2.0 * Math.PI * t / period;
        return amp * Math.Sin(phase);
    }

    private double StatusStripAnchorWidthPx(ElementBounds root, int mug)
    {
        double outer = root.OuterWidth;
        return _layout.StatusStripAnchorMode.Trim() switch
        {
            "Mug" => mug,
            "Dialog" => outer,
            _ => Math.Max(mug, outer)
        };
    }

    private double StatusStripOriginYPx(ElementBounds root, int mug, int iconHeight)
    {
        double y = root.renderY;
        switch (_layout.StatusStripVerticalAlign.Trim().ToLowerInvariant())
        {
            case "center":
                y += (mug - iconHeight) / 2.0;
                break;
            case "bottom":
                y += mug - iconHeight;
                break;
        }

        return y + _layout.StatusStripOffsetY;
    }

    public override void OnRenderGUI(float deltaTime)
    {
        _layout.EnsureDefaults();

        Entity? hudEntity = capi.World.Player?.Entity;
        float rawForHud = IntoxicationResolve.GetRaw(hudEntity, _layout);
        int displayPct = DisplayPercent(rawForHud);

        if (!ShouldRenderIntoxicationHud(displayPct))
        {
            if (_intoxHudWasVisible)
            {
                ResetIntoxHudTransientState();
                _intoxHudWasVisible = false;
            }

            _lastComposedDisplayPercent = displayPct;
            return;
        }

        _intoxHudWasVisible = true;

        if (displayPct != _lastComposedDisplayPercent)
        {
            _lastComposedDisplayPercent = displayPct;
            ComposeHud();
        }

        ElementBounds root = SingleComposer.Bounds;

        int mug = _layout.MugSize;
        float zBeer = _layout.ZBeer;

        if (_beerTex != null && _beerTex.TextureId != 0)
        {
            capi.Render.Render2DTexture(
                _beerTex.TextureId,
                (float)root.renderX,
                (float)root.renderY,
                mug,
                mug,
                zBeer,
                WhiteTint);
        }

        base.OnRenderGUI(deltaTime);

        _statusIconsWaveTimeSec += deltaTime;

        if (_gearSvg != null && _gearSvg.TextureId != 0)
        {
            float rawHud = IntoxicationResolve.GetRaw(capi.World.Player?.Entity, _layout);
            Vec4f fillTint = RgbaToVec4f(IntoxicationPalette.FillRgba(rawHud));
            Vec4f strokeTint = RgbaToVec4f(_layout.GearStrokeColor);

            int gearDraw = _layout.GearDrawSize;
            double inset = _layout.GearCornerInsetFactor;

            double cx = root.renderX + mug - gearDraw * inset + _layout.GearOffsetX;
            double cy = root.renderY + mug - gearDraw * inset + _layout.GearOffsetY;

            _gearRotationDeg += deltaTime * _layout.GearRotateDegPerSec;

            float rot = _gearRotationDeg;
            float tcx = (float)cx;
            float tcy = (float)cy;
            float strokePx = (float)_layout.GearStrokeWidth;
            float outer = gearDraw + 2f * strokePx;
            float zStroke = _layout.ZGear - 1f;
            float zFill = _layout.ZGear;
            int texId = _gearSvg.TextureId;

            IRenderAPI r = capi.Render;

            void DrawGearQuad(float size, float z, Vec4f tint)
            {
                r.GlPushMatrix();
                r.GlTranslate(tcx, tcy, 0f);
                r.GlRotate(rot, 0f, 0f, 1f);
                r.GlTranslate(-size / 2f, -size / 2f, 0f);
                r.Render2DTexture(texId, 0f, 0f, size, size, z, tint);
                r.GlPopMatrix();
            }

            if (strokePx > 0f)
            {
                DrawGearQuad(outer, zStroke, strokeTint);
            }

            DrawGearQuad(gearDraw, zFill, fillTint);
        }

        RefreshActiveStatusEffects();
        UpdateStatusIconAnimations(deltaTime, root);
        RenderActiveEffectIcons(root);
        RenderHudTooltips(deltaTime, root);
    }

    public override double DrawOrder => 0.22;

    public override EnumDialogType DialogType => EnumDialogType.HUD;

    public override bool PrefersUngrabbedMouse => true;

    public override void Dispose()
    {
        _statusTooltipRich?.Dispose();
        _statusTooltipBgTex?.Dispose();
        _gearSvg?.Dispose();
        base.Dispose();
    }
}
