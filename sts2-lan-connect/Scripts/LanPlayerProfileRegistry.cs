using System.Collections.Generic;
using System.Linq;

namespace Sts2LanConnect.Scripts;

internal static class LanPlayerProfileRegistry
{
    private static readonly object Sync = new();
    private static readonly Dictionary<ulong, string> DisplayNames = [];
    private static readonly List<ulong> ObservationOrder = [];
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

    public static void Clear()
    {
        lock (Sync)
        {
            if (DisplayNames.Count == 0 && ObservationOrder.Count == 0)
            {
                return;
            }

            DisplayNames.Clear();
            ObservationOrder.Clear();
            _version++;
        }
    }

    public static void Observe(ulong netId)
    {
        lock (Sync)
        {
            if (EnsureObservedUnsafe(netId))
            {
                _version++;
            }
        }
    }

    public static void Set(ulong netId, string? displayName)
    {
        string normalized = NormalizeDisplayName(displayName);
        lock (Sync)
        {
            bool added = EnsureObservedUnsafe(netId);
            string existingName = DisplayNames[netId];
            if (!added && string.Equals(existingName, normalized, System.StringComparison.Ordinal))
            {
                return;
            }

            DisplayNames[netId] = normalized;
            _version++;
        }
    }

    public static string Resolve(ulong netId)
    {
        lock (Sync)
        {
            if (EnsureObservedUnsafe(netId))
            {
                _version++;
            }

            ulong[] orderedNetIds = GetOrderedNetIdsUnsafe();

            HashSet<string> usedNames = new(StringComparer.OrdinalIgnoreCase);
            int nonHostIndex = 0;
            foreach (ulong playerNetId in orderedNetIds)
            {
                string requestedName = DisplayNames[playerNetId];
                string baseName;
                if (string.IsNullOrWhiteSpace(requestedName))
                {
                    if (playerNetId == LanConnectConstants.LanHostNetId)
                    {
                        baseName = "[房主]";
                    }
                    else
                    {
                        nonHostIndex++;
                        baseName = $"玩家{nonHostIndex}";
                    }
                }
                else
                {
                    if (playerNetId != LanConnectConstants.LanHostNetId)
                    {
                        nonHostIndex++;
                    }

                    baseName = requestedName;
                }

                string resolvedName = MakeUnique(baseName, usedNames);
                if (playerNetId == netId)
                {
                    return resolvedName;
                }
            }

            return netId == LanConnectConstants.LanHostNetId ? "[房主]" : "玩家";
        }
    }

    public static string NormalizeDisplayName(string? value)
    {
        string trimmed = (value ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        char[] sanitizedChars = trimmed
            .Where(static c => !char.IsControl(c))
            .ToArray();
        string sanitized = new(sanitizedChars);
        if (sanitized.Length <= LanConnectConstants.MaxDisplayNameLength)
        {
            return sanitized;
        }

        return sanitized[..LanConnectConstants.MaxDisplayNameLength];
    }

    private static string MakeUnique(string baseName, HashSet<string> usedNames)
    {
        string normalizedBaseName = NormalizeDisplayName(baseName);
        if (usedNames.Add(normalizedBaseName))
        {
            return normalizedBaseName;
        }

        for (int suffix = 2; ; suffix++)
        {
            string suffixText = $"({suffix})";
            int maxBaseLength = LanConnectConstants.MaxDisplayNameLength - suffixText.Length;
            string truncatedBaseName = maxBaseLength > 0 && normalizedBaseName.Length > maxBaseLength
                ? normalizedBaseName[..maxBaseLength]
                : normalizedBaseName;
            string candidate = truncatedBaseName + suffixText;
            if (usedNames.Add(candidate))
            {
                return candidate;
            }
        }
    }

    private static bool EnsureObservedUnsafe(ulong netId)
    {
        if (DisplayNames.ContainsKey(netId))
        {
            return false;
        }

        DisplayNames[netId] = string.Empty;
        ObservationOrder.Add(netId);
        return true;
    }

    private static ulong[] GetOrderedNetIdsUnsafe()
    {
        List<ulong> ordered = new(ObservationOrder.Count);
        if (DisplayNames.ContainsKey(LanConnectConstants.LanHostNetId))
        {
            ordered.Add(LanConnectConstants.LanHostNetId);
        }

        foreach (ulong observedNetId in ObservationOrder)
        {
            if (observedNetId == LanConnectConstants.LanHostNetId)
            {
                continue;
            }

            ordered.Add(observedNetId);
        }

        return ordered.ToArray();
    }
}
