using System;
using Vintagestory.API.Client;
using Vintagestory.API.Config;

namespace PlayerStatusStrip;

public sealed class StripLayoutWizardDialog : GuiDialog
{
    public bool SuppressOnboardingWhenClosed { get; set; } = true;

    public event Action? LayoutWizardClosed;

    private static readonly int[] IconPresetPx = { 32, 42, 64, 78 };

    private static readonly int[] GapPresetPx = { 2, 4, 8, 12, 16 };

    private readonly StatusStripHudElement _hud;
    private readonly IStatusStripHudApi _hudApi;
    private readonly StripLayoutWizardPreviewProvider _previewProvider = new();
    private readonly string[] _areaCodes;
    private readonly string[] _areaDisplay;
    private readonly string[] _insetCodes;
    private readonly string[] _insetDisplay;
    private readonly string[] _iconCodes;
    private readonly string[] _iconDisplay;
    private readonly string[] _gapCodes;
    private readonly string[] _gapDisplay;
    private int _areaIndex;
    private int _insetPresetIndex;
    private int _iconPresetIndex;
    private int _gapPresetIndex;
    private bool _layoutAppliedFromWizard;

    public StripLayoutWizardDialog(ICoreClientAPI capi, StatusStripHudElement hud, IStatusStripHudApi hudApi)
        : base(capi)
    {
        _hud = hud;
        _hudApi = hudApi;
        _hudApi.RegisterProvider(_previewProvider);
        _hudApi.SetPreviewExclusiveProvider(_previewProvider);
        StatusStripLayoutConfig baseline = StatusStripLayoutConfig.Reload(capi);
        baseline.EnsureDefaults();
        _areaCodes = new[]
        {
            "RightTop", "RightBottom", "LeftTop", "LeftBottom", "CenterTop",
            "RightMiddle", "LeftMiddle"
        };
        _areaDisplay = new string[_areaCodes.Length];
        for (int i = 0; i < _areaCodes.Length; i++)
        {
            string langKey = "playerstatusstrip:wizard-area-" + _areaCodes[i];
            string label = Lang.Get(langKey);
            _areaDisplay[i] = string.Equals(label, langKey, StringComparison.Ordinal) ? _areaCodes[i] : label;
        }

        _insetCodes = new[] { "tight", "standard", "relaxed", "generous" };
        _insetDisplay = new string[_insetCodes.Length];
        for (int i = 0; i < _insetCodes.Length; i++)
        {
            string langKey = "playerstatusstrip:wizard-preset-inset-" + _insetCodes[i];
            string label = Lang.Get(langKey);
            _insetDisplay[i] = string.Equals(label, langKey, StringComparison.Ordinal) ? _insetCodes[i] : label;
        }

        _iconCodes = new[] { "32", "42", "64", "78" };
        _iconDisplay = new string[_iconCodes.Length];
        for (int i = 0; i < _iconCodes.Length; i++)
        {
            string langKey = "playerstatusstrip:wizard-preset-size-" + _iconCodes[i];
            string label = Lang.Get(langKey);
            _iconDisplay[i] = string.Equals(label, langKey, StringComparison.Ordinal)
                ? _iconCodes[i] + " px"
                : label;
        }

        _gapCodes = new[] { "2", "4", "8", "12", "16" };
        _gapDisplay = new string[_gapCodes.Length];
        for (int i = 0; i < _gapCodes.Length; i++)
        {
            string langKey = "playerstatusstrip:wizard-preset-gap-" + _gapCodes[i];
            string label = Lang.Get(langKey);
            _gapDisplay[i] = string.Equals(label, langKey, StringComparison.Ordinal) ? _gapCodes[i] + " px" : label;
        }

        _areaIndex = IndexOfArea(baseline.DialogArea);
        _insetPresetIndex = StripLayoutInsetPresets.NearestStepIndex(
            baseline.DialogArea,
            baseline.DialogOffsetX,
            baseline.DialogOffsetY);
        _iconPresetIndex = NearestIntPresetIndex(
            baseline.StatusIconSize <= 0 ? 42 : baseline.StatusIconSize,
            IconPresetPx);
        _gapPresetIndex = NearestIntPresetIndex(Math.Max(0, baseline.StatusIconGapPx), GapPresetPx);

        SetupDialog();
        PushLayoutPreview();
    }

