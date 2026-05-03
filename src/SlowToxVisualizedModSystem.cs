using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SlowToxVisualized;

public class SlowToxVisualizedModSystem : ModSystem
{
    private ICoreClientAPI? _clientApi;
    private Action? _onLevelFinalize;
    private SlowToxIntoxicationHud? _intoxicationHud;

    public override void StartPre(ICoreAPI api)
    {
        base.StartPre(api);
        api.Logger.Notification("[Slow Tox Visualized] Mod loaded.");
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        _clientApi = api;
        _intoxicationHud = new SlowToxIntoxicationHud(api);

        api.Input.RegisterHotKeyFirst(
            "slowtoxvisualized_reloadhudlayout",
            $"Slow Tox Visualized: reload HUD layout ({HudLayoutConfig.LayoutConfigFileName})",
            GlKeys.F9,
            HotkeyType.HelpAndOverlays,
            false,
            false,
            false);
        api.Input.SetHotKeyHandler("slowtoxvisualized_reloadhudlayout", _ =>
        {
            _intoxicationHud?.ReloadLayoutFromDisk();
            api.Logger.Notification("[Slow Tox Visualized] HUD layout reload hotkey handled.");
            return true;
        });

        _onLevelFinalize = () =>
        {
            if (_intoxicationHud == null)
            {
                return;
            }

            if (_intoxicationHud.TryOpen() != true)
            {
                api.Logger.Warning("[Slow Tox Visualized] Intoxication HUD TryOpen failed after level finalize.");
                return;
            }

            api.Logger.Notification("[Slow Tox Visualized] Intoxication HUD TryOpen ok.");
            api.Event.RegisterCallback(_ =>
            {
                ElementBounds? b = _intoxicationHud?.SingleComposer?.Bounds;
                if (b != null)
                {
                    api.Logger.Notification(
                        $"[Slow Tox Visualized] HUD bounds render=({b.renderX:F0},{b.renderY:F0}) outer={b.OuterWidthInt}x{b.OuterHeightInt}");
                }
            }, 300);
        };
        api.Event.LevelFinalize += _onLevelFinalize;
    }

    public override void Dispose()
    {
        if (_clientApi != null && _onLevelFinalize != null)
        {
            _clientApi.Event.LevelFinalize -= _onLevelFinalize;
        }

        _onLevelFinalize = null;
        _clientApi = null;

        _intoxicationHud?.TryClose();
        _intoxicationHud?.Dispose();
        _intoxicationHud = null;
        base.Dispose();
    }
}
