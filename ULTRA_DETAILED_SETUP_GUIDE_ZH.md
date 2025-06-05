# 超详细Unity项目设置与QFramework集成指南 (中文)

本文档旨在为用户提供一个从零开始创建Unity项目，并集成QFramework框架，最终导入并配置我们重构后的生存管理游戏核心脚本的超详细步骤。

## 第一部分：项目创建和初始设置

### 1. 创建新的Unity项目

这一步将指导您如何使用 Unity Hub 创建一个全新的 Unity 项目。

1.  **打开 Unity Hub**:
    *   在您的电脑上找到并打开 Unity Hub 应用程序。

2.  **新建项目**:
    *   在 Unity Hub 界面中，点击左上角的 "Projects" (项目) 标签页（如果尚未选中）。
    *   点击右上角的 "New project" (新建项目) 按钮。

3.  **选择Unity版本与模板**:
    *   **Editor Version (编辑器版本)**: 在新建项目窗口的顶部，选择一个您已安装的Unity编辑器版本。
        *   **推荐**: 选择一个长期支持 (LTS) 版本以获得更好的稳定性和持续支持，例如 `2021.3.x LTS` 或 `2022.3.x LTS`。您可以从Unity官网下载特定版本的编辑器。
    *   **Templates (模板)**: 从模板列表中选择一个基础模板。
        *   **推荐选择**: "2D (URP)"。由于我们的游戏界面（UI）是核心交互部分，并且游戏逻辑不一定依赖复杂的3D渲染，2D Universal Render Pipeline (URP) 模板是一个轻量且高效的选择。URP也支持3D，如果未来需要简单的3D元素也易于扩展。
        *   如果您确定项目会有大量3D场景和模型作为核心，也可以选择 "3D (URP)"。对于QFramework的集成和核心脚本的设置，这两个模板的初始差异不大。

4.  **设置项目名称和位置**:
    *   **Project name (项目名称)**: 在 "Project name" 输入框中，为您的项目取一个名字，例如 `MySurvivalGame`。
    *   **Location (存储位置)**: 点击位置输入框右侧的文件夹图标，选择一个您希望在电脑上存储该项目的文件夹。
    *   **Unity Cloud Organization (Unity云组织)**: 通常可以保持默认或选择您的个人组织。

5.  **创建项目**:
    *   确认以上设置无误后，点击右下角的 "Create project" (创建项目) 按钮。
    *   Unity 编辑器将会启动，并开始创建和初始化您的新项目。这个过程可能需要几分钟，具体时间取决于您的电脑性能和所选的Unity版本。请耐心等待直至项目完全打开。

### 2. 导入QFramework

QFramework 是一款强大的Unity开发框架，我们的项目依赖它来组织代码和管理游戏系统。以下是几种常见的导入方式：

*   **方式一：通过Unity Asset Store (Unity资源商店 - 如果QFramework已在此发布)**
    1.  **打开Asset Store窗口**: 在Unity编辑器的主菜单栏中，选择 `Window` -> `Asset Store`。这可能会在Unity编辑器内打开一个标签页，或者打开一个外部浏览器窗口链接到Asset Store网站。
    2.  **搜索QFramework**: 在Asset Store的搜索框中，输入 "QFramework" 并按回车。
    3.  **下载与导入**:
        *   在搜索结果中找到官方的QFramework资源包。
        *   点击 "Download" (下载) 按钮。如果之前已下载过，按钮可能是 "Re-download" (重新下载) 或 "Import" (导入)。
        *   下载完成后，按钮会变为 "Import" (导入)。点击它。
    4.  **确认导入内容**: Unity会弹出一个 "Import Unity Package" (导入Unity包) 的窗口，其中列出了包内所有的文件和文件夹。确保所有内容都被勾选（通常默认是全部勾选）。
    5.  点击右下角的 "Import" (导入) 按钮。Unity会将QFramework的文件添加到您的项目中。

*   **方式二：通过GitHub Release (如果提供 `.unitypackage` 包)**
    1.  **访问GitHub Releases**: 打开您的网络浏览器，前往QFramework的官方GitHub仓库 (通常地址类似 `https://github.com/liangxiegame/QFramework`，请以实际官方地址为准)，找到 "Releases" (发布) 页面。
    2.  **下载.unitypackage**: 从Releases页面下载最新稳定版本的 `.unitypackage` 文件。它通常会被命名为类似 `QFramework_vx.x.x.unitypackage`。
    3.  **导入自定义包**:
        *   返回Unity编辑器。
        *   在主菜单栏中，选择 `Assets` -> `Import Package` -> `Custom Package...` (导入包 -> 自定义包...)。
        *   在弹出的文件浏览器中，找到并选择您刚刚下载的 `.unitypackage` 文件，然后点击 "Open" (打开)。
    4.  **确认导入内容**: Unity会弹出 "Import Unity Package" 窗口。确保所有内容都被勾选。
    5.  点击 "Import" (导入) 按钮。

*   **方式三：通过 Package Manager (包管理器 - 如果QFramework支持Git URL或已在官方/Scoped Registry)**
    *   这种方式通常用于更现代的包管理流程。
    1.  **打开Package Manager**: 在Unity编辑器的主菜单栏中，选择 `Window` -> `Package Manager`。
    2.  **从Git URL添加 (如果支持)**:
        *   在Package Manager窗口的左上角，点击 "+" (加号) 图标。
        *   从下拉菜单中选择 "Add package from git URL..." (从git URL添加包...)。
        *   在弹出的输入框中，粘贴QFramework的Git仓库URL (通常以 `.git` 结尾，例如 `https://github.com/liangxiegame/QFramework.git`，请使用官方提供的URL)。
        *   点击 "Add" (添加)。Unity会尝试从该URL克隆并导入包。
    3.  **通过Scoped Registry添加 (如果支持)**:
        *   如果QFramework托管在特定的Scoped Registry上，您可能需要先配置它。
        *   菜单栏 -> `Edit` -> `Project Settings...`。
        *   在左侧导航栏选择 `Package Manager`。
        *   在 `Scoped Registries` 部分，添加QFramework维护者提供的Registry信息（名称、URL、Scope(s)）。
        *   配置完成后，返回Package Manager窗口，在顶部的包源选择中（可能显示为 "My Registries" 或 "Unity Registry"），切换到您添加的Registry或选择 "My Registries"，然后搜索并安装QFramework。

*   **导入后验证**:
    *   导入完成后，留意Unity编辑器的Console（控制台）窗口（可通过 `Window -> General -> Console` 打开）是否有QFramework成功初始化或导入完成的日志信息。
    *   检查Unity的菜单栏是否新增了QFramework相关的菜单项（例如，可能会有一个名为 "QFramework" 或 "QF" 的顶级菜单）。
    *   **重要**: 本项目后续的脚本都依赖于QFramework的核心功能，如 `GameArchitecture.Interface` (用于获取架构单例) 以及 `this.GetModel<T>()`, `this.GetSystem<T>()`, `this.SendCommand<T>()` 等QFramework提供的便捷API。确保QFramework已正确导入是项目能运行的前提。

### 3. 导入TextMeshPro

TextMeshPro是Unity推荐的文本渲染解决方案，它比旧版的UI Text提供了更高级的文本格式化和更好的视觉效果。我们的UI预制件和脚本可能依赖它。

1.  **检查是否已安装**:
    *   在较新的Unity版本 (如2020+)，TextMeshPro通常已经作为包预装在项目中。您可以在 `Package Manager` 窗口的 "In Project" (项目中) 或 "Unity Registry" 列表中找到它。
2.  **导入核心资源 (Essentials)**:
    *   即使包已安装，TextMeshPro的一些核心资源（如默认字体、着色器等）也需要手动导入到您的 `Assets` 文件夹中才能在项目中使用。
    *   在Unity编辑器的主菜单栏中，选择 `Window` -> `TextMeshPro` -> `Import TMP Essentials`。
    *   在弹出的导入窗口中，点击 "Import" (导入) 按钮。这会将必要的资源添加到您的项目中，通常位于 `Assets/TextMesh Pro` 目录下。
3.  **导入示例与附加资源 (可选)**:
    *   同一菜单下 (`Window -> TextMeshPro`) 可能还有 "Import TMP Examples & Extras" 选项。这些是示例场景、字体和附加资源，对于学习TextMeshPro很有用，但对于我们项目的基本运行不是必需的。如果您是初学者或想查看示例，可以导入它们。

### 4. 创建推荐的项目文件夹结构

一个清晰、一致的文件夹结构有助于管理项目资源和脚本，方便团队协作和后续维护。以下是我们推荐的基础结构：

1.  **打开Project窗口**: 确保Unity编辑器的 `Project` (项目) 窗口可见。
2.  **在`Assets`下创建主文件夹**:
    *   在 `Project` 窗口中，选中 `Assets` 文件夹。
    *   右键点击 `Assets` 文件夹 (或在空白处右键) -> `Create` -> `Folder`。
    *   将新文件夹命名为 `_Project`。使用下划线开头可以使其在文件夹列表中排序靠前，方便快速找到项目专属内容。

