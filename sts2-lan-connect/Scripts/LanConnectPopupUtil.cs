using System.Linq;
using Godot;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using static Sts2LanConnect.Scripts.LanConnectNetUtil;

namespace Sts2LanConnect.Scripts;

internal static class LanConnectPopupUtil
{
    public static void ShowInfo(string body)
    {
        NErrorPopup? popup = NErrorPopup.Create("STS2 LAN Connect", body, showReportBugButton: false);
        if (popup != null)
        {
            NModalContainer.Instance?.Add(popup);
        }
    }

    public static void ShowEndpointInfo(string intro, string endpoint, string? footer = null)
    {
        ShowEndpointOptions(intro, new[]
        {
            new LanEndpointCandidate
            {
                Address = endpoint.Split(':')[0],
                InterfaceName = "默认地址",
                NetworkKind = "手动指定",
                SortScore = 0,
                IsRecommended = true
            }
        }, footer);
    }

    public static void ShowEndpointOptions(string intro, IReadOnlyList<LanEndpointCandidate> endpoints, string? footer = null)
    {
        string body = BuildEndpointBody(intro, endpoints, footer);
        string endpointToCopy = endpoints.FirstOrDefault(candidate => candidate.IsRecommended)?.Endpoint
            ?? endpoints.FirstOrDefault()?.Endpoint
            ?? $"127.0.0.1:{LanConnectConstants.DefaultPort}";

        if (Engine.GetMainLoop() is not SceneTree tree || tree.Root == null)
        {
            DisplayServer.ClipboardSet(endpointToCopy);
            ShowInfo($"{body}\n\n地址已自动复制到剪贴板。");
            return;
        }

        AcceptDialog dialog = new()
        {
            Title = "STS2 LAN Connect",
            DialogText = body,
            DialogAutowrap = true,
            OkButtonText = "关闭",
            DialogHideOnOk = true,
            DialogCloseOnEscape = true
        };

        foreach (LanEndpointCandidate candidate in endpoints.Reverse())
        {
            string copyText = candidate.IsRecommended ? $"复制推荐地址 {candidate.Address}" : $"复制地址 {candidate.Address}";
            Button copyButton = dialog.AddButton(copyText, false, string.Empty);
            copyButton.Connect(Button.SignalName.Pressed, Callable.From(() =>
            {
                DisplayServer.ClipboardSet(candidate.Endpoint);
                dialog.DialogText = $"{body}\n\n已复制到剪贴板：{candidate.Endpoint}";
            }));
        }

        dialog.GetOkButton().Connect(Button.SignalName.Pressed, Callable.From(dialog.QueueFree));
        dialog.CloseRequested += dialog.QueueFree;

        tree.Root.AddChild(dialog);
        dialog.PopupCentered();
    }

    private static string BuildEndpointBody(string intro, IReadOnlyList<LanEndpointCandidate> endpoints, string? footer)
    {
        System.Text.StringBuilder builder = new();
        builder.AppendLine(intro);
        builder.AppendLine();
        builder.AppendLine("可用地址：");

        foreach (LanEndpointCandidate candidate in endpoints)
        {
            builder.AppendLine(candidate.ToDisplayLine());
        }

        if (!string.IsNullOrWhiteSpace(footer))
        {
            builder.AppendLine();
            builder.Append(footer);
        }

        return builder.ToString().TrimEnd();
    }
}
