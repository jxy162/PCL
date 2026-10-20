# AliCraftLauncher 项目概览

> 本文档描述本仓库的定位、技术架构与改造记录。
> 项目基于开源项目 Plain Craft Launcher 2（PCL2）二次开发，原始出处见 README。

## 一、项目定位

- **性质**：个人自用的 Minecraft 启动器
- **来源**：Plain Craft Launcher 2（PCL2）开源代码的衍生分支
- **目标**：在保留 PCL 强大功能的前提下，进行品牌化与界面重设计，并恢复被官方移除的下载功能

## 二、技术架构

| 工程 | 语言 | 目标框架 | 职责 |
| --- | --- | --- | --- |
| `Plain Craft Launcher 2` | VB.NET + WPF | .NET Framework 4.8 | 启动器主程序（界面、业务模块、页面） |
| `PCLCS` | C# | .NET Framework 4.8 | 启动核心：版本解析、资源枚举、Java 处理 |
| `MeloongCore` | C# | .NET Standard 2.0 | 通用能力：日志、配置、下载、文件、加密、进度 |
| `MeloongCore.Wpf` | C# | .NET Framework 4.8 | WPF 适配层：对话框、控件扩展、转换器 |

### 主程序模块划分

- `Modules/Base`：基础框架（任务加载器 LoaderBase/LoaderTask、网络与多线程下载引擎、动画框架、校验框架）
- `Modules/Minecraft`：版本扫描与缓存、下载、启动、登录、崩溃分析、Java 管理、整合包安装
- `Modules/Resource`：社区资源（CurseForge / Modrinth）检索与详情
- `Pages`：各功能页面（启动、版本、下载、设置、更多）
- `Controls`：自研 WPF 控件库（卡片、按钮、列表、弹窗、滚动条等）
- `Resources`：内置资源（字体、帮助文档、依赖 DLL、主题模板 Custom.xaml）

## 三、本分支改造记录

1. **品牌化**
   - 程序集名 / 窗口标题 / 界面文字统一为 `AliCraftLauncher`
   - 全新自绘 LOGO（渐变徽章 + 等距方块），替换原 PCL 图标与标题栏徽标
2. **功能恢复**
   - `ModSecret.RestrictMcDownload = False`：恢复原版下载与整合包导入
3. **内容精简**
   - 移除赞助 / 会员 / 土豆码 / 主题解锁等相关界面与逻辑
4. **配置方式**
   - 微软登录 Client ID 与 CurseForge API Key 通过环境变量注入（`PCL_MS_CLIENT_ID` / `PCL_CURSEFORGE_API_KEY`），不写入源码
5. **界面重设计**（进行中）
   - 参考现代启动器设计语言，重做配色、控件与布局

## 四、路线图

- [x] 品牌更名与 LOGO
- [x] 恢复下载与整合包导入
- [x] 移除赞助 / 会员内容
- [ ] 深浅双主题与新配色体系
- [ ] 自定义标题栏、底部导航与壁纸背景
- [ ] 界面细节与动效打磨

## 五、许可与致谢

本项目遵循上游项目 Plain Craft Launcher 的许可协议（见 [LICENCE](LICENCE)）。
核心代码版权归原作者 **龙腾猫跃（LTCat）** 及 PCL 社区贡献者所有，特此致谢。