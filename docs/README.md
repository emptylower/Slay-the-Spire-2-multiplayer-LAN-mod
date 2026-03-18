# STS2 LAN Connect 文档导航

这组文档面向贡献者和维护者，目标是回答三个问题：

1. 这个项目的原理是什么
2. 代码和目录是怎么组织的
3. 应该如何构建、打包、安装和发布

建议阅读顺序：

- `docs/architecture.md`
  项目原理、运行时链路、主要模块职责
- `docs/source-layout.md`
  仓库结构、源码组织、哪些目录是生成产物、后续可继续整理的方向
- `docs/build-and-release.md`
  构建、打包、安装、发布流程，以及本地与仓库内产物的统一约定
- `docs/interfaces.md`
  manifest 结构、`config.json` 字段、脚本参数、产物契约
- `docs/STS2_LAN_CONNECT_USER_GUIDE_ZH.md`
  面向玩家的使用说明

补充说明：

- 发布包内的 `README.md` 来自仓库根目录的 `RELEASE_README.md`
- `references/decompiled/` 下是反编译参考文件，只用于理解官方接口，不参与本项目编译
