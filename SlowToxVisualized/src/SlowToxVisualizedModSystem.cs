using System;
using PlayerStatusStrip;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace SlowToxVisualized;

public class SlowToxVisualizedModSystem : ModSystem
{
    private ICoreClientAPI? _clientApi;
    private SlowToxStatusStripProvider? _statusProvider;
    private IStatusStripHudApi? _statusApi;
    private Action? _onLeftWorldSlowTox;
    private Action? _onLevelFinalizeSlowTox;

    public override void StartPre(ICoreAPI api)
    {
        base.StartPre(api);
        api.Logger.Notification("[SlowTox Visualized] Mod loaded.");
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        base.StartClientSide(api);
        _clientApi = api;

        _onLeftWorldSlowTox = () => UnregisterStatusProvider();
        api.Event.LeftWorld += _onLeftWorldSlowTox;

        _onLevelFinalizeSlowTox = () => TryRegisterStatusProvider(api);
        api.Event.LevelFinalize += _onLevelFinalizeSlowTox;

        if (!TryRegisterStatusProvider(api))
        {
            return;
        }

        api.Logger.Notification("[SlowTox Visualized] Registered status provider in Player Status HUD (legacy standalone HUD renderer disabled).");

        const string reloadLayoutHotkey = "slowtoxvisualized_reloadlayout";
        if (!api.Input.HotKeys.ContainsKey(reloadLayoutHotkey))
        {
            api.Input.RegisterHotKeyFirst(
                reloadLayoutHotkey,
                $"SlowTox Visualized: reload status data layout ({HudLayoutConfig.LayoutConfigFileName})",
                GlKeys.F9,
                HotkeyType.HelpAndOverlays,
                false,
                false,
                false);
        }

        api.Input.SetHotKeyHandler(reloadLayoutHotkey, _ =>
        {
            _statusProvider?.ReloadLayout();
            api.Logger.Notification("[SlowTox Visualized] Data layout reload hotkey handled.");
            return true;
        });
    }

    private bool TryRegisterStatusProvider(ICoreClientAPI api)
    {
        if (_statusProvider != null)
        {
            return true;
        }

        PlayerStatusStripModSystem? stripSystem = api.ModLoader.GetModSystem<PlayerStatusStripModSystem>();
        _statusApi = stripSystem?.StatusApi;
        if (_statusApi == null)
        {
            api.Logger.Error("[SlowTox Visualized] Player Status HUD API is unavailable. Ensure playerstatusstrip is installed and enabled.");
            return false;
        }

        _statusProvider = new SlowToxStatusStripProvider(api);
        _statusApi.RegisterProvider(_statusProvider);
        return true;
    }

    private void UnregisterStatusProvider()
    {
        if (_statusApi != null && _statusProvider != null)
        {
            _statusApi.UnregisterProvider(_statusProvider);
        }

        _statusProvider = null;
        _statusApi = null;
    }

    public override void Dispose()
    {
        UnregisterStatusProvider();

        if (_clientApi != null)
        {
            if (_onLeftWorldSlowTox != null)
            {
                _clientApi.Event.LeftWorld -= _onLeftWorldSlowTox;
            }

            if (_onLevelFinalizeSlowTox != null)
            {
                _clientApi.Event.LevelFinalize -= _onLevelFinalizeSlowTox;
            }
        }

        _onLeftWorldSlowTox = null;
        _onLevelFinalizeSlowTox = null;
        _clientApi = null;
        base.Dispose();
    }
}
