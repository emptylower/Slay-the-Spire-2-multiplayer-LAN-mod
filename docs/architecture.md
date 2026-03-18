# STS2 LAN Connect 架构说明

## 目标

`STS2 LAN Connect` 的目标不是重写《杀戮尖塔 2》的多人系统，而是在官方现有多人链路上补出一条可手动输入 `IPv4 / IP:Port` 的 LAN 直连入口。

设计原则：

- 保留 Steam 原生多人入口
- 复用官方 `ENet` 与 `JoinFlow`
- 尽量把改动限制在 UI 注入、连接初始化和少量同步逻辑
- 不引入自定义多人协议

## 打包模型

官方 `v0.99.1` 之后：

- 模组声明文件必须是独立的 `<mod_id>.json`
- `json` 决定该模组是否包含 `.dll` / `.pck`

本项目当前的 manifest 是：

- `sts2-lan-connect/sts2_lan_connect.json`

它声明：

- `has_dll: true`
- `has_pck: true`

也就是说，本项目虽然已经适配新的 manifest 结构，但运行时仍然同时依赖：

- `sts2_lan_connect.dll`
- `sts2_lan_connect.pck`

## 启动链路

入口文件：

- `sts2-lan-connect/Scripts/Entry.cs`

初始化顺序：

1. 加载 `config.json`
2. 重置聊天与昵称同步状态
3. 安装运行时监控节点

运行时监控节点在：

- `sts2-lan-connect/Scripts/LanConnectRuntimeMonitor.cs`

它的职责不是直接修改游戏流程，而是周期性扫描场景树，确保需要的 UI 和同步逻辑被补到正确的页面或节点上。

## UI 注入点

### 多人首页

文件：

- `sts2-lan-connect/Scripts/Patches.MultiplayerSubmenu.cs`

职责：

- 注入 `局域网创建`
- 注入 `局域网继续`

### Host 页面

文件：

- `sts2-lan-connect/Scripts/Patches.HostSubmenu.cs`
- `sts2-lan-connect/Scripts/LanConnectHostFlow.cs`

职责：

- 在 Host 页面补充 LAN Host 入口
- 触发官方 `StartENetHost(...)`
- Host 成功后继续沿用官方开局 / 继续游戏页面跳转

### Join 页面

文件：

- `sts2-lan-connect/Scripts/Patches.JoinFriendScreen.cs`
- `sts2-lan-connect/Scripts/LanConnectNetUtil.cs`

职责：

- 注入 `LAN IP` 输入框和 `Join via IP`
- 解析 `IP` 或 `IP:Port`
- 生成 `ENetClientConnectionInitializer`
- 直接走官方 `JoinGameAsync(...)`

## 聊天与昵称同步

相关文件：

- `sts2-lan-connect/Scripts/LanChatPanel.cs`
- `sts2-lan-connect/Scripts/LanChatSync.cs`
- `sts2-lan-connect/Scripts/LanChatMessage.cs`
- `sts2-lan-connect/Scripts/LanChatUiPatches.cs`
- `sts2-lan-connect/Scripts/LanPlayerProfileSync.cs`
- `sts2-lan-connect/Scripts/LanPlayerProfileMessage.cs`
- `sts2-lan-connect/Scripts/LanPlayerProfileRegistry.cs`

职责划分：

- `LanChatPanel`：聊天 UI
- `LanChatSync`：聊天消息缓存、发送、接收、版本号管理
- `LanChatMessage`：聊天消息结构
- `LanChatUiPatches`：把聊天面板挂到大厅 / 对局 UI
- `LanPlayerProfileSync`：昵称广播与应用
- `LanPlayerProfileMessage`：昵称同步消息结构
- `LanPlayerProfileRegistry`：昵称规范化、缓存与查询

## 本地配置与持久化

文件：

- `sts2-lan-connect/Scripts/LanConnectConfig.cs`

配置文件路径：

- 运行时最终会落在游戏 MOD 目录下的 `config.json`

目前保存的信息包括：

- 最近一次连接地址
- 客户端 `netId`
- 偏好昵称
- 聊天面板位置
- 聊天面板折叠状态

## 常量与辅助模块

- `sts2-lan-connect/Scripts/LanConnectConstants.cs`
  维护默认端口、UI 节点名、长度限制等常量
- `sts2-lan-connect/Scripts/LanConnectPopupUtil.cs`
  统一弹窗展示
- `sts2-lan-connect/Scripts/LanConnectNetUtil.cs`
  地址解析、候选地址生成、`netId` 生成等网络辅助逻辑

## 当前边界

当前版本明确不做：

- 局域网房间自动发现
- IPv6
- NAT 穿透 / 公网联机
- 自定义多人协议
- 对其他多人 MOD 的兼容承诺
