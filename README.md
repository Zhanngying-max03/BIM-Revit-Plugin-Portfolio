# BIM Revit Plugin Portfolio

Revit二次开发插件作品集 | C# | Revit API | BIM自动化

## 🎯 项目概述

我是BIM二次开发学习者，本仓库收录了4个可运行的Revit插件，
覆盖构件统计、工程量核算、土方计算、AI辅助审查等工程场景。

## 🛠 技术栈

- 语言：C#
- 平台：Revit 202X API
- 工具：Visual Studio, Git, ClosedXML, DeepSeek API

## 📦 插件列表

### 1. 模型批量治理工具
- 功能：统计模型中任意构件类别的数量、面积、体积
- 技术点：FilteredElementCollector, 参数提取, WPF
- [查看源码](./01.ModelStatistics)

### 2. 住宅工程量核算系统
- 功能：自动计算混凝土净量，扣除洞口，导出国标清单Excel
- 技术点：构件扣减逻辑, GB50500, ClosedXML
- [查看源码](./02.QuantityTakeoff)

### 3. 场地土方平衡工具
- 功能：基于地形表面计算挖填方量
- 技术点：Revit场地API, 体积计算
- [查看源码](./03.EarthworkCalculator)

### 4. AI辅助BIM审查助手
- 功能：提取构件数据，调用DeepSeek进行工程量合理性检查
- 技术点：API调用, Python交互, AI+BIM
- [查看源码](./04.AI_BIM_Assistant)

## 🚀 快速运行

1. 克隆本仓库
2. 用Visual Studio打开对应.sln文件
3. 添加RevitAPI.dll和RevitAPIUI.dll引用
4. 编译，将生成的.dll和.addin文件放入Revit插件目录
5. 打开Revit → 附加模块 → 运行插件

## 📸 运行截图