3.  **在`_Project`下创建核心子文件夹**:
    *   选中刚刚创建的 `_Project` 文件夹。
    *   同样通过右键 -> `Create` -> `Folder` 的方式，在其下创建以下子文件夹：
        *   `Scenes` (用于存放您自己创建的游戏场景，以区别于可能由插件导入的示例场景)
        *   `Scripts` (用于存放所有C#脚本)
        *   `Prefabs` (用于存放预制件)
        *   (根据您的项目需求，还可以创建 `Art`, `Audio`, `Animations`, `Shaders`, `Materials` 等文件夹来组织美术、音频等资源)

4.  **在`_Project/Scripts/`下创建游戏逻辑脚本文件夹**:
    *   选中 `_Project/Scripts/` 文件夹。
    *   创建一个名为 `GameCore` 的子文件夹 (也可以使用 `GameLogic`, `GameFeatures` 或您认为更合适的名称)。这个文件夹将用于存放我们重构后的所有核心游戏脚本。

5.  **在`_Project/Scripts/GameCore/`下创建模块化文件夹**:
    *   选中 `_Project/Scripts/GameCore/` 文件夹。
    *   根据QFramework的架构和我们项目的模块划分，创建以下子文件夹：
        *   `Models` (存放数据模型脚本，如 `ResourceModel.cs`, `SurvivorModel.cs`)
        *   `Systems` (存放系统逻辑脚本，如 `DayNightSystem.cs`, `CombatSystem.cs`)
        *   `UI` (存放UI控制脚本，如 `DayDisplay.cs`, `SurvivorDisplay.cs`)
            *   在 `UI` 文件夹下，可以进一步创建一个 `Items` 子文件夹，用于存放列表项UI的脚本 (例如 `SurvivorListItemUI.cs`, `TechDisplayItem.cs`)。
        *   `Commands` (存放命令脚本，如 `StartResearchCommand.cs`)
        *   `Events` (存放事件定义脚本，如 `GameEvents.cs`)
        *   **各功能模块文件夹**: 为每个主要的游戏功能模块创建一个独立的文件夹，例如：
            *   `Combat`
            *   `Enemies`
            *   `Exploration`
            *   `Quests`
            *   `Research`
            *   `Resources`
            *   `Survivors`
            *   `Time`
            *   `Workstations`

6.  **在`_Project/Prefabs/`下创建UI预制件文件夹**:
    *   选中 `_Project/Prefabs/` 文件夹。
    *   创建一个名为 `UI` 的子文件夹。
    *   在 `UI` 文件夹下，可以进一步创建一个 `Items` 子文件夹，用于存放列表项的UI预制件 (例如 `SurvivorItem_PF.prefab`, `TechItem_PF.prefab`)。

完成这些步骤后，您的 `Assets` 文件夹结构应该大致如下：
```
Assets/
  _Project/
    Scenes/
    Scripts/
      GameCore/
        Models/
        Systems/
        UI/
          Items/
        Commands/
        Events/
        Combat/
        Enemies/
        Exploration/
        Quests/
        Research/
        Resources/
        Survivors/
        Time/
        Workstations/
    Prefabs/
      UI/
        Items/
    (Art/, Audio/, etc.)
  (QFramework/, TextMesh Pro/, 其他插件或导入的资源包文件夹)
```

### 5. 导入游戏脚本和内容

现在，我们将把之前重构好的游戏核心脚本导入到新创建的项目结构中。

*   **前提假设**:
    *   您已经拥有一个包含所有重构后C#脚本的文件夹。在这个文件夹内部，应该有一个名为 `GameScritps` 的子文件夹，其内部结构与我们之前在 `_Project/Scripts/GameCore/` 下创建的结构（Models, Systems, UI, Commands, Events, 各模块文件夹）完全一致。
    *   所有脚本都使用了 `YourGameNamespace` (或您项目中统一的命名空间)。

*   **导入操作**:
    1.  **打开文件浏览器**: 在您的电脑操作系统中，打开文件浏览器（如Windows的资源管理器或Mac的Finder）。
    2.  **定位源脚本文件夹**: 找到包含您所有重构后游戏脚本的那个根文件夹。
    3.  **拖拽导入**:
        *   从您的文件浏览器中，将那个名为 `GameScritps` 的文件夹（包含其所有子文件夹和脚本）**直接拖拽**到Unity `Project` 窗口中，具体目标位置是您在步骤4.5中创建的 `Assets/_Project/Scripts/GameCore/` 文件夹。
        *   **或者**，您可以先在 `Assets/_Project/Scripts/GameCore/` 文件夹下删除所有自动生成的子文件夹（Models, Systems等，如果它们是空的），然后将 `GameScritps` 文件夹内的 *所有内容* (即Models, Systems, UI, Commands, Events以及所有模块文件夹) 拖拽到 `Assets/_Project/Scripts/GameCore/` 下。目的是让 `GameCore` 文件夹下直接包含 `Models`, `Systems` 等。
        *   **请根据您源文件夹的组织方式选择最合适的拖拽方式，确保最终 `Assets/_Project/Scripts/GameCore/` 下的结构是我们期望的模块化结构。**

    4.  **编译与检查**:
        *   当您将文件拖拽到Unity编辑器中后，Unity会自动开始导入这些脚本并尝试编译它们。
        *   密切关注Unity编辑器右下角的状态栏。如果有一个旋转的小图标，表示Unity正在处理。
        *   导入和编译完成后，打开 `Console` 窗口 (`Window -> General -> Console`)。
        *   **检查是否有编译错误** (通常显示为红色的错误消息)。
            *   **命名空间问题**: 如果出现大量关于类型或命名空间找不到的错误，请首先确认所有导入的脚本是否都正确地使用了 `YourGameNamespace` (或者您在项目中规划的统一命名空间)。如果源脚本的命名空间与新项目不一致，您需要手动修改所有脚本的 `namespace YourGameNamespace { ... }` 部分，或者使用IDE（如Visual Studio, Rider）的重构功能来批量更改命名空间。**强烈建议在开始导入前就确保源脚本的命名空间是统一且符合您期望的。**
            *   **QFramework依赖**: 确保QFramework已成功导入并且其API可用。
            *   **其他依赖**: 检查是否有脚本依赖了项目中尚未导入的其他插件或资源包。
        *   解决所有编译错误，直到Console窗口不再显示红色错误消息。黄色警告消息也应尽量审查并处理，但通常不会阻止游戏运行。

    5.  **导入其他资源**:
        *   如果您的游戏项目还包含其他美术资源（如精灵图片、3D模型、纹理）、音频文件、动画等，请按照类似的逻辑，在 `Assets/_Project/` 下创建相应的文件夹 (如 `Art/Sprites`, `Audio/Music`, `Animations`)，然后将这些资源文件从您的文件浏览器拖拽到Unity Project窗口的对应位置。

至此，您的新Unity项目已创建，QFramework和TextMeshPro已导入，推荐的文件夹结构已建立，并且核心游戏脚本也已导入。下一步通常是创建或配置主场景，并将游戏管理脚本（如GameInitializer, GameLoop）和UI挂载到场景中的GameObject上。

## 第二部分：核心架构与UI场景搭建

在第一部分中，我们完成了项目创建、框架导入和脚本迁移。现在，我们将搭建游戏运行的基础场景和核心UI框架。

### 1. 创建主游戏场景

1.  **导航到场景文件夹**: 在Unity编辑器的 `Project` (项目) 窗口中，展开 `Assets` -> `_Project` -> `Scenes` 文件夹。
2.  **创建新场景**:
    *   在 `Scenes` 文件夹的空白区域右键。
    *   从上下文菜单中选择 `Create` -> `Scene`。
    *   将新创建的场景文件命名为 `MainScene` (或者您喜欢的其他主场景名称，如 `GameScene`)。
3.  **打开场景**: 双击 `MainScene.unity` 文件以在编辑器中打开它。
4.  **保存场景**: 即使是空场景，也最好按一下 `Ctrl+S` (Windows) 或 `Cmd+S` (Mac) 来保存它，确保所有更改都被记录。

### 2. 设置核心游戏逻辑对象

我们需要一个持久化的GameObject来承载全局的游戏管理脚本。

1.  **创建空GameObject**:
    *   在 `Hierarchy` (层级) 窗口的空白区域右键。
    *   选择 `Create Empty`。
    *   选中新创建的GameObject，在 `Inspector` (检查器) 窗口中，将其名称修改为 `_GameManager_` (下划线开头和结尾有助于使其在Hierarchy中排序靠前或视觉上突出，这是一种常见的组织方式)。
2.  **挂载核心脚本**:
    *   确保 `_GameManager_` GameObject仍处于选中状态。
    *   在 `Inspector` 窗口中，点击最下方的 "Add Component" (添加组件) 按钮。
    *   在弹出的搜索框中，输入 `GameInitializer`。
    *   从搜索结果中选择 `GameInitializer` 脚本 (它应该位于 `Assets/_Project/Scripts/GameCore/` 目录下，但Unity的搜索通常能直接找到)。点击它即可添加。
    *   再次点击 "Add Component" 按钮。
    *   搜索并添加 `GameLoop` 脚本。
3.  **说明**:
    *   `GameInitializer.cs`: 这个脚本是我们之前在QFramework中设定的，它会在游戏启动时（通常在其 `Awake()` 或 `Start()` 方法中）执行一次。它的核心职责是初始化整个游戏的架构，包括创建和注册所有的Models (数据模型) 和 Systems (系统逻辑)。这是游戏能够按照QFramework模式运行起来的入口点。
    *   `GameLoop.cs`: 这个脚本的 `Update()` 方法会被Unity引擎在每一帧调用。我们通常在这个 `Update()` 方法内部，按顺序调用各个游戏系统（如 `DayNightSystem`, `WorkstationSystem`, `ExplorationSystem` 等）的 `Update()` 或 `Tick()` 方法（如果它们需要每帧更新的话），从而驱动整个游戏世界的逻辑持续运行和演变。

### 3. 创建基础UI环境

现在我们来搭建UI显示的基础。

1.  **创建Canvas**:
    *   在 `Hierarchy` (层级) 窗口的空白区域右键。
    *   选择 `UI` -> `Canvas`。
    *   这会自动创建一个名为 `Canvas` 的GameObject，并且通常还会自动在场景中添加一个名为 `EventSystem` 的GameObject。
        *   **Canvas**: 这是所有UI元素的根容器。所有您想在屏幕上显示的UI（如文本、按钮、图片、滑动条等）都必须是这个 `Canvas` GameObject或其子Canvas的子对象。
        *   **EventSystem**: 这个对象负责处理用户输入事件，例如鼠标点击、触摸、键盘输入等，并将这些事件分发给对应的UI元素。一个场景中通常只需要一个 `EventSystem`。

2.  **配置Canvas Scaler**:
    *   选中刚刚创建的 `Canvas` GameObject。
    *   在 `Inspector` (检查器) 窗口中，找到 `Canvas Scaler (Script)` 组件。这个组件非常重要，它决定了您的UI如何适应不同的屏幕分辨率和宽高比。
    *   将其 `UI Scale Mode` (UI缩放模式) 属性从默认的 `Constant Pixel Size` (恒定像素大小) 修改为 `Scale With Screen Size` (随屏幕尺寸缩放)。
    *   **Reference Resolution (参考分辨率)**: 设置一个您设计UI时使用的基础分辨率。例如，如果您主要针对PC平台，可以设置为 X: `1920`, Y: `1080`。这意味着当游戏运行在1920x1080分辨率的屏幕上时，UI元素会以其原始设计尺寸显示。
    *   **Screen Match Mode (屏幕匹配模式)**: 这个设置决定了当实际屏幕宽高比与参考分辨率的宽高比不同时，UI如何进行缩放。
        *   `Match Width Or Height` (匹配宽度或高度) 是一个常用的选项。您可以拖动下方的 `Match` 滑块：
            *   滑块完全偏向 `Width (0)`: UI会优先匹配参考宽度，高度会相应缩放。如果屏幕比参考的更宽，UI元素可能会在垂直方向上显得“压缩”或有更多空白；如果屏幕更窄，则可能垂直裁剪。
            *   滑块完全偏向 `Height (1)`: UI会优先匹配参考高度，宽度会相应缩放。
            *   滑块在中间 (0.5): UI会同时考虑宽度和高度的匹配。
        *   根据您游戏的主要目标平台和UI设计风格选择合适的模式和Match值。通常可以先设为0.5，然后在不同分辨率下测试效果。

### 4. 创建并配置主要的UI面板

我们的游戏有多个信息显示面板（如资源、日期、事件等）。我们将为它们创建容器，并挂载相应的控制脚本。

*   **通用步骤概述**:
    1.  在 `Canvas` 下创建一个空的GameObject作为面板的根容器。
    2.  为这个GameObject取一个清晰的名字 (例如 `ResourceDisplay_Panel`)。
    3.  将对应的UI控制脚本 (例如 `ResourceDisplay.cs`) 附加到这个GameObject上。
    4.  在这个面板GameObject下，创建实际的UI元素 (如文本框 `TextMeshPro - Text`，图片 `Image`，按钮 `Button` 等)。
    5.  将这些子UI元素链接到面板控制脚本在Inspector中暴露的公共字段上。

*   **示例：创建 `DayDisplay_Panel` (日间与基地数据显示面板)**:
    1.  **创建面板容器**: 在 `Hierarchy` 窗口中，右键选中 `Canvas` GameObject，然后选择 `Create Empty`。将这个新创建的GameObject命名为 `DayDisplay_Panel`。
    2.  **挂载脚本**: 选中 `DayDisplay_Panel` GameObject。在 `Inspector` 窗口中，点击 "Add Component" 按钮，搜索 `DayDisplay` 并添加 `DayDisplay (Script)`。
    3.  **创建子UI元素**:
        *   右键 `DayDisplay_Panel` -> `UI` -> `Text - TextMeshPro`。将创建的文本对象命名为 `DayText`。
        *   重复此操作，再创建两个TextMeshPro文本对象，分别命名为 `TimeText` 和 `BaseHealthText`。
        *   **调整位置与样式**: 选中每个文本对象，使用 `Scene` (场景) 视图中的RectTransform工具（快捷键 `T`）或在 `Inspector` 中直接修改其 `Rect Transform` 组件的锚点 (Anchors)、位置 (Pos X, Pos Y)、宽度 (Width)、高度 (Height) 等属性，将它们放置在屏幕上您期望的位置。您也可以修改 `TextMeshPro - Text (UI)` 组件中的字体大小、颜色、对齐方式等。
    4.  **链接脚本字段**:
        *   选中 `DayDisplay_Panel` GameObject。
        *   在 `Inspector` 窗口中找到 `DayDisplay (Script)` 组件。您会看到它有几个公共字段，如 `Day Text`, `Time Text`, `Base Health Text`。
        *   从 `Hierarchy` 窗口中，将 `DayText` GameObject **拖拽**到 `DayDisplay (Script)` 组件的 `Day Text` 字段上。
        *   同样地，将 `TimeText` GameObject 拖拽到 `Time Text` 字段，将 `BaseHealthText` GameObject 拖拽到 `Base Health Text` 字段。

*   **为其他简单UI面板重复此过程**:
    *   **`ResourceDisplay_Panel`**:
        *   创建 `ResourceDisplay_Panel` GameObject，挂载 `ResourceDisplay.cs` 脚本。
        *   在其下创建多个 `TextMeshPro - Text` 组件，例如 `FoodText`, `PowerText`, `AmmoText`, `MedicineText`, `ResearchPointsText`, `ElectronicPartsText`。
        *   调整它们的布局。
        *   将这些文本组件分别链接到 `ResourceDisplay` 脚本中对应的公共字段 (如 `foodText`, `powerText` 等)。
    *   **`EventDisplay_Panel`**:
        *   创建 `EventDisplay_Panel` GameObject，挂载 `EventDisplay.cs` 脚本。
        *   在其下创建一个 `TextMeshPro - Text` 组件，命名为 `EventText`。
        *   链接到 `EventDisplay` 脚本的 `eventText` 字段。
    *   **`CombatLogDisplay_Panel`**:
        *   创建 `CombatLogDisplay_Panel` GameObject，挂载 `CombatLogDisplay.cs` 脚本。
        *   在其下创建一个 `TextMeshPro - Text` 组件，命名为 `CombatLogText`。为了能显示多行并滚动，通常会这样做：
            1.  在 `CombatLogDisplay_Panel` 下创建 `UI -> Scroll View`。将其命名为 `LogScrollView`。
            2.  删除 `LogScrollView` 下默认的 `Scrollbar Horizontal` 和 `Scrollbar Vertical` 的把手部分（如果不需要自定义外观，保留即可，或者只保留垂直滚动条）。调整 `Scroll View` 的 `RectTransform`。
            3.  展开 `LogScrollView` -> `Viewport` -> `Content`。选中 `Content` GameObject。
            4.  将 `CombatLogText` (之前创建的TextMeshPro文本) **作为 `Content` GameObject的子对象**。
            5.  调整 `CombatLogText` 的 `RectTransform`：通常使其宽度与 `Content` 相同，高度可以设置为一个较大的初始值或由 `Content Size Fitter` 控制。
            6.  在 `Content` GameObject上添加一个 `Content Size Fitter` 组件。将其 `Vertical Fit` 设置为 `Preferred Size`。这样 `Content` 的高度会根据 `CombatLogText` 的内容自动调整。
            7.  在 `LogScrollView` GameObject上，将其 `Scroll Rect` 组件的 `Content` 字段链接到 `Content` GameObject。
        *   最后，将 `CombatLogText` (TextMeshPro组件) 链接到 `CombatLogDisplay` 脚本的 `combatLogText` 字段。
    *   **`QuestLogDisplay_Panel`**:
        *   创建 `QuestLogDisplay_Panel` GameObject，挂载 `QuestLogDisplay.cs` 脚本。
        *   在其下创建所需的 `TextMeshPro - Text` 组件，如 `ActiveQuestsText`, `CompletedQuestsText`, `QuestNotificationText`。
        *   链接到 `QuestLogDisplay` 脚本对应的字段。

    **注意**: 对于那些需要显示动态列表的UI (如 `SurvivorDisplay`, `WorkstationDisplay`, `ResearchDisplay`)，我们除了创建面板容器、挂载主脚本之外，还需要为其指定列表项的“模板”（预制件）和列表项生成的“容器”（一个Transform）。这将在下一步详细说明。

### 5. 创建和配置UI列表项的预制件 (非常详细的步骤)

许多UI界面需要动态地显示一个项目列表，例如幸存者列表、可研究的科技列表等。为实现这一点，我们会为列表中的单项创建一个模板（预制件），然后在运行时根据数据动态生成这些项。

*   **目标**: 为 `SurvivorDisplay`, `WorkstationDisplay`, 和 `ResearchDisplay` 创建它们各自所需的列表项预制件。

*   **(A) 幸存者列表项 (`SurvivorItem_PF`) 的预制件**:
    1.  **导航到预制件目录**: 在 `Project` (项目) 窗口中，找到或创建 `Assets/_Project/Prefabs/UI/Items/` 目录。选中 `Items` 文件夹。
    2.  **创建空预制件**: 在 `Items` 文件夹的空白区域右键 -> `Create` -> `Prefab`。将新创建的预制件命名为 `SurvivorItem_PF`。
    3.  **进入预制件编辑模式**: 双击 `SurvivorItem_PF` 文件。Unity的场景视图和Hierarchy窗口会切换到预制件编辑模式。
    4.  **构建UI结构 (在预制件内部)**:
        *   **根对象调整**: 选中 `SurvivorItem_PF` 的根。为了让它能被正确布局在UI中，给它添加一个RectTransform。右键根 -> `UI` -> `Image`。然后，在Inspector中，将 `Image` 组件的 `Source Image` 设置为 `None` (或一个透明的背景图)，并将 `Color` 的Alpha通道(A)设为0，使其不可见但拥有RectTransform。调整其大小为你期望的单个列表项的大小（例如，Width: 400, Height: 100）。
        *   **添加文本 (TextMeshPro)**: 右键 `SurvivorItem_PF` 的根 -> `UI` -> `Text - TextMeshPro`。创建一个并将其命名为 `NameText`。使用 `RectTransform` 工具将其放置在预制件的左上角区域。
        *   **重复添加其他UI元素**:
            *   `StatusText` (TextMeshPro Text): 用于显示状态。
            *   `ProfessionText` (TextMeshPro Text): 用于显示职业。
            *   `FoodSlider` (UI -> Slider): 用于显示食物水平。调整其子对象（Background, Fill Area, Handle Slide Area）的样式，可能需要删除Handle。
            *   `FoodValueText` (TextMeshPro Text): 放在 `FoodSlider` 旁边，显示具体数值。
            *   `RestSlider` (UI -> Slider): 用于显示休息水平。
            *   `RestValueText` (TextMeshPro Text): 放在 `RestSlider` 旁边。
            *   `WorkstationText` (TextMeshPro Text): 用于显示分配的工作站。
        *   仔细排列这些元素在预制件的矩形区域内，确保它们清晰可读。
    5.  **挂载脚本**:
        *   选中 `SurvivorItem_PF` 预制件的根GameObject。
        *   在 `Inspector` 窗口，点击 "Add Component"。
        *   搜索 `SurvivorListItemUI` 并添加该脚本 (它应该位于 `Assets/_Project/Scripts/GameCore/UI/Items/SurvivorListItemUI.cs`)。
    6.  **链接脚本字段**:
        *   确保 `SurvivorItem_PF` 的根GameObject仍被选中。
        *   在 `Inspector` 中找到 `Survivor List Item UI (Script)` 组件。
        *   将步骤4中创建的各个UI子元素（例如，`NameText` GameObject本身，或者其上的 `TextMeshProUGUI` 组件）**拖拽**到脚本对应的公共字段上（如 `nameText`, `statusText`, `foodSlider` 等）。确保拖拽的是正确的组件类型（例如，`Text` 字段需要 `TextMeshProUGUI` 组件，`Slider` 字段需要 `Slider` 组件）。
    7.  **保存并退出**: 按 `Ctrl+S` (或 `Cmd+S`) 保存预制件。点击 `Hierarchy` 窗口左上角的左向箭头，退出预制件编辑模式，返回主场景。

*   **(B) 工作站列表项 (`WorkstationItem_PF`) 的预制件**:
    1.  **创建预制件**: 在 `Assets/_Project/Prefabs/UI/Items/` 目录下，创建名为 `WorkstationItem_PF` 的新预制件。
    2.  **编辑UI结构**: 双击进入编辑模式。为其根添加 `UI -> Image` (同样设为透明背景)。根据需要调整大小 (例如 Width: 450, Height: 80)。
    3.  **添加子UI元素**:
        *   `StationTypeText` (TextMeshPro Text)
        *   `AssignedSurvivorsText` (TextMeshPro Text)
        *   `ProductionProgressBar` (UI -> Slider)
        *   `ProductionProgressText` (TextMeshPro Text)
        *   `ManageButton` (UI -> Button - TextMeshPro). 确保按钮的子Text元素也使用TextMeshPro，并修改按钮文本为“管理”或“分配”。
    4.  **挂载脚本**: 将 `Assets/_Project/Scripts/GameCore/UI/Items/WorkstationListItemUI.cs` 脚本拖拽到 `WorkstationItem_PF` 的根GameObject上。
    5.  **链接脚本字段**: 选中预制件根，将创建的UI元素拖拽到 `WorkstationListItemUI` 脚本对应的公共字段上。
    6.  **保存并退出**。

*   **(C) 科技列表项 (`TechItem_PF`) 的预制件**:
    1.  **创建预制件**: 在 `Assets/_Project/Prefabs/UI/Items/` 目录下，创建名为 `TechItem_PF` 的新预制件。
    2.  **编辑UI结构**: 双击进入编辑模式。为其根添加 `UI -> Image` (透明背景)。调整大小 (例如 Width: 500, Height: 120)。
    3.  **添加子UI元素**:
        *   `NameText` (TextMeshPro Text)
        *   `DescriptionText` (TextMeshPro Text) - 可能需要设置其Text组件的Overflow为Truncate或Best Fit，并允许多行。
        *   `StatusText` (TextMeshPro Text)
        *   `FeedbackText` (TextMeshPro Text) - 用于显示成本、前置条件未满足等信息。
        *   `ResearchButton` (UI -> Button - TextMeshPro) - 修改按钮文本为“研究”。
        *   `ProgressBar` (UI -> Slider) - 用于显示研究进度。
    4.  **挂载脚本**: 将 `Assets/_Project/Scripts/GameCore/UI/Items/TechDisplayItem.cs` 脚本拖拽到 `TechItem_PF` 的根GameObject上。
    5.  **链接脚本字段**: 选中预制件根，将创建的UI元素拖拽到 `TechDisplayItem` 脚本对应的公共字段上。
    6.  **保存并退出**。

### 6. 链接预制件到对应的Display脚本

现在，我们需要告诉各个 `XXXDisplay` 脚本使用我们刚刚创建的预制件模板。

*   **配置 `SurvivorDisplay`**:
    1.  **创建面板**: 如果尚未创建，在 `Hierarchy` 中，右键 `Canvas` -> `Create Empty`，命名为 `SurvivorDisplay_Panel`。
    2.  **挂载主脚本**: 将 `SurvivorDisplay.cs` 脚本从 `Project` 窗口 (`Assets/_Project/Scripts/GameCore/UI/`) 拖拽到 `SurvivorDisplay_Panel` GameObject上。
    3.  **链接预制件**: 选中 `SurvivorDisplay_Panel`。在 `Inspector` 中找到 `SurvivorDisplay (Script)` 组件。
        *   将其 `Survivor Item Prefab` 字段，从 `Project` 窗口 (`Assets/_Project/Prefabs/UI/Items/`) 拖拽 `SurvivorItem_PF` 预制件到此字段。
    4.  **创建列表容器**: 在 `SurvivorDisplay_Panel` 下，右键 -> `Create Empty`，命名为 `ListContainer`。这个GameObject将用来容纳所有动态生成的幸存者条目。
        *   调整 `ListContainer` 的 `RectTransform`，使其在 `SurvivorDisplay_Panel` 内占据您希望列表显示的区域。
        *   可以为 `ListContainer` 添加一个 `Vertical Layout Group` 组件 (Add Component -> Layout -> Vertical Layout Group) 来自动排列列表项。根据需要配置其Padding, Spacing, Child Alignment, Child Force Expand等。为了让列表项有自己的高度，可以取消勾选 `Child Control Height`。
        *   为了让 `ListContainer` 的高度能随内容扩展（如果希望在Scroll View中使用），可以为其添加 `Content Size Fitter` 组件，并将 `Vertical Fit` 设置为 `Preferred Size`。
    5.  **链接列表容器**: 将 `Hierarchy` 中的 `ListContainer` GameObject拖拽到 `SurvivorDisplay (Script)` 组件的 `Survivor List Container` 字段上。

*   **配置 `WorkstationDisplay`**:
    1.  **创建面板**: 创建 `WorkstationDisplay_Panel` GameObject (在 `Canvas` 下)，挂载 `WorkstationDisplay.cs` 脚本。
    2.  **链接预制件**: 将 `WorkstationItem_PF` 预制件拖拽到 `WorkstationDisplay` 脚本的 `Workstation Item Prefab` 字段。
    3.  **创建并链接列表容器**: 在 `WorkstationDisplay_Panel` 下创建 `ListContainer` GameObject，调整其布局（可使用 `Vertical Layout Group`），然后将其拖拽到 `WorkstationDisplay` 脚本的 `Workstation List Container` 字段。

*   **配置 `ResearchDisplay`**:
    1.  **创建面板**: 创建 `ResearchDisplay_Panel` GameObject (在 `Canvas` 下)，挂载 `ResearchDisplay.cs` 脚本。
    2.  **链接预制件**: 将 `TechItem_PF` 预制件拖拽到 `ResearchDisplay` 脚本的 `Tech UIPrefab` 字段。
    3.  **链接其他UI元素**: `ResearchDisplay` 还需要链接一些显示当前研究信息和资源总览的UI元素。
        *   **分类容器**: 创建三个空的子GameObject，分别命名为 `AvailableTechContainer`, `InProgressTechContainer`, `CompletedTechContainer`。将它们拖拽到 `ResearchDisplay` 脚本对应的 `Available Tech UIParent`, `In Progress Tech UIParent`, `Completed Tech UIParent` 字段上。为这些容器也添加 `Vertical Layout Group` 以便自动布局。
        *   **当前研究信息**: 创建并链接 `currentResearchNameText` (TextMeshPro Text), `currentResearchDescriptionText` (TextMeshPro Text), `researchProgressSlider` (Slider), `researchProgressPercentageText` (TextMeshPro Text) 到 `ResearchDisplay` 脚本的对应字段。
        *   **资源显示**: `ResearchDisplay` 可能也包含显示各种资源（如食物、电力等）的文本。如果这些与 `ResourceDisplay_Panel` 中的文本是独立的，则需要在这里也创建并链接它们 (如 `foodText`, `powerText` 等字段)。如果它们与 `ResourceDisplay` 共用，则无需重复创建。

完成以上所有步骤后，您的 `MainScene` 场景就搭建好了核心的游戏逻辑对象和基础的UI框架。此时，您可以尝试点击Unity编辑器的播放按钮来运行游戏。由于数据模型（Models）在初始化时（`OnInit`）可能还没有填充初始数据（例如，幸存者、工作站、科技等列表可能是空的），所以界面上可能只显示“无”或空列表，这是正常的。后续步骤会涉及如何在Model中填充初始数据。

## 第三部分：运行游戏与初步验证

在搭建好基础场景和UI框架后，我们来运行游戏并进行一些初步的检查。

1.  **1. 保存场景和项目**:
    *   **保存场景**: 在Unity编辑器的主菜单栏中，选择 `File` -> `Save Scene` (文件 -> 保存场景)，或者使用快捷键 `Ctrl+S` (Windows) / `Cmd+S` (Mac)。确保您当前的 `MainScene` 已保存所有更改。
    *   **保存项目**: 虽然Unity通常会在关键操作后自动保存项目级别的更改，但手动保存一下总是个好习惯。选择 `File` -> `Save Project` (文件 -> 保存项目)。

2.  **2. 运行游戏**:
    *   点击Unity编辑器顶部中央的 **播放 (Play) 按钮** (一个指向右方的三角形图标)。
    *   Unity会编译代码（如果自上次运行以来有更改），然后启动游戏。您应该会在 `Game` (游戏) 视图窗口中看到游戏画面。

3.  **3. 观察初始状态**:
    *   **UI显示**: 您应该能看到之前在 `Canvas` 下创建的各个UI面板 (如 `DayDisplay_Panel`, `ResourceDisplay_Panel` 等)。
        *   `DayDisplay` 应该显示初始的天数、时间和基地生命值（这些值来自于 `GameDataModel` 的初始设置）。
        *   `ResourceDisplay` 应该显示各种资源的初始数量（来自于 `ResourceModel`）。
        *   对于列表型UI (`SurvivorDisplay`, `WorkstationDisplay`, `ResearchDisplay`)，如果对应的Model在初始化时没有添加任何数据，那么这些列表区域可能是空的，或者显示“无”、“目前没有幸存者”等提示信息。这是正常的，因为我们还没有向Model中填充初始的幸存者、工作站或科技数据。
        *   其他UI如事件日志、战斗日志等，初始时也应该是空的。
    *   **控制台 (Console) 窗口**:
        *   打开 `Console` 窗口 (`Window -> General -> Console`)。
        *   **检查错误 (红色图标)**: 是否有任何红色的错误信息？如果有，这通常表示存在严重问题，需要根据错误提示进行排查（常见问题见第六部分）。例如，某个脚本没有正确挂载、某个必要的Inspector字段没有链接、或者代码中存在空引用等。
        *   **检查警告 (黄色图标)**: 是否有黄色的警告信息？
            *   一些警告可能是无害的，或者是提示性的。例如，在 `GameInitializer` 的 `Awake()` 方法中如果过早地尝试获取某个System（而该System是在 `OnInit()` 中才注册到QFramework架构的），可能会产生一个关于未能立即获取到该System的警告，但这通常不影响后续 `Start()` 或 `OnInit()` 中的正确获取。
            *   然而，大量的警告或者某些特定警告（如找不到资源、组件丢失等）也可能预示着潜在的问题。
        *   理想情况下，Console窗口在游戏启动时不应有红色错误。

4.  **4. 初步数据查看 (回顾)**:
    *   正如之前在“第三部分：如何在Unity编辑器中使用和查看项目”中提到的，您可以通过临时添加 `Debug.Log()` 语句到某个UI Controller脚本的 `Start()` 方法中，来打印并验证Model是否已按预期初始化，以及是否包含初始数据。
    *   例如，在 `ResourceDisplay.cs` 的 `Start()` 方法的末尾添加：
        ```csharp
        // (确保顶部有 using QFramework; 和 using YourGameNamespace.Resources; 等)
        var resourceModel = this.GetModel<ResourceModel>();
        if (resourceModel != null)
        {
            UnityEngine.Debug.Log($"启动时食物数量: {resourceModel.GetAmount(GameResourceType.Food)}");
            UnityEngine.Debug.Log($"启动时电力数量: {resourceModel.GetAmount(GameResourceType.Power)}");
        }
        else
        {
            UnityEngine.Debug.LogError("ResourceDisplay: 未能获取到ResourceModel以进行数据验证！");
        }
        ```
    *   运行游戏后，在Console窗口查看这些日志输出，确认数值是否符合预期（例如，在 `ResourceModel` 的 `OnInit` 中设定的初始值）。
    *   **测试完毕后，记得移除或注释掉这些临时的 `Debug.Log()` 语句。**

5.  **5. 后续步骤提示**:
    *   恭喜！如果游戏能够顺利运行起来，并且UI骨架基本显示正常（即使内容为空），那么您已经成功搭建了项目的基础运行环境。
    *   现在这仅仅是一个“骨架”。游戏内的具体目标、交互逻辑、胜利/失败条件、更丰富的游戏内容（如具体的敌人行为、任务目标、科技效果等）都需要根据游戏的设计文档进一步实现和填充。
    *   您可以开始探索各个Model的初始化方法 (通常是 `OnInit()` 或一个类似 `PopulateInitialData()` 的方法)，尝试在其中添加一些初始数据（例如，创建几个默认的幸存者、解锁一些基础科技、设置初始任务等），然后重新运行游戏，观察UI的变化。

## 第四部分：游戏核心系统简介及其配置 (补充)

以下是本项目中主要核心系统的功能简介。一些系统可能有可以在Unity Inspector中调整的参数，如果该System脚本挂载在场景中的某个GameObject上。

*   **资源系统 (ResourceModel)**:
    *   **功能**: 管理游戏中所有类型的资源（如食物、电力、弹药、研究点、电子零件等）的当前数量。负责资源的增加和消耗。UI (`ResourceDisplay`) 会显示这些数据。
    *   **配置**: 资源的初始数量和类型定义通常在 `ResourceModel` 的 `OnInit()` 方法中，通过调用 `RegisterResource` (或类似方法) 完成。

*   **敌人系统 (EnemyModel, EnemySpawningSystem, CombatSystem)**:
    *   **功能**: `EnemyModel` 存储当前活动敌人的数据。`EnemySpawningSystem` (如果独立存在，或逻辑集成在其他System如 `GameLoop` 或 `CombatSystem` 中) 负责按规则（如波数、时间、特定事件）生成敌人。`CombatSystem` 处理敌人与幸存者或基地的战斗交互。
    *   **配置**: 敌人的类型、属性（生命值、攻击力等）可能定义在 `Enemy.cs` 类或对应的敌人预制件上。生成规则（如生成点、时间间隔、数量）可能在 `EnemySpawningSystem` 的Inspector字段中（如果它是MonoBehaviour）或其代码逻辑中。

*   **时间系统 (DayNightSystem, GameDataModel)**:
    *   **功能**: `DayNightSystem` (通常是MonoBehaviour或由 `GameLoop` 驱动) 管理游戏内时间的流逝、昼夜的逻辑循环，并可能触发与时间相关的事件（如新的一天开始）。`GameDataModel` 中的 `CurrentDay` (BindableProperty) 存储当前天数，基地生命值 `BaseHealth` (BindableProperty) 也可能受时间事件影响。UI (`DayDisplay`) 会显示这些信息。
    *   **配置**: 如果 `DayNightSystem.cs` 挂载在场景中的GameObject（如 `_GameManager_`）上，它可能会有如 `public float SecondsPerDay = 60f;` 这样的公共字段，允许您在Inspector中调整一天在现实世界中持续多少秒。

*   **幸存者系统 (SurvivorModel, SurvivorManagerSystem)**:
    *   **功能**: `SurvivorModel` 存储所有幸存者的详细数据（ID, 姓名, 属性, 职业, 状态, 食物/休息水平, 分配的工作站ID等）。`SurvivorManagerSystem` 负责创建新幸存者、更新幸存者的生理需求（食物消耗、休息值增减）、根据需求和行为改变其状态 (如空闲、工作、休息、受伤、需要关注、远征中)，并处理如进食、开始/结束休息等具体行为。
    *   **配置**: 幸存者的初始属性范围、职业类型 (`SurvivorProfession` 枚举)、状态类型 (`SurvivorStatus` 枚举) 在对应脚本中定义。需求消耗/恢复速率 (如 `mFoodConsumptionRate`) 通常在 `SurvivorManagerSystem` 内部作为私有字段定义，修改它们需要改代码。新幸存者的创建通常由代码（如事件响应或初始设置）调用 `CreateNewSurvivor` 方法。

*   **工作站系统 (WorkstationModel, WorkstationSystem)**:
    *   **功能**: `WorkstationModel` 存储所有已建造工作站的数据（ID, 类型, 分配的幸存者列表, 生产进度等）。`WorkstationSystem` 负责处理工作站的建造（包括检查建造成本和消耗资源）、驱动工作站的生产循环（更新进度、产出资源）、以及处理幸存者被分配到工作站或从工作站解除分配的逻辑（与 `SurvivorManagerSystem` 协作）。
    *   **配置**: 工作站类型 (`WorkstationType` 枚举) 在专门的文件或 `Workstation.cs` 中定义。每个工作站的具体参数（如生产周期 `ProductionCycleTime`, 产出资源类型和数量 `OutputResourceType`/`OutputAmountPerCycle`, 容量 `MaxSurvivorSlots`）在 `Workstation.cs` 的构造函数中根据类型初始化。建造成本在 `WorkstationSystem` 的 `OnInit()` 方法内的 `mWorkstationBuildCosts` 字典中定义。

*   **研究系统 (ResearchModel, ResearchSystem)**:
    *   **功能**: `ResearchModel` 存储所有可用科技项目的数据，包括其ID、名称、描述、研究成本、前置条件、当前状态（未解锁、可研究、研究中、已完成）和效果。`ResearchSystem` 管理当前正在研究的科技项目、更新其研究进度（基于时间或投入的研究点）、并在研究完成后应用科技效果（可能影响其他系统，如工作站效率、战斗能力等）。
    *   **配置**: 整个科技树（所有科技及其属性）在 `ResearchModel` 的 `PopulateInitialTechnologies()` 方法中通过代码硬编码定义。修改科技树、成本、效果等都需要直接修改此方法的代码。科技效果类型 (`TechnologyEffectType` 枚举) 和具体效果的应用逻辑在 `ResearchSystem` 的 `CompleteResearch` 方法中处理。

*   **探索系统 (ExplorationModel, ExplorationSystem)**:
    *   **功能**: `ExplorationModel` 存储所有可探索兴趣点 (POI) 的数据（ID, 名称, 难度, 探索时间, 潜在奖励, 状态等）以及当前正在进行的远征队信息。`ExplorationSystem` 负责处理开始远征的逻辑（检查条件如幸存者状态、POI状态）、更新远征队的行程和探索进度、并在远征完成后根据POI的定义和一些随机因素来决定远征结果（如获得的资源、幸存者状态变化、触发的事件等）。
    *   **配置**: 所有POI的详细信息在 `ExplorationModel` 的 `PopulateInitialPOIs()` (或类似名称的方法) 中通过代码定义。远征的参数（如旅行时间计算方式、受伤概率等）在 `ExplorationSystem` 的相关方法中处理。

*   **战斗系统 (CombatSystem, EnemyModel)**:
    *   **功能**: `CombatSystem` 负责处理所有战斗相关的计算和状态变更，例如幸存者对敌人的攻击、敌人对幸存者或基地的攻击、伤害计算（可能考虑武器、护甲、科技加成等）、目标选择逻辑、战斗事件的触发（如敌人死亡）。`EnemyModel` 存储当前战场上所有敌人的数据。
    *   **配置**: 战斗参数（如基础攻击力、防御力、伤害公式、暴击率等）可能在 `CombatSystem` 内部定义，或者与幸存者/敌人的属性关联。武器或技能的特定效果也在这里处理。某些全局战斗参数（如之前提到的 `SurvivorAttackPowerMultiplier`）可能由 `CombatSystem` 的 `BindableProperty` 或可配置字段提供。

*   **随机事件系统 (GEventSystem, EventModel)**:
    *   **功能**: `GEventSystem` (通常指导演系统或全局事件系统) 负责在游戏过程中按一定规则（如满足特定条件、按概率、按固定时间间隔或冷却时间）触发各种随机事件。`EventModel` 可能用来存储当前正在处理的随机事件或事件历史。各种具体的随机事件脚本（如 `FoodSpoilageEvent`, `SurvivorSicknessEvent` 等，它们派生自一个共同的 `RandomEvent` 基类）包含各自事件被触发时的具体执行逻辑（`Execute` 方法）。
    *   **配置**: 随机事件的列表、触发条件、概率、冷却时间等通常在 `GEventSystem` 的初始化逻辑中配置（例如，通过一个工厂列表 `mEventFactories`，每个工厂负责创建一种特定事件的实例并定义其触发参数）。修改或添加新事件需要修改 `GEventSystem` 和创建新的事件脚本。

*   **任务系统 (QuestModel, QuestSystem)**:
    *   **功能**: `QuestModel` 存储所有任务（主线、支线、教程等）的数据，包括任务ID、标题、描述、当前状态（未激活、进行中、已完成、失败）、目标列表（每个目标有其类型、所需数量、当前进度）、以及完成任务后的奖励。`QuestSystem` 负责根据游戏进展或玩家行为来激活新任务、监听相关的游戏事件或检查游戏状态以更新任务目标的进度、在所有目标都达成后将任务标记为完成、并发放任务奖励。
    *   **配置**: 所有任务的详细定义（包括其目标和奖励）都在 `QuestModel` 的 `PopulateInitialQuests()` (或类似名称的方法) 中通过代码硬编码。添加新任务或修改现有任务都需要编辑这个方法。任务目标类型 (`QuestObjectiveType` 枚举) 和奖励类型 (`QuestRewardType` 枚举) 的处理逻辑在 `QuestSystem` 中实现。

**如何找到并修改系统配置**:
1.  **确定哪个系统**: 根据您想调整的功能，确定它属于哪个系统。
2.  **检查System脚本**: 打开对应的System脚本 (例如 `Assets/_Project/Scripts/GameCore/Time/DayNightSystem.cs`)。查看其顶部是否有 `public` 或 `[SerializeField]` 标记的字段，这些是可能在Inspector中配置的。
3.  **找到GameObject**: 如果该System是在场景中运行的（**本项目中，大部分System是通过 `GameInitializer` 注册为非MonoBehaviour的类，直接在代码中配置。少数如 `DayNightSystem` 如果被设计为MonoBehaviour，则会挂载在场景中的某个GameObject上，通常是 `_GameManager_` 或一个专门的 `TimeManager` 对象**）。
4.  **修改Inspector**: 如果System是MonoBehaviour并挂载在GameObject上，选中该GameObject，在 `Inspector` 窗口找到对应的脚本组件，修改其暴露出的字段值。
5.  **代码配置**: 对于绝大多数系统逻辑和核心数据（如科技树、任务链、工作站参数、建造成本、事件触发条件等），它们更可能是在对应Model或System的初始化方法中通过代码定义的（例如在 `OnInit()` 或 `PopulateInitial...()` 方法中，或者直接在类的构造函数或字段初始化器中）。这种情况下，您需要直接修改C#代码来调整这些配置。

## 第五部分：如何扩展或修改 (非常初步的指引)

当您熟悉了项目的基础结构后，可能会希望添加新内容或修改现有功能。以下是一些初步的指引方向：

*   **添加新的随机事件**:
    1.  在 `Assets/_Project/Scripts/GameCore/Events/` (或一个专门的 `RandomEvents/` 子文件夹，如果项目这样组织) 下创建一个新的C#脚本，例如 `MyNewRandomEvent.cs`。
    2.  让这个类继承自 `RandomEvent` 基类。
    3.  实现构造函数（在其中设置事件的 `Title` 属性，也可设置基础概率 `BaseChance`、冷却时间 `Cooldown` 等，如果基类支持的话）。
    4.  重写 (override) `Execute(IArchitecture architecture)` 方法。在这个方法中，使用 `architecture.GetModel<T>()` 和 `architecture.GetSystem<T>()` 来获取需要交互的数据模型和系统，然后编写事件的具体逻辑（例如，改变某个资源数量、修改一个幸存者的状态、触发一个新的UI提示等）。最后，务必设置事件的 `Description` 属性，用以向玩家解释发生了什么。
    5.  找到 `GEventSystem.cs` (或类似名称的随机事件管理系统)。在其初始化方法（可能是 `OnInit` 或构造函数）中，找到一个名为 `mEventFactories` (或类似名称，通常是一个 `List<Func<RandomEvent>>`) 的列表或字典。
    6.  将您的新事件的创建委托（通常是 `() => new MyNewRandomEvent()`）添加到这个集合中，这样随机事件系统才能在合适的时机考虑触发您的新事件。

*   **添加新的科技**:
    1.  打开 `Assets/_Project/Scripts/GameCore/Research/ResearchModel.cs` 文件。
    2.  找到 `PopulateInitialTechnologies()` (或项目实际使用的方法名) 方法。
    3.  参照现有科技的格式，在列表中添加一个新的 `Technology` 对象实例。您需要为其提供一个唯一的ID (字符串)、本地化的名称 (Name) 和描述 (Description) (这些应该是 `BindableProperty<string>` 类型，直接赋值其 `.Value`)、研究成本 (`ResearchPointCost`)、前置科技ID列表 (`PrerequisiteTechIds`，一个 `List<string>`)、以及一个或多个科技效果 (`Effects`，一个 `List<TechnologyEffect>`)。
    4.  如果您的新科技需要一种全新的效果类型：
        *   打开定义 `TechnologyEffectType` 枚举的文件 (可能在 `Technology.cs` 或 `GameEvents.cs` 或专门的枚举脚本中)，在枚举中添加新的效果成员。
        *   打开 `Assets/_Project/Scripts/GameCore/Research/ResearchSystem.cs` 文件，找到 `CompleteResearch()` 方法。在其内部处理科技效果的 `switch (effect.EffectType)` 语句中，为您的新效果类型添加一个新的 `case`分支，并编写实现该效果的具体逻辑（例如，调用某个目标System的新方法来修改游戏状态）。

*   **添加新的任务**:
    1.  打开 `Assets/_Project/Scripts/GameCore/Quests/QuestModel.cs` 文件。
    2.  找到 `PopulateInitialQuests()` (或类似名称) 的方法。
    3.  按照现有任务的格式，创建一个新的 `Quest` 对象实例。为其提供唯一的ID、本地化的标题 (Title) 和描述 (Description)、目标列表 (`Objectives`，一个 `List<QuestObjective>`)、奖励列表 (`Rewards`，一个 `List<QuestReward>`)，以及可能的激活条件或依赖的事件。
    4.  对于每个 `QuestObjective`，您需要指定其类型 (`ObjectiveType`，如收集资源、建造建筑、探索地点等)、目标值 (`RequiredAmount`)、以及描述。
    5.  如果任务需要新的目标类型或奖励类型：
        *   您可能需要修改相关的枚举定义，如 `QuestObjectiveType` 或 `QuestRewardType` (如果它们存在的话)。
        *   然后，您需要修改 `Assets/_Project/Scripts/GameCore/Quests/QuestSystem.cs` 中处理任务进度更新 (`UpdateQuestProgress` 或类似方法) 和奖励发放 (`ApplyReward` 或类似方法) 的逻辑，以确保新的类型能够被正确处理。

*   **添加新的工作站**:
    1.  打开定义 `WorkstationType` 枚举的文件 (很可能在 `Assets/_Project/Scripts/GameCore/Workstations/Workstation.cs` 内部，或者一个共享的枚举文件如 `GameEnums.cs` 或 `GameEvents.cs`)，在枚举中添加新的工作站类型名称。
    2.  打开 `Assets/_Project/Scripts/GameCore/Workstations/Workstation.cs` 文件。在其构造函数 (`public Workstation(WorkstationType type, ...)` 或类似的初始化方法) 中，为您的新 `WorkstationType` 添加一个 `case` 分支（如果使用 `switch` 语句的话），或者相应的 `if/else if` 逻辑，来设置这个新类型工作站的特有参数，例如生产周期 (`ProductionCycleTime`)、产出的资源类型 (`OutputResourceType`) 和数量 (`OutputAmountPerCycle`)、最大幸存者容量 (`MaxSurvivorSlots`) 等。
    3.  打开 `Assets/_Project/Scripts/GameCore/Workstations/WorkstationSystem.cs` 文件。在其 `OnInit()` 方法中，找到名为 `mWorkstationBuildCosts` 的字典，为您的新工作站类型添加一条建造成本记录，指定需要的资源类型 (`GameResourceType`) 和数量。
    4.  （可选）如果新类型的工作站有独特的UI显示需求（例如，不同于现有类型的图标或特殊信息），您可能需要创建或修改对应的 `WorkstationListItemUI.cs` 脚本或其预制件 (`WorkstationItem_PF`)。
    5.  （可选）如果新工作站引入了全新的生产逻辑或交互方式，您可能还需要在 `Workstation.cs` 中添加新的方法，并在 `WorkstationSystem.cs` 中调用它们。

*   **一般性建议**:
    *   **理解模块职责**: 在进行任何修改之前，花点时间理解您要修改的功能主要由哪个Model负责存储数据，由哪个System负责处理相关逻辑，以及哪个UI脚本负责显示和用户交互。这有助于您在正确的位置进行修改。
    *   **遵循QFramework的核心模式**:
        *   **获取数据**: UI和System通常通过 `this.GetModel<MyModel>()` 来安全地获取数据模型的引用。
        *   **执行操作/改变状态**: UI层（或其他系统）通常通过 `this.SendCommand(new MyCommand(params))` 来发送一个命令，请求执行某个操作。Command的 `OnExecute()` 方法内部会获取相应的System，并调用其方法来完成实际工作。
        *   **响应状态变化**: UI元素通常通过注册监听Model或System中的 `BindableProperty<T>` 属性的变化，或者通过 `this.RegisterEvent<MyEvent>(eventData => { /*处理逻辑*/ })` 来监听特定事件，从而在数据变化时自动更新显示。System之间也可能通过事件来进行解耦的通信。
    *   **小步快跑，勤于测试**: 每次只做一个小的改动或添加一个小的功能，然后立即运行游戏进行测试。这样更容易定位问题。
    *   **善用Console**: Unity的Console窗口是您最好的朋友。仔细阅读其中的错误（红色）和警告（黄色）信息，它们通常能直接或间接地指出问题所在。
    *   **版本控制**: 如果您使用Git或其他版本控制系统，请在进行重要修改前提交当前稳定的版本，这样如果改坏了可以方便地回滚。如果没有版本控制，至少手动备份一下项目文件夹。

## 第六部分：故障排除 (常见问题)

在您探索和修改项目的过程中，可能会遇到一些常见问题。以下是一些排查方向：

*   **UI不显示预期数据 / 列表为空 / 数值不更新**:
    1.  **Console错误**: 始终第一步检查Unity的 `Console` 窗口是否有任何红色错误或相关黄色警告。这些信息往往直接指向问题根源。
    2.  **脚本挂载**: 确认相关的UI Controller脚本 (例如 `DayDisplay.cs`, `SurvivorDisplay.cs`) 是否已经正确地附加到了场景中对应的GameObject上 (例如 `DayDisplay_Panel`, `SurvivorDisplay_Panel`)。
    3.  **Inspector字段链接**:
        *   **UI元素**: 选中挂载了UI Controller脚本的GameObject，检查其在 `Inspector` 窗口中暴露出的公共字段 (如 `dayText`, `survivorItemPrefab`, `survivorListContainer` 等) 是否都已正确链接了场景中的UI元素或Project中的预制件。如果某个字段显示为 "None (Type Mismatch)" 或就是空的 "None"，那么脚本将无法操作该UI元素。
        *   **列表项预制件内部**: 如果是列表项UI (如 `SurvivorListItemUI`) 的问题，需要双击进入该预制件的编辑模式，选中挂载了列表项脚本的根GameObject，检查其内部的文本、滑动条等UI元素是否也正确链接到了脚本的字段上。
    4.  **GameInitializer与数据初始化**:
        *   确认场景中存在一个激活的GameObject挂载了 `GameInitializer.cs` 脚本，并且这个脚本成功执行了。`GameInitializer` 负责注册所有的Models和Systems。
        *   检查对应的数据模型 (Model) 的 `OnInit()` 方法（或 `PopulateInitialData()` 等类似方法）是否被调用，以及是否正确地填充了初始数据。如果Model中就没有数据，UI自然无法显示。
    5.  **事件与绑定**:
        *   **事件驱动**: 如果UI更新依赖于QFramework事件，请确认事件是否在正确时机被发送 (`this.SendEvent()`)，以及UI脚本是否正确注册监听了该事件 (`this.RegisterEvent<T>()`)，并且事件类型完全匹配。
        *   **BindableProperty**: 如果UI更新依赖于 `BindableProperty<T>`，请确认UI脚本是否正确地调用了 `.RegisterWithInit(callback)` 或 `.Register(callback)` 来订阅其变化，并且回调方法 (`callback`) 中的UI更新逻辑是否正确。同时，确保在 `OnDestroy()` 中对这些绑定进行了解注册 (`.UnRegister()`)。
    6.  **数据源确认**: 使用 `Debug.Log()` 在UI脚本的 `Start()` 或数据更新回调中打印从Model获取到的数据，确认数据本身是否符合预期。

*   **点击UI按钮没有反应**:
    1.  **Console错误**: 老规矩，先看Console。
    2.  **按钮OnClick事件链接**: 选中场景中的按钮GameObject，在 `Inspector` 窗口找到 `Button` 组件。展开其 `OnClick()` 事件列表。
        *   确保列表中至少有一个条目。
        *   该条目的第一个字段（通常显示为 "RuntimeOnly"）应该指向挂载了处理该按钮点击的脚本的GameObject (例如，某个UI Panel)。
        *   第二个下拉菜单应该选择了正确的脚本组件，然后是该脚本中期望被调用的 `public` 方法 (例如 `MyDisplayScript.OnStartResearchButtonClicked`)。
        *   如果方法需要参数，确保参数已正确配置。
    3.  **UI Controller方法逻辑**: 检查被按钮调用的那个UI Controller方法内部的逻辑。它是否正确地构建并发送了Command (`this.SendCommand(new MyCommand())`)？
    4.  **Command执行逻辑**: 打开对应的Command脚本，检查其 `OnExecute()` 方法。它是否能正确获取到所需的System (`this.GetSystem<MySystem>()`)？System的引用是否为空？
    5.  **System方法逻辑**: Command调用的System方法内部是否有条件判断导致逻辑没有按预期执行？System方法内部是否有 `Debug.Log` 可以帮助追踪其执行路径和内部变量状态？

*   **编译错误 (代码无法运行，Console显示红色错误)**:
    1.  **仔细阅读错误信息**: Console中的编译错误通常会非常明确地指出哪个脚本文件的哪一行代码出了问题，以及错误的类型（例如 "Type or namespace name 'XXX' could not be found", "Method 'YYY' has some invalid arguments", "Member 'ZZZ' cannot be accessed with an instance reference" 等）。
    2.  **常见原因与检查点**:
        *   **拼写与大小写**: C# 是大小写敏感的。请仔细核对类名、方法名、变量名、命名空间等的拼写和大小写是否与它们定义时完全一致。
        *   **`using`语句缺失**: 如果代码中使用了某个特定命名空间下的类型（例如 `List<T>` 需要 `using System.Collections.Generic;`，或者项目自定义的命名空间如 `YourGameNamespace.Survivors`），请确保脚本文件顶部有相应的 `using` 语句。
        *   **参数不匹配**: 调用方法时，提供的参数数量、类型、顺序必须与方法定义时的参数列表完全一致。
        *   **访问权限**: 是否尝试从外部访问一个类的 `private` 或 `protected` 成员？
        *   **标点符号**: 是否遗漏了行尾的分号 `;`？花括号 `{}` 是否正确配对？圆括号 `()` 或方括号 `[]` 是否使用正确？
        *   **脚本文件名与类名不一致**: Unity要求挂载到GameObject上的MonoBehaviour脚本，其文件名必须与脚本中定义的公共类名完全一致（包括大小写）。

*   **NullReferenceException (空引用错误)**:
    *   这是Unity开发中最常见的运行时错误之一，意味着您尝试在一个值为 `null` (即没有指向任何对象实例) 的变量上调用方法或访问其成员。
    *   **定位错误**: Console中的错误信息通常会包含一个调用栈 (Call Stack)，它会显示错误发生在哪一行代码，以及是哪个方法调用了导致错误的方法。双击错误信息可以直接跳转到出错的代码行。
    *   **排查方向**:
        *   **Inspector字段未链接**: 如果出错的变量是一个 `public` 或 `[SerializeField]` 的字段，并且您期望它在Unity编辑器中被赋值（例如，拖拽一个GameObject或Prefab上去），请检查该字段在Inspector中是否仍然是 "None"。
        *   **GetComponent失败**: 如果您使用 `GetComponent<T>()` 来获取一个组件的引用，但当前GameObject上并没有挂载该类型的组件，那么 `GetComponent<T>()` 会返回 `null`。后续使用这个 `null` 引用就会导致空引用错误。在使用 `GetComponent` 的结果前，最好进行空检查。
        *   **对象已被销毁**: 如果一个GameObject已经被 `Destroy()` 了，那么之前获取到的对它或其组件的引用可能会变成 `null` (或者Unity会将其伪装成 `null` 并给出特定提示)。
        *   **方法返回值为空**: 某个方法可能在特定条件下返回 `null`，而调用方没有检查这个返回值就直接使用了。
        *   **Model/System未正确获取或初始化**: 在QFramework的 `IController` 或 `AbstractCommand` 中使用 `this.GetModel<T>()` 或 `this.GetSystem<T>()` 时，如果对应的Model/System没有在 `GameInitializer` 中被正确注册，或者获取时机过早（例如在 `Awake` 中，而Model/System在 `OnInit` 中注册），都可能导致获取到 `null`。

## 第七部分：结语

恭喜您完成了本用户指南的初步阅读！希望这份文档为您提供了一个清晰的起点，帮助您理解和上手这个生存管理游戏项目，并为您后续的开发、修改或内容添加工作打下坚实的基础。

游戏开发是一个充满创造力、挑战和乐趣的旅程。我们鼓励您：

*   **动手实践**: 不要害怕尝试！按照指南中的步骤操作，试着修改一些参数，或者按照“扩展指引”部分添加一些简单的自定义内容。实际操作是学习和理解的最佳途径。
*   **深入代码**: 当您对项目结构和基本工作流程有了整体认识后，不妨选择一两个您感兴趣的功能模块，深入阅读其相关的Model, System, UI, Command和Event脚本，理解它们是如何协同工作的。
*   **查阅QFramework文档**: QFramework本身是一个功能丰富的框架。如果您想更深入地了解其设计理念、高级用法（如对象池、事件的更高级模式、UI Kit、IOC容器等），或者遇到与框架本身相关的问题，查阅QFramework的官方文档、教程或社区（如GitHub、QQ群、论坛等）会非常有帮助。
*   **学习Unity官方文档**: 对于Unity引擎自身的功能（如物理、动画、渲染管线、编辑器操作等），Unity的官方文档和教程是权威且全面的学习资源。

请记住，遇到问题是正常的，解决问题的过程也是学习和成长的过程。善用Console的错误信息，学会调试代码，并积极寻求信息和帮助。

祝您在本项目中的探索旅程愉快且富有成效！如果您有任何建议或发现文档中的不足之处，也欢迎反馈。
