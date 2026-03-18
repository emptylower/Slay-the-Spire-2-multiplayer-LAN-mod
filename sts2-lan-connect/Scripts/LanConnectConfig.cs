using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Godot;
using MegaCrit.Sts2.Core.Logging;

namespace Sts2LanConnect.Scripts;

internal sealed class LanConnectConfigData
{
    public string LastEndpoint { get; set; } = string.Empty;

    public ulong ClientNetId { get; set; }

    public string PreferredPlayerName { get; set; } = string.Empty;

    public float? ChatPanelPositionX { get; set; }

    public float? ChatPanelPositionY { get; set; }

    public bool ChatPanelCollapsed { get; set; }
}

internal static class LanConnectConfig
{
    private const string ConfigFileName = "config.json";

    private static readonly object Sync = new();

    private static LanConnectConfigData _data = new();

    public static string LastEndpoint
    {
        get
        {
            lock (Sync)
            {
                return _data.LastEndpoint;
            }
        }
        set
        {
            lock (Sync)
            {
                if (_data.LastEndpoint == value)
                {
                    return;
                }

                _data.LastEndpoint = value;
                SaveUnsafe();
            }
        }
    }

    public static ulong ClientNetId
    {
        get
        {
            lock (Sync)
            {
                if (_data.ClientNetId <= LanConnectConstants.LanHostNetId)
                {
                    _data.ClientNetId = LanConnectNetUtil.GenerateClientNetId();
                    SaveUnsafe();
                }

                return _data.ClientNetId;
            }
        }
    }

    public static string PreferredPlayerName
    {
        get
        {
            lock (Sync)
            {
                return _data.PreferredPlayerName;
            }
        }
        set
        {
            string normalized = LanPlayerProfileRegistry.NormalizeDisplayName(value);
            lock (Sync)
            {
                if (_data.PreferredPlayerName == normalized)
                {
                    return;
                }

                _data.PreferredPlayerName = normalized;
                SaveUnsafe();
            }

            LanPlayerProfileSync.MarkLocalProfileDirty();
        }
    }

    public static Vector2? ChatPanelPosition
    {
        get
        {
            lock (Sync)
            {
                if (!_data.ChatPanelPositionX.HasValue || !_data.ChatPanelPositionY.HasValue)
                {
                    return null;
                }

                return new Vector2(_data.ChatPanelPositionX.Value, _data.ChatPanelPositionY.Value);
            }
        }
    }

    public static void SetChatPanelPosition(Vector2 position)
    {
        lock (Sync)
        {
            if (_data.ChatPanelPositionX.HasValue &&
                _data.ChatPanelPositionY.HasValue &&
                Math.Abs(_data.ChatPanelPositionX.Value - position.X) < 0.5f &&
                Math.Abs(_data.ChatPanelPositionY.Value - position.Y) < 0.5f)
            {
                return;
            }

            _data.ChatPanelPositionX = position.X;
            _data.ChatPanelPositionY = position.Y;
            SaveUnsafe();
        }
    }

    public static bool ChatPanelCollapsed
    {
        get
        {
            lock (Sync)
            {
                return _data.ChatPanelCollapsed;
            }
        }
        set
        {
            lock (Sync)
            {
                if (_data.ChatPanelCollapsed == value)
                {
                    return;
                }

                _data.ChatPanelCollapsed = value;
                SaveUnsafe();
            }
        }
    }

    public static void Load()
    {
        lock (Sync)
        {
            string path = GetConfigPath();
            if (!File.Exists(path))
            {
                SaveUnsafe();
                return;
            }

            try
            {
                string json = File.ReadAllText(path, Encoding.UTF8);
                _data = JsonSerializer.Deserialize<LanConnectConfigData>(json) ?? new LanConnectConfigData();
                _data.PreferredPlayerName = LanPlayerProfileRegistry.NormalizeDisplayName(_data.PreferredPlayerName);
            }
            catch (Exception ex)
            {
                Log.Warn($"sts2_lan_connect failed to read config: {ex.Message}");
                _data = new LanConnectConfigData();
                SaveUnsafe();
            }
        }
    }

    private static void SaveUnsafe()
    {
        string path = GetConfigPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        string json = JsonSerializer.Serialize(_data, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        File.WriteAllText(path, json, Encoding.UTF8);
    }

    private static string GetConfigPath()
    {
        string modDirectory = ResolveModDirectory();
        return Path.Combine(modDirectory, ConfigFileName);
    }

    private static string ResolveModDirectory()
    {
        string? assemblyLocation = Assembly.GetExecutingAssembly().Location;
        string? assemblyDirectory = string.IsNullOrWhiteSpace(assemblyLocation) ? null : Path.GetDirectoryName(assemblyLocation);
        if (!string.IsNullOrWhiteSpace(assemblyDirectory) && Directory.Exists(assemblyDirectory))
        {
            return assemblyDirectory;
        }

        return Path.Combine(AppContext.BaseDirectory, "mods", "sts2_lan_connect");
    }
}
