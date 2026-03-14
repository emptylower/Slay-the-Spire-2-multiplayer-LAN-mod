using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;

namespace Sts2LanConnect.Scripts;

internal static class LanConnectNetUtil
{
    internal sealed class LanEndpointCandidate
    {
        public required string Address { get; init; }

        public required string InterfaceName { get; init; }

        public required string NetworkKind { get; init; }

        public required int SortScore { get; init; }

        public bool IsRecommended { get; set; }

        public bool IsLoopback { get; init; }

        public string Endpoint => $"{Address}:{LanConnectConstants.DefaultPort}";

        public string ToDisplayLine()
        {
            string prefix = IsRecommended ? "[推荐] " : string.Empty;
            return $"{prefix}{Endpoint}  ({InterfaceName}，{NetworkKind})";
        }
    }

    public static bool TryParseEndpoint(string raw, out string ip, out ushort port, out string error)
    {
        ip = string.Empty;
        port = LanConnectConstants.DefaultPort;
        error = string.Empty;

        string input = raw.Trim();
        if (string.IsNullOrWhiteSpace(input))
        {
            error = "请输入 IPv4 地址，格式为 192.168.1.20 或 192.168.1.20:33771。";
            return false;
        }

        if (string.Equals(input, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            ip = "127.0.0.1";
            return true;
        }

        if (input.Count(static c => c == ':') > 1)
        {
            error = "V1 只支持 IPv4 地址，不支持 IPv6。";
            return false;
        }

        int colonIndex = input.LastIndexOf(':');
        string ipPart = input;
        if (colonIndex >= 0)
        {
            ipPart = input[..colonIndex].Trim();
            string portPart = input[(colonIndex + 1)..].Trim();
            if (!ushort.TryParse(portPart, out port))
            {
                error = "端口格式无效，请输入 1-65535 之间的数字。";
                return false;
            }
        }

        if (!IPAddress.TryParse(ipPart, out IPAddress? address) || address.AddressFamily != AddressFamily.InterNetwork)
        {
            error = "请输入有效的 IPv4 地址。";
            return false;
        }

        ip = address.ToString();
        return true;
    }

    public static IReadOnlyList<LanEndpointCandidate> GetLanAddressCandidates()
    {
        List<LanEndpointCandidate> candidates = NetworkInterface.GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up)
            .SelectMany(GetInterfaceCandidates)
            .GroupBy(candidate => candidate.Address, StringComparer.Ordinal)
            .Select(group => group
                .OrderByDescending(candidate => candidate.SortScore)
                .ThenBy(candidate => candidate.InterfaceName, StringComparer.OrdinalIgnoreCase)
                .First())
            .OrderByDescending(candidate => candidate.SortScore)
            .ThenBy(candidate => candidate.InterfaceName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(candidate => candidate.Address, StringComparer.Ordinal)
            .ToList();

        candidates.Add(new LanEndpointCandidate
        {
            Address = "127.0.0.1",
            InterfaceName = "本机回环",
            NetworkKind = "仅同机测试",
            SortScore = int.MinValue,
            IsLoopback = true
        });

        for (int i = 0; i < candidates.Count; i++)
        {
            if (candidates[i].IsLoopback)
            {
                continue;
            }

            candidates[i].IsRecommended = true;
            break;
        }

        return candidates;
    }

    public static string GetPrimaryLanAddress()
    {
        return GetLanAddressCandidates()
            .FirstOrDefault(candidate => !candidate.IsLoopback)?.Address ?? "127.0.0.1";
    }

    public static ulong GenerateClientNetId()
    {
        Span<byte> bytes = stackalloc byte[8];
        RandomNumberGenerator.Fill(bytes);
        ulong value = BitConverter.ToUInt64(bytes);
        return value <= 1 ? value + 2 : value;
    }

    private static int ScoreAddress(IPAddress address)
    {
        byte[] bytes = address.GetAddressBytes();
        if (bytes[0] == 192 && bytes[1] == 168)
        {
            return 3;
        }

        if (bytes[0] == 10)
        {
            return 2;
        }

        if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31)
        {
            return 1;
        }

        return 0;
    }

    private static IEnumerable<LanEndpointCandidate> GetInterfaceCandidates(NetworkInterface nic)
    {
        if (nic.NetworkInterfaceType == NetworkInterfaceType.Loopback)
        {
            yield break;
        }

        IPInterfaceProperties properties;
        try
        {
            properties = nic.GetIPProperties();
        }
        catch
        {
            yield break;
        }

        bool hasIpv4Gateway = properties.GatewayAddresses
            .Any(gateway => gateway.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.Any.Equals(gateway.Address));

        foreach (IPAddress address in properties.UnicastAddresses.Select(info => info.Address))
        {
            if (!IsShareableIpv4(address))
            {
                continue;
            }

            yield return new LanEndpointCandidate
            {
                Address = address.ToString(),
                InterfaceName = GetInterfaceLabel(nic),
                NetworkKind = GetNetworkKind(nic, address),
                SortScore = ScoreAddress(address) * 100 + ScoreInterface(nic) * 10 + (hasIpv4Gateway ? 5 : 0)
            };
        }
    }

    private static bool IsShareableIpv4(IPAddress address)
    {
        if (address.AddressFamily != AddressFamily.InterNetwork || IPAddress.IsLoopback(address))
        {
            return false;
        }

        byte[] bytes = address.GetAddressBytes();
        if (bytes[0] == 0)
        {
            return false;
        }

        return !(bytes[0] == 169 && bytes[1] == 254);
    }

    private static int ScoreInterface(NetworkInterface nic)
    {
        return nic.NetworkInterfaceType switch
        {
            NetworkInterfaceType.Ethernet => 4,
            NetworkInterfaceType.GigabitEthernet => 4,
            NetworkInterfaceType.Wireless80211 => 3,
            NetworkInterfaceType.Ppp => 2,
            NetworkInterfaceType.Tunnel => 2,
            _ => 1
        };
    }

    private static string GetInterfaceLabel(NetworkInterface nic)
    {
        string name = string.IsNullOrWhiteSpace(nic.Name) ? nic.Description : nic.Name;
        if (string.IsNullOrWhiteSpace(nic.Description) || string.Equals(name, nic.Description, StringComparison.OrdinalIgnoreCase))
        {
            return name;
        }

        return $"{name}/{nic.Description}";
    }

    private static string GetNetworkKind(NetworkInterface nic, IPAddress address)
    {
        if (IPAddress.IsLoopback(address))
        {
            return "仅同机测试";
        }

        byte[] bytes = address.GetAddressBytes();
        bool isPrivate = bytes[0] == 10
            || (bytes[0] == 192 && bytes[1] == 168)
            || (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31);

        return nic.NetworkInterfaceType switch
        {
            NetworkInterfaceType.Ethernet or NetworkInterfaceType.GigabitEthernet when isPrivate => "有线局域网",
            NetworkInterfaceType.Wireless80211 when isPrivate => "Wi-Fi 局域网",
            NetworkInterfaceType.Ppp or NetworkInterfaceType.Tunnel => "虚拟网络/VPN",
            _ when isPrivate => "局域网",
            _ => "其他 IPv4"
        };
    }
}
