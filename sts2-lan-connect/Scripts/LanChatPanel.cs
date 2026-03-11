using System.Text;
using Godot;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace Sts2LanConnect.Scripts;

internal sealed partial class LanChatPanel : PanelContainer
{
    private static readonly Vector2 ExpandedPanelSize = new(380f, 256f);
    private static readonly Vector2 CollapsedPanelSize = new(380f, 44f);
    private const float PanelMargin = 16f;

    private RichTextLabel? _transcript;
    private NMegaLineEdit? _input;
    private Control? _content;
    private Button? _toggleButton;
    private int _lastChatVersion = -1;
    private int _lastProfileVersion = -1;
    private Vector2 _lastViewportSize = Vector2.Zero;
    private bool _isDragging;
    private Vector2 _dragPointerOffset;

    public override void _Ready()
    {
        Name = LanConnectConstants.ChatPanelName;
        ProcessMode = ProcessModeEnum.Always;
        MouseFilter = MouseFilterEnum.Stop;
        FocusMode = FocusModeEnum.None;
        AnchorLeft = 0f;
        AnchorTop = 0f;
        AnchorRight = 0f;
        AnchorBottom = 0f;
        CustomMinimumSize = ExpandedPanelSize;
        Size = ExpandedPanelSize;

        if (GetChildCount() == 0)
        {
            BuildUi();
        }

        CallDeferred(nameof(ApplyInitialLayout));
        RefreshTranscript(force: true);
    }

    public override void _Process(double delta)
    {
        Vector2 viewportSize = GetViewportRect().Size;
        if (_isDragging)
        {
            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                SetPanelPosition(ClampPosition(GetGlobalMousePosition() - _dragPointerOffset));
            }
            else
            {
                _isDragging = false;
                LanConnectConfig.SetChatPanelPosition(Position);
            }
        }
        else if (viewportSize != _lastViewportSize)
        {
            SetPanelPosition(ClampPosition(Position));
        }

