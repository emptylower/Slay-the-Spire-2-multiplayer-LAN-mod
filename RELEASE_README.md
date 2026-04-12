# STS2 LAN Connect Release

这是 `STS2 LAN Connect` 发布包内的 `README.md`。

当前版本：

- MOD 版本：`0.1.3`
- GitHub Release tag：`v0.1.3`

## 这个包里有什么

- `README.md`
- `sts2_lan_connect.dll`
- `sts2_lan_connect.pck`
- `sts2_lan_connect.json`
- `STS2_LAN_CONNECT_USER_GUIDE_ZH.md`
- `install-sts2-lan-connect-linux.sh`
- `install-sts2-lan-connect-macos.sh`
- `install-sts2-lan-connect-windows.ps1`
- `install-sts2-lan-connect-windows.bat`

## 这是什么

`STS2 LAN Connect` 会给《杀戮尖塔 2》多人界面增加一条局域网联机路径：

- 多人首页新增 `局域网创建`
- 多人首页新增 `局域网继续`
- Join 页面新增 `LAN IP` 输入框和 `Join via IP`
- 房主地址弹窗支持一键复制 `IP:Port`

## v0.99.1 打包说明

- 官方现在以独立的 `sts2_lan_connect.json` 作为模组声明文件
- `json` 会声明本 MOD 同时包含 `.dll` 和 `.pck`
- 本包已经按新结构组织好，直接解压后安装即可

## 一键安装

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

### Windows

```powershell
powershell -ExecutionPolicy Bypass -File .\install-sts2-lan-connect-windows.ps1
```

或者直接双击：

```text
install-sts2-lan-connect-windows.bat
```

## 存档迁移

安装脚本会顺手做一次从“无 MOD 存档”到“modded 存档”的单向同步：

- 只复制缺失文件
- 不覆盖任何已存在的 modded 文件
- 不删除 modded 目录里已有的额外文件

如果只想安装，不想同步存档：

- Linux：`./install-sts2-lan-connect-linux.sh --no-save-sync`
- macOS：`./install-sts2-lan-connect-macos.sh --no-save-sync`
- Windows：`powershell -ExecutionPolicy Bypass -File .\install-sts2-lan-connect-windows.ps1 -NoSaveSync`

## 基本使用

1. 房主进入多人模式首页，点击 `局域网创建` 或 `局域网继续`
2. 房主把自己的局域网 IP / 虚拟局域网 IP 发给队友
3. 队友进入 `加入` 页面
4. 在 `LAN IP` 输入框中填写 `IP` 或 `IP:33771`
5. 点击 `Join via IP`

默认端口：

```text
33771
```

更详细的玩家说明见：

- `STS2_LAN_CONNECT_USER_GUIDE_ZH.md`

## 兼容性说明

- 所有玩家必须加载同版本 MOD
- V1 只支持手动 IPv4 / `IP:Port`
- 如需远程“局域网”联机，建议搭配虚拟局域网工具

## 版权说明

- 本项目仅用于学习、研究和 MOD 开发测试
- 《Slay the Spire 2》及相关版权归 Mega Crit 所有
