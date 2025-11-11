<p align="center"><img width="501" height="106" src="./Assets/StarryFramework/Info/images/StarryFramework-Logo.png"></p>

<p align="center">
	<a href="https://github.com/starryforest-ymxk/StarryFramework/blob/master/LICENSE.md"><img src="https://img.shields.io/badge/license-MIT-blue.svg" title="license-mit" /></a>
    <a href="https://github.com/starryforest-ymxk/StarryFramework/releases"><img src="https://img.shields.io/github/v/release/starryforest-ymxk/StarryFramework?color=green"/></a>
</p>

|[中文](./Assets/StarryFramework/Info/README.md)|[English](./Assets/StarryFramework/Info/README_EN.md)|

- [关于StarryFramework](#关于starryframework)
  - [核心特性](#核心特性)
  - [文件夹结构](#文件夹结构)
  - [基础模块介绍](#基础模块介绍)
  - [扩展模块介绍](#扩展模块介绍)
  - [框架依赖](#框架依赖)
- [框架下载](#框架下载)
- [快速开始](#快速开始)
  - [基础配置](#基础配置)
  - [扩展模块配置](#扩展模块配置)
  - [简单用法](#简单用法)
- [版权声明与致谢](#版权声明与致谢)
- [支持](#支持)

---

## 关于StarryFramework

StarryFramework 是一个轻量化的 Unity 游戏开发框架，旨在提供一系列开箱即用的功能模块，帮助开发者快速构建高质量的游戏项目。框架采用 MOM（Manager-Of-Managers）架构设计，实现模块间零耦合，支持灵活的模块组合和扩展。

### 核心特性

- **模块化设计**：所有功能以独立模块形式存在，可根据项目需求自由组合
- **零耦合架构**：模块间通过事件系统通信，实现松耦合
- **统一入口**：通过 `Framework` 静态类提供统一的 API 访问入口
- **易于扩展**：支持自定义模块开发，可轻松集成新功能
- **编辑器友好**：为每个模块提供自定义 Inspector 面板，方便配置和调试
- **生命周期管理**：严格的模块生命周期控制，确保按正确顺序初始化和销毁

### 文件夹结构

```
/Assets
├── /StarryFramework           		
│   ├── /Runtime               		
│   │   ├── /Attributes        			# 自定义特性
│   │   ├── /Framework         			# 框架核心，包括核心类和模块
│   │   └── /Scene             		
│   │       └── GameFramework.unity 	# 框架启动场景
│   ├── /Editor                			# 模块编辑器
│   ├── /Extensions            			# 扩展模块
│   ├── /Info                  			# 框架文档
│   └── /Resources             		
└── /Plugins                   		
```

### 基础模块介绍

在目前提供的版本中，包含八个基础模块：

1. **Event Module** 事件模块：基于委托实现的事件系统，支持模块间解耦通信。
2. **Save Module** 存档模块：完整的存档管理系统，支持多存档和自动存档。
3. **FSM Module** 有限状态机模块：灵活的状态机管理系统，支持多状态机并存。

4. **ObjectPool Module** 对象池模块：高效的对象池管理系统，减少频繁实例化开销。
5. **Timer Module** 计时器模块：功能丰富的计时器管理系统，支持多种计时器类型。
6. **Scene Module** 场景管理模块：提供场景加载、卸载和切换功能，支持场景切换动画。
7. **Resource Module** 资源管理模块：基于Resources 和 Addressables 的资源管理系统。
8. **UI Module** UI模块：完整的 UI 窗体管理系统，支持 UI 分组和层级管理。

### 扩展模块介绍

在目前提供的版本中，包含一个扩展模块：

- **Audio Module** 音频模块

  考虑到在实际开发中，部分项目会采取音频中间件的方法管理游戏音频，因此本框架提供了针对于Fmod的音频模块。这个音频模块对Fmod提供的API进行封装，使得接口更加简洁，易于使用。

  音频模块提供了音频，BGM的托管，支持多输出通道的音量控制，支持将音频动态附着到物体上；支持针对于不同场景的BGM自动播放，音频预加载等。

  音频模块的内容在`StarryFramework_AudioExtention.unitypackage`中。

### 框架依赖

StarryFramework 依赖以下 Unity Package Manager (UPM) 包和第三方插件：

- 必需依赖
  - Newtonsoft.Json (`com.unity.nuget.newtonsoft-json`)
  - Addressables (`com.unity.addressables`)
  - DOTween (第三方插件，已集成在框架包中)

- 可选依赖
  - FMOD for Unity

---

## 框架下载

- 下载unitypackage包

  前往[发布页面](https://github.com/starryforest-ymxk/StarryFramework/releases)下载`StarryFramework.unitypackage`

  如果的项目使用fmod并且需要框架关于fmod的扩展的话，同时下载`StarryFramework_AudioExtention.unitypackage`

- 或者：克隆项目仓库

  ``` 
  git clone https://github.com/starryforest-ymxk/StarryFramework.git
  ```

  仓库中采用fmod管理音频文件，因此包含了基础框架和扩展内容。同时，仓库内的`Assets/Test`内还包含了一些框架各模块的测试代码。

---

## 快速开始

### 基础配置

1. 导入框架依赖
   - 将框架的依赖包导入到项目中：`Newtonsoft.Json`、`Addressables`

2. 导入框架
   - 将下载的`StarryFramework.unitypackage`导入到项目中

3. 配置启动场景

   - 打开 `StarryFramework/Runtime/Scene/GameFramework.unity` 场景

   - 在 Unity 菜单中选择 `File > Build Settings`

   - 将 `GameFramework.unity` 添加到 `Scenes in Build` 列表

   - **确保 GameFramework 场景的 buildIndex 为 0**（拖动到列表第一位）

   - 这个场景将作为游戏的启动场景，在游戏运行期间始终保持加载状态


3. 调整框架设置

   - 在 Unity 菜单中选择 `Window > StarryFramework > Settings Panel` 打开框架设置面板
   - 如果在游戏中启用框架的功能，则需要选用`Framework Start`的启动方式，并设置框架初始的加载场景；如果只是简单测试一些代码功能，只使用`Normal Start` 的启动方式
     - Framework Start：进入运行模式后，框架先加载GameFramework场景，再加载初始场景
     - Normal Start：Unity的默认行为，进入运行模式后保留在当前场景
   - Modules列表包含了框架启用的模块
     - 模块之间的顺序代表了他们的优先级，越靠近列表前部的模块优先级越高
     - 优先级越高的模块会更早地初始化，更晚地注销，在生命周期中每帧更早地调用

   - 在每个子模块对应的编辑器面板上，可以为模块配置每个模块的具体设置（Scene模块，Timer模块，Save模块等）

4. 配置场景摄像机

   - `GameFramework` 场景中的 `Camera` 对象挂载了 `SceneChangeCameraControl` ：
     - 此摄像机用于渲染场景切换动画，默认只在播放切换动画时启用；可勾选 `Is Main Camera` 将其作为全局主相机


### 扩展模块配置

**Audio Module（音频扩展模块）**

音频模块是一个可选的扩展模块，基于 FMOD 音频中间件实现。如果项目需要使用 FMOD 进行音频管理，可以按照以下步骤配置：

**前置要求**

1. 安装 FMOD for Unity
   - 前往 [FMOD 官网](https://www.fmod.com/) 下载 FMOD for Unity 插件
   - 将 FMOD 插件导入到 Unity 项目中
   - 确保 FMOD 已正确配置并能正常工作

2. 导入音频扩展模块
   - 下载 `StarryFramework_AudioExtention.unitypackage`
   - 将扩展包导入到项目中
   - 扩展模块会自动安装到 `/Assets/StarryFramework/Extensions/Runtime/Audio Module` 目录

**配置步骤**

1. 添加音频模块到框架
   - 打开框架设置面板（`Window > StarryFramework > Settings Panel`）
   - 在 Modules 列表中添加 `AudioComponent`
   - 根据需要调整模块的优先级顺序
   - 将Audio Module中的Audio预制体拖入到场景GameFramework物体下面，作为子物体
2. 配置音频模块设置
   - 在 AudioComponent 的编辑器面板中配置音频设置，如全局加载的音频库，每个场景的BGM等等




### 简单用法

**基础调用方式**

所有框架功能通过 `Framework` 静态类访问，需要引用 `StarryFramework` 命名空间：

```csharp
using StarryFramework;

public class Example : MonoBehaviour
{
    void Start()
    {
        // 触发事件
        Framework.EventComponent.InvokeEvent("GameStart");
        // 保存数据
        Framework.SaveComponent.SaveData("手动存档");
        // 加载场景
        Framework.SceneComponent.LoadScene("MainGame");
    }
}
```



**事件模块示例**

```csharp
//添加事件监听
Framework.EventComponent.AddEventListener("OnPlayerDeath", OnPlayerDeath);

// 移除事件监听
Framework.EventComponent.RemoveEventListener("OnPlayerDeath", OnPlayerDeath);

// 触发事件
Framework.EventComponent.InvokeEvent<int>("OnScoreChanged", 100);
```



**存档模块示例**

```csharp
// 创建新存档
Framework.SaveComponent.CreateNewData(true, "新游戏");

// 修改玩家数据
Framework.SaveComponent.PlayerData.playerName = "玩家1";
Framework.SaveComponent.PlayerData.level = 5;

// 保存数据
Framework.SaveComponent.SaveData("进度保存");
// 加载存档
bool success = Framework.SaveComponent.LoadData(0);
```



**场景模块示例**

```csharp
// 使用默认动画加载场景
Framework.SceneComponent.LoadSceneDefault("Level2");
// 使用自定义进度条加载场景
Framework.SceneComponent.LoadSceneProgressBar(
    "Level2", 
    "UI/LoadingScreen", 
    () => Debug.Log("加载完成")
);
```



**UI 模块示例**

```csharp
// 打开 UI 窗体
Framework.UIComponent.OpenUIForm("MainMenu", "Menu", false);

// 打开设置窗口，暂停被覆盖的 UI
Framework.UIComponent.OpenUIForm("SettingsPanel", "Dialog", true);

// 关闭 UI 窗体
Framework.UIComponent.CloseUIForm("SettingsPanel");
```

---

## 版权声明与致谢

StarryFramework采用[MIT协议](https://github.com/starryforest-ymxk/StarryFramework/blob/master/LICENSE.md)

本项目使用了以下开源项目库：

- **Newtonsoft.Json**

  本项目使用了 Newtonsoft.Json（Json.NET）进行 JSON 序列化和反序列化。由 Newtonsoft 开发，采用 MIT 协议。详见 [Newtonsoft.Json 官网](https://www.newtonsoft.com/json)

- **Unity Addressables**

  本项目使用了 Unity Technologies 提供的 Addressables 系统进行资源管理。详见 [Unity Addressables 文档](https://docs.unity3d.com/Packages/com.unity.addressables@latest)

- **FMOD Studio**

  本项目使用了FMOD Studio和Firelight Technologies Pty Ltd提供的技术。访问[FMOD官网](https://www.fmod.com/)获取并集成FMOD

- **Dotween**（包含在`StarryFramework.unitypackage`中）

  本项目使用了DOTween，由Daniele Giardini - Demigiant开发，用于动画管理。详见 [DOTween官网](http://dotween.demigiant.com/)

同时，感谢好友[NoSLoofah](https://github.com/NoSLoofah)在编写框架时提供的帮助。

---

## 支持

**文档与教程**

- [API速查手册](./Assets/StarryFramework/Info/API速查手册.md)



**问题反馈**

如果你在使用 StarryFramework 时遇到问题，可以通过以下方式寻求帮助：

1. **查看文档**：首先查看本文档和API快速查阅手册，大部分常见问题都有说明
2. **检查示例**：参考项目文件 `/Assets/Test` 目录下的测试场景和代码示例
3. **GitHub Issues**：在项目的 GitHub 仓库提交 Issue



**参与贡献**

我们欢迎任何形式的贡献！

- 🐛 报告 Bug
- 💡 提出新功能建议
- 📝 改进文档
- 🔧 提交代码优化
- ⭐ 为项目点 Star



**获取更新**

- **GitHub 仓库**：https://github.com/starryforest-ymxk/StarryFramework
- **最新版本**：查看 [Releases](https://github.com/starryforest-ymxk/StarryFramework/releases) 页面

---

**感谢使用 StarryFramework！祝你的游戏开发顺利！** 🌟
