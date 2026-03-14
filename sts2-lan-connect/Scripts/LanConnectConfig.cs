using System;
using System.IO;
using System.Reflection;
using System.Text.Json;
using MegaCrit.Sts2.Core.Logging;

namespace Sts2LanConnect.Scripts;

internal sealed class LanConnectConfigData
{
    public string LastEndpoint { get; set; } = string.Empty;

    public ulong ClientNetId { get; set; }
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
                string json = File.ReadAllText(path);
                _data = JsonSerializer.Deserialize<LanConnectConfigData>(json) ?? new LanConnectConfigData();
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
        File.WriteAllText(path, json);
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
