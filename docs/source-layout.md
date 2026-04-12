# STS2 LAN Connect 源码结构

## 顶层目录

```text
.
├── sts2-lan-connect/        # MOD 工程本体
├── scripts/                 # 构建、打包、安装脚本
├── docs/                    # 当前维护中的项目文档
├── references/decompiled/   # 反编译参考文件，不参与编译
├── releases/                # 提交到仓库的预编译发布包
└── local-release/           # 本地生成的打包产物（gitignored）
```

## `sts2-lan-connect/`

这个目录是实际参与编译和打包的 MOD 工程。

核心文件：

- `sts2-lan-connect/sts2_lan_connect.csproj`
  C# 项目文件，引用游戏目录中的 `sts2.dll`、`0Harmony.dll`、`Steamworks.NET.dll`
- `sts2-lan-connect/project.godot`
  Godot 工程配置
- `sts2-lan-connect/sts2_lan_connect.json`
  官方 `v0.99.1+` 所需的 manifest
- `sts2-lan-connect/tools/build_pck.gd`
  PCK 打包脚本

生成目录：

- `sts2-lan-connect/.godot/`
  Godot 生成目录，不应手改
- `sts2-lan-connect/build/`
  PCK 输出目录

## `sts2-lan-connect/Scripts/`

当前所有运行时代码基本都平铺在这个目录下，阅读时可以按职责分组理解。

### 启动与运行时监控

- `Entry.cs`
- `LanConnectRuntimeMonitor.cs`

### UI 注入

- `Patches.MultiplayerSubmenu.cs`
- `Patches.HostSubmenu.cs`
- `Patches.JoinFriendScreen.cs`
- `LanChatUiPatches.cs`
- `LanChatPanel.cs`

### Host / Join 流程

- `LanConnectHostFlow.cs`
- `LanConnectNetUtil.cs`
- `LanConnectPopupUtil.cs`
- `LanConnectConstants.cs`

### 本地配置

- `LanConnectConfig.cs`

### 聊天与昵称同步

- `LanChatMessage.cs`
- `LanChatSync.cs`
- `LanPlayerProfileMessage.cs`
- `LanPlayerProfileRegistry.cs`
- `LanPlayerProfileSync.cs`

## `scripts/`

脚本职责：

- `build-sts2-lan-connect-windows.ps1`
- `build-sts2-lan-connect.sh`
  构建 DLL、打包 PCK，并可选复制到游戏 `mods/` 目录

- `package-sts2-lan-connect-windows.ps1`
- `package-sts2-lan-connect.sh`
  生成本地发布目录和 zip，统一输出到 `local-release/`

- `install-sts2-lan-connect-windows.ps1`
- `install-sts2-lan-connect-windows.bat`
- `install-sts2-lan-connect-linux.sh`
- `install-sts2-lan-connect-macos.sh`
  把发布包安装到游戏目录，并做一次单向存档同步

## `releases/` 与 `local-release/`

这是现在统一后的约定：

- `local-release/`
  只放本地生成的临时打包产物，不提交到 Git
- `releases/`
  只放确认过的仓库发布包，参与版本管理

这样可以避免以前 `sts2-lan-connect/release/`、`local-release/`、`releases/` 三处重复存放类似内容。

## `references/decompiled/`

这个目录里放的是反编译参考文件，不参与本项目编译。

用途：

- 辅助理解官方 Hook / Mod Loader / API 行为
- 作为维护者查阅官方实现时的快照参考

不应把它们误认为当前项目源码的一部分。

## 当前代码结构的整理建议

当前 `Scripts/` 目录虽然不大，但职责已经比较多，后续如果继续扩展，建议把它拆成以下子目录：

- `Scripts/Bootstrap/`
- `Scripts/Patches/`
- `Scripts/Ui/`
- `Scripts/Net/`
- `Scripts/Sync/`
- `Scripts/Config/`

本次整理先把“仓库结构”和“文档结构”统一清楚，没有改动运行时代码目录，以免在同一次提交里同时引入逻辑风险和结构风险。
