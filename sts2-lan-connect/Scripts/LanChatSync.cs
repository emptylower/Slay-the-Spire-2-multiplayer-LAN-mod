using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Game.Lobby;
using MegaCrit.Sts2.Core.Nodes.Multiplayer;
using MegaCrit.Sts2.Core.Runs;

namespace Sts2LanConnect.Scripts;

internal sealed record LanChatEntry(ulong SenderNetId, string Text);

internal static class LanChatSync
{
    private static readonly FieldInfo? StartLobbyField = typeof(NRemoteLobbyPlayerContainer).GetField("_lobby", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly FieldInfo? LoadLobbyField = typeof(NRemoteLoadLobbyPlayerContainer).GetField("_lobby", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly MessageHandlerDelegate<LanChatMessage> ChatHandler = HandleChatMessage;
    private static readonly Action<NetErrorInfo> DisconnectedHandler = HandleDisconnected;
    private static readonly object Sync = new();
    private static readonly List<LanChatEntry> Entries = [];

    private static INetGameService? _registeredService;
    private static int _version;

    public static int Version
    {
        get
        {
            lock (Sync)
            {
                return _version;
            }
        }
    }

    public static void Reset()
    {
        UnregisterFromCurrentService();
        _registeredService = null;
        ClearEntries();
    }

    public static void Tick()
    {
        TryObserveRunService();
    }

    public static void ObserveStartLobbyContainer(NRemoteLobbyPlayerContainer container)
    {
        ObserveService(StartLobbyField?.GetValue(container) as StartRunLobby is { } lobby ? lobby.NetService : null);
    }

    public static void ObserveLoadLobbyContainer(NRemoteLoadLobbyPlayerContainer container)
    {
        ObserveService(LoadLobbyField?.GetValue(container) as LoadRunLobby is { } lobby ? lobby.NetService : null);
    }

    public static void ObserveRunUi()
    {
        TryObserveRunService();
    }

    public static IReadOnlyList<LanChatEntry> GetEntriesSnapshot()
    {
        lock (Sync)
        {
            return Entries.ToArray();
        }
    }

    public static bool TrySendLocalMessage(string rawText, out string error)
    {
        string normalized = NormalizeMessage(rawText);
        if (normalized.Length == 0)
        {
            error = string.Empty;
            return false;
        }

        INetGameService? service = _registeredService;
        if (service == null || !service.IsConnected)
        {
            error = "当前未连接联机，无法发送聊天消息。";
            return false;
        }

        AddEntry(service.NetId, normalized);
        service.SendMessage(new LanChatMessage
        {
            text = normalized
        });

        error = string.Empty;
        return true;
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
                ClearEntries();
            }

            return;
        }

        if (ReferenceEquals(_registeredService, service))
        {
            return;
        }

        UnregisterFromCurrentService();
        _registeredService = service;
        _registeredService.RegisterMessageHandler(ChatHandler);
        _registeredService.Disconnected += DisconnectedHandler;
        ClearEntries();
        Log.Info($"sts2_lan_connect chat sync attached to {_registeredService.GetType().Name}; netId={_registeredService.NetId}");
    }

    private static void UnregisterFromCurrentService()
    {
        if (_registeredService == null)
        {
            return;
        }

        try
        {
            _registeredService.UnregisterMessageHandler(ChatHandler);
        }
        catch (Exception ex)
        {
            Log.Warn($"sts2_lan_connect failed to unregister chat handler: {ex.Message}");
        }

        try
        {
            _registeredService.Disconnected -= DisconnectedHandler;
        }
        catch (Exception ex)
        {
            Log.Warn($"sts2_lan_connect failed to unregister chat disconnect handler: {ex.Message}");
        }
    }

    private static void HandleDisconnected(NetErrorInfo _)
    {
        UnregisterFromCurrentService();
        _registeredService = null;
        ClearEntries();
    }

    private static void HandleChatMessage(LanChatMessage message, ulong senderId)
    {
        AddEntry(senderId, message.text);
    }

    private static void AddEntry(ulong senderId, string text)
    {
        string normalized = NormalizeMessage(text);
        if (normalized.Length == 0)
        {
            return;
        }

        LanPlayerProfileRegistry.Observe(senderId);

        lock (Sync)
        {
            Entries.Add(new LanChatEntry(senderId, normalized));
            if (Entries.Count > LanConnectConstants.MaxChatEntries)
            {
                Entries.RemoveAt(0);
            }

            _version++;
        }
    }

    private static void ClearEntries()
    {
        lock (Sync)
        {
            if (Entries.Count == 0)
            {
                return;
            }

            Entries.Clear();
            _version++;
        }
    }

    private static string NormalizeMessage(string? value)
    {
        string trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        StringBuilder builder = new(trimmed.Length);
        bool previousWasWhitespace = false;
        foreach (char character in trimmed)
        {
            char normalizedCharacter = char.IsControl(character) ? ' ' : character;
            if (char.IsWhiteSpace(normalizedCharacter))
            {
                if (previousWasWhitespace)
                {
                    continue;
                }

                builder.Append(' ');
                previousWasWhitespace = true;
                continue;
            }

            builder.Append(normalizedCharacter);
            previousWasWhitespace = false;
        }

        string normalized = builder.ToString().Trim();
        if (normalized.Length <= LanConnectConstants.MaxChatMessageLength)
        {
            return normalized;
        }

        return normalized[..LanConnectConstants.MaxChatMessageLength];
    }
}
