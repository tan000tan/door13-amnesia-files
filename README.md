# 第十三扇门：失忆档案

![Unity](https://img.shields.io/badge/Unity-2022.3.62f3c1%20LTS-black?logo=unity)
![URP](https://img.shields.io/badge/Render%20Pipeline-URP%2014.0.11-blue)
![Platform](https://img.shields.io/badge/Platform-Android%207.0%2B-green?logo=android)
![Version](https://img.shields.io/badge/Version-v2.1-orange)
![Language](https://img.shields.io/badge/Language-简体中文-red)
![License](https://img.shields.io/badge/License-All%20Rights%20Reserved-lightgrey)

> 你在一间陌生的实验室中醒来，不记得自己是谁。
> 墙上的计数告诉你：你已经来过这里 12 次。
> 穿过第十三扇门，走进回廊 7 号——一个关于记忆、实验和十二个孩子的真相，正在等待。

**第一人称恐怖解谜 · 安卓独立游戏** —— 本仓库包含完整的 Unity 源码工程、构建脚本与验收文档。

---

## ✨ 游戏亮点

| | |
|---|---|
| 🔁 **4 次循环** | 每次循环都会改写你对这座实验室的理解 |
| 🚪 **7 个房间** | 从照片墙到容器室，每个房间都藏着一段记忆 |
| 🎭 **4 个结局** | 全部重写，情感递进——包括一个需要你自己找到的隐藏真结局 |
| 🧸 **12 个孩子** | 完整名单、完整故事线，墙上手写拼出最后一块真相 |

**核心玩法**：第一人称探索 · 环境叙事 · 谜题解密（物品收集、物品组合、照片墙排序等）· 多结局分支。

## 🛠 技术特性

- **存档系统**：自动保存 + 手动保存 + 版本迁移
- **设置系统**：主/音乐/SFX 音量、FOV、视角灵敏度、镜头平滑、移动灵敏度、Y 轴反转
- **UI 适配**：刘海屏 / 圆角 / 导航栏安全区适配
- **交互系统**：全局输入锁、物品组合、可读物文档
- **程序化音频**：Pad / Wind / Machine / Sweep / Tone / Noise，全部实时合成，零音频文件
- **全流程冒烟测试**：49 步自动化验收脚本（`Tools/verify`），构建即回归

## 📦 版本

| 版本 | versionCode | APK 大小 | 说明 |
|---|---|---|---|
| v2.0 正式版 | 12 | 16.0 MB | 8 个阶段开发完成：完整系统 + 剧情完善 + 美术品质 + 质量测试 |
| **v2.1（当前）** | — | 22.25 MB | 修复视角方向、文字空白、钥匙不可拾取三个 Bug；冒烟测试 49/49 全过 |

## 📁 目录结构

```text
第十三扇门_失忆档案/
├─ Assets/               游戏源码、场景、材质、UI（C# 脚本）
├─ ProjectSettings/      Unity 工程配置（URP、Android 平台设置）
├─ Packages/             URP 14.0.11 等包依赖
├─ Builds/               正式 APK：第十三扇门_失忆档案_Final.apk
├─ Tools/
│  ├─ build_door13.sh    一键构建脚本（编译→场景重建→冒烟→打包）
│  ├─ verify/            49 步自动化冒烟测试
│  └─ logs/              构建日志
├─ 版本说明_v2.0.md       v2.0 完整版本说明与变更记录
├─ 最终交付清单.md         交付物与 aapt 实测验证数据
├─ 视觉品质提升报告.md     材质/颜色/灯光统一体系
└─ 阶段8_最终质量测试报告.md
```

## 🔨 构建

需要 Unity 2022.3.62f3c1 LTS（含 Android Build Support / IL2CPP）与 Bash 环境（Git Bash 即可）：

```bash
# 一键全流程：编译检查 → 场景重建 → 冒烟测试 → IL2CPP 打包
Tools/build_door13.sh all
```

产物输出至 `Builds/第十三扇门_失忆档案_Final.apk`（ARM64 / IL2CPP / minSdk 24 / targetSdk 34）。

> ⚠️ 当前 APK 为调试签名，仅供个人安装体验；正式分发请替换 keystore。
> 包名 `com.door13.amnesia`，Android 7.0+ 直接安装。

## 📚 相关仓库

- **[door13-dev-center](https://github.com/tan000tan/door13-dev-center)** — 开发中心：项目进度、开发日志、技术方案、版本记录、Bug 记录与各阶段验收报告。

## 📄 许可

源码与美术资源保留所有权利（All Rights Reserved），仅供学习交流。
