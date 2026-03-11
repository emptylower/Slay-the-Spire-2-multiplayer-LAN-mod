using System;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.addons.mega_text;

namespace Sts2LanConnect.Scripts;

internal static class LanPlayerProfileSync
{
    private const double ResendIntervalSeconds = 2.5d;

    private static readonly FieldInfo? StartLobbyField = typeof(NRemoteLobbyPlayerContainer).GetField("_lobby", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? LoadLobbyField = typeof(NRemoteLoadLobbyPlayerContainer).GetField("_lobby", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? RemotePlayerIdField = typeof(NRemoteLobbyPlayer).GetField("_playerId", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly MessageHandlerDelegate<LanPlayerProfileMessage> ProfileHandler = HandleProfileMessage;

    private static INetGameService? _registeredService;
    private static double _secondsUntilResend;
    private static bool _localProfileDirty = true;
    private static string _lastSentDisplayName = string.Empty;

    public static void Reset()
    {
        UnregisterFromCurrentService();
        LanPlayerProfileRegistry.Clear();
        _registeredService = null;
        _secondsUntilResend = 0d;
        _localProfileDirty = true;
        _lastSentDisplayName = string.Empty;
    }

    public static void MarkLocalProfileDirty()
    {
        _localProfileDirty = true;
        _secondsUntilResend = 0d;
    }

    public static void Tick(double delta)
    {
        _secondsUntilResend -= delta;
        TryObserveRunService();

        if (_registeredService == null || !_registeredService.IsConnected)
        {
            return;
        }

        string displayName = GetRequestedDisplayName();
        LanPlayerProfileRegistry.Set(_registeredService.NetId, displayName);

        if (!_localProfileDirty && _secondsUntilResend > 0d && string.Equals(_lastSentDisplayName, displayName, StringComparison.Ordinal))
        {
            return;
        }

        _registeredService.SendMessage(new LanPlayerProfileMessage
        {
            displayName = displayName
        });

        _lastSentDisplayName = displayName;
        _localProfileDirty = false;
        _secondsUntilResend = ResendIntervalSeconds;
    }

    public static void ObserveStartLobbyContainer(NRemoteLobbyPlayerContainer container)
    {
        ObserveService(StartLobbyField?.GetValue(container) as StartRunLobby is { } lobby ? lobby.NetService : null);
    }

    public static void ObserveLoadLobbyContainer(NRemoteLoadLobbyPlayerContainer container)
    {
        ObserveService(LoadLobbyField?.GetValue(container) as LoadRunLobby is { } lobby ? lobby.NetService : null);
    }

    public static void ObservePlayerStateContainer()
    {
        TryObserveRunService();
    }

    public static void ApplyDisplayName(NRemoteLobbyPlayer playerNode)
    {
        if (RemotePlayerIdField?.GetValue(playerNode) is not ulong playerId)
        {
            return;
        }

        MegaLabel? nameplate = playerNode.GetNodeOrNull<MegaLabel>("%NameplateLabel");
        if (nameplate == null)
        {
            return;
        }

        LanPlayerProfileRegistry.Observe(playerId);
        nameplate.SetTextAutoSize(ResolveDisplayName(playerId));
    }

    public static void ApplyDisplayName(NMultiplayerPlayerState playerState)
    {
        if (playerState.Player == null)
        {
            return;
        }

        MegaLabel? nameplate = playerState.GetNodeOrNull<MegaLabel>("%NameplateLabel");
        if (nameplate == null)
        {
            return;
        }

        ulong playerId = playerState.Player.NetId;
        LanPlayerProfileRegistry.Observe(playerId);
        nameplate.SetTextAutoSize(ResolveDisplayName(playerId));
    }

    private static void TryObserveRunService()
    {
        if (RunManager.Instance?.NetService is { } runService)
        {
            ObserveService(runService);
        }
    }

    private static void ObserveService(INetGameService? service)
    {
        if (service is not NetHostGameService && service is not NetClientGameService)
        {
            if (_registeredService != null)
            {
                UnregisterFromCurrentService();
                _registeredService = null;
                _secondsUntilResend = 0d;
                _localProfileDirty = true;
                _lastSentDisplayName = string.Empty;
                LanPlayerProfileRegistry.Clear();
            }

            return;
        }

        if (ReferenceEquals(_registeredService, service))
        {
            return;
        }

        UnregisterFromCurrentService();
        LanPlayerProfileRegistry.Clear();
        _registeredService = service;
        _registeredService.RegisterMessageHandler(ProfileHandler);
        _localProfileDirty = true;
        _secondsUntilResend = 0d;
        _lastSentDisplayName = string.Empty;
        LanPlayerProfileRegistry.Set(_registeredService.NetId, GetRequestedDisplayName());
        Log.Info($"sts2_lan_connect profile sync attached to {_registeredService.GetType().Name}; netId={_registeredService.NetId}");
    }

    private static void UnregisterFromCurrentService()
    {
        if (_registeredService == null)
        {
            return;
        }

        try
        {
            _registeredService.UnregisterMessageHandler(ProfileHandler);
        }
        catch (Exception ex)
        {
            Log.Warn($"sts2_lan_connect failed to unregister profile handler: {ex.Message}");
        }
    }

    private static void HandleProfileMessage(LanPlayerProfileMessage message, ulong senderId)
    {
        LanPlayerProfileRegistry.Set(senderId, message.displayName);
    }

    private static string ResolveDisplayName(ulong netId)
    {
        return LanPlayerProfileRegistry.Resolve(netId);
    }

    private static string GetRequestedDisplayName()
    {
        return LanPlayerProfileRegistry.NormalizeDisplayName(LanConnectConfig.PreferredPlayerName);
    }
}
