# STS2 LAN Connect 安装、使用与 Debug 教学

这份文档是当前仓库内的单文件教学说明，覆盖三件事：

- 如何安装 `STS2 LAN Connect`
- 如何实际使用它进行本地/局域网联机
- 如何在开发和测试阶段做构建、日志排查与问题定位

## 1. 你现在手上有什么

当前仓库已经具备可直接构建和打包的工程与脚本：

- MOD 工程目录：`/Users/mac/Desktop/STS2_Learner/sts2-lan-connect`
- 构建脚本：`/Users/mac/Desktop/STS2_Learner/scripts/build-sts2-lan-connect.sh`
- 打包脚本：`/Users/mac/Desktop/STS2_Learner/scripts/package-sts2-lan-connect.sh`
- 用户说明：`/Users/mac/Desktop/STS2_Learner/STS2_LAN_CONNECT_USER_GUIDE_ZH.md`
- 反编译记录：`/Users/mac/Desktop/STS2_Learner/LAN_MOD_RESEARCH.md`
- macOS 一键安装脚本：`/Users/mac/Desktop/STS2_Learner/scripts/install-sts2-lan-connect-macos.sh`
- Windows 一键安装脚本：`/Users/mac/Desktop/STS2_Learner/scripts/install-sts2-lan-connect-windows.ps1`

当前默认产物名固定为：

- `sts2_lan_connect.dll`
- `sts2_lan_connect.pck`

## 2. 安装方式

### 2.1 直接安装已打好的包

如果你已经有构建好的压缩包：

`/Users/mac/Desktop/STS2_Learner/sts2-lan-connect/release/sts2_lan_connect-macos.zip`

解压后会得到目录：

```text
sts2_lan_connect/
├── sts2_lan_connect.dll
├── sts2_lan_connect.pck
├── install-sts2-lan-connect-macos.sh
├── install-sts2-lan-connect-windows.ps1
├── install-sts2-lan-connect-windows.bat
└── STS2_LAN_CONNECT_USER_GUIDE_ZH.md
```

把整个 `sts2_lan_connect` 文件夹放进游戏的 `mods` 目录。

如果你不想手动复制，也可以直接运行发布包里自带的一键安装脚本。

### 2.2 从当前仓库自行构建再安装

在仓库根目录执行：

```bash
source ~/.zprofile
cd /Users/mac/Desktop/STS2_Learner
./scripts/build-sts2-lan-connect.sh
```

这个脚本会自动：

1. 编译 C# 代码
2. 打出 `.pck`
3. 把 `.dll + .pck` 复制到游戏的 `mods/sts2_lan_connect/`

如果你想顺手出一个发布包：

```bash
./scripts/package-sts2-lan-connect.sh
```

### 2.2.1 一键安装并同步存档

#### macOS

```bash
chmod +x ./scripts/install-sts2-lan-connect-macos.sh
./scripts/install-sts2-lan-connect-macos.sh --package-dir /Users/mac/Desktop/STS2_Learner/sts2-lan-connect/release/sts2_lan_connect
```