    private int IndexOfArea(string? area)
    {
        if (string.IsNullOrWhiteSpace(area))
        {
            return 0;
        }

        string code = area.Trim();
        if (string.Equals(code, "CenterBottom", StringComparison.OrdinalIgnoreCase))
        {
            code = "CenterTop";
        }

        int idx = Array.IndexOf(_areaCodes, code);
        return idx >= 0 ? idx : 0;
    }

    private static int NearestIntPresetIndex(int value, int[] presets)
    {
        int best = 0;
        int bestDist = int.MaxValue;
        for (int i = 0; i < presets.Length; i++)
        {
            int d = Math.Abs(value - presets[i]);
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }

        return best;
    }

    public override string? ToggleKeyCombinationCode => null;

    private void SetupDialog()
    {
        const double dialogW = 390;
        const double secTitleH = 22;
        const double sectionTitleBottomPad = 6;
        const double beforeLayoutPad = 16;
        const double btnRowH = 34;
        const double bottomPad = 20;
        const double rowH = 28;
        const double rowGap = 6;
        const double sectionGap = 10;
        const double buttonGap = 8;
        const double footerTopPad = 36;
        const double footerBottomPad = 10;
        const double labelW = 128;
        const double labelValueGap = 8;

        double titleH = GuiStyle.TitleBarHeight;
        double pad = GuiStyle.ElementToDialogPadding;
        double textWidth = dialogW - 2 * pad;
        string introText = Lang.Get("playerstatusstrip:wizard-intro");
        string footerHintText = Lang.Get("playerstatusstrip:wizard-footer-hint");
        double introH = EstimateAutoTextHeight(introText, 18, 10, 118);
        double footerH = EstimateAutoTextHeight(footerHintText, 18, 10, 96);

        LayoutFlow flow = new(pad, titleH + 8, textWidth);
        HeaderSectionLayout header = LayoutHeaderSection(ref flow, introH, sectionGap + beforeLayoutPad);
        FormSectionLayout form = LayoutFormSection(
            ref flow,
            secTitleH,
            sectionTitleBottomPad,
            labelW,
            labelValueGap,
            rowH,
            rowGap,
            sectionGap);
        TipSectionLayout tip = LayoutTipSection(ref flow, footerH, footerTopPad, footerBottomPad);
        ActionsSectionLayout actions = LayoutActionsSection(ref flow, btnRowH, buttonGap);
        double dialogH = flow.Y + bottomPad;

        ElementBounds dialogBounds = ElementBounds.Fixed(EnumDialogArea.CenterMiddle, 0, 0, dialogW, dialogH);
        ElementBounds bgBounds = ElementBounds.Fill;
        CairoFont labelFont = CairoFont.WhiteSmallText();
        CairoFont sectionFont = CairoFont.WhiteDetailText();
        CairoFont introFooterFont = CairoFont.WhiteSmallText();
        CairoFont footerHintFont = CairoFont.WhiteSmallText()
            .WithColor(new double[] { 1.0, 0.82, 0.38, 1.0 });

        SingleComposer = capi.Gui
            .CreateCompo("playerstatusstrip-layout-wizard", dialogBounds)
            .AddShadedDialogBG(bgBounds)
            .AddDialogTitleBar(Lang.Get("playerstatusstrip:wizard-title"), OnTitleClose)
            .AddStaticTextAutoBoxSize(introText, introFooterFont, EnumTextOrientation.Left, header.Intro)
            .AddStaticText(Lang.Get("playerstatusstrip:wizard-section-layout"), sectionFont, form.LayoutTitle)
            .AddStaticText(Lang.Get("playerstatusstrip:wizard-label-corner"), labelFont, form.CornerLabel)
            .AddDropDown(_areaCodes, _areaDisplay, _areaIndex, OnAreaCodeSelected, form.CornerValue, CairoFont.WhiteSmallText(), "areaDd")
            .AddStaticText(Lang.Get("playerstatusstrip:wizard-label-inset"), labelFont, form.InsetLabel)
            .AddDropDown(_insetCodes, _insetDisplay, _insetPresetIndex, OnInsetPresetSelected, form.InsetValue, CairoFont.WhiteSmallText(), "insetDd")
            .AddStaticText(Lang.Get("playerstatusstrip:wizard-section-appearance"), sectionFont, form.AppearanceTitle)
            .AddStaticText(Lang.Get("playerstatusstrip:wizard-label-icon-preset"), labelFont, form.IconLabel)
            .AddDropDown(_iconCodes, _iconDisplay, _iconPresetIndex, OnIconPresetSelected, form.IconValue, CairoFont.WhiteSmallText(), "iconDd")
            .AddStaticText(Lang.Get("playerstatusstrip:wizard-label-gap-preset"), labelFont, form.GapLabel)
            .AddDropDown(_gapCodes, _gapDisplay, _gapPresetIndex, OnGapPresetSelected, form.GapValue, CairoFont.WhiteSmallText(), "gapDd")
            .AddStaticTextAutoBoxSize(footerHintText, footerHintFont, EnumTextOrientation.Left, tip.Tip)
            .AddButton(
                Lang.Get("playerstatusstrip:wizard-apply"),
                OnApplyClicked,
                actions.Apply,
                CairoFont.WhiteSmallText().WithOrientation(EnumTextOrientation.Center),
                EnumButtonStyle.Normal,
                "btnApply")
            .AddButton(
                Lang.Get("playerstatusstrip:wizard-not-now"),
                OnSkipClicked,
                actions.Skip,
                CairoFont.WhiteSmallText().WithOrientation(EnumTextOrientation.Center),
                EnumButtonStyle.Small,
                "btnSkip")
            .Compose();
    }

