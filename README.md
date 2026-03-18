# STS2 LAN Connect

`STS2 LAN Connect` 是一个《杀戮尖塔 2》多人联机 MOD。

它在不移除原有 Steam 多人入口的前提下，为多人模式补充了一条基于手动 `IPv4 / IP:Port` 的局域网直连路径，方便：

- 同一局域网内联机
- 借助虚拟局域网 / Mesh VPN 做远程“局域网”联机
- 在 Steam 好友入口之外，直接通过 IP 加入房主

当前版本：

- MOD 版本：`0.1.3`
- GitHub Release tag：`v0.1.3`

## v0.99.1 适配

- 适配官方 `v0.99.1` 的新模组清单结构
- 官方现在以独立的 `<mod_id>.json` 作为模组声明入口，`json` 本身才是必需文件
- `json` 需要声明该模组是否包含 `.pck` / `.dll`
- 本项目实现逻辑不变，但由于仍然依赖 Godot 资源和 C# 程序集，所以发布包继续包含 `sts2_lan_connect.json`、`sts2_lan_connect.dll` 和 `sts2_lan_connect.pck`
- 旧的 `mod_manifest.json` 不再打进 `.pck`

## 当前功能

- 多人首页新增 `局域网创建`
- 多人首页新增 `局域网继续`
- Join 页面新增 `LAN IP` 输入框和 `Join via IP`
- Join 页面和多人首页新增联机昵称输入，并同步到大厅/对局内玩家显示
- 大厅和对局内新增 LAN 聊天面板，支持拖动、折叠和位置持久化
- 复用游戏内置 `ENet` 与 `JoinFlow`
- 默认端口：`33771`
- 支持最近一次连接地址持久化
- 房主地址弹窗支持一键复制 `IP:Port`
- 提供 macOS / Windows 一键安装脚本
- 提供无 MOD 存档到 modded 存档的一键迁移 / 单向同步
- 安装脚本只补齐缺失的 modded 存档文件，不覆盖已有 modded 存档

## 仓库结构

- `sts2-lan-connect/`
  MOD 工程目录
- `scripts/`
  构建、打包、安装脚本
- `releases/`
  提交到仓库的预编译发布包
- `sts2-lan-connect/release/sts2_lan_connect/`
  本地打包生成的发布目录
- `LAN_MOD_RESEARCH.md`
  反编译与链路确认记录
- `LAN_MOD_IMPLEMENTATION_PLAN.md`
  实施思路与范围说明
- `STS2_LAN_CONNECT_USER_GUIDE_ZH.md`
  面向玩家的使用说明
- `STS2_LAN_CONNECT_INSTALL_USE_DEBUG_GUIDE_ZH.md`
  安装、使用与调试说明

## 下载预编译版本

如果你不想自己构建，可以直接下载预编译的发布包：

- **Windows 版本**: [`releases/sts2_lan_connect-v0.1.3-windows.zip`](releases/sts2_lan_connect-v0.1.3-windows.zip)
- **macOS 版本**: 暂未提供预编译版本，请参考下方构建说明自行构建

