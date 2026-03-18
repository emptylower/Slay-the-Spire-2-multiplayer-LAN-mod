# STS2 LAN Connect

`STS2 LAN Connect` 是一个《杀戮尖塔 2》多人联机 MOD。

它保留原生 Steam 多人流程，同时补充一条基于 `IPv4 / IP:Port` 的局域网直连路径，方便在同一局域网、虚拟局域网或 Mesh VPN 环境下直接联机。

当前版本：

- MOD 版本：`0.1.3`
- GitHub Release tag：`v0.1.3`

## 项目定位

- 复用游戏现有的 `ENet` 与 `JoinFlow`，不自定义多人协议
- 在原有多人界面上增加 `局域网创建`、`局域网继续`、`Join via IP`
- 增加联机昵称同步与 LAN 聊天面板
- 适配官方 `v0.99.1` 之后的新模组清单结构

当前打包模型：

- 独立的 `sts2_lan_connect.json` 是模组声明入口
- `json` 声明本 MOD 同时包含 `dll` 和 `pck`
- `mod_manifest.json` 已废弃，不再打进 `.pck`

## 文档导航

- `docs/README.md`：文档导航与阅读顺序
- `docs/architecture.md`：项目原理、运行时链路、模块职责
- `docs/source-layout.md`：源码结构、目录职责、后续整理建议
- `docs/build-and-release.md`：构建、打包、安装、发布流程
- `docs/interfaces.md`：manifest、配置文件、脚本参数与产物约定
- `docs/STS2_LAN_CONNECT_USER_GUIDE_ZH.md`：面向玩家的使用说明
- `RELEASE_README.md`：发布包内 `README.md` 的来源文件

## 仓库结构

- `sts2-lan-connect/`：Godot + C# MOD 工程
- `scripts/`：构建、打包、安装脚本
- `docs/`：当前维护中的项目文档
- `references/decompiled/`：反编译参考文件，不参与编译
- `releases/`：提交到仓库的预编译发布包
- `local-release/`：本地生成的打包产物目录，已加入 `.gitignore`

## 构建速览

前置条件：

- 本地已安装《Slay the Spire 2》
- `.NET SDK 9`
- Godot `4.5.1`

常用命令：

```bash
./scripts/build-sts2-lan-connect.sh
./scripts/package-sts2-lan-connect.sh
```

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-sts2-lan-connect-windows.ps1
powershell -ExecutionPolicy Bypass -File .\scripts\package-sts2-lan-connect-windows.ps1
```

更完整的参数说明见 `docs/build-and-release.md` 与 `docs/interfaces.md`。

## 产物位置

| 类型 | 路径 | 说明 |
| --- | --- | --- |
| MOD manifest | `sts2-lan-connect/sts2_lan_connect.json` | 官方 `v0.99.1+` 模组声明文件 |
| DLL | `sts2-lan-connect/.godot/mono/temp/bin/Debug/sts2_lan_connect.dll` | C# 编译输出 |
| PCK | `sts2-lan-connect/build/sts2_lan_connect.pck` | Godot 打包输出 |
| 本地打包目录 | `local-release/sts2_lan_connect/` | 本地发布目录 |
| 本地 zip | `local-release/sts2_lan_connect-v0.1.3-windows.zip` | 本地 Windows 包 |
| 仓库发布包 | `releases/sts2_lan_connect-v0.1.3-windows.zip` | 提交到仓库的预编译包 |

安装目标：

- Windows：`<SteamLibrary>/steamapps/common/Slay the Spire 2/mods/sts2_lan_connect/`
- macOS：`<SteamLibrary>/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/mods/sts2_lan_connect/`

## Release 说明

- 仓库中的正式预编译包统一放在 `releases/`
- 本地打包过程统一输出到 `local-release/`
- 发布包内包含什么、如何安装，统一以 `RELEASE_README.md` 为准

当前仓库内的 Windows 预编译包：

- `releases/sts2_lan_connect-v0.1.3-windows.zip`

## 兼容性与边界

- 所有联机玩家必须加载相同版本的 MOD，否则会触发官方 `ModMismatch`
- 当前版本只支持手动 `IPv4 / IP:Port`
- 不包含局域网自动发现、IPv6、NAT 穿透或公网联机支持
- 远程“局域网”联机建议搭配 Tailscale / Headscale / ZeroTier / 蒲公英 等虚拟局域网工具

## 版权与说明

- 本项目仅用于学习、研究和 MOD 开发测试
- 《Slay the Spire 2》及相关版权归 Mega Crit 所有
- 本项目与 Mega Crit 无官方关联