        _lastViewportSize = viewportSize;
        RefreshTranscript(force: false);
    }

    private void BuildUi()
    {
        VBoxContainer root = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };

        HBoxContainer titleBar = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };

        titleBar.Connect(Control.SignalName.GuiInput, Callable.From<InputEvent>(OnTitleBarGuiInput));

        Label title = new()
        {
            Text = "LAN 聊天",
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            MouseFilter = MouseFilterEnum.Ignore
        };

        _transcript = new RichTextLabel
        {
            Name = LanConnectConstants.ChatTranscriptName,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            FitContent = false,
            ScrollFollowing = true,
            SelectionEnabled = true,
            BbcodeEnabled = false,
            CustomMinimumSize = new Vector2(0f, 180f)
        };

        HBoxContainer inputRow = new()
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };

        _content = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };

        _input = new NMegaLineEdit
        {
            Name = LanConnectConstants.ChatInputName,
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            PlaceholderText = "输入消息后按回车发送"
        };

        Button sendButton = new()
        {
            Name = LanConnectConstants.ChatSendButtonName,
            Text = "发送",
            CustomMinimumSize = new Vector2(88f, 0f)
        };

        _toggleButton = new Button
        {
            Text = "收起",
            CustomMinimumSize = new Vector2(72f, 0f)
        };

        _input.Connect(LineEdit.SignalName.TextSubmitted, Callable.From<string>(_ => SubmitMessage()));
        sendButton.Connect(Button.SignalName.Pressed, Callable.From(SubmitMessage));
        _toggleButton.Connect(Button.SignalName.Pressed, Callable.From(ToggleCollapsed));

        titleBar.AddChild(title);
        titleBar.AddChild(_toggleButton);
        inputRow.AddChild(_input);
        inputRow.AddChild(sendButton);
        _content.AddChild(_transcript);
        _content.AddChild(inputRow);
        root.AddChild(titleBar);
        root.AddChild(_content);
        AddChild(root);
    }

    private void ApplyInitialLayout()
    {
        _lastViewportSize = GetViewportRect().Size;
        bool collapsed = LanConnectConfig.ChatPanelCollapsed;
        ApplyCollapsedState(collapsed, persist: false);
        Vector2 position = LanConnectConfig.ChatPanelPosition ?? GetDefaultPosition();
        SetPanelPosition(ClampPosition(position));
    }

    private void SubmitMessage()
    {
        if (_input == null)
        {
            return;
        }

        if (!LanChatSync.TrySendLocalMessage(_input.Text, out string error))
        {
            if (!string.IsNullOrWhiteSpace(error))
            {
                LanConnectPopupUtil.ShowInfo(error);
            }

            return;
        }

        _input.Text = string.Empty;
        _input.GrabFocus();
        RefreshTranscript(force: true);
    }

    private void OnTitleBarGuiInput(InputEvent inputEvent)
    {
        if (inputEvent is not InputEventMouseButton mouseButton || mouseButton.ButtonIndex != MouseButton.Left)
        {
            return;
        }

        if (mouseButton.Pressed)
        {
            _isDragging = true;
            _dragPointerOffset = GetGlobalMousePosition() - GlobalPosition;
        }
        else if (_isDragging)
        {
            _isDragging = false;
            LanConnectConfig.SetChatPanelPosition(Position);
        }
    }

    private void ToggleCollapsed()
    {
        ApplyCollapsedState(!LanConnectConfig.ChatPanelCollapsed, persist: true);
    }

    private void ApplyCollapsedState(bool collapsed, bool persist)
    {
        if (_content != null)
        {
            _content.Visible = !collapsed;
        }

        if (_toggleButton != null)
        {
            _toggleButton.Text = collapsed ? "展开" : "收起";
        }

        CustomMinimumSize = collapsed ? CollapsedPanelSize : ExpandedPanelSize;
        Size = collapsed ? CollapsedPanelSize : ExpandedPanelSize;
        SetPanelPosition(ClampPosition(Position));
        if (persist)
        {
            LanConnectConfig.ChatPanelCollapsed = collapsed;
        }
    }

    private void SetPanelPosition(Vector2 position)
    {
        Position = position;
    }

    private Vector2 GetDefaultPosition()
    {
        Vector2 viewportSize = GetViewportRect().Size;
        Vector2 panelSize = LanConnectConfig.ChatPanelCollapsed ? CollapsedPanelSize : ExpandedPanelSize;
        return new Vector2(
            Mathf.Max(PanelMargin, viewportSize.X - panelSize.X - PanelMargin),
            Mathf.Max(PanelMargin, viewportSize.Y - panelSize.Y - PanelMargin));
    }

    private Vector2 ClampPosition(Vector2 position)
    {
        Vector2 viewportSize = GetViewportRect().Size;
        Vector2 panelSize = LanConnectConfig.ChatPanelCollapsed ? CollapsedPanelSize : ExpandedPanelSize;
        float maxX = Mathf.Max(PanelMargin, viewportSize.X - panelSize.X - PanelMargin);
        float maxY = Mathf.Max(PanelMargin, viewportSize.Y - panelSize.Y - PanelMargin);
        return new Vector2(
            Mathf.Clamp(position.X, PanelMargin, maxX),
            Mathf.Clamp(position.Y, PanelMargin, maxY));
    }

    private void RefreshTranscript(bool force)
    {
        if (_transcript == null)
        {
            return;
        }

        int chatVersion = LanChatSync.Version;
        int profileVersion = LanPlayerProfileRegistry.Version;
        if (!force && chatVersion == _lastChatVersion && profileVersion == _lastProfileVersion)
        {
            return;
        }

        var entries = LanChatSync.GetEntriesSnapshot();
        if (entries.Count == 0)
        {
            _transcript.Text = "暂无聊天消息。";
        }
        else
        {
            StringBuilder builder = new();
            foreach (LanChatEntry entry in entries)
            {
                if (builder.Length > 0)
                {
                    builder.Append('\n');
                }

                string senderName = LanPlayerProfileRegistry.Resolve(entry.SenderNetId);
                builder.Append(senderName);
                builder.Append(": ");
                builder.Append(entry.Text);
            }

            _transcript.Text = builder.ToString();
            int lastLineIndex = _transcript.GetLineCount() - 1;
            if (lastLineIndex >= 0)
            {
                _transcript.ScrollToLine(lastLineIndex);
            }
        }

        _lastChatVersion = chatVersion;
        _lastProfileVersion = profileVersion;
    }
}