下载后解压，然后参考 [3. 一键安装](#3-一键安装) 进行安装。

## 快速开始

### 0. 构建前准备

本项目需要在本地有《Slay the Spire 2》安装目录，因为编译时会直接引用游戏目录中的：

- `sts2.dll`
- `0Harmony.dll`
- `Steamworks.NET.dll`

因此：

- 不能依赖 GitHub Actions 直接构建
- 需要在本地 Windows / macOS 机器上手动 build
- Windows 侧推荐使用标准版 Godot `4.5.1` 来打 `pck`，不要依赖不完整的 mono 发行包

Windows 本地构建建议准备：

- `.NET SDK 9`
- Godot `4.5.1` 标准版可执行文件，例如 `Godot_v4.5.1-stable_win64_console.exe`
- 本地可用的游戏目录，例如 `<SteamLibrary>\steamapps\common\Slay the Spire 2`

### 1. 构建

macOS：

```bash
./scripts/build-sts2-lan-connect.sh
```

Windows：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-sts2-lan-connect-windows.ps1
```

如果自动探测不到游戏目录或 Godot，可通过环境变量或命令行参数传入：

- `STS2_ROOT`
- `GODOT_BIN`
- `DOTNET_BIN`

Windows 建议优先使用标准版 Godot 4.5.1 可执行文件来打 `pck`。

Windows 推荐直接显式传参：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-sts2-lan-connect-windows.ps1 `
  -Sts2Root '<SteamLibrary>\steamapps\common\Slay the Spire 2' `
  -GodotBin '<GodotDir>\Godot_v4.5.1-stable_win64_console.exe'
```

构建成功后：

- `sts2_lan_connect.dll` 会输出到 `sts2-lan-connect/.godot/mono/temp/bin/Debug/`
- `sts2_lan_connect.pck` 会输出到 `sts2-lan-connect/build/`
- `sts2_lan_connect.json` 位于 `sts2-lan-connect/`
- 默认情况下脚本还会把这三个文件复制到游戏目录下的 `mods/sts2_lan_connect/`

### 2. 打包

macOS：

```bash
./scripts/package-sts2-lan-connect.sh
```

Windows：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\package-sts2-lan-connect-windows.ps1
```

Windows 推荐显式传参：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\package-sts2-lan-connect-windows.ps1 `
  -Sts2Root '<SteamLibrary>\steamapps\common\Slay the Spire 2' `
  -GodotBin '<GodotDir>\Godot_v4.5.1-stable_win64_console.exe'
```

打包成功后会生成：

- 本地发布目录：`sts2-lan-connect/release/sts2_lan_connect/`
- 本地 zip 包：`sts2-lan-connect/release/sts2_lan_connect-v0.1.3-windows.zip`

如果你希望把预编译包也提交到仓库，通常再手动复制一份到：

- `releases/sts2_lan_connect-v0.1.3-windows.zip`

### 3. 一键安装

macOS：

```bash
chmod +x ./scripts/install-sts2-lan-connect-macos.sh
./scripts/install-sts2-lan-connect-macos.sh --package-dir ./sts2-lan-connect/release/sts2_lan_connect
```

Windows：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\install-sts2-lan-connect-windows.ps1 -PackageDir .\sts2-lan-connect\release\sts2_lan_connect
```

## 构建说明

当前版本：

- MOD 版本：`0.1.3`
- Release tag：`v0.1.3`

## 典型联机流程

1. 房主启动游戏并加载同版本 MOD
2. 在多人首页点击 `局域网创建`
3. 把本机局域网地址或虚拟局域网地址发给队友
4. 队友在 `加入` 页面输入 `IP` 或 `IP:33771`，并可按需填写联机昵称
5. 点击 `Join via IP`
6. 进入大厅或对局后，可通过右下角 LAN 聊天面板交流

同机测试时，成员端可直接填：

```text
127.0.0.1
```

## Release 内容

当前发布目录：

- `sts2-lan-connect/release/sts2_lan_connect/`

当前打包文件：

- `releases/sts2_lan_connect-v0.1.3-windows.zip`
- `sts2-lan-connect/release/sts2_lan_connect-v0.1.3-windows.zip`

发布包内包含：

- `sts2_lan_connect.dll`
- `sts2_lan_connect.pck`
- `sts2_lan_connect.json`
- `README.md`
- `STS2_LAN_CONNECT_USER_GUIDE_ZH.md`
- `install-sts2-lan-connect-macos.sh`
- `install-sts2-lan-connect-windows.ps1`
- `install-sts2-lan-connect-windows.bat`

## 注意事项

- 所有联机玩家必须加载相同版本的 MOD，否则会触发官方 `ModMismatch`
- 当前 V1 只支持手动 IPv4 / `IP:Port`，不做自动发现
- 远程“局域网”联机建议搭配 Tailscale / Headscale / ZeroTier / 蒲公英 等虚拟局域网方案

## 版权与说明

- 本项目仅用于学习、研究和 MOD 开发测试
- 《Slay the Spire 2》及相关版权归 Mega Crit 所有
- 本项目与 Mega Crit 无官方关联
