using System;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Screens.CharacterSelect;
using MegaCrit.Sts2.Core.Nodes.Screens.CustomRun;
using MegaCrit.Sts2.Core.Nodes.Screens.DailyRun;
using MegaCrit.Sts2.Core.Nodes.Screens.MainMenu;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Saves;

namespace Sts2LanConnect.Scripts;

internal static class LanConnectHostFlow
{
    private static bool _useLanHostOnce;

    public static void QueueLanHost()
    {
        _useLanHostOnce = true;
    }

    public static bool ConsumeQueuedLanHost()
    {
        if (!_useLanHostOnce)
        {
            return false;
        }

        _useLanHostOnce = false;
        return true;
    }

    public static async Task StartLanHostAsync(GameMode gameMode, Control loadingOverlay, NSubmenuStack stack)
    {
        loadingOverlay.Visible = true;

        try
        {
            NetHostGameService? netService = TryStartLanHost();
            if (netService == null)
            {
                return;
            }

            PushNewRunHostScreen(gameMode, stack, netService);

            await Task.Yield();
            LanConnectPopupUtil.ShowEndpointOptions(
                "LAN 主机已启动。\n把下面任一可达地址发给好友：",
                LanConnectNetUtil.GetLanAddressCandidates(),
                "同机双开测试请使用 127.0.0.1:33771。");
        }
        catch
        {
            NErrorPopup? popup = NErrorPopup.Create(new NetErrorInfo(NetError.InternalError, selfInitiated: false));
            if (popup != null)
            {
                NModalContainer.Instance?.Add(popup);
            }

            throw;
        }
        finally
        {
            loadingOverlay.Visible = false;
        }
    }

    public static async Task StartLanContinueAsync(Control loadingOverlay, NSubmenuStack stack)
    {
        loadingOverlay.Visible = true;

        try
        {
            ReadSaveResult<SerializableRun> loadResult = SaveManager.Instance.LoadAndCanonicalizeMultiplayerRunSave(LanConnectConstants.LanHostNetId);
            if (!loadResult.Success || loadResult.SaveData == null)
            {
                string error = string.IsNullOrWhiteSpace(loadResult.ErrorMessage) ? "未找到可继续的联机存档，或该存档已经损坏。" : loadResult.ErrorMessage;
                LanConnectPopupUtil.ShowInfo($"无法继续局域网联机存档。\n{error}");
                return;
            }

            NetHostGameService? netService = TryStartLanHost();
            if (netService == null)
            {
                return;
            }

            PushLoadedRunHostScreen(stack, netService, loadResult.SaveData);

            await Task.Yield();
            LanConnectPopupUtil.ShowEndpointOptions(
                "LAN 联机存档已载入。\n把下面任一可达地址发给原队友：",
                LanConnectNetUtil.GetLanAddressCandidates(),
                "仅原房间成员可继续加入，新玩家无法加入已保存的联机局。\n同机双开测试请使用 127.0.0.1:33771。");
        }
        catch
        {
            NErrorPopup? popup = NErrorPopup.Create(new NetErrorInfo(NetError.InternalError, selfInitiated: false));
            if (popup != null)
            {
                NModalContainer.Instance?.Add(popup);
            }

            throw;
        }
        finally
        {
            loadingOverlay.Visible = false;
        }
    }

    private static NetHostGameService? TryStartLanHost()
    {
        NetHostGameService netService = new();
        NetErrorInfo? error = netService.StartENetHost(LanConnectConstants.DefaultPort, LanConnectConstants.DefaultMaxPlayers);
        if (!error.HasValue)
        {
            return netService;
        }

        NErrorPopup? popup = NErrorPopup.Create(error.Value);
        if (popup != null)
        {
            NModalContainer.Instance?.Add(popup);
        }

        return null;
    }

    private static void PushNewRunHostScreen(GameMode gameMode, NSubmenuStack stack, NetHostGameService netService)
    {
        switch (gameMode)
        {
            case GameMode.Standard:
            {
                NCharacterSelectScreen submenu = stack.GetSubmenuType<NCharacterSelectScreen>();
                submenu.InitializeMultiplayerAsHost(netService, LanConnectConstants.DefaultMaxPlayers);
                stack.Push(submenu);
                break;
            }
            case GameMode.Daily:
            {
                NDailyRunScreen submenu = stack.GetSubmenuType<NDailyRunScreen>();
                submenu.InitializeMultiplayerAsHost(netService);
                stack.Push(submenu);
                break;
            }
            default:
            {
                NCustomRunScreen submenu = stack.GetSubmenuType<NCustomRunScreen>();
                submenu.InitializeMultiplayerAsHost(netService, LanConnectConstants.DefaultMaxPlayers);
                stack.Push(submenu);
                break;
            }
        }
    }

    private static void PushLoadedRunHostScreen(NSubmenuStack stack, NetHostGameService netService, SerializableRun run)
    {
        if (run.Modifiers.Count > 0)
        {
            if (run.DailyTime.HasValue)
            {
                NDailyRunLoadScreen dailyLoadScreen = stack.GetSubmenuType<NDailyRunLoadScreen>();
                dailyLoadScreen.InitializeAsHost(netService, run);
                stack.Push(dailyLoadScreen);
                return;
            }

            NCustomRunLoadScreen customLoadScreen = stack.GetSubmenuType<NCustomRunLoadScreen>();
            customLoadScreen.InitializeAsHost(netService, run);
            stack.Push(customLoadScreen);
            return;
        }

        NMultiplayerLoadGameScreen standardLoadScreen = stack.GetSubmenuType<NMultiplayerLoadGameScreen>();
        standardLoadScreen.InitializeAsHost(netService, run);
        stack.Push(standardLoadScreen);
    }
}
