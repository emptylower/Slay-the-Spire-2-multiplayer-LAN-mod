# STS2 LAN Connect 构建与发布

## 前置条件

构建这个项目需要本地已有《Slay the Spire 2》安装目录，因为编译时会直接引用游戏里的程序集：

- `sts2.dll`
- `0Harmony.dll`
- `Steamworks.NET.dll`

建议环境：

- `.NET SDK 9`
- Godot `4.5.1`
- 本地可运行的游戏安装目录

## 构建脚本

### Windows

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-sts2-lan-connect-windows.ps1
```

常见显式传参：

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\build-sts2-lan-connect-windows.ps1 `
  -Sts2Root 'D:\SteamLibrary\steamapps\common\Slay the Spire 2' `
  -GodotBin 'C:\Path\To\Godot_v4.5.1-stable_win64_console.exe'
```

### macOS

```bash
./scripts/build-sts2-lan-connect.sh
```

### Linux

```bash
./scripts/build-sts2-lan-connect.sh
```

## 打包脚本

### Windows

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\package-sts2-lan-connect-windows.ps1
```

### macOS

```bash
./scripts/package-sts2-lan-connect.sh
```

### Linux

```bash
./scripts/package-sts2-lan-connect.sh
```

打包脚本会先调用构建脚本，再生成本地发布目录和 zip。

## 安装脚本

### Windows

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\install-sts2-lan-connect-windows.ps1 `
  -PackageDir .\local-release\sts2_lan_connect
```

### macOS

```bash
chmod +x ./scripts/install-sts2-lan-connect-macos.sh
./scripts/install-sts2-lan-connect-macos.sh --package-dir ./local-release/sts2_lan_connect
```

### Linux

```bash
chmod +x ./scripts/install-sts2-lan-connect-linux.sh
./scripts/install-sts2-lan-connect-linux.sh --package-dir ./local-release/sts2_lan_connect
```

如果只想安装 MOD，不想同步存档：

- Windows：追加 `-NoSaveSync`
- Linux：追加 `--no-save-sync`
- macOS：追加 `--no-save-sync`

## 输出位置

### 编译 / 打包输出

| 路径 | 说明 |
| --- | --- |
| `sts2-lan-connect/sts2_lan_connect.json` | manifest |
| `sts2-lan-connect/.godot/mono/temp/bin/Debug/sts2_lan_connect.dll` | DLL 输出 |
| `sts2-lan-connect/build/sts2_lan_connect.pck` | PCK 输出 |

### 本地产物

| 路径 | 说明 |
| --- | --- |
| `local-release/sts2_lan_connect/` | 本地发布目录 |
| `local-release/sts2_lan_connect-v<version>-windows.zip` | 本地 Windows 包 |
| `local-release/sts2_lan_connect-v<version>-macos.zip` | 本地 macOS 包 |
| `local-release/sts2_lan_connect-v<version>-linux.zip` | 本地 Linux 包 |

### 仓库发布包

| 路径 | 说明 |
| --- | --- |
| `releases/sts2_lan_connect-v<version>-windows.zip` | 提交到仓库的 Windows 包 |

## `.build_output/` 的作用

Windows 构建脚本会额外使用：

- `.build_output/mods/`
- `.build_output/appdata/`
- `.build_output/localappdata/`
- `.build_output/tmp/`

这些目录都是本地临时工作目录，主要用于：

- 给 `dotnet build` 提供临时 MOD 复制位置
- 给 Godot headless 提供隔离的 `APPDATA / LOCALAPPDATA / TEMP`

它们都是临时产物，不属于发布内容。

## 统一后的发布约定

现在仓库中的“发布相关目录”约定为：

- `local-release/`
  本地临时产物，脚本自动生成，`.gitignore` 已忽略
- `releases/`
  经过确认后提交到仓库的预编译发布包

`sts2-lan-connect/release/` 不再作为正式流程的一部分。

## 一次完整的发布流程

1. 修改 `sts2-lan-connect/sts2_lan_connect.json` 中的版本号
2. 运行打包脚本，生成 `local-release/`
3. 检查 `local-release/sts2_lan_connect/` 内的文件是否完整
4. 确认 zip 可以正常安装
5. 需要提交仓库发布包时，把 zip 复制到 `releases/`
6. 提交代码和对应版本的发布包
