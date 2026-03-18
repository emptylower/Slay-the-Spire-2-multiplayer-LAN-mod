# STS2 LAN Connect 接口文档

## 1. 模组 manifest

文件：

- `sts2-lan-connect/sts2_lan_connect.json`

当前结构：

```json
{
  "id": "sts2_lan_connect",
  "name": "STS2 LAN Connect",
  "author": "Slay-the-Spire-2-multiplayer-LAN-mod",
  "description": "Adds LAN host/join support to Slay the Spire 2 multiplayer.",
  "version": "0.1.3",
  "has_pck": true,
  "has_dll": true,
  "dependencies": [],
  "affects_gameplay": true
}
```

字段说明：

| 字段 | 类型 | 含义 |
| --- | --- | --- |
| `id` | `string` | 模组唯一 ID，也是同目录下 `.dll` / `.pck` 的基名 |
| `name` | `string` | 显示名称 |
| `author` | `string` | 作者 / 维护者标识 |
| `description` | `string` | 简介 |
| `version` | `string` | 模组版本号 |
| `has_pck` | `bool` | 是否存在 `sts2_lan_connect.pck` |
| `has_dll` | `bool` | 是否存在 `sts2_lan_connect.dll` |
| `dependencies` | `string[]` | 依赖的其他 mod ID |
| `affects_gameplay` | `bool` | 是否参与联机时的 MOD 一致性检查 |

## 2. 运行时配置文件

文件名：

- `config.json`

运行时位置：

- 最终保存在游戏 MOD 目录的 `mods/sts2_lan_connect/config.json`

字段说明：

| 字段 | 类型 | 含义 |
| --- | --- | --- |
| `LastEndpoint` | `string` | 最近一次输入的连接地址 |
| `ClientNetId` | `ulong` | 本地客户端用于 ENet 连接的 ID |
| `PreferredPlayerName` | `string` | 偏好昵称 |
| `ChatPanelPositionX` | `float?` | 聊天面板 X 位置 |
| `ChatPanelPositionY` | `float?` | 聊天面板 Y 位置 |
| `ChatPanelCollapsed` | `bool` | 聊天面板是否折叠 |

## 3. 构建脚本接口

### Windows `build-sts2-lan-connect-windows.ps1`

| 参数 | 类型 | 含义 |
| --- | --- | --- |
| `-Sts2Root` | `string` | 游戏根目录 |
| `-GodotBin` | `string` | Godot 可执行文件路径 |
| `-DotnetBin` | `string` | dotnet 可执行文件，默认 `dotnet` |
| `-ModsDir` | `string` | 指定复制到哪个 MOD 目录 |
| `-SkipInstallCopy` | `switch` | 只构建，不复制到游戏目录 |

### macOS `build-sts2-lan-connect.sh`

| 选项 | 含义 |
| --- | --- |
| `--game-dir <path>` | 游戏根目录 |
| `--godot-bin <path>` | Godot 可执行文件 |
| `--dotnet-bin <path>` | dotnet 可执行文件 |
| `--mods-dir <path>` | 指定 MOD 输出目录 |
| `--skip-install-copy` | 只构建，不复制到游戏目录 |

## 4. 打包脚本接口

### Windows `package-sts2-lan-connect-windows.ps1`

| 参数 | 类型 | 含义 |
| --- | --- | --- |
| `-Sts2Root` | `string` | 游戏根目录 |
| `-GodotBin` | `string` | Godot 可执行文件 |
| `-DotnetBin` | `string` | dotnet 可执行文件 |

### macOS `package-sts2-lan-connect.sh`

使用环境变量：

| 变量 | 含义 |
| --- | --- |
| `STS2_ROOT` | 游戏根目录 |
| `GODOT_BIN` | Godot 可执行文件 |
| `DOTNET_BIN` | dotnet 可执行文件 |

## 5. 安装脚本接口

### Windows `install-sts2-lan-connect-windows.ps1`

| 参数 | 类型 | 含义 |
| --- | --- | --- |
| `-GameDir` | `string` | 游戏目录 |
| `-PackageDir` | `string` | 发布包目录，默认当前脚本目录 |
| `-UserDataDir` | `string` | 用户存档目录 |
| `-NoSaveSync` | `switch` | 跳过存档同步 |

### macOS `install-sts2-lan-connect-macos.sh`

| 选项 | 含义 |
| --- | --- |
| `--app-path <path>` | `SlayTheSpire2.app` 完整路径 |
| `--game-dir <path>` | 包含 `SlayTheSpire2.app` 的游戏目录 |
| `--data-dir <path>` | 用户存档目录 |
| `--package-dir <path>` | 发布包目录 |
| `--no-save-sync` | 跳过存档同步 |

## 6. 发布包内容契约

本地发布目录 `local-release/sts2_lan_connect/` 内应包含：

- `sts2_lan_connect.json`
- `sts2_lan_connect.dll`
- `sts2_lan_connect.pck`
- `README.md`
- `STS2_LAN_CONNECT_USER_GUIDE_ZH.md`
- `install-sts2-lan-connect-macos.sh`
- `install-sts2-lan-connect-windows.ps1`
- `install-sts2-lan-connect-windows.bat`

其中：

- `README.md` 来自仓库根目录的 `RELEASE_README.md`
- `STS2_LAN_CONNECT_USER_GUIDE_ZH.md` 来自 `docs/STS2_LAN_CONNECT_USER_GUIDE_ZH.md`