#### Windows

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\install-sts2-lan-connect-windows.ps1 -PackageDir .\sts2-lan-connect\release\sts2_lan_connect
```

这两个脚本会：

1. 把 MOD 文件安装到游戏目录
2. 把无 MOD 存档单向同步到 modded 存档目录
3. 只补齐缺失的 modded 文件，不覆盖任何已存在的 modded 文件

### 2.3 安装路径

#### macOS

当前这台机器的实际加载目录是：

```text
/Users/mac/Library/Application Support/Steam/steamapps/common/Slay the Spire 2/SlayTheSpire2.app/Contents/MacOS/mods/sts2_lan_connect/
```

#### Windows

Windows 版通常应放在：

```text
<Steam>/steamapps/common/Slay the Spire 2/mods/sts2_lan_connect/
```

## 3. 第一次启用

第一次加载 MOD 时，游戏会先检测到 `.pck`，但如果你还没有在游戏里同意模组警告，它会跳过加载。

当前日志里已经实际出现过这条信息：

```text
Found mod pck file .../mods/sts2_lan_connect/sts2_lan_connect.pck
Skipping loading mod sts2_lan_connect.pck, user has not yet seen the mods warning
```

所以第一次启动时要做这件事：

1. 启动游戏
2. 在游戏里同意启用 MOD
3. 如果游戏要求，重启一次

只有做完这一步，MOD 的初始化日志和 UI 注入才会真正生效。

## 4. 如何使用

### 4.1 局域网创建

启用 MOD 后，进入多人游戏首页。

你会看到新增入口：

- `局域网创建`

点击后会：

1. 走游戏内置 ENet Host 链路
2. 进入标准模式房间/角色选择
3. 弹出提示，显示本机可分享给好友的局域网地址

示例：

```text
192.168.1.20:33771
```

这是你发给好友的地址。

### 4.2 Join via IP

进入多人游戏的“加入”页面后，页面下方会新增一个 `LAN 直连` 区块，包含：

- 一个 IP 输入框
- 一个 `Join via IP` 按钮

支持输入：

```text
192.168.1.20
192.168.1.20:33771
localhost
127.0.0.1
```

不支持：

- IPv6
- 自动扫描局域网房间

如果不写端口，默认端口就是：

```text
33771
```

### 4.3 推荐测试顺序

先按这个顺序测，不要一开始就上跨平台：

1. 同机双开：`127.0.0.1`
2. 同平台局域网：`Mac -> Mac` 或 `Win -> Win`
3. 跨平台局域网：`Mac -> Win` 与 `Win -> Mac`

## 5. Debug 与排查

### 5.1 最常用的命令

构建并复制到游戏目录：

```bash
./scripts/build-sts2-lan-connect.sh
```

构建并打发布 zip：

```bash
./scripts/package-sts2-lan-connect.sh
```

### 5.2 核对产物是否存在

先确认游戏目录里真的有这两个文件：

```text
.../mods/sts2_lan_connect/sts2_lan_connect.dll
.../mods/sts2_lan_connect/sts2_lan_connect.pck
```

### 5.3 查看游戏日志

macOS 当前日志目录：

```text
/Users/mac/Library/Application Support/SlayTheSpire2/logs/
```

最常看的是：

```text
/Users/mac/Library/Application Support/SlayTheSpire2/logs/godot.log
```

### 5.4 日志里应该看什么

#### A. 先确认游戏找到了 MOD

关键字：

```text
Found mod pck file
sts2_lan_connect.pck
```

如果没看到，先排查放置目录。

#### B. 如果看到了但没加载

关键字：

```text
Skipping loading mod ... user has not yet seen the mods warning
```

这说明：

- 目录对了
- 但游戏里还没确认启用 MOD

#### C. 真正初始化成功后应该看到

关键字：

```text
sts2_lan_connect initialized.
```

如果没有这条，但已经不再出现 “mods warning” 的跳过日志，就需要排查 DLL 是否加载失败。

#### C-2. 打开对应页面后，UI 注入成功应看到

打开多人模式相关页面后，再看日志，当前版本会额外打印：

```text
sts2_lan_connect injected LAN join UI via ...
sts2_lan_connect injected LAN create button via ...
```

这两条分别说明：

- Join 页 `LAN 直连` 区块已经实际注入到场景树
- 多人首页 `局域网创建` 按钮已经实际注入到场景树

如果已经有 `sts2_lan_connect initialized.`，但没有这两条，那么问题就不在 MOD 加载，而在页面生命周期没有触发到 UI 注入逻辑。

如果这两条已经出现，但界面上仍然看不到 UI，那么问题就基本收敛为：

- 注入节点挂到了错误的父节点
- 或者布局/尺寸导致控件实际存在但不可见

#### D. 如果 Steam 不可用

如果不是从 Steam 正常启动，日志可能出现：

```text
Steamworks initialization failed
Steam is running: False
```

这会影响：

- Steam 好友列表
- Steam Host / Steam Join

但不影响：

- `局域网创建`
- `Join via IP`

### 5.5 当前功能对应的关键代码入口

多人首页注入：

- `/Users/mac/Desktop/STS2_Learner/sts2-lan-connect/Scripts/Patches.MultiplayerSubmenu.cs`

Join 页面注入：

- `/Users/mac/Desktop/STS2_Learner/sts2-lan-connect/Scripts/Patches.JoinFriendScreen.cs`

LAN Host 流程：

- `/Users/mac/Desktop/STS2_Learner/sts2-lan-connect/Scripts/LanConnectHostFlow.cs`

IP 解析与本机地址检测：

- `/Users/mac/Desktop/STS2_Learner/sts2-lan-connect/Scripts/LanConnectNetUtil.cs`

最近一次地址持久化：

- `/Users/mac/Desktop/STS2_Learner/sts2-lan-connect/Scripts/LanConnectConfig.cs`

### 5.6 配置文件位置

当前实现会把最近一次连接地址写入 `config.json`。

正常情况下它位于 MOD 目录内，例如 macOS：

```text
.../mods/sts2_lan_connect/config.json
```

### 5.7 常见问题

#### 1. 页面里没看到 `局域网创建`

- 确认 MOD 已真正加载，不只是被扫描到
- 查看日志里是否已经出现 `sts2_lan_connect initialized.`

#### 2. 页面里没看到 `LAN 直连` 输入框

- 同样先确认 MOD 真的加载了
- 再看是否有 `failed to inject LAN join controls` 之类日志
- 再看是否已经出现 `sts2_lan_connect injected LAN join UI via ...`

#### 3. 点了 Join via IP 没反应

- 先看输入是否合法
- 当前只接受 IPv4 / `localhost`
- 再看主机端是否已经先开了 `局域网创建`

#### 4. 同机测试失败

先只测：

```text
Host: 局域网创建
Join: 127.0.0.1
```

如果这个都不通，优先排查 MOD 是否真的加载、顶层 `局域网创建` 按钮是否真正进入 ENet Host。

#### 5. 局域网下别人连不上

- 确认双方在同一网段
- 确认主机系统防火墙没有拦 `33771`
- 确认分享出去的是主机的局域网地址，不是 `127.0.0.1`

## 6. 当前状态与下一步建议

当前已经完成：

- 工程构建
- 打包
- 复制到游戏目录
- 日志级验证：游戏能发现 `sts2_lan_connect.pck`

当前还需要你手动完成的最关键一步：

- 在游戏里第一次同意 MOD 警告，让 MOD 真正执行初始化

完成这一步后，建议马上做一次最小验证：

1. 打开游戏
2. 确认 Join 页是否出现 `LAN 直连`
3. 确认多人首页是否出现 `局域网创建`
4. 做一轮 `127.0.0.1` 同机双开测试

如果你下一步要继续推进，最合理的顺序是：

1. 先做 UI 真机截图验证
2. 再做同机 localhost 测试
3. 最后做 Mac / Windows 局域网互连测试
