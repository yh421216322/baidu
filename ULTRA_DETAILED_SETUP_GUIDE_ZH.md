# 超详细Unity项目设置与QFramework集成指南 (中文)

本文档旨在为用户提供一个从零开始创建Unity项目，并集成QFramework框架，最终导入并配置我们重构后的生存管理游戏核心脚本的超详细步骤。本文档力求每一步都清晰明了，即使是对Unity编辑器操作不太熟悉的用户也能按部就班地完成项目的基础搭建。

## 第一部分：项目创建和初始设置

### 1. 创建新的Unity项目

这一步将指导您如何使用 Unity Hub 创建一个全新的 Unity 项目。

1.  **打开 Unity Hub (Unity 中心)**:
    *   在您的电脑上找到并双击打开 `Unity Hub` 应用程序图标。

2.  **新建项目 (New Project)**:
    *   在 Unity Hub 窗口中，通常默认会显示 "Projects" (项目) 标签页。如果不是，请点击左侧导航栏的 "Projects"。
    *   点击界面右上角的蓝色 "New project" (新建项目) 按钮。

3.  **选择Unity编辑器版本与项目模板**:
    *   **Editor Version (编辑器版本)**: 在 "New project" 窗口的顶部，会有一个下拉菜单让您选择已安装在您电脑上的Unity编辑器版本。
        *   **强烈推荐**: 选择一个**长期支持版本 (Long-Term Support, LTS)**，因为它们通常更稳定，并且在较长时间内会获得Unity官方的更新和错误修复。例如，`2021.3.x LTS` 或 `2022.3.x LTS` (请将 `x` 替换为您实际安装的具体次版本号，如 `2021.3.18f1`)。如果您尚未安装合适的LTS版本，可以从Unity官方网站或通过Unity Hub的 "Installs" (安装) 标签页下载并安装。
    *   **Templates (模板)**: 在版本选择下方，您会看到一个模板列表。这些模板为不同类型的项目提供了基础设置。
        *   **推荐选择**: **"2D (URP)"** 模板。
            *   **原因**: 我们的生存管理游戏项目，其核心交互主要通过2D用户界面 (UI) 实现。虽然游戏背景可能是2D或简单的3D，但UI的清晰度和性能至关重要。"2D (URP)" 模板使用 Universal Render Pipeline (通用渲染管线)，它对2D渲染有良好支持，性能也较好，并且如果未来需要在场景中加入简单的3D元素，URP同样能够很好地支持。
            *   如果您已明确项目将包含复杂的3D场景元素作为核心玩法的一部分，也可以考虑选择 "3D (URP)" 模板。对于本文档后续的QFramework集成和核心脚本设置步骤而言，这两个模板在初始阶段的差异不大。

4.  **设置项目名称和存储位置**:
    *   **Project name (项目名称)**: 在 "Project Settings" (项目设置) 下的 "Project name" 输入框中，为您的项目指定一个名称。例如：`MySurvivalGame_QFramework` (添加后缀以明确项目特点是个好习惯)。
    *   **Location (存储位置)**: 点击 "Location" 输入框右侧的三个小点图标，会弹出一个文件浏览器窗口。请选择一个您希望在电脑硬盘上存储这个新Unity项目的文件夹路径。建议选择一个空间充足且易于查找的位置。
    *   **(可选) Unity Cloud Organization (Unity云组织)**: 如果您使用Unity的云服务（如 Collaborate, Cloud Build），可以在这里选择一个组织。对于初学者或本地开发，通常可以保持默认设置或选择您的个人账户关联的组织。

5.  **创建项目**:
    *   仔细检查您选择的编辑器版本、模板、项目名称和存储位置。
    *   确认无误后，点击窗口右下角的 "Create project" (创建项目) 按钮。
    *   Unity 编辑器将会启动，并开始创建新项目所需的文件和文件夹。这个过程可能需要几分钟到十几分钟不等，具体时间取决于您的电脑硬件配置和所选Unity版本的初始资源大小。在此期间，请耐心等待，直到Unity编辑器完全打开并显示新项目的界面。

### 2. 导入QFramework框架

QFramework 是我们项目选用的核心开发框架，它提供了一套优秀的代码组织结构和实用工具，有助于提升开发效率和项目可维护性。

*   **选择一种导入方式**: 以下列出了几种常见的QFramework导入方法。请根据您获取QFramework的渠道选择最合适的一种。

    *   **方式一：通过Unity Asset Store (Unity资源商店) - 如果QFramework已在此正式发布**
        1.  **打开Asset Store窗口**: 在Unity编辑器的主菜单栏中，选择 `Window` (窗口) -> `Asset Store`。这通常会在Unity编辑器内打开一个新的标签页，或者在较新版本的Unity中可能会提示您在外部浏览器中打开Asset Store网站。
        2.  **搜索QFramework**: 在Asset Store的搜索栏中，准确输入 "QFramework" 并按回车键或点击搜索按钮。
        3.  **下载与导入**:
            *   在搜索结果中，找到由QFramework官方（通常是 "liangxiegame" 或其团队）发布的资源包。请注意辨别，避免下载非官方或过时的版本。
            *   点击资源包页面上的 "Download" (下载) 按钮。如果该按钮显示为 "Add to My Assets" (添加到我的资源)，请先点击它，然后在您的资源库中找到它并进行下载。如果之前已下载过，按钮可能直接显示为 "Import" (导入) 或 "Re-download" (重新下载)。
            *   等待下载完成。下载完毕后，"Download" 按钮会变为 "Import" (导入)。点击 "Import"。
        4.  **确认导入内容**: Unity会准备包内容，然后弹出一个名为 "Import Unity Package" (导入Unity包) 的窗口。这个窗口会以树状结构列出包内所有即将导入的文件和文件夹。
            *   **确保所有文件都被选中**: 通常情况下，应保持所有文件和文件夹的复选框为勾选状态（这是默认设置），以确保QFramework完整导入。
            *   点击窗口右下角的 "Import" (导入) 按钮。
        5.  Unity会开始将QFramework的文件和文件夹复制到您的项目 `Assets` 目录下。完成后，QFramework即集成到项目中。

    *   **方式二：通过GitHub Release下载 `.unitypackage` 文件**
        1.  **访问QFramework的GitHub Releases页面**: 打开您的网络浏览器，访问QFramework的官方GitHub仓库。通常，其地址会是 `https://github.com/liangxiegame/QFramework` (请务必确认是官方仓库)。在仓库主页，寻找 "Releases" (发布) 或 "Tags" (标签) 部分。
        2.  **下载最新的稳定版 `.unitypackage`**: 在Releases页面，找到最新标记为稳定版 (Stable) 或非预发布 (Pre-release) 的版本。在该版本的 "Assets" (资源) 列表中，下载后缀为 `.unitypackage` 的文件。例如，`QFramework.vx.x.x.unitypackage` (其中x.x.x是版本号)。
        3.  **在Unity中导入自定义包**:
            *   返回到已打开的Unity编辑器。
            *   在主菜单栏中，选择 `Assets` (资源) -> `Import Package` (导入包) -> `Custom Package...` (自定义包...)。
            *   在弹出的操作系统文件浏览器中，导航到您刚刚下载 `.unitypackage` 文件的位置，选中该文件，然后点击 "Open" (打开) 或 "导入"。
        4.  **确认导入内容**: 与方式一类似，Unity会弹出 "Import Unity Package" 窗口。确保所有内容都被勾选。
        5.  点击 "Import" (导入) 按钮。

    *   **方式三：通过 Unity Package Manager (包管理器) 使用 Git URL (推荐，如果框架支持)**
        *   这种方式可以让您更方便地获取更新，是现代Unity包管理的主流方式。
        1.  **打开Package Manager窗口**: 在Unity编辑器的主菜单栏中，选择 `Window` (窗口) -> `Package Manager`。
        2.  **从Git URL添加包**:
            *   在Package Manager窗口的左上角，通常会有一个 "+" (加号) 图标。点击它。
            *   从下拉菜单中选择 "Add package from git URL..." (从git URL添加包...)。
            *   会弹出一个输入框。您需要在此输入QFramework的Git仓库HTTPS URL。这个URL通常以 `.git` 结尾，例如：`https://github.com/liangxiegame/QFramework.git` (请务必从QFramework官方渠道获取准确的Git URL)。
            *   输入URL后，点击 "Add" (添加) 按钮。
            *   Unity会尝试从该Git仓库克隆包信息并将其集成到项目中。这个过程可能需要一些时间，具体取决于您的网络速度和仓库大小。
        3.  **(备选) 通过Scoped Registry添加 (如果QFramework官方提供了Scoped Registry信息)**:
            *   某些包可能通过Scoped Registry分发，这允许开发者添加自定义的包服务器。
            *   如果QFramework提供了此类信息，您需要先在项目设置中配置它：菜单栏 -> `Edit` (编辑) -> `Project Settings...` (项目设置)。
            *   在打开的 "Project Settings" 窗口中，选择左侧的 `Package Manager` (包管理器) 标签。
            *   在右侧的 `Scoped Registries` 部分，点击 "+" (加号) 按钮，然后按照QFramework官方提供的说明填写 `Name` (名称，例如 "QFramework Registry")、`URL` (注册服务器地址) 和 `Scope(s)` (作用域，例如 "com.liangxiegame")。
            *   保存设置后，返回 `Package Manager` 窗口。在窗口顶部的包源选择下拉菜单中（可能初始显示为 "Packages: In Project" 或 "Unity Registry"），选择 "My Registries" (我的注册表) 或您刚刚添加的Registry名称。然后在列表中搜索 "QFramework" 并点击 "Install" (安装)。

*   **导入后验证与处理**:
    1.  **观察Console窗口**: 导入完成后，打开 (或保持打开) Unity的 `Console` (控制台) 窗口 (主菜单 `Window` -> `General` -> `Console`)。
        *   查看是否有来自QFramework的成功初始化、设置或导入完成的日志消息。
        *   **特别注意**：有时QFramework首次导入后，可能会在Console中显示一些提示信息或需要您执行某些操作的向导步骤（例如，点击某个菜单项来生成初始配置文件或运行安装脚本）。请仔细阅读这些信息并按提示操作。
    2.  **检查菜单栏**: 确认Unity编辑器的主菜单栏是否新增了与QFramework相关的菜单项。通常会有一个名为 "QFramework"、"QF" 或 "Framework" 的顶级菜单，其中包含框架的各种工具和设置选项。
    3.  **编译**: Unity会自动编译新导入的脚本。确保Console中没有出现与QFramework相关的编译错误（红色错误信息）。
    4.  **重要性**: 请务必确保QFramework已正确且完整地导入。我们项目后续的所有脚本都将基于QFramework的架构（如 `GameArchitecture.Interface`、`AbstractSystem`、`AbstractModel`、`BindableProperty`、`SendCommand`、`SendEvent`、`GetModel`、`GetSystem` 等核心API）来构建。如果QFramework未正确安装，后续步骤将无法进行。

### 3. 导入TextMeshPro 资源

TextMeshPro (TMP) 是Unity中用于显示文本的首选方案，它提供了比旧版Unity UI Text更丰富的功能和更好的渲染效果。我们的UI预制件和脚本将使用TextMeshPro组件。

1.  **检查TextMeshPro包是否已安装**:
    *   在较新的Unity版本中（通常是2019.x或更高，特别是2020+），TextMeshPro通常已作为内置包存在于项目中。
    *   您可以打开 `Package Manager` 窗口 (`Window` -> `Package Manager`)，在包列表（确保已选择 "Packages: In Project" 或 "Unity Registry"）中搜索 "TextMeshPro"。如果它已安装，会显示一个绿色的勾选标记或 "Remove" (移除)按钮。如果未安装，则会有 "Install" (安装)按钮，请点击安装。

2.  **导入TextMeshPro核心资源 (TMP Essentials)**:
    *   即使TextMeshPro包本身已安装，其正常工作所需的字体、着色器等核心资源也需要手动导入到您的项目 `Assets` 文件夹中。
    *   在Unity编辑器的主菜单栏中，选择 `Window` (窗口) -> `TextMeshPro` -> `Import TMP Essentials` (导入TMP核心资源)。
    *   会弹出一个小的 "Import Unity Package" 窗口，列出即将导入的资源。点击右下角的 "Import" (导入) 按钮。
    *   这些资源通常会被放置在 `Assets/TextMesh Pro` 文件夹下。

3.  **导入TextMeshPro示例与附加资源 (可选，但推荐初学者导入)**:
    *   在TextMeshPro的同一菜单下 (`Window` -> `TextMeshPro`)，通常还有一个 "Import TMP Examples & Extras" (导入TMP示例和附加资源) 的选项。
    *   点击此选项并导入，会添加一些包含示例场景、额外字体和材质的文件夹。这些对于学习TextMeshPro的各种功能和用法非常有帮助，但对于我们项目的基本运行不是必需的。如果您是初学者，建议导入以供参考。

### 4. 创建推荐的项目文件夹结构

一个良好、统一的文件夹结构对于管理日益增多的项目资源和脚本至关重要。它能提高团队协作效率，并使项目更易于理解和维护。

1.  **打开Project (项目) 窗口**: 这是您在Unity编辑器中管理所有项目资源的面板。

2.  **在`Assets` (资源) 文件夹下创建总父文件夹**:
    *   在 `Project` 窗口中，确保 `Assets` 文件夹是当前选中的根目录（或者至少没有选中其子文件夹）。
    *   在 `Assets` 文件夹上右键 (或者在 `Project` 窗口的空白区域右键，如果当前路径是 `Assets`)。
    *   从弹出的上下文菜单中选择 `Create` (创建) -> `Folder` (文件夹)。
    *   新创建的文件夹会处于可编辑名称的状态。将其命名为 `_Project`。
        *   **提示**: 在文件夹名称前使用下划线 `_` 是一种常见的做法，目的是让这个包含您项目核心内容的文件夹在 `Project` 窗口中（按字母顺序排序时）显示在顶部，方便快速访问，以区别于第三方插件或Unity自动生成的文件夹。

