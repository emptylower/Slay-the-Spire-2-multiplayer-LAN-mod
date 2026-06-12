# STS2 LAN Connect 使用说明

## 这是什么

`STS2 LAN Connect` 会给《杀戮尖塔 2》多人模式增加一条 LAN 直连入口。

启用后你可以：

- 继续使用 Steam 原生好友联机
- 也可以直接输入 `IP` 或 `IP:端口` 加入房主

当前主要功能：

- 多人首页新增 `局域网创建`
- 多人首页新增 `局域网继续`
- Join 页面新增 `LAN IP` 输入框和 `Join via IP`
- 支持联机昵称同步
- 支持大厅 / 对局内 LAN 聊天面板

## 安装后目录应包含什么

游戏 `mods/sts2_lan_connect/` 目录内至少应包含：

- `sts2_lan_connect.json`
- `sts2_lan_connect.dll`
- `sts2_lan_connect.pck`

说明：

- 官方 `v0.99.1` 之后，`json` 是模组声明入口
- 本 MOD 当前仍然需要 `.dll` 和 `.pck`

## 一键安装

发布包内已经附带安装脚本。

### Windows

```powershell
powershell -ExecutionPolicy Bypass -File .\install-sts2-lan-connect-windows.ps1
```

或者直接双击：

```text
install-sts2-lan-connect-windows.bat
```

### Linux

```bash
chmod +x ./install-sts2-lan-connect-linux.sh
./install-sts2-lan-connect-linux.sh
```

### macOS

```bash
chmod +x ./install-sts2-lan-connect-macos.sh
./install-sts2-lan-connect-macos.sh
```

如果只想安装 MOD，不想同步存档：

- Windows：追加 `-NoSaveSync`
- Linux：追加 `--no-save-sync`
- macOS：追加 `--no-save-sync`

## 第一次启用

第一次加载 MOD 时：

- 游戏可能会先弹出一次模组警告
- 需要在游戏里确认启用
- 某些情况下需要重启一次游戏

## 如何开局域网主机

1. 启动游戏并加载 MOD
2. 进入多人模式
3. 点击 `局域网创建` 或 `局域网继续`
4. 把房主机器的 `IP:Port` 发给队友

默认端口：

```text
33771
```

同机双开测试时，可以直接使用：

```text
127.0.0.1:33771
```

## 如何通过 IP 加入

1. 进入多人模式的 `加入` 页面
2. 在 `LAN IP` 输入框中填写：

```text
192.168.1.20
```

或：

```text
192.168.1.20:33771
```

3. 点击 `Join via IP`

## 常见问题

### 1. 为什么还是需要三个文件

因为本 MOD 当前仍然同时使用：

- 独立 manifest：`sts2_lan_connect.json`
- C# 逻辑：`sts2_lan_connect.dll`
- Godot 资源：`sts2_lan_connect.pck`

### 2. Steam 好友联机还能用吗

可以。本 MOD 只是增加 LAN 分支，不会移除原生 Steam 多人入口。

### 3. 如果连不上怎么办

优先检查：

- 房主和客户端是否在同一网络 / 虚拟局域网
- 房主系统防火墙是否允许游戏通信
- 是否填写了正确的 IP 和端口

### 4. 支持 IPv6 吗

当前版本不支持。

## 兼容性说明

- 所有联机玩家必须加载相同版本的 MOD
- 当前版本只支持手动 `IPv4 / IP:Port`
- 不支持局域网自动发现