    private static HeaderSectionLayout LayoutHeaderSection(ref LayoutFlow flow, double introHeight, double sectionGapAfter)
    {
        ElementBounds intro = flow.Place(introHeight);
        flow.Gap(sectionGapAfter);
        return new HeaderSectionLayout(intro);
    }

    private static FormSectionLayout LayoutFormSection(
        ref LayoutFlow flow,
        double sectionTitleHeight,
        double sectionTitleBottomPad,
        double labelWidth,
        double labelValueGap,
        double rowHeight,
        double rowGap,
        double sectionGap)
    {
        ElementBounds layoutTitle = flow.Place(sectionTitleHeight);
        flow.Gap(sectionTitleBottomPad);
        (ElementBounds cornerLabel, ElementBounds cornerValue) = PlaceLabeledRow(ref flow, labelWidth, labelValueGap, rowHeight);
        flow.Gap(rowGap);
        (ElementBounds insetLabel, ElementBounds insetValue) = PlaceLabeledRow(ref flow, labelWidth, labelValueGap, rowHeight);
        flow.Gap(sectionGap);

        ElementBounds appearanceTitle = flow.Place(sectionTitleHeight);
        flow.Gap(sectionTitleBottomPad);
        (ElementBounds iconLabel, ElementBounds iconValue) = PlaceLabeledRow(ref flow, labelWidth, labelValueGap, rowHeight);
        flow.Gap(rowGap);
        (ElementBounds gapLabel, ElementBounds gapValue) = PlaceLabeledRow(ref flow, labelWidth, labelValueGap, rowHeight);

        return new FormSectionLayout(
            layoutTitle,
            cornerLabel,
            cornerValue,
            insetLabel,
            insetValue,
            appearanceTitle,
            iconLabel,
            iconValue,
            gapLabel,
            gapValue);
    }

    private static TipSectionLayout LayoutTipSection(ref LayoutFlow flow, double tipHeight, double topPad, double bottomPad)
    {
        flow.Gap(topPad);
        ElementBounds tip = flow.Place(tipHeight);
        flow.Gap(bottomPad);
        return new TipSectionLayout(tip);
    }