3.  **在`_Project`文件夹下创建核心类别子文件夹**:
    *   选中刚刚创建的 `_Project` 文件夹。
    *   右键点击 `_Project` 文件夹 -> `Create` -> `Folder`。创建以下子文件夹，并逐个命名：
        *   `Scenes` (用于存放您自己创建的游戏场景，例如主菜单场景、游戏主场景等)
        *   `Scripts` (用于存放您项目的所有C#脚本)
        *   `Prefabs` (用于存放预制件，即可复用的GameObject模板)
        *   **(可选，但推荐)** 根据您的项目具体需求，还可以创建其他顶级资源类别文件夹，例如：
            *   `Art` (存放美术资源，如贴图、模型、精灵图等)
            *   `Audio` (存放音频资源，如背景音乐、音效)
            *   `Animations` (存放动画文件和动画控制器)
            *   `Materials` (存放材质球)
            *   `Shaders` (存放自定义着色器)

4.  **在`_Project/Scripts/`下创建游戏核心逻辑脚本的根文件夹**:
    *   展开 `_Project` 文件夹，并选中 `Scripts` 子文件夹。
    *   右键点击 `Scripts` 文件夹 -> `Create` -> `Folder`。将这个新文件夹命名为 `GameCore` (或者您也可以使用 `GameLogic`, `GameFeatures`, `Gameplay` 等能清晰表达其用途的名称)。这个 `GameCore` 文件夹将作为我们游戏所有核心玩法C#脚本的根目录。

5.  **在`_Project/Scripts/GameCore/`下根据QFramework架构和游戏模块创建子文件夹**:
    *   选中 `_Project/Scripts/GameCore/` 文件夹。
    *   创建以下子文件夹，用于组织不同类型的脚本：
        *   `Models` (用于存放数据模型脚本，例如 `ResourceModel.cs`, `SurvivorModel.cs` 等。它们负责存储游戏状态数据。)
        *   `Systems` (用于存放系统逻辑脚本，例如 `DayNightSystem.cs`, `CombatSystem.cs` 等。它们负责实现游戏的核心功能和规则。)
        *   `UI` (用于存放所有与用户界面控制相关的脚本，例如 `DayDisplay.cs`, `SurvivorDisplay.cs` 等。)
            *   在 `UI` 文件夹内部，可以进一步创建一个名为 `Items` 的子文件夹。这个 `Items` 文件夹将专门用于存放那些控制动态列表中单个列表项UI的脚本 (例如 `SurvivorListItemUI.cs`, `TechDisplayItem.cs`, `WorkstationListItemUI.cs`)。
        *   `Commands` (用于存放命令脚本，例如 `StartResearchCommand.cs`, `BuildWorkstationCommand.cs` 等。命令用于封装一次性的操作或请求。)
        *   `Events` (用于存放事件定义脚本，例如 `GameEvents.cs`，其中会包含各种游戏内事件的结构体或类定义。)
        *   **各功能模块文件夹**: 为了更好地组织与特定游戏功能紧密相关的脚本，建议为每个主要的游戏功能模块创建一个独立的文件夹。这些文件夹将包含该模块专属的Models, Systems, UI控制脚本, Commands, Events 以及任何其他辅助脚本。例如：
            *   `Combat` (战斗相关)
            *   `Enemies` (敌人相关)
            *   `Exploration` (探索相关)
            *   `Quests` (任务相关)
            *   `Research` (科技研究相关)
            *   `Resources` (资源管理相关)
            *   `Survivors` (幸存者管理相关)
            *   `Time` (时间系统相关)
            *   `Workstations` (工作站相关)

6.  **在`_Project/Prefabs/`下创建UI预制件的子文件夹**:
    *   展开 `_Project` 文件夹，并选中 `Prefabs` 子文件夹。
    *   右键点击 `Prefabs` 文件夹 -> `Create` -> `Folder`。命名为 `UI`。这个文件夹将用于存放所有UI相关的预制件。
    *   在 `UI` 文件夹内部，可以进一步创建一个名为 `Items` 的子文件夹。这个 `Items` 文件夹将专门用于存放那些作为动态列表中单个列表项模板的UI预制件 (例如，我们稍后会创建的 `SurvivorItem_PF.prefab`, `TechItem_PF.prefab`, `WorkstationItem_PF.prefab` 等)。

完成这些步骤后，您的 `Assets` 文件夹结构应该大致如下，这为您后续导入和创建游戏内容提供了一个清晰、有序的基础：
```
Assets/
  ├── _Project/                   # 项目核心内容总父文件夹
  │   ├── Scenes/                 # 存放游戏场景 (.unity文件)
  │   ├── Scripts/                # 存放所有C#脚本
  │   │   └── GameCore/           # 游戏核心逻辑脚本
  │   │       ├── Models/         #   数据模型层
  │   │       ├── Systems/        #   系统逻辑层
  │   │       ├── UI/             #   UI控制层
  │   │       │   └── Items/      #     UI列表项脚本
  │   │       ├── Commands/       #   命令
  │   │       ├── Events/         #   事件定义
  │   │       ├── Combat/         #   战斗模块
  │   │       ├── Enemies/        #   敌人模块
  │   │       ├── Exploration/    #   探索模块
  │   │       ├── Quests/         #   任务模块
  │   │       ├── Research/       #   研究模块
  │   │       ├── Resources/      #   资源模块
  │   │       ├── Survivors/      #   幸存者模块
  │   │       ├── Time/           #   时间模块
  │   │       └── Workstations/   #   工作站模块
  │   ├── Prefabs/                # 存放预制件 (.prefab文件)
  │   │   └── UI/                 #   UI相关预制件
  │   │       └── Items/          #     UI列表项预制件
  │   ├── Art/                    # (可选) 美术资源 (贴图, 模型, 精灵等)
  │   ├── Audio/                  # (可选) 音频资源 (音乐, 音效)
  │   └── (其他自定义资源文件夹...)
  ├── QFramework/                 # QFramework框架文件 (如果通过.unitypackage导入)
  ├── TextMesh Pro/               # TextMeshPro核心资源文件
  └── (其他由Unity或插件生成的文件夹)
```

### 5. 导入游戏脚本和内容

现在，我们将把之前重构好的、基于QFramework的生存管理游戏核心C#脚本导入到我们刚刚精心规划好的项目结构中。

*   **前提假设**:
    *   您已经拥有一个包含所有重构后C#脚本的文件夹。我们假设这个文件夹的顶层直接包含了各个模块的文件夹（如 `Models`, `Systems`, `UI`, `Commands`, `Events`, `Combat`, `Enemies`, 等），或者它包含了一个名为 `GameScritps` 的子文件夹，而这个 `GameScritps` 子文件夹内部才是上述的模块化结构。
    *   **至关重要**: 所有这些C#脚本都必须使用了同一个统一的命名空间，例如 `YourGameNamespace` (或者您在实际项目中决定的其他命名空间)。如果命名空间不一致，导入后Unity将无法正确识别类型之间的关系，导致大量编译错误。

*   **导入操作步骤**:
    1.  **打开您的文件浏览器**: 在您的电脑操作系统中，打开文件浏览器（例如Windows的“资源管理器”或macOS的“访达”）。
    2.  **定位源脚本文件夹**: 导航到包含您所有重构后游戏脚本的那个文件夹。
    3.  **选择要拖拽的内容**:
        *   **情况一**: 如果您的源文件夹直接包含了 `Models`, `Systems`, `UI`, `Commands`, `Events` 以及各个具体模块（`Combat`, `Enemies`等）的文件夹，那么您需要将所有这些文件夹**一起选中**。
        *   **情况二**: 如果您的源文件夹里有一个名为 `GameScritps` 的子文件夹，而这个 `GameScritps` 文件夹内部才包含了上述所有模块文件夹，那么您只需选中这个 `GameScritps` 文件夹。
    4.  **执行拖拽导入**:
        *   将您在步骤3中选中的文件夹（或多个文件夹）从文件浏览器中**直接拖拽**到Unity编辑器的 `Project` (项目) 窗口中。
        *   **目标位置**: 将它们拖拽到您在上一节创建的 `Assets/_Project/Scripts/GameCore/` 文件夹上。当您拖拽到目标文件夹上方时，Unity会高亮显示该文件夹。松开鼠标完成拖拽。
        *   **最终效果**: 无论您如何拖拽，请确保最终的结果是 `Assets/_Project/Scripts/GameCore/` 文件夹下直接包含了 `Models`, `Systems`, `UI`, `Commands`, `Events` 以及所有模块文件夹（`Combat`, `Enemies` 等），并且这些文件夹内部是对应的C#脚本文件。

    5.  **Unity自动处理与编译**:
        *   当您将文件和文件夹拖拽到Unity Project窗口后，Unity会自动检测到新加入的资源。编辑器右下角通常会显示一个旋转的小图标和一个状态提示（如 "Importing assets..." 或 "Compiling scripts..."）。
        *   Unity会首先导入所有文件，然后对所有C#脚本进行编译。这个过程可能需要一些时间，特别是首次导入大量脚本时。

    6.  **检查编译结果与处理错误**:
        *   **打开Console窗口**: 在Unity编辑器的主菜单栏中，选择 `Window` (窗口) -> `General` (通用) -> `Console` (控制台)。这将打开或切换到Console窗口。
        *   **查找编译错误 (Compilation Errors)**:
            *   编译完成后，仔细查看Console窗口中是否有任何以红色错误图标（通常是一个红色圆圈中间带感叹号或叉号）开头的消息。这些是编译错误，它们会阻止游戏运行。
            *   **最常见错误 - 命名空间或类型找不到**:
                *   **原因**: 如果您看到大量类似 "The type or namespace name 'XXX' could not be found (are you missing a using directive or an assembly reference?)" 的错误，这通常意味着：
                    1.  **QFramework未正确导入或初始化**: 确保您已按步骤2成功导入QFramework，并且Console中没有QFramework自身的错误或警告。如果QFramework的核心DLL或脚本丢失，所有依赖它的代码都会报错。
                    2.  **脚本的命名空间不一致**: 检查您导入的脚本是否都严格使用了统一的根命名空间 (例如 `YourGameNamespace`)。如果部分脚本的 `namespace YourGameNamespace.Module { ... }` 与其他脚本不匹配，或者 `using YourGameNamespace.Module;` 语句指向了错误的命名空间，就会出现此类错误。您可能需要手动编辑这些脚本，统一它们的命名空间声明和 `using` 指令。
                    3.  **脚本间依赖问题**: 如果某个脚本A依赖于脚本B，但脚本B因为某种原因未能成功编译或被Unity识别，那么脚本A中引用脚本B类型的地方也会报错。通常解决掉脚本B的错误后，脚本A的错误也会随之消失。
                *   **解决方法**: 逐个处理这些编译错误。双击Console中的错误消息，Unity通常会自动定位到出错的脚本和代码行。根据错误提示进行修正。
            *   **其他编译错误**: 还可能遇到语法错误、方法签名不匹配、访问权限问题等。根据错误信息进行具体分析和修改。
        *   **目标**: 您的目标是消除所有红色的编译错误。直到Console窗口中不再有任何编译错误为止。
        *   **黄色警告消息 (Warnings)**: 黄色警告消息通常不会阻止游戏运行，但它们可能指示了潜在的问题或不推荐的编码实践。建议您也审查这些警告，并尽可能处理掉它们，以确保代码质量。

    7.  **导入其他游戏内容/资源**:
        *   如果您的项目除了C#脚本外，还有其他预先准备好的游戏资源，例如精灵图片 (Sprites)、3D模型 (Models)、纹理 (Textures)、音频文件 (Audio Clips)、动画 (Animations) 等：
        *   请在 `Assets/_Project/` 文件夹下，按照类别创建相应的子文件夹 (例如 `Art/Sprites`, `Art/Models`, `Audio/Music`, `Audio/SFX`, `Animations`)。
        *   然后，从您的电脑文件浏览器中，将这些资源文件拖拽到Unity Project窗口中对应的文件夹下。Unity会自动导入它们。

至此，您的新Unity项目已经创建完毕，QFramework框架和TextMeshPro文本方案也已成功导入。您推荐的项目文件夹结构已经建立，并且最核心的游戏逻辑C#脚本也已导入到项目中并确保没有编译错误。

现在，项目的基础骨架已经搭建完成。接下来的步骤将是在Unity场景中设置核心的游戏管理对象、搭建UI界面、并将脚本组件挂载到相应的GameObject上，使游戏能够真正运行起来。

## 第二部分：核心架构与UI场景搭建

在第一部分中，我们完成了项目创建、框架导入和脚本迁移。现在，我们将搭建游戏运行的基础场景和核心UI框架。

### 1. 创建主游戏场景

1.  **导航到场景文件夹**: 在Unity编辑器的 `Project` (项目) 窗口中，展开 `Assets` -> `_Project` -> `Scenes` 文件夹。
2.  **创建新场景**:
    *   在 `Scenes` 文件夹的空白区域右键。
    *   从上下文菜单中选择 `Create` -> `Scene`。
    *   将新创建的场景文件命名为 `MainScene` (或者您喜欢的其他主场景名称，如 `GameScene`)。
3.  **打开场景**: 双击 `MainScene.unity` 文件以在编辑器中打开它。此时，场景视图 (Scene View) 可能会显示一个默认的空场景，通常包含一个主摄像机 (Main Camera) 和一个平行光 (Directional Light)。
4.  **保存场景**: 即使是空场景，也最好按一下 `Ctrl+S` (Windows) 或 `Cmd+S` (Mac) 来保存它，确保所有更改都被记录。或者通过菜单栏 `File` -> `Save Scene`。

### 2. 设置核心游戏逻辑对象

我们需要一个持久化的GameObject来承载全局的游戏管理脚本，这些脚本将负责初始化和驱动整个游戏的QFramework架构。

1.  **创建空GameObject (`_GameManager_`)**:
    *   在 `Hierarchy` (层级) 窗口的空白区域右键。
    *   从上下文菜单中选择 `Create Empty`。
    *   一个新的名为 "GameObject" 的对象会出现在Hierarchy窗口中。选中它。
    *   在 `Inspector` (检查器) 窗口的顶部，有一个名称输入框。将该GameObject的名称修改为 `_GameManager_`。
        *   **提示**: 使用下划线 `_` 开头和/或结尾是一种常见的命名约定，目的是让这个重要的全局对象在Hierarchy窗口中（按名称排序时）更容易被找到，或者在视觉上与其他类型的对象区分开。

2.  **挂载核心脚本到 `_GameManager_`**:
    *   确保 `_GameManager_` GameObject在Hierarchy窗口中仍处于选中状态。
    *   **挂载 `GameInitializer.cs`**:
        1.  在 `Inspector` (检查器) 窗口中，找到最下方的 "Add Component" (添加组件) 按钮，点击它。
        2.  会弹出一个组件搜索框。在搜索框中输入 `GameInitializer`。
        3.  当 `GameInitializer` 脚本 (它应该位于 `Assets/_Project/Scripts/GameCore/` 目录下，但Unity的搜索通常能直接找到，只要它没有编译错误) 出现在搜索结果列表中时，点击它。这会将 `GameInitializer (Script)` 组件添加到 `_GameManager_` GameObject上。
    *   **挂载 `GameLoop.cs`**:
        1.  再次点击 `_GameManager_` GameObject的 `Inspector` 窗口中的 "Add Component" 按钮。
        2.  在搜索框中输入 `GameLoop`。
        3.  从搜索结果中选择 `GameLoop` 脚本并添加。
    *   **配置 `GameInitializer` 的 `Base Marker` 字段 (新增步骤)**:
        1.  **创建标记对象**: 在 `Hierarchy` (层级) 窗口中，右键 -> `Create Empty`。将这个新的空GameObject命名为 `_BaseLocationMarker_`。
        2.  **定位标记对象**: 选中 `_BaseLocationMarker_`。在 `Inspector` 窗口的 `Transform` 组件中，将其 `Position` (位置) 设置为您希望的基地中心在游戏世界中的坐标。对于一个2D项目，您可能主要调整X和Y值，Z值通常为0或一个固定的UI层级值。例如，可以先保持 `X:0, Y:0, Z:0`。
        3.  **链接到GameInitializer**: 在 `Hierarchy` 窗口中，选中 `_GameManager_` GameObject。
        4.  在 `Inspector` 窗口中找到 `GameInitializer (Script)` 组件。您会看到一个名为 `Base Marker` (类型为 `Transform`) 的公共字段。
        5.  从 `Hierarchy` 窗口中，将 `_BaseLocationMarker_` GameObject **拖拽**到 `GameInitializer (Script)` 组件的 `Base Marker` 字段上。
        6.  **说明**: 这个 `_BaseLocationMarker_` 的位置 (`transform.position`) 将在游戏启动时被 `GameInitializer` 用来设置 `CombatSystem` 中的基地中心位置 (`BasePosition`)。这使得您可以方便地在场景编辑器中直观地调整基地的逻辑位置。

3.  **理解核心脚本的作用**:
    *   `GameInitializer.cs`: 这个脚本是QFramework架构的启动入口。通常，在其 `Awake()` 或 `Start()` 方法中（具体看脚本实现，通常是 `Awake()` 以确保尽早执行），它会负责创建并初始化游戏的总架构 (例如，`GameArchitecture.Instance = new YourGameArchitecture();`)，然后注册所有的全局Systems (系统) 和Models (数据模型)。这是整个游戏能够按照QFramework模式运行起来的**第一步**，也是最关键的一步。
    *   `GameLoop.cs`: 这个脚本的 `Update()` 方法会被Unity引擎在每一游戏帧自动调用一次。我们通常在这个 `Update()` 方法内部，按照预定的顺序，去调用各个需要每帧更新的游戏系统 (Systems) 的 `Update()` 或 `Tick()` 方法 (例如，`DayNightSystem.UpdateDayCycle(Time.deltaTime)`)。这样，`GameLoop` 就充当了驱动整个游戏世界逻辑持续运行和演变的主心跳。

### 3. 创建基础UI环境

接下来，我们将为游戏的用户界面 (User Interface, UI) 创建一个基础的画布 (Canvas) 和事件处理系统。

1.  **创建Canvas (画布)**:
    *   在 `Hierarchy` (层级) 窗口的空白区域右键。
    *   从上下文菜单中选择 `UI` -> `Canvas`。
    *   这会自动创建一个名为 `Canvas` 的GameObject。同时，如果场景中还没有 `EventSystem`，Unity通常会自动创建一个名为 `EventSystem` 的GameObject。
        *   **`Canvas` GameObject**: 这是所有UI元素的根容器。您想在屏幕上显示的所有UI元素（如文本、按钮、图片、滑动条、面板等）都必须是这个 `Canvas` GameObject的子对象，或者是其下其他子Canvas的子对象。它负责组织和渲染UI。
        *   **`EventSystem` GameObject**: 这个对象对于UI的交互至关重要。它负责处理来自用户的输入事件（例如鼠标点击、屏幕触摸、键盘输入等），并将这些事件准确地分发给当前应该接收这些事件的UI元素（例如，哪个按钮被点击了）。一个场景中通常只需要一个 `EventSystem`。确保它存在。

2.  **配置Canvas Scaler (画布缩放器) 组件**:
    *   在 `Hierarchy` 窗口中，选中刚刚创建的 `Canvas` GameObject。
    *   在 `Inspector` (检查器) 窗口中，您会看到 `Canvas` GameObject上默认挂载了几个组件，其中一个是 `Canvas Scaler (Script)`。这个组件的设置对于确保您的UI在不同屏幕分辨率和宽高比的设备上都能有合理且一致的显示效果至关重要。
    *   **UI Scale Mode (UI缩放模式)**: 找到 `UI Scale Mode` 属性。它的默认值通常是 `Constant Pixel Size` (恒定像素大小)。
        *   点击下拉菜单，将其修改为 `Scale With Screen Size` (随屏幕尺寸缩放)。这使得UI元素会根据屏幕的实际大小进行缩放，而不是保持固定的像素尺寸。
    *   **Reference Resolution (参考分辨率)**: 当 `UI Scale Mode` 设置为 `Scale With Screen Size` 后，会出现 `Reference Resolution` (参考分辨率) X 和 Y 输入框。
        *   在这里设置一个您在设计UI时使用的基础分辨率。例如，如果您主要针对高清宽屏显示器进行设计，可以设置为 X: `1920`，Y: `1080`。
        *   这意味着当游戏实际运行在1920x1080分辨率的屏幕上时，UI元素会以其在编辑器中设计的原始尺寸和比例显示。在其他分辨率下，UI元素会以此参考分辨率为基准进行缩放。
    *   **Screen Match Mode (屏幕匹配模式)**: 这个设置决定了当实际屏幕的宽高比与您设置的 `Reference Resolution` 的宽高比不同时，UI整体缩放是更偏向于匹配宽度还是匹配高度。
        *   从下拉菜单中选择一个模式。`Match Width Or Height` (匹配宽度或高度) 是一个非常常用的选项。
        *   当选择 `Match Width Or Height` 后，会出现一个名为 `Match` 的滑块，范围从 0 (Width) 到 1 (Height)。
            *   **滑块在 0 (Width)**: UI会优先保持其在参考分辨率下的宽度比例。如果屏幕变得比参考宽高比更“高”（例如，从16:9变成9:16的竖屏），UI元素可能会在垂直方向上被拉伸或上下出现更多空白。如果屏幕变得更“扁”，则可能垂直方向被裁剪或压缩。
            *   **滑块在 1 (Height)**: UI会优先保持其在参考分辨率下的高度比例。
            *   **滑块在 0.5 (中间)**: UI会尝试在宽度和高度匹配之间取一个平衡。
        *   **建议**: 通常可以先将 `Match` 滑块设置为 `0` (优先匹配宽度，适合横屏游戏) 或 `1` (优先匹配高度，适合竖屏游戏)，或者保持在 `0.5`。然后，您可以在Unity编辑器的 `Game` (游戏) 视图窗口的顶部，手动切换不同的屏幕分辨率和宽高比（例如，从 "Free Aspect" 下拉菜单中选择 "16:9 Landscape", "16:10 Landscape", "iPad Landscape" 等），来观察UI的缩放表现，并根据实际效果调整 `Match` 滑块的值，直到找到最适合您游戏设计的设置。

### 4. 创建并配置主要的静态UI面板

我们的游戏包含多个用于显示不同信息的UI面板（例如显示日期、资源、事件日志等）。现在我们将逐个创建这些面板的容器GameObject，并将它们对应的UI控制脚本挂载上去，然后创建并链接它们内部所需的具体UI元素。

*   **通用步骤概述 (适用于每个静态UI面板)**:
    1.  **创建面板的根GameObject**: 在 `Hierarchy` 窗口中，右键选中 `Canvas` GameObject，选择 `Create Empty`。这将创建一个空的GameObject作为UI面板的容器。
    2.  **重命名面板GameObject**: 选中新创建的空GameObject，在 `Inspector` 窗口顶部的名称框中，为其指定一个清晰的、能反映其功能的名称 (例如 `DayDisplay_Panel`)。以 `_Panel` 作为后缀是一种常见的命名约定。
    3.  **(可选) 调整面板RectTransform**: 如果需要，您可以调整此面板GameObject的 `RectTransform` 组件，使其在屏幕上占据特定区域，或者添加一个背景图片 (`UI -> Image`) 等。但对于纯信息展示面板，通常保持其默认拉伸填充Canvas即可，主要通过其子元素的布局来控制显示。
    4.  **挂载对应的UI Controller脚本**: 选中面板GameObject，在 `Inspector` 窗口点击 "Add Component"，然后搜索并添加该面板对应的UI Controller脚本 (例如，为 `DayDisplay_Panel` 添加 `DayDisplay.cs`)。
    5.  **创建子UI元素**: 在该面板GameObject下，根据脚本的需求，创建并配置所有子UI元素（如文本框、图片、按钮等）。
    6.  **链接子UI元素到脚本字段**: 选中面板GameObject，在 `Inspector` 中找到挂载的UI Controller脚本组件，将其 `public` 字段（这些字段通常用于引用子UI元素）与刚刚创建的对应子UI元素GameObject或其上的组件进行链接（通常通过从Hierarchy拖拽到Inspector字段上）。

---
*   **A. 创建 `DayDisplay_Panel` (日期、时间、基地生命值显示)**

    1.  **创建面板GameObject**:
        *   在 `Hierarchy` (层级) 窗口中，右键选中 `Canvas` GameObject。
        *   从上下文菜单中选择 `Create Empty`。
        *   选中新创建的GameObject (默认可能名为 "GameObject")，在 `Inspector` (检查器) 窗口顶部的名称输入框中，将其名称更改为 `DayDisplay_Panel`。按回车确认。

    2.  **(可选) 调整 `DayDisplay_Panel` 的RectTransform (矩形变换)**:
        *   确保 `DayDisplay_Panel` 在 `Hierarchy` 中被选中。
        *   在 `Inspector` 窗口中，找到 `Rect Transform` 组件。
        *   **锚点预设 (Anchor Presets)**: 点击 `Rect Transform` 组件左上方的九宫格图标。在弹出的预设面板中，按住 `Alt` 键 (Windows) 或 `Option` 键 (Mac)，然后点击左上角的 `top-left` (顶左) 预设。这会将轴心点 (Pivot) 和锚点 (Anchors) 都设置到左上角，并且将对象的位置也对齐到该点。
        *   **位置 (Position) 与尺寸 (Size)**:
            *   `Pos X`: 设置为 `20` (表示从左边缘向右偏移20个像素单位)。
            *   `Pos Y`: 设置为 `-20` (表示从上边缘向下偏移20个像素单位)。
            *   `Width` (宽度): 设置为 `250` (或您认为合适的宽度)。
            *   `Height` (高度): 设置为 `100` (或您认为合适的高度，足够容纳三行文本)。
        *   这些值仅为示例，您可以根据您的UI整体设计自由调整。

    3.  **挂载 `DayDisplay.cs` 脚本**:
        *   确保 `DayDisplay_Panel` GameObject 仍处于选中状态。
        *   在 `Inspector` 窗口中，滚动到底部，点击 "Add Component" (添加组件) 按钮。
        *   在弹出的搜索框中，输入 "DayDisplay"。
        *   当 `DayDisplay (Script)` 出现在搜索结果中时（它应该带有C#脚本图标），点击它。这会将 `DayDisplay` 脚本组件添加到 `DayDisplay_Panel` 上。

    4.  **创建并配置子UI元素 (作为 `DayDisplay_Panel` 的子对象)**:
        *   **创建 `DayText` (天数文本)**:
            1.  在 `Hierarchy` (层级) 窗口中，右键选中 `DayDisplay_Panel` GameObject。
            2.  从上下文菜单中选择 `UI` -> `Text - TextMeshPro`。 (如果这是您第一次在项目中创建TextMeshPro对象，Unity可能会弹出一个 "TMP Importer" 窗口，提示您导入 "TMP Essentials" (TMP核心资源)。请点击 "Import TMP Essentials" 按钮并等待导入完成。)
            3.  选中新创建的 `Text (TMP)` 对象 (它现在是 `DayDisplay_Panel` 的子对象)，在 `Inspector` 窗口顶部的名称框中，将其重命名为 `DayText`。
            4.  **调整 `DayText` 的 `RectTransform`**:
                *   选中 `DayText` GameObject。
                *   在 `Inspector` 中的 `Rect Transform` 组件：
                    *   锚点预设 (Anchor Presets): 按住 `Alt`+`Shift` (Windows) 或 `Option`+`Shift` (Mac)，点击 `top-left` (顶左，同时设置位置和轴心点)。这会将该文本对象拉伸并固定在父对象(`DayDisplay_Panel`)的左上角。为了更精确控制，您可以只设置锚点为 `top-left` (不按Shift)，然后手动调整位置和大小。
                    *   **建议布局**: 假设 `DayDisplay_Panel` 高度为100，我们可以将三个文本分行。对于 `DayText`:
                        *   锚点 (Anchors): Min X=0, Max X=1 (水平拉伸); Min Y=0.66, Max Y=1 (占据顶部1/3)。
                        *   轴心点 (Pivot): X=0.5, Y=1 (顶部居中)。
                        *   位置 (Pos X, Pos Y): 都设为0 (因为锚点已处理)。
                        *   宽度/高度 (Width/Height): 会根据锚点自动调整，或者您可以取消锚点拉伸并手动设置，例如 `Width: 230`, `Height: 25`，然后使用 `Pos X`, `Pos Y` 精确定位。为了简单起见，先采用手动定位：锚点 `top-left`，`Pos X: 10`, `Pos Y: -10`, `Width: 230`, `Height: 25`。
            5.  **配置 `DayText` 的 `TextMeshProUGUI` 组件**:
                *   在 `Inspector` 中找到 `TextMeshProUGUI` 组件。
                *   `Text Input` (文本输入)框: 输入一个占位符文本，例如 "天数: 0"。这有助于您在编辑时看到文本的样子。
                *   `Font Size` (字体大小): 例如，设置为 `20`。
                *   `Color` (字体颜色): 点击颜色条选择您喜欢的颜色 (例如，白色)。
                *   `Alignment` (对齐): 例如，选择 `Left` (水平左对齐) 和 `Middle` (垂直居中对齐)。
        *   **创建 `TimeText` (时间/状态文本)**:
            1.  在 `Hierarchy` 窗口中，右键选中 `DayDisplay_Panel` GameObject。
            2.  选择 `UI` -> `Text - TextMeshPro`。重命名为 `TimeText`。
            3.  **调整 `TimeText` 的 `RectTransform`**:
                *   锚点 (Anchors): `top-left`。
                *   位置和大小: 例如，`Pos X: 10`, `Pos Y: -40` (使其位于 `DayText` 下方，`-10` (DayText的Y) `-25` (DayText的高度) `-5` (间距) = `-40`)，`Width: 230`, `Height: 25`。
            4.  **配置 `TimeText` 的 `TextMeshProUGUI` 组件**:
                *   `Text Input`: "时间: --%" (或 "游戏进行中")。
                *   `Font Size`, `Color`, `Alignment` 自定，与 `DayText` 风格保持一致或略作区分。
        *   **创建 `BaseHealthText` (基地生命值文本)**:
            1.  在 `Hierarchy` 窗口中，右键选中 `DayDisplay_Panel` GameObject。
            2.  选择 `UI` -> `Text - TextMeshPro`。重命名为 `BaseHealthText`。
            3.  **调整 `BaseHealthText` 的 `RectTransform`**:
                *   锚点 (Anchors): `top-left`。
                *   位置和大小: 例如，`Pos X: 10`, `Pos Y: -70` (位于 `TimeText` 下方)，`Width: 230`, `Height: 25`。
            4.  **配置 `BaseHealthText` 的 `TextMeshProUGUI` 组件**:
                *   `Text Input`: "基地生命: 100/100"。
                *   `Font Size`, `Color`, `Alignment` 自定。

    5.  **链接脚本字段到对应的UI元素**:
        1.  在 `Hierarchy` 窗口中，重新选中 `DayDisplay_Panel` GameObject。
        2.  在 `Inspector` 窗口中，找到之前添加的 `DayDisplay (Script)` 组件。
        3.  您会看到该脚本有三个公共字段，等待您链接UI对象：`Day Text` (需要一个 TextMeshProUGUI 组件), `Time Text` (TextMeshProUGUI), `Base Health Text` (TextMeshProUGUI)。
        4.  **执行链接**:
            *   从 `Hierarchy` 窗口中，找到 `DayText` GameObject，用鼠标左键将其**拖拽**到 `DayDisplay (Script)` 组件的 `Day Text` 字段上（该字段右侧通常显示为 "None (TextMeshProUGUI)"）。当您拖拽到正确的字段上时，该字段会高亮显示。松开鼠标即可完成链接。
            *   以同样的方式，将 `Hierarchy` 中的 `TimeText` GameObject 拖拽到 `DayDisplay (Script)` 的 `Time Text` 字段上。
            *   将 `Hierarchy` 中的 `BaseHealthText` GameObject 拖拽到 `DayDisplay (Script)` 的 `Base Health Text` 字段上。
            *   **重要**: 确保您拖拽的是包含 `TextMeshProUGUI` 组件的那个GameObject。如果脚本字段需要的是组件本身而不是GameObject，Unity通常会自动获取，但拖拽GameObject是最稳妥的方式。

---
*   **B. 创建 `ResourceDisplay_Panel` (资源数据显示面板)**

    1.  **创建面板GameObject**:
        *   在 `Hierarchy` 窗口中，右键选中 `Canvas` GameObject -> `Create Empty`。
        *   重命名为 `ResourceDisplay_Panel`。
    2.  **(可选) 调整 `ResourceDisplay_Panel` 的RectTransform**:
        *   例如，锚点预设 (Anchor Presets) 选择 `top-stretch` (顶部水平拉伸)。
        *   `Pos Y`: 设置为 `-10` (使其位于屏幕顶部，并向下偏移10像素作为边距)。
        *   `Height`: 设置为 `60` (或根据您计划的资源文本行数和字体大小调整)。
        *   `Left` 和 `Right` (边距，在锚点为拉伸时出现): 可以都设为 `10` 或 `20`，让面板与屏幕边缘有一些间距。
    3.  **挂载 `ResourceDisplay.cs` 脚本**:
        *   选中 `ResourceDisplay_Panel`。
        *   在 `Inspector` 窗口，点击 "Add Component"，搜索 "ResourceDisplay" 并添加。
    4.  **创建子UI元素 (所有均为 TextMeshPro 文本对象)**:
        *   对于以下每个资源文本，重复此过程：在 `Hierarchy` 中右键 `ResourceDisplay_Panel` -> `UI` -> `Text - TextMeshPro`。然后立即在 `Inspector` 中重命名，并设置 `TextMeshProUGUI` 组件的 `Text Input` 占位符。
            *   `FoodText` (重命名后)，占位符文本："食物: 0"
            *   `PowerText`，占位符文本："电力: 0"
            *   `AmmoText`，占位符文本："弹药: 0"
            *   `MedicineText`，占位符文本："药品: 0"
            *   `ResearchPointsText`，占位符文本："研究点: 0"
            *   `ElectronicPartsText`，占位符文本："电子零件: 0"
        *   **布局这些文本元素**:
            *   选中 `ResourceDisplay_Panel` GameObject。
            *   在 `Inspector` 窗口，点击 "Add Component"，搜索并添加 `Horizontal Layout Group` (水平布局组)。
            *   **配置 `Horizontal Layout Group`**:
                *   `Padding` (内边距): 例如，`Left: 10`, `Right: 10`, `Top: 5`, `Bottom: 5`。
                *   `Spacing` (间距): 例如，设置子元素之间的水平间距为 `15`。
                *   `Child Alignment` (子对象对齐): 例如，设为 `MiddleLeft` (垂直居中，水平从左开始排列)。
                *   `Control Child Size` (控制子对象尺寸): 通常可以勾选 `Width` 和 `Height`，让布局组统一控制。但如果希望文本根据内容自适应宽度，可以不勾选 `Width`，然后在每个文本对象上添加 `Layout Element` 组件并设置其 `Flexible Width`。为简单起见，初学者可以先都勾选，然后调整 `ResourceDisplay_Panel` 的整体宽度。
            *   **调整文本样式**: 选中每一个文本子对象 (如 `FoodText`)，在 `TextMeshProUGUI` 组件中设置 `Font Size` (例如 `18`)，`Color`，并确保 `Alignment` 适合在布局组中使用 (通常是 `Middle` 和 `Center` 或 `Left`)。
    5.  **链接脚本字段**:
        1.  在 `Hierarchy` 窗口中，选中 `ResourceDisplay_Panel` GameObject。
        2.  在 `Inspector` 窗口中，找到 `ResourceDisplay (Script)` 组件。
        3.  将其所有公共文本字段 (`foodText`, `powerText`, `ammoText`, `medicineText`, `researchPointsText`, `electronicPartsText`) 分别与 `Hierarchy` 中对应的 `TextMeshProUGUI` 子元素GameObject (例如 `FoodText` GameObject) **拖拽链接**。

---
*   **C. 创建 `EventDisplay_Panel` (随机事件信息显示)**

    1.  **创建面板GameObject**: 在 `Canvas` 下 `Create Empty`，命名为 `EventDisplay_Panel`。
    2.  **(可选) 调整RectTransform**: 此面板通常用于显示临时出现的事件信息。
        *   锚点预设 (Anchor Presets): 例如，选择 `bottom-center` (底部居中)。
        *   `Pos Y`: 设置为 `50` (使其位于屏幕底部向上50像素)。
        *   `Width`: 例如 `600`。
        *   `Height`: 例如 `100`。
        *   **(可选背景)**: 可以在 `EventDisplay_Panel` 上添加一个 `Image` 组件作为背景板 (UI -> Image)，调整其颜色和透明度，使其与文本内容有区分。确保Image在Hierarchy中位于文本元素下方，或者文本元素是Image的子对象，以保证文本能显示在背景之上。
    3.  **挂载 `EventDisplay.cs` 脚本**: 选中 `EventDisplay_Panel`，Add Component -> `EventDisplay (Script)`。
    4.  **创建子UI元素 (`EventText`)**:
        1.  在 `Hierarchy` 中右键 `EventDisplay_Panel` -> `UI` -> `Text - TextMeshPro`。重命名为 `EventText`。
        2.  **调整 `EventText` 的 `RectTransform`**:
            *   锚点预设 (Anchor Presets): 选择四向拉伸 (九宫格中间那个，按住 `Alt`+`Shift` 点击)。
            *   `Left`, `Right`, `Top`, `Bottom` 边距: 都设置为一个较小的值，例如 `10`，使其在面板内部留有一些空白。
        3.  **配置 `EventText` 的 `TextMeshProUGUI` 组件**:
            *   `Text Input`: "当前无特殊事件发生。"
            *   `Font Size`: 例如 `22`。
            *   `Color`: 合适的颜色。
            *   `Alignment`: 通常设为 `Center` (水平居中) 和 `Middle` (垂直居中)。
            *   `Wrapping & Overflow -> Overflow`: 设置为 `Overflow`，以允许文本内容换行并超出初始框体（如果 `Content Size Fitter` 被用于父级动态调整大小的话），或者根据需要设置。对于固定大小的面板，可能需要调整字体或使用 `Truncate`。
    5.  **链接脚本字段**:
        1.  选中 `EventDisplay_Panel`。
        2.  在 `Inspector` 中找到 `EventDisplay (Script)` 组件。
        3.  将 `Hierarchy` 中的 `EventText` GameObject 拖拽到 `EventDisplay (Script)` 的 `Event Text` 字段上。

---
*   **D. 创建 `CombatLogDisplay_Panel` (战斗日志显示面板)**

    1.  **创建面板GameObject**: 在 `Canvas` 下 `Create Empty`，命名为 `CombatLogDisplay_Panel`。
    2.  **(可选) 调整RectTransform**: 例如，锚点 `bottom-left` (左下角)。
        *   `Pos X`: `20`。
        *   `Pos Y`: `20`。
        *   `Width`: `400`。
        *   `Height`: `150`。
    3.  **挂载 `CombatLogDisplay.cs` 脚本**: 选中 `CombatLogDisplay_Panel`，Add Component -> `CombatLogDisplay (Script)`。
    4.  **创建带滚动条的文本区域 (Scroll View)**:
        1.  **创建Scroll View**: 在 `Hierarchy` 窗口中，右键选中 `CombatLogDisplay_Panel` GameObject -> `UI` -> `Scroll View`。将新创建的 `Scroll View` GameObject命名为 `LogScrollView`。
        2.  **调整 `LogScrollView` 的 `RectTransform`**:
            *   选中 `LogScrollView`。
            *   锚点预设 (Anchor Presets): 选择四向拉伸 (按住 `Alt`+`Shift` 点击九宫格中间的图标)，使其填满 `CombatLogDisplay_Panel`。
            *   `Left`, `Right`, `Top`, `Bottom` 边距可以都设为 `0` (或根据需要设置内边距)。
        3.  **配置 `Scroll Rect` 组件 (在 `LogScrollView` 上)**:
            *   `Horizontal`: 取消勾选此项，因为战斗日志通常是垂直滚动的，不需要水平滚动条。这也会自动隐藏水平滚动条GameObject。
            *   `Vertical`: 保持勾选。
            *   `Movement Type`: 可以设为 `Elastic` (有弹性回弹效果) 或 `Clamped` (无回弹)。
        4.  **配置 `Content` GameObject**:
            *   在 `Hierarchy` 窗口中，展开 `LogScrollView` -> `Viewport` -> `Content`。选中 `Content` GameObject。这个 `Content` 对象是所有实际可滚动内容的直接父级。
            *   **添加 `Vertical Layout Group`**: 在 `Inspector` 窗口为 `Content` GameObject 点击 "Add Component"，搜索并添加 `Vertical Layout Group` (垂直布局组)。
                *   `Padding`: 例如 `Left: 5`, `Right: 5`, `Top: 5`, `Bottom: 5`。
                *   `Spacing`: 例如 `2` (日志条目之间的间距)。
                *   `Child Alignment`: 设为 `UpperLeft` (子对象从左上角开始排列)。
                *   `Control Child Size`: 勾选 `Width` (让子项宽度与Content相同)。不勾选 `Height` (让子项高度自适应)。
                *   `Child Force Expand`: 取消勾选 `Height`。可以勾选 `Width`。
            *   **添加 `Content Size Fitter`**: 点击 "Add Component"，搜索并添加 `Content Size Fitter` (内容尺寸适配器)。
                *   `Horizontal Fit`: 设为 `Unconstrained` (不约束)。
                *   `Vertical Fit`: 设为 `Preferred Size` (首选尺寸)。这将使 `Content` GameObject的高度能够根据其子对象（即日志文本）的总高度自动扩展，从而实现滚动。
        5.  **创建日志文本对象 (`CombatLogText`)**:
            1.  在 `Hierarchy` 窗口中，右键选中上一步的 `Content` GameObject。
            2.  选择 `UI` -> `Text - TextMeshPro`。将此新文本对象命名为 `CombatLogText`。
            3.  **配置 `CombatLogText` 的 `RectTransform`**:
                *   由于父级 `Content` 有 `Vertical Layout Group` 和 `Content Size Fitter`，`CombatLogText` 的 `RectTransform` 通常会自动被布局组管理其位置。确保其宽度能被 `Content` 控制（如果 `Vertical Layout Group` 的 `Control Child Size -> Width` 已勾选）。高度则由文本内容决定。
            4.  **配置 `CombatLogText` 的 `TextMeshProUGUI` 组件**:
                *   `Text Input`: 清空，或输入 "战斗日志:" 作为初始提示。
                *   `Font Size`: 例如 `16`。
                *   `Color`: 合适的颜色。
                *   `Alignment`: `Left` 和 `Top`。
                *   `Wrapping & Overflow -> Overflow`: 确保设置为 `Overflow` (允许文本内容超出RectTransform边界，由`Content Size Fitter`处理容器大小) 或 `Scroll Rect` (如果文本自身需要内部滚动，但不常见于此结构)。
        6.  **链接 `Scroll Rect` 的 `Content` 字段**:
            *   在 `Hierarchy` 窗口中，选中 `LogScrollView` GameObject。
            *   在 `Inspector` 窗口中找到 `Scroll Rect` 组件。
            *   将其 `Content` 字段（目前可能为 "None (Rect Transform)"）链接起来：从 `Hierarchy` 窗口中，将 `Content` GameObject (即 `LogScrollView/Viewport/Content`) **拖拽**到这个 `Content` 字段上。

    5.  **链接脚本字段**:
        1.  在 `Hierarchy` 窗口中，选中 `CombatLogDisplay_Panel` GameObject。
        2.  在 `Inspector` 窗口中，找到 `CombatLogDisplay (Script)` 组件。
        3.  将其 `Combat Log Text` 字段，与 `Hierarchy` 中的 `CombatLogText` GameObject (即 `LogScrollView/Viewport/Content/CombatLogText`) **拖拽链接**。

---
*   **E. 创建 `QuestLogDisplay_Panel` (任务日志显示面板)**

    1.  **创建面板GameObject**: 在 `Canvas` 下 `Create Empty`，命名为 `QuestLogDisplay_Panel`。
    2.  **(可选) 调整RectTransform**: 例如，锚点 `middle-right` (右侧居中)。
        *   `Pos X`: `-20` (距离右边缘20像素向内)。
        *   `Pos Y`: `0`。
        *   `Width`: `300`。
        *   `Height`: `400`。
    3.  **挂载 `QuestLogDisplay.cs` 脚本**: 选中 `QuestLogDisplay_Panel`，Add Component -> `QuestLogDisplay (Script)`。
    4.  **创建子UI元素 (TextMeshPro文本)**:
        *   **`ActiveQuestsText` (活动任务文本)**:
            1.  在 `Hierarchy` 中右键 `QuestLogDisplay_Panel` -> `UI` -> `Text - TextMeshPro`。重命名为 `ActiveQuestsText`。
            2.  调整其 `RectTransform`。例如，锚点 `top-stretch`，`Pos Y: -10`, `Height: 150` (或根据内容调整)，`Left:10`, `Right:10`。
            3.  `TextMeshProUGUI` 组件: `Text Input`: "--- 当前任务 ---\n无"。允许多行 (`Wrapping & Overflow -> Overflow`)。字体、颜色、对齐（通常左上）自定。
        *   **`CompletedQuestsText` (已完成任务文本)**:
            1.  类似地创建，命名为 `CompletedQuestsText`。
            2.  调整其 `RectTransform`，使其位于 `ActiveQuestsText` 下方。例如，锚点 `top-stretch`，`Pos Y: -170` (假设ActiveQuestsText高150，间距10)，`Height: 100`。`Left:10`, `Right:10`。
            3.  `TextMeshProUGUI` 组件: `Text Input`: "--- 已完成的任务 ---\n无"。允许多行。样式自定。
        *   **`QuestNotificationText` (任务通知文本)**:
            1.  类似地创建，命名为 `QuestNotificationText`。
            2.  调整其 `RectTransform`，例如放在面板顶部或底部的一个较小区域。锚点 `top-stretch`，`Pos Y: -10` (如果放在最顶部，其他元素需相应下移)，`Height: 30`。`Left:10`, `Right:10`。
            3.  `TextMeshProUGUI` 组件: `Text Input` 清空。字体、颜色、对齐（通常居中）自定。
        *   **提示**: 如果活动任务或已完成任务列表内容可能非常长，建议将 `ActiveQuestsText` 和 `CompletedQuestsText` 各自放入独立的 `Scroll View` 中，设置方法参考 `CombatLogDisplay_Panel` 的 `LogScrollView`。

    5.  **链接脚本字段**:
        1.  选中 `QuestLogDisplay_Panel`。
        2.  在 `Inspector` 中找到 `QuestLogDisplay (Script)` 组件。
        3.  将其 `Active Quests Text`, `Completed Quests Text`, 和 `Quest Notification Text` 字段，分别与 `Hierarchy` 中创建的对应 `TextMeshProUGUI` 子元素GameObject **拖拽链接**。

---
此时，所有**静态内容为主**的UI面板都已创建完毕，并且它们内部的UI元素也已正确链接到各自的控制脚本。下一步是创建那些用于动态列表项的**预制件 (Prefabs)**。

### 5. 创建和配置UI列表项的预制件 (非常详细的步骤)

对于需要动态展示多个相似条目的UI（例如幸存者列表、工作站列表、科技列表），我们会使用“预制件”(Prefab)作为模板来创建列表中的每一项。下面将详细介绍如何为每种列表创建和配置其对应的预制件。

**(A) 幸存者列表项预制件 (`SurvivorItem_PF`)**

此预制件将作为 `SurvivorDisplay` 中每一位幸存者的UI表现。

1.  **创建空的预制件资源**:
    1.  在 `Project` (项目) 窗口中，导航到您之前创建的预制件存放目录：`Assets/_Project/Prefabs/UI/Items/`。
    2.  确保 `Items` 文件夹被选中。
    3.  在 `Items` 文件夹的空白区域右键，从上下文菜单中选择 `Create` (创建) -> `Prefab`。
    4.  Unity会在 `Items` 文件夹下创建一个新的预制件资源，默认名称可能是 "New Prefab"。立即将其重命名为 `SurvivorItem_PF`，然后按回车确认。

2.  **进入预制件编辑模式**:
    *   在 `Project` 窗口中，双击您刚刚创建的 `SurvivorItem_PF` 预制件。
    *   Unity的界面会切换到预制件编辑模式。`Hierarchy` (层级) 窗口现在会显示 `SurvivorItem_PF` 的内部层级（目前只有一个与预制件同名的根对象），`Scene` (场景) 视图会显示这个预制件的2D预览画布，方便您进行UI布局。

3.  **构建预制件的UI结构 (在预制件内部)**:
    *   **3.1. 设置根GameObject (`SurvivorItem_PF`) 为UI元素**:
        1.  在预制件编辑模式的 `Hierarchy` 窗口中，确保根GameObject `SurvivorItem_PF` 被选中。
        2.  在 `Inspector` (检查器) 窗口中，点击 "Add Component" (添加组件) 按钮。
        3.  在搜索框中输入 "Image"，然后从结果中选择并添加 `Image (Script)` 组件。这将为根GameObject添加一个 `RectTransform` 组件（如果还没有的话），并使其能够被Unity的UI系统识别和布局。
        4.  **配置`Image` 组件 (使其通常作为不可见的容器)**:
            *   在 `Image (Script)` 组件中，将其 `Source Image` 字段通常设置为 `None` (除非您希望每个列表项都有一个背景图片)。
            *   将其 `Color` (颜色) 属性的 Alpha (A) 通道值从 `255` (不透明) 调整为 `0` (完全透明)，这样它本身就不会显示出来，仅作为其子UI元素的容器。
        5.  **调整根GameObject `SurvivorItem_PF` 的 `RectTransform`**:
            *   `Width` (宽度): 设置为您期望的单个幸存者条目的宽度，例如 `400` 像素。
            *   `Height` (高度): 设置为您期望的单个幸存者条目的高度，例如 `120` 像素。这个高度需要足够容纳您计划在条目中显示的所有信息。

    *   **3.2. 添加 `NameText` (用于显示幸存者姓名)**:
        1.  在预制件编辑模式的 `Hierarchy` 窗口中，右键选中根GameObject `SurvivorItem_PF`。
        2.  从上下文菜单中选择 `UI` -> `Text - TextMeshPro`。
        3.  选中新创建的 `Text (TMP)` 对象，在 `Inspector` 窗口顶部的名称框中，将其重命名为 `NameText`。
        4.  **调整 `NameText` 的 `RectTransform`**:
            *   选中 `NameText` GameObject。
            *   在 `Inspector` 中的 `Rect Transform` 组件：
                *   锚点 (Anchors): 设为 `top-left` (左上角)。
                *   轴心点 (Pivot): 设为 `X:0, Y:1` (也是左上角)。
                *   `Pos X`: `10` (表示距离父对象左边缘10像素)。
                *   `Pos Y`: `-10` (表示距离父对象上边缘向下10像素)。
                *   `Width`: `180` (或根据需要调整，确保能容纳较长的名字)。
                *   `Height`: `25` (或适合单行文字的高度)。
        5.  **配置 `NameText` 的 `TextMeshProUGUI` 组件**:
            *   在 `Inspector` 中找到 `TextMeshProUGUI` 组件。
            *   `Text Input` (文本输入)框: 输入一个临时的占位符文本，例如 "姓名: \[幸存者姓名]"。这有助于您在编辑预制件时直观地看到文本的位置和样式。
            *   `Font Size` (字体大小): 例如，设置为 `20`。
            *   `Color` (字体颜色): 选择一个清晰易读的颜色 (例如，白色，如果您的UI背景较暗)。
            *   `Alignment` (对齐): 选择 `Left` (水平左对齐) 和 `Middle` (垂直居中对齐)。

    *   **3.3. 添加 `StatusText` (用于显示幸存者状态)**:
        1.  在 `Hierarchy` 窗口中，右键 `SurvivorItem_PF` -> `UI` -> `Text - TextMeshPro`。重命名为 `StatusText`。
        2.  **调整 `StatusText` 的 `RectTransform`**:
            *   锚点 (Anchors): `top-left`。
            *   `Pos X`: `200` (例如，使其位于 `NameText` 的右侧，或者根据您的布局调整)。
            *   `Pos Y`: `-10` (与 `NameText` 顶部对齐)。
            *   `Width`: `180`。
            *   `Height`: `25`。
        3.  **配置 `StatusText` 的 `TextMeshProUGUI` 组件**:
            *   `Text Input`: "状态: \[当前状态]"。
            *   `Font Size`: 例如 `18`。 `Color`: 合适的颜色。 `Alignment`: `Left`, `Middle`。

    *   **3.4. 添加 `ProfessionText` (用于显示幸存者职业)**:
        1.  右键 `SurvivorItem_PF` -> `UI` -> `Text - TextMeshPro`。重命名为 `ProfessionText`。
        2.  **调整 `RectTransform`**: 锚点 `top-left`。`Pos X: 10`。`Pos Y: -40` (使其位于 `NameText` 的下方，留出一些间距)。`Width: 180`。`Height: `25`。
        3.  **配置 `TextMeshProUGUI`**: `Text Input`: "职业: \[职业名称]"。`Font Size`: `18`。

    *   **3.5. 添加 `WorkstationText` (用于显示分配的工作站)**:
        1.  右键 `SurvivorItem_PF` -> `UI` -> `Text - TextMeshPro`。重命名为 `WorkstationText`。
        2.  **调整 `RectTransform`**: 锚点 `top-left`。`Pos X: 200`。`Pos Y: -40` (与 `ProfessionText` 同行或根据布局调整)。`Width: 180`。`Height: `25`。
        3.  **配置 `TextMeshProUGUI`**: `Text Input`: "岗位: 无"。`Font Size`: `18`。

    *   **3.6. 添加 `FoodSlider` (用于显示食物水平的滑动条)**:
        1.  右键 `SurvivorItem_PF` -> `UI` -> `Slider`。重命名为 `FoodSlider`。
        2.  **调整 `FoodSlider` 的 `RectTransform`**: 锚点 `top-left`。`Pos X: 10`。`Pos Y: -70` (位于 `ProfessionText` 下方)。`Width: 150`。`Height: 20`。
        3.  **配置 `Slider` 组件**:
            *   取消勾选 `Interactable` (此滑动条仅用于显示，用户不可直接拖动)。
            *   `Transition`: 设为 `None`。
            *   `Navigation`: 设为 `None`。
            *   `Min Value`: `0`。
            *   `Max Value`: `1` (我们将在脚本中用0-1的归一化值来控制它，代表百分比)。
            *   `Value`: `0.5` (设置一个初始的占位显示值，例如50%)。
        4.  **调整 `FoodSlider` 视觉样式 (可选但推荐)**:
            *   在 `Hierarchy` 中展开 `FoodSlider`。
            *   选中其子对象 `Handle Slide Area`，然后在 `Inspector` 中禁用或直接删除 `Handle Slide Area` GameObject (因为显示条通常不需要那个圆形滑块把手)。
            *   选中子对象 `Fill Area` -> `Fill`。在 `Image (Script)` 组件中，将其 `Color` 修改为您希望的代表食物的颜色 (例如，绿色)。
            *   选中子对象 `Background`。可以修改其颜色为更暗淡的背景色。

    *   **3.7. 添加 `FoodValueText` (用于显示具体食物数值)**:
        1.  右键 `SurvivorItem_PF` -> `UI` -> `Text - TextMeshPro`。重命名为 `FoodValueText`。
        2.  **调整 `RectTransform`**: 锚点 `top-left`。`Pos X: 170` (使其位于 `FoodSlider` 的右侧)。`Pos Y: -70` (与 `FoodSlider` 垂直对齐)。`Width: 60`。`Height: 20`。
        3.  **配置 `TextMeshProUGUI`**: `Text Input`: "50/100"。`Font Size`: `16`。`Alignment`: `Left`, `Middle`。

    *   **3.8. 添加 `RestSlider` (用于显示休息水平的滑动条)**:
        1.  类似 `FoodSlider` 创建并重命名为 `RestSlider`。
        2.  **调整 `RectTransform`**: 锚点 `top-left`。`Pos X: 240` (例如，使其位于 `FoodValueText` 右侧，或者根据您的整体布局另起一行)。`Pos Y: -70`。`Width: 150`。`Height: 20`。
        3.  **配置 `Slider` 组件**: 同 `FoodSlider` 的配置 (取消Interactable, MinValue=0, MaxValue=1)。`Value` 设为 `0.7` (占位)。`Fill` 颜色可设为代表休息的颜色 (例如，蓝色)。同样可以移除其 `Handle`。
    *   **3.9. 添加 `RestValueText` (用于显示具体休息数值)**:
        1.  类似 `FoodValueText` 创建并重命名为 `RestValueText`。
        2.  **调整 `RectTransform`**: 锚点 `top-left`。`Pos X: 400` (位于 `RestSlider` 右侧)。`Pos Y: -70`。`Width: 60`。`Height: 20`。
        3.  **配置 `TextMeshProUGUI`**: `Text Input`: "70/100"。`Font Size`: `16`。

    *   **仔细排列所有子元素**: 使用 `RectTransform` 工具和 `Inspector` 中的属性，确保所有创建的子UI元素在 `SurvivorItem_PF` 的矩形区域内良好地组织和对齐，没有重叠，并且信息清晰易读。

4.  **挂载 `SurvivorListItemUI.cs` 脚本**:
    *   在预制件编辑模式的 `Hierarchy` (层级) 窗口中，确保 `SurvivorItem_PF` 的根GameObject被选中。
    *   打开 `Project` (项目) 窗口，导航到 `Assets/_Project/Scripts/GameCore/UI/Items/` 文件夹。
    *   找到 `SurvivorListItemUI.cs` 脚本文件。
    *   用鼠标左键将 `SurvivorListItemUI.cs` 脚本**拖拽**到 `SurvivorItem_PF` 根GameObject的 `Inspector` (检查器) 窗口中的空白区域。松开鼠标后，`Survivor List Item UI (Script)` 组件就会被添加到该GameObject上。

5.  **在Inspector中精确链接脚本的公共字段**:
    *   保持 `SurvivorItem_PF` 的根GameObject在 `Hierarchy` 中处于选中状态。
    *   在 `Inspector` (检查器) 窗口中，找到刚挂载的 `Survivor List Item UI (Script)` 组件。
    *   您会看到该脚本暴露出的所有 `public` 字段 (如 `nameText`, `statusText`, `foodSlider`, `foodValueText`, `restSlider`, `restValueText`, `professionText`, `workstationText`)，它们当前的值都是 "None"。
    *   **逐个链接这些字段**:
        *   对于 `Name Text` 字段: 从预制件编辑模式的 `Hierarchy` 窗口中，找到您创建的 `NameText` GameObject，将其**拖拽**到 `Survivor List Item UI (Script)` 组件的 `Name Text` 字段上。
        *   对于 `Status Text` 字段: 从 `Hierarchy` 拖拽 `StatusText` GameObject 到此字段。
        *   对于 `Food Slider` 字段: 从 `Hierarchy` 拖拽 `FoodSlider` GameObject 到此字段。
        *   对于 `Food Value Text` 字段: 从 `Hierarchy` 拖拽 `FoodValueText` GameObject 到此字段。
        *   对于 `Rest Slider` 字段: 从 `Hierarchy` 拖拽 `RestSlider` GameObject 到此字段。
        *   对于 `Rest Value Text` 字段: 从 `Hierarchy` 拖拽 `RestValueText` GameObject 到此字段。
        *   对于 `Profession Text` 字段: 从 `Hierarchy` 拖拽 `ProfessionText` GameObject 到此字段。
        *   对于 `Workstation Text` 字段: 从 `Hierarchy` 拖拽 `WorkstationText` GameObject 到此字段。
    *   **检查**: 确保每个字段都已正确链接，并且链接的是正确的UI元素类型（例如，Text字段链接的是TextMeshProUGUI，Slider字段链接的是Slider）。

6.  **保存预制件并退出编辑模式**:
    *   在Unity编辑器的主菜单栏中选择 `File` -> `Save` (保存)，或者直接按快捷键 `Ctrl+S` (Windows) 或 `Cmd+S` (Mac)，以确保对预制件的所有更改都已保存。
    *   要退出预制件编辑模式并返回到您之前打开的场景（例如 `MainScene`），可以点击 `Hierarchy` (层级) 窗口左上角（通常在搜索框旁边）的一个向左的小箭头图标。它旁边会显示您当前场景的名称（例如 `< MainScene`）。点击这个箭头即可返回。

---
**(B) 工作站列表项预制件 (`WorkstationItem_PF`)**

此预制件将作为 `WorkstationDisplay` 中每一个工作站的UI表现。

1.  **创建空的预制件资源**:
    1.  在 `Project` (项目) 窗口中，导航到 `Assets/_Project/Prefabs/UI/Items/` 目录。
    2.  右键 -> `Create` (创建) -> `Prefab`。
    3.  将新预制件命名为 `WorkstationItem_PF`。

2.  **进入预制件编辑模式**: 双击 `WorkstationItem_PF`。

3.  **构建预制件的UI结构**:
    *   **3.1. 设置根对象为UI元素**: 选中根 `WorkstationItem_PF`，在Inspector中 Add Component -> `Image`。将Image的Source Image设为None, Alpha设为0。调整 `RectTransform`，例如 `Width: 450`, `Height: 90` (示例尺寸，请根据您的设计调整)。
    *   **3.2. 添加 `StationTypeText` (工作站类型文本)**:
        1.  右键 `WorkstationItem_PF` (根) -> `UI` -> `Text - TextMeshPro`。重命名为 `StationTypeText`。
        2.  `RectTransform`: 锚点 `top-left`。`Pos X: 10`, `Pos Y: -10`。`Width: 200`, `Height: 25`。
        3.  `TextMeshProUGUI`: `Text Input`: "类型: \[工作站类型]"。`Font Size`: `20`。`Color`: 白色。`Alignment`: `Left`, `Middle`。
    *   **3.3. 添加 `AssignedSurvivorsText` (已分配幸存者数量文本)**:
        1.  右键 `WorkstationItem_PF` -> `UI` -> `Text - TextMeshPro`。重命名为 `AssignedSurvivorsText`。
        2.  `RectTransform`: 锚点 `top-left`。`Pos X: 220` (或在 `StationTypeText` 右侧)。`Pos Y: -10`。`Width: 200`, `Height: 25`。
        3.  `TextMeshProUGUI`: `Text Input`: "人数: 0/X"。`Font Size`: `18`。
    *   **3.4. 添加 `ProductionProgressBar` (生产进度条)**:
        1.  右键 `WorkstationItem_PF` -> `UI` -> `Slider`。重命名为 `ProductionProgressBar`。
        2.  `RectTransform`: 锚点 `top-left`。`Pos X: 10`, `Pos Y: -40` (位于类型文本下方)。`Width: 300`, `Height: 20`。
        3.  `Slider` 组件: 取消 `Interactable`。`Min Value: 0`, `Max Value: 1` (用于归一化进度)。`Value: 0.25` (占位)。调整 `Fill Area` -> `Fill` 的颜色 (例如，蓝色或黄色)。可删除 `Handle Slide Area` -> `Handle`。
    *   **3.5. 添加 `ProductionProgressText` (生产进度百分比文本)**:
        1.  右键 `WorkstationItem_PF` -> `UI` -> `Text - TextMeshPro`。重命名为 `ProductionProgressText`。
        2.  `RectTransform`: 锚点 `top-left`。`Pos X: 320` (在进度条右侧)。`Pos Y: -40`。`Width: 60`, `Height: 20`。
        3.  `TextMeshProUGUI`: `Text Input`: "25%"。`Font Size`: `16`。`Alignment`: `Left`, `Middle`。
    *   **3.6. 添加 `ManageButton` (管理按钮)**:
        1.  右键 `WorkstationItem_PF` -> `UI` -> `Button - TextMeshPro`。重命名为 `ManageButton`。
            *   **提示**: 如果没有 "Button - TextMeshPro" 选项，可以先创建 `UI -> Button`，然后删除其下的Text子对象，再右键Button -> `UI -> Text - TextMeshPro` 创建一个新的TextMeshPro文本作为按钮标签。或者，如果Button的Text子对象是旧版Text，选中它，然后在Inspector中找到Text组件，点击右上角三个点，选择 "Replace With TextMeshPro"。
        2.  `RectTransform`: 锚点 `middle-right` (或 `bottom-right`)。例如，`Pos X: -10` (距离右边缘10像素向内)。`Pos Y: 0` (如果垂直居中) 或 `Pos Y: -60` (如果在进度条下方)。`Width: 80`, `Height: 30`。
        3.  **修改按钮文本**: 展开 `ManageButton`，选中其下的 `Text (TMP)` (或类似名称) 子对象，在 `Inspector` 中找到 `TextMeshProUGUI` 组件，将其 `Text Input` 修改为 "管理" (或 "分配人员")。

4.  **挂载 `WorkstationListItemUI.cs` 脚本**:
    *   选中 `WorkstationItem_PF` 的根GameObject。
    *   从 `Project` 窗口 (`Assets/_Project/Scripts/GameCore/UI/Items/`) 将 `WorkstationListItemUI.cs` 脚本拖拽到其 `Inspector` 窗口。

5.  **在Inspector中精确链接脚本字段**:
    *   保持 `WorkstationItem_PF` 的根GameObject选中。
    *   在 `Inspector` 中找到 `Workstation List Item UI (Script)` 组件。
    *   **逐个链接**:
        *   `Station Type Text`: 拖拽 `StationTypeText` GameObject。
        *   `Assigned Survivors Text`: 拖拽 `AssignedSurvivorsText` GameObject。
        *   `Production Progress Bar`: 拖拽 `ProductionProgressBar` GameObject。
        *   `Production Progress Text`: 拖拽 `ProductionProgressText` GameObject。
        *   `Manage Button`: 拖拽 `ManageButton` GameObject。

6.  **保存预制件并退出编辑模式**: 按 `Ctrl+S` (或 `Cmd+S`)。点击 `Hierarchy` 左上角的返回箭头。

---
**(C) 科技列表项预制件 (`TechItem_PF`)**

此预制件将作为 `ResearchDisplay` 中每一个科技项目的UI表现。

1.  **创建空预制件**:
    1.  在 `Project` 窗口 `Assets/_Project/Prefabs/UI/Items/` 目录下，右键 -> `Create` -> `Prefab`。
    2.  命名为 `TechItem_PF`。

2.  **进入预制件编辑模式**: 双击 `TechItem_PF`。

3.  **构建预制件的UI结构**:
    *   **3.1. 设置根对象为UI元素**: 选中根 `TechItem_PF`，Inspector中 Add Component -> `Image`。将Image设为透明背景。调整 `RectTransform`，例如 `Width: 500`, `Height: 120`。
    *   **3.2. 添加 `NameText` (科技名称)**:
        1.  右键 `TechItem_PF` -> `UI` -> `Text - TextMeshPro`。重命名为 `NameText`。
        2.  `RectTransform`: 锚点 `top-left`。`Pos X: 10`, `Pos Y: -10`。`Width: 480` (预留较宽空间)。`Height: 25`。
        3.  `TextMeshProUGUI`: `Text Input`: "科技名称: \[科技名称]"。`Font Size`: `22`。
    *   **3.3. 添加 `DescriptionText` (科技描述)**:
        1.  右键 `TechItem_PF` -> `UI` -> `Text - TextMeshPro`。重命名为 `DescriptionText`。
        2.  `RectTransform`: 锚点 `top-left`。`Pos X: 10`, `Pos Y: -40` (位于名称下方)。`Width: 480`。`Height: 40` (预留约2行文本高度)。
        3.  `TextMeshProUGUI`: `Text Input`: "这是科技的详细描述，可能会占用多行显示..."。`Font Size`: `16`。`Color`: 浅灰色或白色。`Alignment`: `Left`, `Top`。确保启用了换行 (通常 `Wrapping & Overflow -> Overflow` 设为 `Overflow` 或 `Truncate`，如果希望在超出时截断并显示省略号则选 `Ellipsis`)。
    *   **3.4. 添加 `StatusText` (状态/成本文本)**:
        1.  右键 `TechItem_PF` -> `UI` -> `Text - TextMeshPro`。重命名为 `StatusText`。
        2.  `RectTransform`: 锚点 `bottom-left` (预制件左下角)。`Pos X: 10`, `Pos Y: 40` (距离底部10，同时给进度条留空间)。`Width: 200`, `Height: 25`。
        3.  `TextMeshProUGUI`: `Text Input`: "状态: 可研究 (成本: 100点)"。`Font Size`: `18`。
    *   **3.5. 添加 `FeedbackText` (反馈信息文本，如前置条件)**:
        1.  右键 `TechItem_PF` -> `UI` -> `Text - TextMeshPro`。重命名为 `FeedbackText`。
        2.  `RectTransform`: 锚点 `bottom-left`。`Pos X: 220` (位于 `StatusText` 右侧)。`Pos Y: 40`。`Width: 270` (延伸到接近按钮的位置)。`Height: 25`。
        3.  `TextMeshProUGUI`: `Text Input`: "(例如：前置条件未满足)"。`Font Size`: `16`。`Color`: 通常设为醒目的颜色，如黄色或红色，用于提示。
    *   **3.6. 添加 `ResearchButton` (研究按钮)**:
        1.  右键 `TechItem_PF` -> `UI` -> `Button - TextMeshPro`。重命名为 `ResearchButton`。
        2.  `RectTransform`: 锚点 `bottom-right` (右下角)。`Pos X: -10` (距离右边缘10像素向内)。`Pos Y: 10` (距离下边缘10像素向上)。`Width: 100`, `Height: 30`。
        3.  修改按钮文本 (其子Text(TMP)对象): "研究"。
    *   **3.7. 添加 `ProgressBar` (研究进度条)**:
        1.  右键 `TechItem_PF` -> `UI` -> `Slider`。重命名为 `ProgressBar`。
        2.  `RectTransform`: 锚点 `bottom-left`。`Pos X: 10`, `Pos Y: 10` (位于状态/反馈文本下方，按钮左侧)。`Width: 370` (或根据按钮位置调整，使其不重叠)。`Height: 20`。
        3.  `Slider` 组件: 取消 `Interactable`。`Min Value: 0`, `Max Value: 1`。`Value: 0`。调整 `Fill Area` -> `Fill` 的颜色 (例如，科技感蓝色)。可删除 `Handle Slide Area` -> `Handle`。

4.  **挂载 `TechDisplayItem.cs` 脚本**:
    *   选中 `TechItem_PF` 的根GameObject。
    *   从 `Project` 窗口 (`Assets/_Project/Scripts/GameCore/UI/Items/`) 将 `TechDisplayItem.cs` 脚本拖拽到其 `Inspector` 窗口。

5.  **在Inspector中精确链接脚本字段**:
    *   保持 `TechItem_PF` 的根GameObject选中。
    *   在 `Inspector` 中找到 `Tech Display Item (Script)` 组件。
    *   **逐个链接**:
        *   `Name Text`: 拖拽 `NameText` GameObject。
        *   `Description Text`: 拖拽 `DescriptionText` GameObject。
        *   `Status Text`: 拖拽 `StatusText` GameObject。
        *   `Research Button`: 拖拽 `ResearchButton` GameObject。
        *   `Progress Bar`: 拖拽 `ProgressBar` GameObject。
        *   `Feedback Text`: 拖拽 `FeedbackText` GameObject。

6.  **保存预制件并退出编辑模式**: 按 `Ctrl+S` (或 `Cmd+S`)。点击 `Hierarchy` 左上角的返回箭头。

---
至此，所有用于动态列表的UI项预制件都已创建完毕，包括其内部的UI元素布局、脚本挂载以及脚本字段的正确链接。这些预制件现在是功能完备的“模板”，可以被它们各自的父级Display脚本用来实例化和填充数据。

### 6. 链接预制件到对应的Display脚本

在2.5节中，我们为动态列表的每一项创建了详细的预制件 (Prefabs)。现在，我们需要将这些预制件告知给它们各自的父级Display脚本 (例如，`SurvivorDisplay.cs` 需要知道 `SurvivorItem_PF` 这个预制件)，并设置好用于容纳这些动态列表项的容器 (通常是一个空GameObject，其RectTransform定义了列表的显示区域和滚动行为)。

**(A) 配置 `SurvivorDisplay` (幸存者列表显示)**

1.  **创建/定位 `SurvivorDisplay_Panel` GameObject**:
    *   如果您在之前的 "2.4 创建并配置主要的静态UI面板" 步骤中尚未创建 `SurvivorDisplay_Panel`，请现在创建它：
        1.  在 `Hierarchy` (层级) 窗口中，右键选中 `Canvas` GameObject。
        2.  选择 `Create Empty`。
        3.  选中新创建的GameObject，在 `Inspector` (检查器) 窗口顶部的名称框中，将其名称更改为 `SurvivorDisplay_Panel`。
    *   **(可选) 调整 `SurvivorDisplay_Panel` 的RectTransform**: 根据您的UI布局设计，调整其锚点、位置和大小。例如，可以将其放在屏幕的某个特定区域，或者设计成一个可滚动的面板。确保它有足够的空间来容纳幸存者列表。

2.  **挂载 `SurvivorDisplay.cs` 脚本**:
    *   确保 `SurvivorDisplay_Panel` GameObject 在 `Hierarchy` 中被选中。
    *   在 `Inspector` 窗口中，点击 "Add Component" (添加组件) 按钮。
    *   在搜索框中输入 "SurvivorDisplay"，然后从结果中选择并添加 `SurvivorDisplay (Script)`。

3.  **链接 `SurvivorItem_PF` 预制件**:
    *   保持 `SurvivorDisplay_Panel` GameObject 选中状态。
    *   在 `Inspector` 窗口中找到 `SurvivorDisplay (Script)` 组件。
    *   您会看到一个名为 `Survivor Item Prefab` (或脚本中定义的类似公共字段名，类型为 `GameObject`) 的字段，当前显示为 "None (GameObject)"。
    *   打开 `Project` (项目) 窗口，导航到 `Assets/_Project/Prefabs/UI/Items/` 文件夹。
    *   找到您在2.5(A)节创建的 `SurvivorItem_PF` 预制件。
    *   用鼠标左键将 `SurvivorItem_PF` 预制件从 `Project` 窗口**直接拖拽**到 `SurvivorDisplay (Script)` 组件的 `Survivor Item Prefab` 字段上。链接成功后，该字段会显示预制件的名称 (`SurvivorItem_PF`)。

4.  **创建并配置列表容器 (`SurvivorListContainer`)**:
    1.  **创建空GameObject作为容器**:
        *   在 `Hierarchy` (层级) 窗口中，右键选中 `SurvivorDisplay_Panel` GameObject。
        *   从上下文菜单中选择 `Create Empty`。
        *   选中这个新创建的空GameObject，在 `Inspector` 窗口中将其重命名为 `SurvivorListContainer` (或 `ListContent`，或其他能表明其用途的名称)。
    2.  **调整 `SurvivorListContainer` 的 `RectTransform`**:
        *   这个容器将决定所有幸存者列表项的排列区域。
        *   选中 `SurvivorListContainer`。
        *   在 `Inspector` 的 `Rect Transform` 组件中，通常需要将其设置为**在父级 `SurvivorDisplay_Panel` 内拉伸**，以便列表可以占据整个或大部分面板区域。
            *   锚点预设 (Anchor Presets): 点击九宫格图标，按住 `Alt`+`Shift` (Windows) 或 `Option`+`Shift` (Mac)，点击九宫格中间的**四向拉伸图标** (通常是右下角的那个，表示上下左右都拉伸)。
            *   `Left`, `Right`, `Top`, `Bottom`: 将这些值都设为 `0` (或根据需要在面板内留出一些边距，例如都设为 `10`)。
        *   **重要提示**: 如果 `SurvivorDisplay_Panel` 本身设计为可滚动（即它是一个 `Scroll View` 的 `Content` 对象），那么这里的 `SurvivorListContainer` 就是那个 `Content` 对象，其 `RectTransform` 设置会略有不同（通常是宽度拉伸，高度由内容决定）。为了本指南的清晰度，我们先假设 `SurvivorDisplay_Panel` 是一个固定大小的区域，如果列表内容超出，则需要外部的 `Scroll View` 来使其滚动。

    5.  **(强烈推荐) 为 `SurvivorListContainer` 添加布局组件**:
        *   选中 `SurvivorListContainer` GameObject。
        *   **添加 `Vertical Layout Group` (垂直布局组)**:
            1.  在 `Inspector` 窗口中，点击 "Add Component"。
            2.  搜索 "Vertical Layout Group" 并添加。
            3.  **配置 `Vertical Layout Group`**:
                *   `Padding` (内边距): `Left`, `Right`, `Top`, `Bottom` (例如，都设为 `5` 或 `10`，让列表项与容器的边缘有一定间距)。
                *   `Spacing` (间距): 设置子对象之间的垂直间距，例如 `5` 或 `10` 像素。
                *   `Child Alignment` (子对象对齐): 通常设为 `UpperLeft` (从左上角开始向下排列子对象)。
                *   `Control Child Size` (控制子对象尺寸):
                    *   `Width`: **勾选**此项。这会使得所有被添加到此容器的列表项 (即 `SurvivorItem_PF` 的实例) 的宽度都被强制设置为与 `SurvivorListContainer` 的宽度一致。
                    *   `Height`: **通常不勾选**此项。这样每个列表项可以保持其预制件中设定的高度 (例如我们之前为 `SurvivorItem_PF` 设定的120像素)。如果勾选，则所有项的高度也会被强制统一。
                *   `Child Force Expand` (子对象强制扩展):
                    *   `Width`: 可以**勾选** (配合 `Control Child Size -> Width`)。
                    *   `Height`: **不勾选**。
        *   **(如果列表内容可能超出 `SurvivorDisplay_Panel` 的高度，则需要将 `SurvivorListContainer` 放入 `Scroll View` 中)**:
            1.  **创建 `Scroll View`**: 在 `SurvivorDisplay_Panel` 下右键 -> `UI` -> `Scroll View`。命名为 `SurvivorListScrollView`。调整其 `RectTransform` 以填充 `SurvivorDisplay_Panel` 中希望用于滚动的区域。
            2.  **配置 `Scroll Rect`**: 在 `SurvivorListScrollView` 上，取消 `Horizontal` 滚动。
            3.  **将 `SurvivorListContainer` 作为 `Content`**: 将之前创建的 `SurvivorListContainer` 拖拽到 `SurvivorListScrollView` -> `Viewport` -> `Content` 的位置（如果 `Scroll View` 创建时已有名为 `Content` 的子对象，可删除默认的，用我们的 `SurvivorListContainer` 替换，或将 `SurvivorListContainer` 的所有组件和子项配置到默认的 `Content` 上，然后重命名）。
            4.  **链接 `Content`**: 选中 `SurvivorListScrollView`，将其 `Scroll Rect` 组件的 `Content` 字段链接到 `SurvivorListContainer` (即现在的Content对象)。
            5.  **为 `SurvivorListContainer` (即Content) 添加 `Content Size Fitter`**:
                *   选中 `SurvivorListContainer`。
                *   `Inspector` -> "Add Component" -> `Content Size Fitter`。
                *   `Horizontal Fit`: `Unconstrained`。
                *   `Vertical Fit`: `Preferred Size`。这非常重要，它使得 `Content` (即 `SurvivorListContainer`) 的高度能根据所有子列表项的总高度自动扩展，从而让 `Scroll View` 知道可以滚动的范围。

    6.  **链接列表容器到 `SurvivorDisplay` 脚本**:
        *   在 `Hierarchy` 窗口中，选中 `SurvivorDisplay_Panel` GameObject。
        *   在 `Inspector` 窗口中找到 `SurvivorDisplay (Script)` 组件。
        *   将其 `Survivor List Container` (类型为 `Transform`) 字段，从 `Hierarchy` 窗口中将您配置好的 `SurvivorListContainer` GameObject **拖拽**到此字段上。

---
**(B) 配置 `WorkstationDisplay` (工作站列表显示)**

1.  **创建/定位 `WorkstationDisplay_Panel` GameObject**:
    *   (如果尚未创建) 在 `Hierarchy` 中，右键 `Canvas` -> `Create Empty`。重命名为 `WorkstationDisplay_Panel`。
    *   (可选) 调整其 `RectTransform` (锚点、位置、大小)。例如，可以放置在屏幕的另一侧，或者与 `SurvivorDisplay_Panel` 相邻。

2.  **挂载 `WorkstationDisplay.cs` 脚本**:
    *   选中 `WorkstationDisplay_Panel`。
    *   `Inspector` -> "Add Component" -> 搜索 "WorkstationDisplay" 并添加。

3.  **链接 `WorkstationItem_PF` 预制件**:
    *   保持 `WorkstationDisplay_Panel` 选中。
    *   在 `Inspector` 的 `WorkstationDisplay (Script)` 组件中，找到 `Workstation Item Prefab` 字段。
    *   从 `Project` 窗口 (`Assets/_Project/Prefabs/UI/Items/`) 将 `WorkstationItem_PF` 预制件**拖拽**到此字段。

4.  **创建并配置列表容器 (`WorkstationListContainer`)**:
    1.  在 `WorkstationDisplay_Panel` 下 `Create Empty`，重命名为 `WorkstationListContainer`。
    2.  调整其 `RectTransform` 以定义列表显示区域。
    3.  **(推荐)** 在 `WorkstationListContainer` 上添加并配置 `Vertical Layout Group` 和 `Content Size Fitter` 组件，步骤与为 `SurvivorListContainer` 配置时完全相同 (参考2.6(A)步骤5)。确保 `Content Size Fitter` 的 `Vertical Fit` 为 `Preferred Size`。如果希望工作站列表可滚动，则此容器同样应是 `Scroll View` 的 `Content`。

5.  **链接列表容器到 `WorkstationDisplay` 脚本**:
    *   选中 `WorkstationDisplay_Panel`。
    *   在 `Inspector` 的 `WorkstationDisplay (Script)` 组件中，找到 `Workstation List Container` (类型 `Transform`) 字段。
    *   从 `Hierarchy` 将 `WorkstationListContainer` GameObject **拖拽**到此字段。

---
**(C) 配置 `ResearchDisplay` (科技列表显示)**

`ResearchDisplay` 稍微复杂一些，因为它可能有多个列表容器（可研究、研究中、已完成）并且还有显示当前研究详情的独立UI元素。

1.  **创建/定位 `ResearchDisplay_Panel` GameObject**:
    *   (如果尚未创建) 在 `Hierarchy` 中，右键 `Canvas` -> `Create Empty`。重命名为 `ResearchDisplay_Panel`。
    *   (可选) 调整其 `RectTransform` (例如，使其占据屏幕中央或一个较大的弹出式窗口区域)。

2.  **挂载 `ResearchDisplay.cs` 脚本**:
    *   选中 `ResearchDisplay_Panel`。
    *   `Inspector` -> "Add Component" -> 搜索 "ResearchDisplay" 并添加。

3.  **链接 `TechItem_PF` 预制件**:
    *   保持 `ResearchDisplay_Panel` 选中。
    *   在 `Inspector` 的 `ResearchDisplay (Script)` 组件中，找到 `Tech UIPrefab` (或在脚本中定义的对应名称) 字段。
    *   从 `Project` 窗口 (`Assets/_Project/Prefabs/UI/Items/`) 将 `TechItem_PF` 预制件**拖拽**到此字段。

4.  **创建并配置多个列表容器 (Transform字段)**:
    *   `ResearchDisplay.cs` 脚本需要链接多个 `Transform` 类型的父容器，用于分类显示不同状态的科技。对每一个容器执行以下操作：
    *   **`AvailableTechContainer` (可研究科技列表容器)**:
        1.  在 `ResearchDisplay_Panel` 下 `Create Empty`，重命名为 `AvailableTechContainer`。
        2.  调整其 `RectTransform`，例如，使其位于面板的某个指定区域用于显示“可研究”的科技列表。
        3.  **(推荐)** 为其添加并配置 `Vertical Layout Group` 组件。根据内容量，可能还需要将其放入一个 `Scroll View` 中，此时 `AvailableTechContainer` 将作为 `Scroll View` 的 `Content` 对象，并需要 `Content Size Fitter` (Vertical Fit: `Preferred Size`)。
    *   **`InProgressTechContainer` (研究中科技列表容器)**:
        1.  类似地，在 `ResearchDisplay_Panel` 下 `Create Empty`，重命名为 `InProgressTechContainer`。
        2.  调整其 `RectTransform`。这个列表通常只显示一项（当前研究的科技）或为空，所以可能不需要复杂的布局或滚动。一个简单的 `Vertical Layout Group` 可能就足够了，或者甚至不需要，如果 `ResearchDisplay` 脚本直接管理该项的位置。为保持一致性，可以添加 `Vertical Layout Group`。
    *   **`CompletedTechContainer` (已完成科技列表容器)**:
        1.  在 `ResearchDisplay_Panel` 下 `Create Empty`，重命名为 `CompletedTechContainer`。
        2.  调整其 `RectTransform`。
        3.  **(推荐)** 为其添加并配置 `Vertical Layout Group` 和 (如果需要滚动) `Content Size Fitter`。

5.  **链接列表容器到 `ResearchDisplay` 脚本**:
    *   选中 `ResearchDisplay_Panel` GameObject。
    *   在 `Inspector` 窗口的 `ResearchDisplay (Script)` 组件中：
        *   将 `Hierarchy` 中的 `AvailableTechContainer` GameObject **拖拽**到 `Available Tech UIParent` 字段。
        *   将 `Hierarchy` 中的 `InProgressTechContainer` GameObject **拖拽**到 `In Progress Tech UIParent` 字段。
        *   将 `Hierarchy` 中的 `CompletedTechContainer` GameObject **拖拽**到 `Completed Tech UIParent` 字段。

6.  **链接其他 `ResearchDisplay` 特有的UI元素字段**:
    *   `ResearchDisplay.cs` 还有一些用于显示当前正在研究的科技项目的详细信息的UI字段。您需要在 `ResearchDisplay_Panel` 下（或者一个专门为此信息创建的子面板GameObject下）创建这些UI元素，然后将它们链接到脚本的对应字段。
    *   **创建并链接 `CurrentResearchNameText` (当前研究名称)**:
        1.  在 `ResearchDisplay_Panel` 下（或一个名为 `CurrentResearchInfoPanel` 的子对象下，以组织结构）创建 `UI -> Text - TextMeshPro`。重命名为 `CurrentResearchNameText_Element` (或任何清晰的唯一名称)。
        2.  调整其 `RectTransform` 和 `TextMeshProUGUI` 组件的属性 (例如，占位符文本: "当前研究: 无", 字体大小, 颜色, 对齐方式)。
        3.  选中 `ResearchDisplay_Panel`，在 `Inspector` 中找到 `ResearchDisplay (Script)` 组件，然后将 `CurrentResearchNameText_Element` GameObject **拖拽**到 `Current Research Name Text` 字段上。
    *   **创建并链接 `CurrentResearchDescriptionText` (当前研究描述)**:
        1.  类似地，在 `ResearchDisplay_Panel` (或其子面板) 下创建 `UI -> Text - TextMeshPro`，重命名为 `CurrentResearchDescriptionText_Element`。
        2.  调整其布局，确保它可以显示较长的描述文本 (可能需要设置 `TextMeshProUGUI` 的 `Wrapping & Overflow -> Overflow` 为 `Overflow` 或 `Truncate`，并调整 `RectTransform` 的高度)。
        3.  链接到 `ResearchDisplay (Script)` 的 `Current Research Description Text` 字段。
    *   **创建并链接 `ResearchProgressSlider` (当前研究进度条)**:
        1.  在 `ResearchDisplay_Panel` (或其子面板) 下创建 `UI -> Slider`，命名为 `ResearchProgressSlider_Element`。
        2.  调整其布局。在 `Slider` 组件中，取消勾选 `Interactable`，设置 `Min Value` 为 0, `Max Value` 为 1, 初始 `Value` 为 0。可以移除 `Handle` 子对象，并调整 `Fill` 和 `Background` 的颜色。
        3.  链接到 `ResearchDisplay (Script)` 的 `Research Progress Slider` 字段。
    *   **创建并链接 `ResearchProgressPercentageText` (当前研究进度百分比)**:
        1.  在 `ResearchDisplay_Panel` (或其子面板) 下创建 `UI -> Text - TextMeshPro`，重命名为 `ResearchProgressPercentageText_Element`。
        2.  调整其布局，通常使其位于 `ResearchProgressSlider_Element` 的旁边或上方。
        3.  在 `TextMeshProUGUI` 组件中，设置 `Text Input` 为 "0%"，并调整字体、颜色、对齐。
        4.  链接到 `ResearchDisplay (Script)` 的 `Research Progress Percentage Text` 字段。
    *   **(资源显示 - 如果适用)**:
        *   在 `ResearchDisplay.cs` 脚本的 `RefreshResourceTexts()` 方法中，我们看到它会更新一些资源文本 (如 `foodText`, `powerText`, `researchPointsText` 等)。这意味着 `ResearchDisplay_Panel` 也需要包含这些文本元素，或者这些字段应从 `ResourceDisplay_Panel` 获取引用（后者不推荐，应保持面板独立）。
        *   **因此，请在 `ResearchDisplay_Panel` 下也创建这些 `TextMeshPro - Text` 元素** (例如 `FoodText_OnResearchPanel`, `PowerText_OnResearchPanel` 等，以区分于 `ResourceDisplay_Panel` 中的同名字段，尽管脚本中的字段名可能就是 `foodText`)。
        *   然后，将这些新创建的文本元素**拖拽链接**到 `ResearchDisplay (Script)` 组件上对应的公共字段 (`foodText`, `powerText`, `researchPointsText`, `electronicPartsText`, `ammoText`, `medicineText`)。

---
完成以上所有链接后，您的UI系统的基础骨架就搭建完毕了。所有主UI面板都已在场景中创建，它们各自的控制脚本已挂载，并且脚本所需的UI元素引用（无论是静态文本还是动态列表的模板和容器）都已通过Unity Inspector正确配置。

## 第三部分：运行游戏与初步验证

在完成第一部分的项目创建、框架导入、脚本迁移，以及第二部分的核心架构和UI场景搭建（包括所有静态面板、动态列表项预制件的创建、配置和链接）之后，现在是时候第一次运行游戏并进行一些基础的验证了。

1.  **1. 保存所有更改**:
    *   **保存当前场景**: 在Unity编辑器的主菜单栏中，选择 `File` (文件) -> `Save Scene` (保存场景)。如果您对 `MainScene` 进行了大量修改，确保这些修改都已保存。快捷键通常是 `Ctrl+S` (Windows) 或 `Cmd+S` (Mac)。
    *   **保存项目设置**: 同样在主菜单栏中，选择 `File` (文件) -> `Save Project` (保存项目)。这将保存所有项目级别的设置，例如包管理器更改、编辑器偏好设置等，确保所有配置都已持久化。

2.  **2. 运行游戏**:
    *   在Unity编辑器界面顶部的工具栏中，找到并点击 **播放 (Play) 按钮**。这个按钮通常是一个指向右方的实心三角形图标 ▶️。
    *   点击后，Unity会：
        *   首先检查是否有未编译的脚本更改。如果有，它会先进行编译。留意编辑器右下角是否有编译进度条或状态提示。如果此时出现编译错误，请返回 **第一部分第5.6节 (检查编译结果与处理错误)** 进行排查，直到所有编译错误被清除。
        *   编译成功后，游戏将在 `Game` (游戏) 视图窗口中开始运行。

3.  **3. 观察初始状态 (预期表现与常见现象)**:
    *   **UI面板显示**:
        *   您应该能在 `Game` (游戏) 视图中看到您在 `MainScene` 的 `Canvas` 下创建的各个UI面板，例如 `DayDisplay_Panel`、`ResourceDisplay_Panel`、`SurvivorDisplay_Panel` 等。它们的位置和基本外观应该与您在编辑器中设置的一致。
        *   **静态数据显示**:
            *   `DayDisplay_Panel`: 应该显示来自 `GameDataModel` 的初始天数 (例如 "天数: 1")、时间 (可能是一个初始的百分比或状态，如 "时间: 0%") 和基地生命值 (例如 "基地生命: 100/100")。这些值是在 `GameDataModel` 的 `OnInit` 中设置并通过 `BindableProperty` 自动更新到 `DayDisplay` 的。
            *   `ResourceDisplay_Panel`: 应该显示各种资源的初始数量，例如 "食物: 50", "电力: 20" 等。这些值是在 `ResourceModel` 的 `OnInit` 中通过 `RegisterResource` 设置的。
            *   `EventDisplay_Panel`: 初始时应该显示类似 "当前无特殊事件发生。" 的文本。
            *   `CombatLogDisplay_Panel`: 初始时战斗日志区域应该是空的，或者只显示您设置的占位符文本。
            *   `QuestLogDisplay_Panel`: 活动任务和已完成任务列表初始时可能是空的，显示 "无"；通知文本也应该是空的。
        *   **动态列表显示**:
            *   `SurvivorDisplay_Panel`, `WorkstationDisplay_Panel`, `ResearchDisplay_Panel`: **这些列表区域极有可能是空的，或者不显示任何条目。这是完全正常的，因为我们还没有在对应的数据模型 (`SurvivorModel`, `WorkstationModel`, `ResearchModel`) 的初始化逻辑中添加任何实际的幸存者、工作站或科技数据。** 它们的 `RefreshXxxList()` 方法被调用时，会从模型中获取数据，如果模型中没有数据，列表自然为空。
    *   **控制台 (Console) 窗口检查**:
        *   保持 `Console` 窗口 (`Window -> General -> Console`) 可见。
        *   **红色错误信息**: 游戏运行后，Console窗口中**不应该**出现任何红色的错误信息。如果出现红色错误，通常意味着存在运行时错误，例如：
            *   `NullReferenceException` (空引用异常): 这是最常见的。通常是因为某个脚本尝试访问一个未被正确赋值（链接）的变量（例如，某个UI元素的Inspector字段忘记拖拽链接了，或者 `this.GetModel<T>()` 返回了null但代码没有检查）。错误信息会指明出错的脚本和行号，请仔细检查该行代码涉及的变量。
            *   脚本执行顺序问题：某个脚本依赖的另一个脚本或数据尚未准备好。
            *   预制件问题：例如，尝试实例化的预制件丢失，或者预制件上的脚本组件丢失。
            *   请根据错误信息和本指南第六部分的故障排除建议进行排查。
        *   **黄色警告信息**:
            *   可能会有一些黄色的警告信息。例如，如果您在 `GameInitializer` 的 `Awake()` 方法中过早地尝试通过 `GameArchitecture.Interface.GetSystem<T>()` 获取某个System的引用，而该System是在 `GameInitializer` 的 `OnInit()` (通常在 `Start()` 中被调用) 中才注册到架构中，那么在 `Awake` 时获取可能会失败并产生一个警告，说明该System尚未注册。但只要后续在 `Start` 或 `OnInit` 中能正确获取和使用，这类早期获取的警告可能不影响最终运行。
            *   其他警告，如“变量XX已赋值但从未使用”，可以暂时忽略，但最好在后续开发中清理。
            *   如果警告指示某些关键资源找不到或组件缺失，则需要认真对待。
        *   **目标**: 确保游戏启动后没有阻碍核心流程的红色错误。

4.  **4. 初步数据查看与验证 (通过代码)**:
    *   如果您想在游戏运行时主动验证某些数据模型是否已按预期初始化并包含正确的初始数据，可以在某个UI Controller脚本（例如 `DayDisplay.cs` 或 `ResourceDisplay.cs`）的 `Start()` 方法的末尾（确保在获取Model之后）临时添加一些 `Debug.Log()` 语句。
    *   **示例：在 `ResourceDisplay.cs` 的 `Start()` 方法中添加**:
        ```csharp
        // (确保脚本顶部有 using QFramework; 和 using YourGameNamespace.Resources; 等)
        // ... 已有的 Start() 方法内容 ...

        // 临时添加以下代码进行数据验证
        var resourceModel = this.GetModel<ResourceModel>();
        if (resourceModel != null)
        {
            UnityEngine.Debug.Log($"[数据验证] ResourceDisplay - Start(): 启动时食物数量: {resourceModel.GetAmount(GameResourceType.Food)}");
            UnityEngine.Debug.Log($"[数据验证] ResourceDisplay - Start(): 启动时电力数量: {resourceModel.GetAmount(GameResourceType.Power)}");
            // 您可以为每个在ResourceModel中注册的资源都添加类似的日志
        }
        else
        {
            UnityEngine.Debug.LogError("[数据验证] ResourceDisplay - Start(): 未能获取到ResourceModel以进行数据验证！");
        }
        ```
    *   **运行游戏并查看Console**: 保存脚本后，返回Unity编辑器并运行游戏。然后切换到 `Console` 窗口。您应该能看到您添加的 `[数据验证]` 日志信息，其中显示了对应资源的初始数量。这可以帮助您确认 `ResourceModel` 是否已按其 `OnInit()` 方法中的定义正确初始化了这些资源。
    *   您可以对其他Model（如 `GameDataModel` 查看初始天数，`SurvivorModel` 查看初始幸存者数量（如果已添加），等）进行类似的数据打印验证。
    *   **重要**: 在完成验证后，**务必记得返回代码中，将这些临时的 `Debug.Log()` 语句删除或注释掉**，以避免在最终发布的游戏中产生不必要的日志输出。

5.  **5. 后续步骤展望**:
    *   如果您已成功运行游戏，UI骨架显示正常，并且Console中没有重大错误，那么恭喜您！您已经成功地将项目的基础框架搭建起来了。
    *   目前，游戏可能看起来还比较“空”，因为大部分数据模型（如幸存者列表、科技树、任务列表、可探索地点等）的初始化方法（通常是 `OnInit()` 或 `PopulateInitialData()`）中可能还没有包含具体的初始数据。
    *   **接下来的探索方向可能包括**:
        *   **填充初始数据**: 打开各个Model脚本 (例如 `SurvivorModel.cs`, `ResearchModel.cs`, `QuestModel.cs`, `ExplorationModel.cs`, `WorkstationModel.cs`)，找到它们的 `OnInit()` 或类似的初始化数据填充方法。参照脚本中的注释或现有结构，尝试添加一些初始的幸存者、科技、任务、工作站、POI等数据。保存脚本并重新运行游戏，观察UI上是否能正确显示这些新添加的数据。
        *   **理解游戏流程**: 根据您对游戏设计的理解（或参考游戏设计文档，如果存在的话），尝试在游戏中执行一些基本操作（如果UI上已有交互按钮的话），并通过之前介绍的 `CODE_FLOW_VISUALIZATION.html` 和 `CODE_FLOW_EXPLANATION_ZH.md` 文档来理解这些操作背后的代码逻辑。
        *   **深入特定模块**: 选择一个您最感兴趣的游戏模块（如幸存者管理、科技研究、探索等），仔细阅读该模块相关的Model, System, UI, Command, Event脚本，尝试理解其内部工作原理。

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
    *   **配置**: 战斗参数（如基础攻击力、防御力、伤害公式、暴击率等）可能在 `CombatSystem` 内部定义，或者与幸存者/敌人的属性关联。武器或技能的特定效果也在这里处理。某些全局战斗参数（如之前提到的 `SurvivorAttackPowerMultiplier`）可能由 `CombatSystem` 的 `BindableProperty` 或可配置字段提供。`CombatSystem` 的 `BasePosition` 字段现在可以通过 `GameInitializer` 的 `baseMarker` Transform 在Inspector中配置。僵尸的预制件路径 (`zombiePrefabName`) 在 `CombatSystem.SpawnZombieWaveForDay` 方法中硬编码，需要确保预制件位于 `Resources` 文件夹下对应的路径。

*   **随机事件系统 (GEventSystem, EventModel)**:
    *   **功能**: `GEventSystem` (通常指导演系统或全局事件系统) 负责在游戏过程中按一定规则（如满足特定条件、按概率、按固定时间间隔或冷却时间）触发各种随机事件。`EventModel` 可能用来存储当前正在处理的随机事件或事件历史。各种具体的随机事件脚本（如 `FoodSpoilageEvent`, `SurvivorSicknessEvent` 等，它们派生自一个共同的 `RandomEvent` 基类）包含各自事件被触发时的具体执行逻辑（`Execute` 方法）。
    *   **配置**: 随机事件的列表、触发条件、概率、冷却时间等通常在 `GEventSystem` 的初始化逻辑中配置（例如，通过一个工厂列表 `mEventFactories`，每个工厂负责创建一种特定事件的实例并定义其触发参数）。修改或添加新事件需要修改 `GEventSystem` 和创建新的事件脚本。

*   **任务系统 (QuestModel, QuestSystem)**:
    *   **功能**: `QuestModel` 存储所有任务（主线、支线、教程等）的数据，包括任务ID、标题、描述、当前状态（未激活、进行中、已完成、失败）、目标列表（每个目标有其类型、所需数量、当前进度）、以及完成任务后的奖励。`QuestSystem` 负责根据游戏进展或玩家行为来激活新任务、监听相关的游戏事件或检查游戏状态以更新任务目标的进度、在所有目标都达成后将任务标记为完成、并发放任务奖励。
    *   **配置**: 所有任务的详细定义（包括其目标和奖励）都在 `QuestModel` 的 `PopulateInitialQuests()` (或类似名称的方法) 中通过代码硬编码。添加新任务或修改现有任务都需要编辑这个方法。任务目标类型 (`QuestObjectiveType` 枚举) 和奖励类型 (`QuestRewardType` 枚举) 的处理逻辑在 `QuestSystem` 中实现。

**如何找到并修改系统配置**:
1.  **确定哪个系统**: 根据您想调整的功能，确定它属于哪个系统。
2.  **检查System脚本**: 打开对应的System脚本 (例如 `Assets/_Project/Scripts/GameCore/Time/DayNightSystem.cs`)。查看其顶部是否有 `public` 或 `[SerializeField]` 标记的字段，这些是可能在Inspector中配置的。
3.  **找到GameObject**: 如果该System是在场景中运行的（**本项目中，大部分System是通过 `GameInitializer` 注册为非MonoBehaviour的类，直接在代码中配置。少数如 `DayNightSystem` 如果被设计为MonoBehaviour，则会挂载在场景中的某个GameObject上，通常是 `_GameManager_` 或一个专门的 `TimeManager` 对象**）。`GameInitializer` 自身也挂载在 `_GameManager_` 上，并且现在有一个 `Base Marker` 字段可配置。
4.  **修改Inspector**: 如果System是MonoBehaviour并挂载在GameObject上，或者如 `GameInitializer` 一样有可配置字段，选中该GameObject，在 `Inspector` 窗口找到对应的脚本组件，修改其暴露出的字段值。
5.  **代码配置**: 对于绝大多数系统逻辑和核心数据（如科技树、任务链、工作站参数、建造成本、事件触发条件等），它们更可能是在对应Model或System的初始化方法中通过代码定义的（例如在 `OnInit()` 或 `PopulateInitial...()` 方法中，或者直接在类的构造函数或字段初始化器中）。这种情况下，您需要直接修改C#代码来调整这些配置。

## 第五部分：如何扩展或修改 (非常初步的指引)

当您熟悉了项目的基础结构后，可能会希望添加新内容或修改现有功能。以下是一些初步的指引方向：

*   **添加新的随机事件**:
    1.  在 `Assets/_Project/Scripts/GameCore/Events/` (或一个专门的 `RandomEvents/` 子文件夹，如果项目这样组织) 下创建一个新的C#脚本，例如 `MyNewRandomEvent.cs`。
    2.  让这个类继承自 `RandomEvent` 基类。
    3.  实现构造函数（在其中设置事件的 `Title` 属性，也可设置基础概率 `BaseChance`、冷却时间 `Cooldown` 等，如果基类支持的话）。
    4.  重写 (override) `Execute(IArchitecture architecture)` 方法。在这个方法中，使用 `architecture.GetModel<T>()` 和 `architecture.GetSystem<T>()` 来获取需要交互的数据模型和系统，然后编写事件的具体逻辑（例如，改变某个资源数量、修改一个幸存者的状态、触发一个新的UI提示等）。最后，务必设置事件的 `Description` 属性，用以向玩家解释发生了什么。
    5.  找到 `GEventSystem.cs` (即 `EventSystem.cs`)。在其 `OnInit()` 方法中，找到 `mEventFactories` 列表。
    6.  将您的新事件的创建委托（例如 `() => new MyNewRandomEvent()`）添加到 `mEventFactories.Add(...)`。

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
    4.  **创建新的工作站UI预制件**: 参考 **第二部分 2.5 (B) 工作站列表项预制件 (`WorkstationItem_PF`)** 的步骤，创建一个新的预制件或复制并修改现有预制件，以反映新工作站的视觉特征和所需信息。确保其上的 `WorkstationListItemUI.cs` 脚本能正确处理新类型（如果需要特殊显示的话）。
    5.  **(重要)** 如果您希望新的工作站类型能被建造，您还需要更新允许玩家建造工作站的UI（例如，一个建筑菜单）。这可能涉及到修改该UI的控制脚本，以将新的 `WorkstationType` 添加到可建造列表中，并确保它能正确触发 `BuildWorkstationCommand`。

*   **修改敌人 (Zombie)**:
    *   **属性与行为**: 敌人的基础属性 (生命、攻击、速度) 在 `CombatSystem.cs` 的 `SpawnZombieWaveForDay` 方法中根据天数动态生成。僵尸的移动逻辑在 `Zombie.cs` (`Move` 方法) 中，攻击逻辑在 `CombatSystem.cs` 中处理。
    *   **视觉表现**: 僵尸的视觉由 `Zombie_PF` 预制件 (`Assets/_Project/Prefabs/Enemies/Zombie_PF.prefab`) 决定。您可以修改此预制件来改变僵尸的外观。其行为由挂载的 `ZombieView.cs` 脚本驱动，该脚本将其逻辑数据 (`Zombie.cs`) 的状态（如位置）同步到场景中的Transform。
    *   **生成路径**: `CombatSystem.cs` 中的 `zombiePrefabName` 变量硬编码为 `"Prefabs/Enemies/Zombie_PF"`。这意味着Unity会从项目中的任何 `Resources` 文件夹下查找路径为 `Prefabs/Enemies/Zombie_PF.prefab` 的预制件。**因此，您必须确保将 `Zombie_PF.prefab` 放置在例如 `Assets/Resources/Prefabs/Enemies/` 目录下。** 如果您想更改预制件的名称或其在 `Resources` 下的路径，必须同步修改 `CombatSystem.cs` 中的这个字符串。

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
    1.  **Console错误**: 始终第一步检查Unity的 `Console` (控制台) 窗口是否有任何红色错误或相关黄色警告。这些信息往往直接指向问题根源。
    2.  **脚本挂载**: 确认相关的UI Controller脚本 (例如 `DayDisplay.cs`, `SurvivorDisplay.cs`) 是否已经正确地附加到了场景中对应的GameObject上 (例如 `DayDisplay_Panel`, `SurvivorDisplay_Panel`)。
    3.  **Inspector字段链接**:
        *   **UI元素**: 选中挂载了UI Controller脚本的GameObject，检查其在 `Inspector` 窗口中暴露出的公共字段 (如 `dayText`, `survivorItemPrefab`, `survivorListContainer` 等) 是否都已正确链接了场景中的UI元素或Project中的预制件。如果某个字段显示为 "None (Type Mismatch)" 或就是空的 "None"，那么脚本将无法操作该UI元素。**这是导致UI不更新的最常见原因之一。**
        *   **列表项预制件内部**: 如果是列表项UI (如 `SurvivorListItemUI.cs`, `TechDisplayItem.cs`) 的问题，需要双击进入该预制件的编辑模式，选中挂载了列表项脚本的根GameObject，检查其内部的文本、滑动条等UI元素是否也正确链接到了脚本的字段上。
    4.  **GameInitializer与数据初始化**:
        *   确认场景中存在一个激活的GameObject（例如 `_GameManager_`）挂载了 `GameInitializer.cs` 脚本，并且这个脚本成功执行了（其 `Awake` 或 `Start` 方法中的日志应出现在Console中）。`GameInitializer` 负责注册所有的Models和Systems。
        *   检查对应的数据模型 (Model) 的 `OnInit()` 方法（或 `PopulateInitialData()` 等类似方法）是否被调用，以及是否正确地填充了初始数据。如果Model中就没有数据（例如，没有初始幸存者、科技等），UI自然无法显示这些数据。
    5.  **事件与绑定**:
        *   **事件驱动**: 如果UI更新依赖于QFramework事件，请确认事件是否在正确时机被发送 (`this.SendEvent()`)，以及UI脚本是否正确注册监听了该事件 (`this.RegisterEvent<T>()`)，并且事件类型完全匹配。
        *   **BindableProperty**: 如果UI更新依赖于 `BindableProperty<T>`，请确认UI脚本是否正确地调用了 `.RegisterWithInit(callback)` 或 `.Register(callback)` 来订阅其变化，并且回调方法 (`callback`) 中的UI更新逻辑是否正确。同时，确保在 `OnDestroy()` (对于MonoBehaviour脚本如UI Controllers) 或使用 `.UnRegisterWhenGameObjectDestroyed(this.gameObject)` 来自动解注册这些绑定，以防止内存泄漏或空引用。
    6.  **数据源确认**: 使用 `Debug.Log()` 在UI脚本的 `Start()` 或数据更新回调中打印从Model获取到的数据，或者直接打印 `BindableProperty.Value`，确认数据本身是否符合预期。

*   **点击UI按钮没有反应**:
    1.  **Console错误**: 再次强调，首先检查Console是否有错误。
    2.  **按钮OnClick事件链接**: 选中场景中的按钮GameObject，在 `Inspector` 窗口找到 `Button` 组件。展开其 `OnClick()` 事件列表。
        *   确保列表中至少有一个条目。
        *   该条目的第一个字段（通常显示为 "RuntimeOnly"）应该指向挂载了处理该按钮点击的脚本的GameObject (例如，某个UI Panel，或者列表项预制件的根对象)。
        *   第二个下拉菜单应该选择了正确的脚本组件，然后是该脚本中期望被调用的 `public` 方法 (例如 `MyDisplayScript.OnMyButtonClick` 或 `TechDisplayItem.HandleResearchButtonClick`)。
        *   如果方法需要参数，确保参数已正确配置（通常对于UI按钮回调，方法设计为无参或接收简单类型参数）。
    3.  **EventSystem是否存在**: 确保场景中有一个激活的 `EventSystem` GameObject。没有它，所有UI事件（包括按钮点击）都不会被处理。
    4.  **UI Controller方法逻辑**: 检查被按钮调用的那个UI Controller方法内部的逻辑。它是否正确地构建并发送了Command (`this.SendCommand(new MyCommand())`)？是否有条件判断阻止了Command的发送？
    5.  **Command执行逻辑**: 打开对应的Command脚本，检查其 `OnExecute()` 方法。它是否能正确获取到所需的System (`this.GetSystem<MySystem>()`)？System的引用是否为空？
    6.  **System方法逻辑**: Command调用的System方法内部是否有条件判断导致逻辑没有按预期执行？System方法内部是否有 `Debug.Log` 可以帮助追踪其执行路径和内部变量状态？

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
        *   **Inspector字段未链接**: 如果出错的变量是一个 `public` 或 `[SerializeField]` 的字段，并且您期望它在Unity编辑器中被赋值（例如，拖拽一个GameObject或Prefab上去），请检查该字段在Inspector中是否仍然是 "None"。**这在UI设置中非常常见！** 仔细检查所有 `XXXDisplay` 脚本和 `XXXListItemUI` 脚本上链接的UI元素是否都已正确赋值。
        *   **`transform.Find()`失败**: 如果您在 `Awake()` 或 `Start()` 中使用 `transform.Find("ExpectedChildName")` 来获取子对象引用，请确保子对象的名称与代码中的字符串完全匹配，并且层级关系正确。如果找不到，`GetComponent<T>()` 会在 `null` 对象上调用，导致错误。
        *   **GetComponent失败**: 如果您使用 `GetComponent<T>()` 来获取一个组件的引用，但当前GameObject上并没有挂载该类型的组件，那么 `GetComponent<T>()` 会返回 `null`。后续使用这个 `null` 引用就会导致空引用错误。在使用 `GetComponent` 的结果前，最好进行空检查。
        *   **对象已被销毁**: 如果一个GameObject已经被 `Destroy()` 了，那么之前获取到的对它或其组件的引用可能会变成 `null` (或者Unity会将其伪装成 `null` 并给出特定提示)。
        *   **方法返回值为空**: 某个方法可能在特定条件下返回 `null`，而调用方没有检查这个返回值就直接使用了。例如，`GetModel<T>()` 或 `GetSystem<T>()` 如果在 `GameInitializer` 完成注册前调用，或者类型名写错，都可能返回 `null`。
        *   **Model/System未正确获取或初始化**: 在QFramework的 `IController` 或 `AbstractCommand` 中使用 `this.GetModel<T>()` 或 `this.GetSystem<T>()` 时，如果对应的Model/System没有在 `GameInitializer` 中被正确注册，或者获取时机过早（例如在 `Awake` 中，而Model/System在 `OnInit` 中注册），都可能导致获取到 `null`。确保在 `Start()` 或之后获取，或者在 `OnInit` 中获取（对于System和Model自身）。

*   **游戏中看不到僵尸出现，或者僵尸出现位置/样子不对**:
    1.  **Console错误**: 检查是否有与 `CombatSystem`, `EnemyModel`, `ZombieView`, `ObjectPoolSystem` 或 `Resources.Load` 相关的错误。
    2.  **预制件路径与名称**: 打开 `CombatSystem.cs`，找到 `SpawnZombieWaveForDay` 方法。确认其中 `zombiePrefabName` 变量的值 (例如 `"Prefabs/Enemies/Zombie_PF"`) 与您实际的僵尸预制件在 `Assets/Resources/` 文件夹下的路径和名称**完全一致** (包括大小写，不含 `.prefab` 后缀)。
        *   例如，如果 `zombiePrefabName` 是 `"Prefabs/Enemies/Zombie_PF"`，那么您的预制件必须存放在 `Assets/Resources/Prefabs/Enemies/Zombie_PF.prefab`。
    3.  **`Zombie_PF` 预制件配置**:
        *   在 `Project` 窗口找到 `Zombie_PF.prefab` 并双击进入编辑模式。
        *   确认其根GameObject上已正确挂载了 `ZombieView.cs` 脚本。
        *   确认预制件中负责显示的子对象（例如，您可能命名为 `VisualSprite`）上的 `SpriteRenderer` 组件已启用，并且其 `Sprite` 字段已赋值为一个可见的僵尸图片。或者，如果 `ZombieView.cs` 的 `Awake()` 或 `Setup()` 负责动态加载或设置Sprite，请确保该逻辑无误。
        *   检查 `ZombieView` 脚本在Inspector中暴露的 `Sprite Renderer` 字段是否已正确链接到 `VisualSprite` GameObject上的 `SpriteRenderer` 组件（如果脚本中不是通过 `GetComponentInChildren` 或 `GetComponent` 自动获取的话，但我们生成的 `ZombieView.cs` 在 `Awake` 中尝试了 `GetComponent<SpriteRenderer>()`，所以如果 `SpriteRenderer` 在根对象上，应能自动获取）。
    4.  **`_BaseLocationMarker_` 配置**:
        *   在 `Hierarchy` 窗口中找到 `_GameManager_` GameObject。
        *   检查其 `GameInitializer (Script)` 组件的 `Base Marker` 字段是否已链接到场景中的 `_BaseLocationMarker_` GameObject。
        *   选中 `_BaseLocationMarker_`，检查其 `Transform` 组件的 `Position` 是否是您期望的基地中心位置。僵尸的移动目标 (`BasePosition`) 是基于此设置的。
    5.  **对象池系统 (`ObjectPoolSystem.cs`)**:
        *   检查Console中是否有来自对象池系统的错误，例如 "未能加载名为 'Prefabs/Enemies/Zombie_PF' 的预制件"。这通常意味着路径错误或预制件不在 `Resources` 文件夹下。
        *   对象池系统在 `Spawn` 时如果无法生成对象，现在应该会返回 `null` 并在Console中报错，`CombatSystem` 中也有相应的错误日志。
    6.  **`ZombieView.cs` 脚本逻辑**: 检查 `ZombieView` 的 `Awake()`, `Setup()`, `UpdatePosition()` 方法中是否有逻辑错误或空引用，特别是与 `mZombieData` 或 `spriteRenderer` 相关的部分。

## 第七部分：结语

恭喜您完成了本用户指南的初步阅读！希望这份文档为您提供了一个清晰的起点，帮助您理解和上手这个生存管理游戏项目，并为您后续的开发、修改或内容添加工作打下坚实的基础。

游戏开发是一个充满创造力、挑战和乐趣的旅程。我们鼓励您：

*   **动手实践**: 不要害怕尝试！按照指南中的步骤操作，试着修改一些参数，或者按照“扩展指引”部分添加一些简单的自定义内容。实际操作是学习和理解的最佳途径。
*   **深入代码**: 当您对项目结构和基本工作流程有了整体认识后，不妨选择一两个您最感兴趣的功能模块，深入阅读其相关的Model, System, UI, Command和Event脚本，理解它们是如何协同工作的。
*   **查阅QFramework文档**: QFramework本身是一个功能丰富的框架。如果您想更深入地了解其设计理念、高级用法（如对象池、事件的更高级模式、UI Kit、IOC容器等），或者遇到与框架本身相关的问题，查阅QFramework的官方文档、教程或社区（如GitHub、QQ群、论坛等）会非常有帮助。
*   **学习Unity官方文档**: 对于Unity引擎自身的功能（如物理、动画、渲染管线、编辑器操作等），Unity的官方文档和教程是权威且全面的学习资源。

请记住，遇到问题是正常的，解决问题的过程也是学习和成长的过程。善用Console的错误信息，学会调试代码，并积极寻求信息和帮助。

祝您在本项目中的探索旅程愉快且富有成效！如果您有任何建议或发现文档中的不足之处，也欢迎反馈。
