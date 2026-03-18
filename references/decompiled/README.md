# 反编译参考文件

这个目录里的文件只用于维护和研究，不参与本项目编译。

## 文件说明

- `Hook.cs`
  反编译得到的 Hook 参考快照，用来理解游戏暴露了哪些扩展点

- `ModManager.legacy.cs`
  旧版 `ModManager` 反编译快照，主要用于历史对照  
  注意：它不代表当前 `v0.99.1+` 的官方加载逻辑，尤其不能作为新版 manifest 结构的依据

## 为什么把它们移出仓库根目录

这两个文件容易被误认为是当前项目源码，但实际上：

- 它们不在 `sts2-lan-connect/sts2_lan_connect.csproj` 编译范围内
- 它们只是反编译参考，不是项目实现

把它们放到 `references/decompiled/`，可以减少对贡献者的误导。