    private static ActionsSectionLayout LayoutActionsSection(ref LayoutFlow flow, double rowHeight, double buttonGap)
    {
        double buttonWidth = (flow.Width - buttonGap) / 2;
        ElementBounds apply = ElementBounds.Fixed(flow.X, flow.Y, buttonWidth, rowHeight);
        ElementBounds skip = ElementBounds.Fixed(flow.X + buttonWidth + buttonGap, flow.Y, buttonWidth, rowHeight);
        flow.Gap(rowHeight);
        return new ActionsSectionLayout(apply, skip);
    }

    private static (ElementBounds label, ElementBounds value) PlaceLabeledRow(
        ref LayoutFlow flow,
        double labelWidth,
        double spacing,
        double rowHeight)
    {
        double valueWidth = flow.Width - labelWidth - spacing;
        ElementBounds label = ElementBounds.Fixed(flow.X, flow.Y, labelWidth, rowHeight);
        ElementBounds value = ElementBounds.Fixed(flow.X + labelWidth + spacing, flow.Y, valueWidth, rowHeight);
        flow.Gap(rowHeight);
        return (label, value);
    }

    private static double EstimateAutoTextHeight(string text, double lineHeight, double verticalPadding, double minHeight)
    {
        int lines = 1;
        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                lines++;
            }
        }

        return Math.Max(minHeight, lines * lineHeight + verticalPadding);
    }

    private struct LayoutFlow
    {
        internal LayoutFlow(double x, double y, double width)
        {
            X = x;
            Y = y;
            Width = width;
        }

        internal readonly double X;
        internal readonly double Width;
        internal double Y;

        internal ElementBounds Place(double height)
        {
            ElementBounds bounds = ElementBounds.Fixed(X, Y, Width, height);
            Y += height;
            return bounds;
        }

        internal void Gap(double value)
        {
            Y += value;
        }
    }

    private readonly struct HeaderSectionLayout
    {
        internal HeaderSectionLayout(ElementBounds intro)
        {
            Intro = intro;
        }

        internal readonly ElementBounds Intro;
    }

    private readonly struct FormSectionLayout
    {
        internal FormSectionLayout(
            ElementBounds layoutTitle,
            ElementBounds cornerLabel,
            ElementBounds cornerValue,
            ElementBounds insetLabel,
            ElementBounds insetValue,
            ElementBounds appearanceTitle,
            ElementBounds iconLabel,
            ElementBounds iconValue,
            ElementBounds gapLabel,
            ElementBounds gapValue)
        {
            LayoutTitle = layoutTitle;
            CornerLabel = cornerLabel;
            CornerValue = cornerValue;
            InsetLabel = insetLabel;
            InsetValue = insetValue;
            AppearanceTitle = appearanceTitle;
            IconLabel = iconLabel;
            IconValue = iconValue;
            GapLabel = gapLabel;
            GapValue = gapValue;
        }

        internal readonly ElementBounds LayoutTitle;
        internal readonly ElementBounds CornerLabel;
        internal readonly ElementBounds CornerValue;
        internal readonly ElementBounds InsetLabel;
        internal readonly ElementBounds InsetValue;
        internal readonly ElementBounds AppearanceTitle;
        internal readonly ElementBounds IconLabel;
        internal readonly ElementBounds IconValue;
        internal readonly ElementBounds GapLabel;
        internal readonly ElementBounds GapValue;
    }

    private readonly struct TipSectionLayout
    {
        internal TipSectionLayout(ElementBounds tip)
        {
            Tip = tip;
        }

        internal readonly ElementBounds Tip;
    }

    private readonly struct ActionsSectionLayout
    {
        internal ActionsSectionLayout(ElementBounds apply, ElementBounds skip)
        {
            Apply = apply;
            Skip = skip;
        }

        internal readonly ElementBounds Apply;
        internal readonly ElementBounds Skip;
    }

    private void OnAreaCodeSelected(string code, bool selected)
    {
        if (!selected)
        {
            return;
        }

        int idx = Array.IndexOf(_areaCodes, code);
        if (idx >= 0)
        {
            _areaIndex = idx;
        }

        PushLayoutPreview();
    }

    private void OnInsetPresetSelected(string code, bool selected)
    {
        if (!selected)
        {
            return;
        }

        int idx = Array.IndexOf(_insetCodes, code);
        if (idx >= 0)
        {
            _insetPresetIndex = idx;
        }

        PushLayoutPreview();
    }

    private void OnIconPresetSelected(string code, bool selected)
    {
        if (!selected)
        {
            return;
        }

        int idx = Array.IndexOf(_iconCodes, code);
        if (idx >= 0)
        {
            _iconPresetIndex = idx;
        }

        PushLayoutPreview();
    }

    private void OnGapPresetSelected(string code, bool selected)
    {
        if (!selected)
        {
            return;
        }

        int idx = Array.IndexOf(_gapCodes, code);
        if (idx >= 0)
        {
            _gapPresetIndex = idx;
        }

        PushLayoutPreview();
    }

    private void PushLayoutPreview()
    {
        if (SingleComposer == null)
        {
            return;
        }

        if (!TryBuildConfig(out StatusStripLayoutConfig cfg, out _))
        {
            return;
        }

        _hud.ApplyLayoutPreview(cfg);
    }

    private void OnTitleClose()
    {
        TryClose();
    }

    private bool OnSkipClicked()
    {
        TryClose();
        return true;
    }

    private bool OnApplyClicked()
    {
        if (!TryBuildConfig(out StatusStripLayoutConfig cfg, out string error))
        {
            capi.Logger.Notification("[Player Status HUD] Layout wizard: " + error);
            return true;
        }

        cfg.EnsureDefaults();
        capi.StoreModConfig(cfg, StatusStripLayoutConfig.LayoutConfigFileName);
        _layoutAppliedFromWizard = true;
        _hud.ReloadLayoutFromDisk(showLayoutSummaryChat: false);
        capi.Logger.Notification(Lang.Get("playerstatusstrip:wizard-applied"));
        TryClose();
        return true;
    }

    private bool TryBuildConfig(out StatusStripLayoutConfig cfg, out string error)
    {
        cfg = StatusStripLayoutConfig.Reload(capi);
        cfg.EnsureDefaults();
        error = "";
        try
        {
            cfg.DialogArea = _areaCodes[Math.Clamp(_areaIndex, 0, _areaCodes.Length - 1)];
            (double ix, double iy) = StripLayoutInsetPresets.OffsetsForArea(cfg.DialogArea, _insetPresetIndex);
            cfg.DialogOffsetX = ix;
            cfg.DialogOffsetY = iy;
            cfg.StatusIconSize = IconPresetPx[Math.Clamp(_iconPresetIndex, 0, IconPresetPx.Length - 1)];
            cfg.StatusIconGapPx = GapPresetPx[Math.Clamp(_gapPresetIndex, 0, GapPresetPx.Length - 1)];
            cfg.StatusStripSide = StripLayoutWizardStripSide.ForDialogArea(cfg.DialogArea);
            return true;
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return false;
        }
    }

    public override bool TryClose()
    {
        if (IsOpened() && SuppressOnboardingWhenClosed)
        {
            StatusStripOnboardingConfig ob = StatusStripOnboardingConfig.LoadOrCreate(capi);
            ob.SuppressAutoLayoutWizard = true;
            ob.WizardDismissedForModVersion = PlayerStatusStripModVersion.Current(capi);
            StatusStripOnboardingConfig.Save(capi, ob);
        }

        bool ok = base.TryClose();
        if (ok)
        {
            _hudApi.SetPreviewExclusiveProvider(null);
            _hudApi.UnregisterProvider(_previewProvider);
            if (!_layoutAppliedFromWizard)
            {
                _hud.ReloadLayoutFromDisk(showLayoutSummaryChat: false);
            }

            LayoutWizardClosed?.Invoke();
        }

        return ok;
    }
}
