# STS2 LAN Connect 使用说明

## 功能

`STS2 LAN Connect` 会给《杀戮尖塔 2》多人界面增加一条局域网联机路径：

- 多人首页新增 `局域网创建`
- Join 页面新增 `LAN IP` 输入框和 `Join via IP`

启用 MOD 后：

- 你仍然可以照常用 Steam 好友列表加入房间
- 也可以直接输入 `IP` 或 `IP:端口` 连接局域网主机

## 安装

将 MOD 文件夹放进游戏的 `mods` 目录。

目录内至少应包含：

- `sts2_lan_connect.dll`
- `sts2_lan_connect.pck`

### Windows

游戏目录通常类似：

```text
<Steam>/steamapps/common/Slay the Spire 2/mods/sts2_lan_connect/
```

### macOS

Mac 版实际加载目录在 `.app` 内：

```text
/Users/<用户名>/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/mods/sts2_lan_connect/
```

## 一键安装与存档迁移

发布包内会附带两个安装脚本：

- `install-sts2-lan-connect-macos.sh`
- `install-sts2-lan-connect-windows.ps1`
- `install-sts2-lan-connect-windows.bat`

这两个脚本都会做两件事：

1. 一键把 `sts2_lan_connect.dll` 和 `sts2_lan_connect.pck` 安装到游戏 `mods` 目录
2. 把无 MOD 存档单向同步到 modded 存档目录

同步规则：

- 源目录：无 MOD 存档
- 目标目录：modded 存档
- 只会补齐缺失的 modded 文件
- 已存在的 modded 文件一律不覆盖
- 不会删除 modded 目录里已有的额外文件

建议在关闭游戏后执行。

### macOS 一键安装

在解压后的发布包目录里执行：

```bash
chmod +x ./install-sts2-lan-connect-macos.sh
./install-sts2-lan-connect-macos.sh
```

### Windows 一键安装

在解压后的发布包目录里执行：

```powershell
powershell -ExecutionPolicy Bypass -File .\install-sts2-lan-connect-windows.ps1
```

或者直接双击：

```text
install-sts2-lan-connect-windows.bat
```

如果只想安装 MOD，不想同步存档：

- macOS：`./install-sts2-lan-connect-macos.sh --no-save-sync`
- Windows：`powershell -ExecutionPolicy Bypass -File .\install-sts2-lan-connect-windows.ps1 -NoSaveSync`

## 第一次启用

第一次加载 MOD 时，游戏会先弹出一次模组警告确认。

- 需要在游戏里同意启用 MOD
- 某些情况下游戏会要求重启一次后才正式加载

## 如何开局域网主机

1. 启动游戏并启用 MOD。
2. 进入多人游戏模式。
3. 在多人首页点击 `局域网创建`。
4. 进入角色选择/房间后，MOD 会弹出一条提示，显示当前应分享给好友的地址，例如：

```text
192.168.1.20:33771
```

把这个地址发给你的好友。

## 如何通过 IP 加入

1. 进入多人游戏模式。
2. 打开 `加入` 页面。
3. 在页面下方的 `LAN 直连` 输入框中填写：

```text
192.168.1.20
```

或者：

```text
192.168.1.20:33771
```

4. 点击 `Join via IP`。

支持：

- `localhost`
- `127.0.0.1`
- 局域网 IPv4 地址

不支持：

- IPv6
- 局域网自动发现列表

## 常见问题

### 1. 明明在同一局域网，还是连不上

- 确认主机和客户端在同一网段
- 确认主机端系统防火墙允许游戏通信
- 确认没有输错端口，默认端口是 `33771`

### 2. Steam 好友加入功能还在吗

在。这个 MOD 不会替换 Steam 好友加入，只是新增一条 LAN 分支。

### 3. 如果不是从 Steam 启动会怎样

- Steam 好友房间列表可能不可用
- `局域网创建` 和 `Join via IP` 这条 LAN 分支仍然可以工作

### 4. 为什么输入框只支持 IPv4

这是 V1 的范围控制。后续如果要扩到自动发现或 IPv6，再单独做增强版本。
