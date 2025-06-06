# Unity项目用户指南 - 生存管理游戏

## 第一部分：项目入门

### 1. 欢迎语

您好！欢迎来到我们的生存管理游戏项目！本指南旨在帮助您了解项目的基础结构、核心玩法以及如何进行后续的探索和开发。无论您是刚接触Unity和游戏开发，还是有一定经验的开发者，我们都希望这份文档能为您提供清晰的指引。

### 2. 项目概述

这是一款基于Unity引擎开发的生存管理类游戏。在这个游戏中，玩家需要在一个充满挑战的环境中努力生存下去。核心玩法围绕以下几个方面展开：

*   **资源管理**：收集和管理各种生存必需的资源，如食物、电力、建材等。合理的资源分配是生存的关键。
*   **人员分配**：招募或管理幸存者，并将他们分配到不同的岗位上，例如生产、研究、探索或防御。每个幸存者都可能有其独特的技能和需求。
*   **科技研究**：通过研究新的科技来解锁更高级的建筑、工具或能力，从而提升生存基地的效率和幸存者的能力。
*   **地图探索**：派出幸存者队伍探索未知的区域，寻找稀有资源、发现新的地点或触发特殊事件。
*   **基地防御**：应对可能出现的各种威胁（如敌人入侵），需要合理布置防御设施和人员，保护基地和幸存者的安全。

### 3. QFramework简介

本项目采用了一款名为 **QFramework** 的国产优秀Unity开发框架。简单来说，QFramework帮助我们更好地组织游戏代码，实现各个功能模块之间的“解耦合”，使得项目结构更清晰，更易于维护和扩展。

**它的主要作用包括：**

*   **分层架构**：鼓励将游戏逻辑划分为不同的层次（如表现层、逻辑层、数据层），让每个模块职责分明。
*   **消息传递**：提供便捷的事件系统，让不同模块之间可以方便地进行通信，而不需要直接相互引用，从而降低了模块间的依赖性。
*   **工具集**：内置了许多实用的工具和管理器，如UI管理、对象池、资源加载等，可以提高开发效率。

您不需要立即深入了解QFramework的所有细节，但在后续接触代码时，会逐渐体会到它带来的便利。

### 4. 项目结构概览

当您在Unity编辑器中打开本项目时，`Assets` 文件夹是您最主要的关注点。以下是一些关键的子文件夹及其用途：

*   **`Assets/Scripts/GameScritps/`**: 这是存放游戏核心C#脚本的地方。
    *   **`Models/`**: 存放数据模型脚本。数据模型负责存储游戏的核心数据状态，例如当前有哪些资源、所有幸存者的信息、科技的研发状态等。它们通常不包含复杂的游戏逻辑，主要是数据的载体。
    *   **`Systems/`**: 存放系统逻辑脚本。系统负责处理具体的功能逻辑，例如战斗系统如何计算伤害，探索系统如何处理远征结果，工作站系统如何进行生产等。它们会读取和修改`Models`中的数据。
    *   **`UI/`**: 存放与用户界面（UI）相关的脚本。这些脚本负责控制UI元素的显示和交互，例如更新资源文本、显示科技树、处理按钮点击等。
    *   **`Commands/`**: 存放命令脚本。在QFramework中，命令是一种封装单次操作的方式，例如“开始一项研究”、“建造一个工作站”。UI或其他系统可以通过发送命令来执行这些操作。
    *   **`Events/`**: 存放事件定义脚本。事件用于模块间的通信。例如，当一项研究完成时，`ResearchSystem`可能会发送一个“研究完成”的事件，UI或其他系统可以监听这个事件并做出相应的响应。
    *   **功能模块文件夹 (例如 `Survivors/`, `Workstations/`, `Research/`, `Exploration/`, `Enemies/`, `Time/`, `Combat/`, `Quests/`)**: 这些文件夹通常以游戏的核心功能模块命名，内部可能包含与该模块相关的Models, Systems, Commands, Events以及其他辅助脚本。例如，`Survivors/` 文件夹下会有`Survivor.cs` (定义幸存者数据结构)、`SurvivorModel.cs`、`SurvivorManagerSystem.cs`等。

*   **`Assets/Scenes/`**: 存放游戏的场景文件。例如，主菜单场景、游戏主场景等。

*   **`Assets/Prefabs/UI/`**: （如果存在，或建议创建）通常用于存放UI元素的预制件（Prefabs）。预制件是一种可复用的游戏对象模板，例如一个按钮、一个信息面板、或者一个列表项的UI。

通过了解这些主要文件夹的用途，您可以更快地定位到您感兴趣的代码或资源。在后续的文档中，我们将更详细地介绍各个核心系统和它们的交互方式。

## 第二部分：核心概念与工作流程 (面向零基础)

为了更好地理解和使用本项目，我们先来熟悉几个Unity和游戏开发中的核心概念。

*   **场景 (Scenes)**
    *   场景是构建游戏世界的舞台。你可以把它想象成电影中的一个“场面”或者戏剧中的一“幕”。我们的游戏可能包含多个场景，例如主菜单场景、游戏关卡场景等。
    *   在Unity编辑器的`Project`（项目）窗口中，通常可以在 `Assets/Scenes/` 文件夹下找到所有的场景文件。它们以 `.unity` 作为后缀名。
    *   要打开一个场景进行编辑或查看，只需在`Project`窗口中双击场景文件即可。例如，双击 `MainScene.unity` 就可以打开主游戏场景。
    *   当您点击Unity编辑器顶部的“播放”按钮时，当前在编辑器中激活并打开的场景就是游戏开始运行的场景。

*   **游戏对象 (GameObjects) 与组件 (Components)**
    *   **游戏对象 (GameObject)** 是构成场景的基本“物件”或“东西”。场景中的每一个可见或不可见元素，比如角色、灯光、摄像机、UI按钮，甚至是一些不直接显示但负责管理游戏逻辑的空对象，都是一个游戏对象。你可以把它们看作是舞台上的演员或道具。
    *   **组件 (Component)** 则是附加到游戏对象上的功能模块或“能力脚本”。一个游戏对象本身只是一个容器，是组件赋予了它特定的属性和行为。例如：
        *   一个“玩家”游戏对象可能附加了：
            *   `Transform` 组件（所有GameObject都有）：决定其在场景中的位置、旋转和缩放。
            *   `Sprite Renderer` 或 `Mesh Renderer` 组件：使其可见，显示图片或3D模型。
            *   一个自定义的 `PlayerMovement.cs` 脚本组件：使其能够响应玩家输入并移动。
            *   一个 `Rigidbody` 组件：使其受物理引擎控制。
        *   将脚本（如我们编写的C#脚本）附加到游戏对象上，就是给这个游戏对象添加了一个自定义组件。

*   **预制件 (Prefabs)**
    *   预制件（Prefab）是一种可复用的游戏对象模板。想象一下，如果您需要在游戏中创建很多个外观和行为都一样的敌人，或者列表中的多个结构相同的项目，为每一个都手动重新创建和配置会非常繁琐。
    *   通过预制件，您可以先创建一个“原型”游戏对象，配置好它所有的组件和属性，然后将其保存为一个预制件。之后，您就可以根据这个预制件模板在游戏中动态地创建任意多个相同的实例。
    *   在本项目中，我们大量使用预制件来动态生成UI列表项。例如：
        *   `SurvivorDisplay.cs` 脚本会使用一个 `survivorItemPrefab`（幸存者条目预制件）来为每一个幸存者动态创建一个UI显示项。
        *   `WorkstationDisplay.cs` 脚本会使用 `workstationItemPrefab` 来显示每个工作站。
        *   `ResearchDisplay.cs` 脚本会使用 `techUIPrefab` 来显示每个科技项目。
    *   这些预制件通常存放在 `Assets/Prefabs/` 或更具体的 `Assets/Prefabs/UI/` 目录下。对预制件的修改会自动应用到所有基于它创建的实例上（除非实例的属性被单独覆盖了）。

*   **脚本 (Scripts)**
    *   脚本是包含程序代码的文件，用于定义游戏对象的行为、游戏逻辑、数据处理等。在Unity中，最常用的脚本语言是C#。
    *   我们编写的C#脚本（如 `PlayerController.cs`, `GameManager.cs`, `SurvivorDisplay.cs` 等）需要被附加到场景中的某个游戏对象上才能执行。一个脚本就是一个自定义的组件。
    *   脚本中通常包含：
        *   **变量 (Variables)**：用于存储数据。在脚本中声明为 `public` 的变量（或者用 `[SerializeField]` 特性标记的 `private` 变量）通常会显示在Unity编辑器的Inspector（检查器）窗口中，方便我们直接在编辑器里调整数值而无需修改代码。
        *   **方法 (Methods)**：也称为函数，包含一系列指令，用于执行特定的操作或行为。例如，`Start()` 和 `Update()` 是Unity脚本中特殊的生命周期方法，分别在对象启用时和每一帧被自动调用。我们自定义的方法则可以根据游戏逻辑被其他脚本调用或响应事件。

## 第三部分：如何在Unity编辑器中使用和查看项目

本部分将指导您如何在Unity编辑器中运行项目、查看关键的游戏组件和配置。

*   **启动游戏**
    *   确保您已在 `Project` 窗口中双击打开了主游戏场景（例如 `Assets/Scenes/MainScene.unity`）。
    *   点击Unity编辑器顶部中央的 **播放按钮** (一个三角形图标)。游戏将在Game（游戏）视图中运行。
    *   再次点击播放按钮可以停止游戏运行。

*   **核心游戏管理对象**
    *   在复杂的游戏项目中，通常会有一个或几个“全局管理器”游戏对象。这些对象在场景加载时创建，并负责初始化和管理整个游戏的运行。
    *   在 `MainScene` 打开的状态下，查看 `Hierarchy`（层级）窗口。您可能会找到一个命名类似 `_GameManager_`、`_App_`、`Services` 或者 `GameInitializer` 的空游戏对象。有时，这些核心脚本也可能挂载在场景中始终存在的对象上，比如 `Main Camera`。
    *   选中这个对象，在 `Inspector`（检查器）窗口中，您通常能看到挂载了以下关键脚本：
        *   `GameInitializer.cs` (或类似名称): 这个脚本负责在游戏启动时进行各种初始化工作，例如创建和注册所有的数据模型（Models）和系统（Systems）到QFramework的架构中。这是游戏能正确运行起来的第一步。
        *   `GameLoop.cs` (或类似名称): 这个脚本通常包含一个 `Update()` 方法，它会在每一帧被Unity调用。在 `GameLoop` 的 `Update()` 方法中，会依次调用各个游戏系统（如 `DayNightSystem`, `ResearchSystem`, `WorkstationSystem` 等）的 `Update` 或 `Tick` 方法，从而驱动整个游戏的逻辑进展。

*   **UI系统查看与交互**
    *   **找到UI根Canvas**: 在 `Hierarchy` 窗口中，寻找一个名为 `Canvas` (或者包含 "Canvas" 字样) 的游戏对象。这是Unity UI系统的根，所有的UI元素都作为其子对象存在。场景中可能会有多个Canvas，但通常会有一个主Canvas用于大部分游戏内UI。
    *   **定位UI面板**: 展开 `Canvas` 对象，您可以看到构成UI的各个面板或元素。根据项目约定，这些面板GameObject的命名可能与其对应的控制脚本相似。例如：
        *   `DayDisplay_Panel` (或类似名称): 可能挂载了 `DayDisplay.cs` 脚本，负责显示天数、时间、基地生命等。
        *   `ResourceDisplay_Panel`: 可能挂载了 `ResourceDisplay.cs`，负责显示各类资源的数量。
        *   `SurvivorDisplay_Panel`: 可能挂载了 `SurvivorDisplay.cs`，负责显示幸存者列表。
        *   `WorkstationDisplay_Panel`: 可能挂载了 `WorkstationDisplay.cs`，负责显示工作站列表。
        *   `ResearchDisplay_Panel`: 可能挂载了 `ResearchDisplay.cs`，负责显示科技树和研究进度。
        *   `ExplorationDisplay_Panel`: 可能挂载了 `ExplorationDisplay.cs`，负责探索界面。
        *   `QuestLogDisplay_Panel`: 可能挂载了 `QuestLogDisplay.cs`，负责任务日志。
        *   `CombatLogDisplay_Panel`: 可能挂载了 `CombatLogDisplay.cs`，负责战斗日志。
        *   `EventDisplay_Panel`: 可能挂载了 `EventDisplay.cs`，负责随机事件的显示。
    *   选中这些面板对象，可以在 `Inspector` 窗口中看到它们挂载的脚本以及脚本暴露出的公共字段（例如用于链接Text组件的字段）。

*   **关键：如何配置动态列表的预制件 (详细步骤)**
    *   **通用说明**: 游戏中很多列表（如幸存者列表、工作站列表、科技列表）都是动态生成的。这意味着列表中的每一项都是基于一个“模板”（即预制件）创建的。控制这些列表显示的脚本（我们称之为 `XXXDisplay.cs`，例如 `SurvivorDisplay.cs`）通常会在Inspector窗口中暴露一个公共字段（例如 `survivorItemPrefab`），需要您将对应的预制件拖拽到这个字段上。
    *   **步骤 (以`SurvivorDisplay` 和 `survivorItemPrefab` 为例，其他类似)**:

        1.  **创建/找到预制件目录**: 在 `Project` 窗口中，建议在 `Assets/Prefabs/` 文件夹下创建一个名为 `UI` 的子文件夹，专门存放UI相关的预制件。如果这些目录已存在，请直接使用。

        2.  **创建预制件本身**:
            *   在 `Assets/Prefabs/UI/` 目录下，右键 -> Create -> Prefab。
            *   将新创建的预制件命名为一个清晰的名称，例如 `SurvivorItem_PF`。

        3.  **编辑预制件UI结构**:
            *   双击刚刚创建的 `SurvivorItem_PF` 预制件，Unity会进入预制件编辑模式（场景视图会显示预制件的内容，Hierarchy窗口也会只显示预制件的层级）。
            *   **重要**: 预制件的根GameObject本身通常需要是一个UI元素（例如 `Image` 或一个空RectTransform作为容器）。如果不是，先右键根GameObject -> UI -> Image，然后将Image的Source Image设为None，Alpha设为0，使其不可见但提供RectTransform。或者，如果根已经是UI元素，则直接在其下添加子元素。
            *   在预制件的根GameObject下，添加所需的UI子元素。例如，对于幸存者条目，您可能需要：
                *   **姓名文本**: 右键根GameObject -> UI -> Text - TextMeshPro (如果项目使用TextMeshPro，这是推荐的；如果使用旧版UI Text，则选择 Text)。将其命名为 `NameText`。调整其在预制件内的位置和大小。
                *   **状态文本**: 类似地，添加一个 `StatusText` (TextMeshPro)。
                *   **食物条**: 右键根GameObject -> UI -> Slider。命名为 `FoodSlider`。调整其外观（例如去掉滑块把手，改变填充颜色）。
                *   **食物值文本**: 添加一个 `FoodValueText` (TextMeshPro) 放在食物条旁边或上面。
                *   **休息条**: 类似地，添加 `RestSlider`。
                *   **休息值文本**: 添加 `RestValueText` (TextMeshPro)。
                *   **职业文本**: 添加 `ProfessionText` (TextMeshPro)。
                *   **工作站文本**: 添加 `WorkstationText` (TextMeshPro)。
            *   使用RectTransform工具仔细排列这些UI元素，确保它们在预制件的边界内，并且看起来美观。

        4.  **挂载子项UI脚本**:
            *   选中 `SurvivorItem_PF` 预制件的根GameObject。
            *   在 `Inspector` 窗口中，点击 "Add Component" 按钮。
            *   搜索并添加对应的UI列表项脚本，例如 `SurvivorListItemUI.cs`。

        5.  **链接脚本字段到UI元素**:
            *   确保 `SurvivorItem_PF` 预制件的根GameObject仍处于选中状态。
            *   在 `Inspector` 窗口中找到刚刚添加的 `SurvivorListItemUI` 脚本组件。
            *   您会看到脚本中定义的所有 `public` UI元素字段（例如 `nameText`, `statusText`, `foodSlider` 等）。
            *   从 `Hierarchy` 窗口（当前应显示预制件的内部层级）中，将步骤3中创建的对应UI元素（如 `NameText` GameObject）分别拖拽到 `SurvivorListItemUI` 脚本的相应字段上。例如，将 `NameText` GameObject 拖到 `Name Text` 字段。

        6.  **保存预制件**:
            *   在预制件编辑模式下，通常对预制件的修改会自动保存。您可以点击编辑器左上角的返回箭头退出预制件编辑模式，或者按 `Ctrl+S` (Windows) / `Cmd+S` (Mac) 确保保存。

        7.  **链接预制件到Display脚本**:
            *   返回到您的主场景 (e.g., `MainScene`)。
            *   在 `Hierarchy` 窗口中，找到挂载了 `SurvivorDisplay.cs` 脚本的那个GameObject (例如 `SurvivorDisplay_Panel`)。
            *   选中它。
            *   在 `Inspector` 窗口中，找到 `SurvivorDisplay` 脚本组件。您应该能看到一个名为 `Survivor Item Prefab` (或类似名称) 的公共字段。
            *   从 `Project` 窗口 (`Assets/Prefabs/UI/`) 中，将您刚刚创建并配置好的 `SurvivorItem_PF` 预制件拖拽到这个 `Survivor Item Prefab` 字段上。

    *   **对其他列表进行类似操作**:
        *   **工作站列表**:
            *   预制件: `WorkstationItem_PF` (或类似名称)。
            *   脚本: `WorkstationListItemUI.cs`。
            *   Display脚本: `WorkstationDisplay.cs` (字段如 `workstationItemPrefab`)。
            *   `WorkstationListItemUI` 脚本需要链接其内部的 `stationTypeText`, `assignedSurvivorsText`, `productionProgressBar`, `productionProgressText`, `manageButton` 等UI元素。
        *   **科技列表**:
            *   预制件: `TechItem_PF` (或类似名称)。
            *   脚本: `TechDisplayItem.cs`。
            *   Display脚本: `ResearchDisplay.cs` (字段如 `techUIPrefab`)。
            *   `TechDisplayItem` 脚本需要链接其内部的 `nameText`, `descriptionText`, `statusText`, `researchButton`, `progressBar`, `feedbackText` 等UI元素。

        **核心要点**：`XXXDisplay.cs` 负责管理整个列表的刷新逻辑（何时刷新，从哪里获取数据列表）。它通过实例化对应的 `XXXItemPrefab` 来创建每一项。而 `XXXListItemUI.cs` (或 `XXXItem.cs`) 脚本则负责管理单个列表项内部的UI元素如何显示该项的数据，以及如何响应针对该特定项的用户交互（例如，通过回调将其传递给 `XXXDisplay` 处理）。

*   **数据查看 (初步)**
    *   在游戏开发过程中，有时您可能想快速查看某些模型中的数据。一个简单的方法是使用 `Debug.Log()`。
    *   例如，如果您想查看当前所有幸存者的数量，可以临时修改某个UI Controller脚本（比如 `SurvivorDisplay.cs`）的 `Start()` 方法，或者在一个测试按钮的点击回调中添加如下代码：
        ```csharp
        // 确保脚本顶部有 using QFramework; 和 using YourGameNamespace.Survivors;
        var survivorModel = this.GetModel<SurvivorModel>(); // 获取数据模型
        if (survivorModel != null)
        {
            UnityEngine.Debug.Log("当前幸存者数量: " + survivorModel.GetAllSurvivors().Count);
            // 你也可以遍历并打印每个幸存者的名字
            // foreach(var survivor in survivorModel.GetAllSurvivors())
            // {
            //     UnityEngine.Debug.Log("幸存者: " + survivor.Name.Value);
            // }
        }
        ```
    *   当这段代码执行后，您可以在Unity编辑器的 `Console`（控制台）窗口查看到打印出来的日志信息。记得在测试完毕后移除或注释掉这些临时的 `Debug.Log` 语句。

*   **游戏核心系统简介及其配置 (已在第三部分提及，此处补充)**
    *   在第三部分的“核心游戏管理对象”和“游戏核心系统简介及其配置”中，我们已经讨论了如何找到 `GameInitializer` 和 `GameLoop`，以及如何通过 `Inspector` 修改某些System的公共参数。这里对各个系统的功能做更集中的简述。

## 第四部分：游戏核心系统简介及其配置 (补充)

以下是本项目中主要核心系统的功能简介。一些系统可能有可以在Unity Inspector中调整的参数，如果该System脚本挂载在场景中的某个GameObject上。

*   **资源系统 (ResourceModel)**:
    *   **功能**: 管理游戏中所有类型的资源（如食物、电力、弹药、研究点等）的当前数量。负责资源的增加和消耗。
    *   **配置**: 通常资源初始值和类型在 `ResourceModel` 的 `OnInit` 或构造函数中定义。大部分操作通过调用其方法进行。

*   **敌人系统 (EnemyModel, EnemySpawningSystem)**:
    *   **功能**: `EnemyModel` 存储当前场景中所有敌人的数据。`EnemySpawningSystem` (如果存在，或逻辑在 `CombatSystem` 或 `GameLoop` 中) 负责根据游戏逻辑（如波数、时间）生成新的敌人。
    *   **配置**: 敌人类型、属性可能在 `Enemy.cs` 或相关预制件中定义。生成规则可能在 `EnemySpawningSystem` 的Inspector字段中（如生成间隔、每波数量等）。

*   **时间系统 (DayNightSystem, GameDataModel)**:
    *   **功能**: `DayNightSystem` 管理游戏内时间的流逝、昼夜循环的逻辑。`GameDataModel` 中的 `CurrentDay` 存储当前天数。
    *   **配置**: `DayNightSystem.cs` 如果挂在GameObject上，可能会有 `SecondsPerDay` (一天持续多少真实秒) 这样的公共字段可在Inspector中调整。

*   **幸存者系统 (SurvivorModel, SurvivorManagerSystem)**:
    *   **功能**: `SurvivorModel` 存储所有幸存者的数据。`SurvivorManagerSystem` 负责幸存者的创建、需求（食物、休息）更新、状态管理（空闲、工作、受伤等）以及行为（如进食、休息）。
    *   **配置**: 幸存者的基础属性、需求消耗速率等可能在 `SurvivorManagerSystem` 或 `Survivor.cs` 的构造函数/默认值中定义。

*   **工作站系统 (WorkstationModel, WorkstationSystem)**:
    *   **功能**: `WorkstationModel` 存储所有已建造工作站的数据。`WorkstationSystem` 负责工作站的建造（包括成本检查与资源消耗）、驱动生产过程、处理幸存者分配到工作站或从工作站解除分配的逻辑。
    *   **配置**: 工作站的类型 (`WorkstationType` 枚举)、生产周期、产出、升级参数等在 `Workstation.cs` 的构造函数或相关配置中定义。建造成本在 `WorkstationSystem` 的 `mWorkstationBuildCosts` 字典中定义。

*   **研究系统 (ResearchModel, ResearchSystem)**:
    *   **功能**: `ResearchModel` 存储所有科技项目的数据及其状态（未解锁、可研究、研究中、已完成）。`ResearchSystem` 管理当前正在研究的科技、处理研究进度、在研究完成后应用科技效果。
    *   **配置**: 科技树、每个科技的成本、前置条件、效果等在 `ResearchModel` 的 `PopulateInitialTechnologies()` 方法中初始化。

*   **探索系统 (ExplorationModel, ExplorationSystem)**:
    *   **功能**: `ExplorationModel` 存储所有兴趣点(POI)和当前活动远征的数据。`ExplorationSystem` 负责开始远征（检查条件、设置幸存者状态）、更新远征进度、处理远征完成后的结果（奖励、事件等）。
    *   **配置**: POI的定义（名称、难度、探索时间、奖励等）在 `ExplorationModel` 的 `PopulateInitialPOIs()` (或类似方法) 中初始化。

*   **战斗系统 (CombatSystem)**:
    *   **功能**: 处理所有与战斗相关的逻辑，例如幸存者攻击敌人、敌人攻击基地或幸存者、伤害计算、武器效果等。
    *   **配置**: 攻击力、防御力、伤害公式等核心战斗参数可能在 `CombatSystem` 内部或相关实体（如幸存者、敌人）的属性中定义。某些战斗调整值（如全局攻击力乘数）可能由 `CombatSystem` 的可配置字段提供。

*   **随机事件系统 (GEventSystem, EventModel)**:
    *   **功能**: `GEventSystem` (通常指导演系统) 负责按一定规则（如概率、冷却时间）触发随机事件。`EventModel` 可能存储当前活动事件或事件历史。各种具体的随机事件（如 `FoodSpoilageEvent`, `SurvivorSicknessEvent`）派生自 `RandomEvent` 基类，并包含其具体执行逻辑。
    *   **配置**: 随机事件的触发概率、冷却时间、具体参数等可能在 `GEventSystem` 的 `mEventFactories` (或类似列表) 初始化时配置，或者在每个具体事件类的构造函数中定义。

*   **任务系统 (QuestModel, QuestSystem)**:
    *   **功能**: `QuestModel` 存储所有任务的数据及其状态（未激活、进行中、完成、失败）。`QuestSystem` 负责激活任务、监听游戏事件或条件以更新任务目标进度、在所有目标完成后更新任务状态、并触发任务奖励。
    *   **配置**: 任务的定义（标题、描述、目标、奖励、前置条件等）在 `QuestModel` 的 `PopulateInitialQuests()` 方法中初始化。

**如何找到并修改系统配置**:
1.  **确定哪个系统**: 根据您想调整的功能，确定它属于哪个系统。
2.  **检查System脚本**: 打开对应的System脚本 (例如 `Assets/Scripts/GameScritps/Time/DayNightSystem.cs`)。查看其顶部是否有 `public` 或 `[SerializeField]` 标记的字段，这些是可能在Inspector中配置的。
3.  **找到GameObject**: 如果该System是在场景中运行的（大部分System都是通过 `GameInitializer` 注册为非MonoBehaviour的类，但少数如 `DayNightSystem` 如果设计为MonoBehaviour则会挂载在对象上），您需要在 `Hierarchy` 窗口找到挂载了这个System脚本的GameObject (通常是 `_GameManager_` 或类似的核心对象)。
4.  **修改Inspector**: 选中该GameObject，在 `Inspector` 窗口找到对应的脚本组件，修改其暴露出的字段值。
5.  **代码配置**: 对于大多数系统逻辑和数据（如科技树、任务链、工作站参数、建造成本），它们更可能是在对应Model或System的初始化方法中通过代码定义的（例如在 `OnInit()` 或 `PopulateInitial...()` 方法中）。这种情况下，您需要直接修改C#代码来调整这些配置。

## 第五部分：如何扩展或修改 (非常初步的指引)

当您熟悉了项目的基础结构后，可能会希望添加新内容或修改现有功能。以下是一些初步的指引方向：

*   **添加新的随机事件**:
    1.  在 `Scripts/GameScritps/Events/` (或一个专门的 `RandomEvents/` 子文件夹) 下创建一个新的C#脚本，例如 `MyNewRandomEvent.cs`。
    2.  让这个类继承自 `RandomEvent` 基类。
    3.  实现构造函数（设置标题、描述、基础概率等）和 `Execute(IArchitecture architecture)` 方法（定义事件的具体效果）。
    4.  找到 `GEventSystem.cs` (或类似名称的随机事件管理系统)。在其初始化方法（可能是 `OnInit` 或构造函数）中，找到一个名为 `mEventFactories` 或类似的列表/字典。
    5.  将您的新事件工厂（通常是 `() => new MyNewRandomEvent()`）添加到这个集合中，这样系统才能触发它。

*   **添加新的科技**:
    1.  打开 `ResearchModel.cs` 文件。
    2.  找到 `PopulateInitialTechnologies()` (或类似名称) 的方法。
    3.  按照现有科技的格式，添加一个新的 `Technology` 对象到列表中，定义其ID、名称、描述、研究成本、前置科技ID、以及效果 (`TechnologyEffect`)。
    4.  如果您的新科技需要一种全新的效果类型：
        *   修改 `TechnologyEffectType` 枚举（在 `Technology.cs` 或 `GameEvents.cs` 或专门的枚举文件中）以添加新的效果类型。
        *   修改 `ResearchSystem.cs` 中的 `CompleteResearch()` 方法，在 `switch (effect.EffectType)`语句中添加对新效果类型的处理逻辑（例如，调用某个目标System的新方法）。

*   **添加新的任务**:
    1.  打开 `QuestModel.cs` 文件。
    2.  找到 `PopulateInitialQuests()` (或类似名称) 的方法。
    3.  按照现有任务的格式，添加一个新的 `Quest` 对象，定义其ID、标题、描述、目标 (`QuestObjective`列表)、奖励 (`QuestReward`列表)、以及可能的激活条件或事件。
    4.  如果任务需要新的目标类型或奖励类型：
        *   可能需要修改相关的枚举（如 `QuestObjectiveType`, `QuestRewardType`）。
        *   修改 `QuestSystem.cs` 中处理任务进度更新和奖励发放的逻辑，以支持新的类型。

*   **添加新的工作站**:
    1.  打开定义 `WorkstationType` 枚举的文件 (可能在 `Workstation.cs` 或 `GameEvents.cs` 或专门的枚举文件中)，添加新的工作站类型。
    2.  打开 `Workstation.cs`，在其构造函数或初始化逻辑中为新的类型定义基础参数（如生产周期、产出类型和数量等）。
    3.  打开 `WorkstationSystem.cs`，在 `OnInit()` 方法中的 `mWorkstationBuildCosts` 字典里为新的工作站类型定义建造成本和所需资源。
    4.  （可选）如果新工作站有独特的UI显示或交互，您可能还需要创建或修改对应的 `WorkstationListItemUI.cs` 或其预制件。

*   **一般性建议**:
    *   **理解模块职责**: 在修改前，尽量理解您要修改的功能属于哪个Model（数据存储）、哪个System（逻辑处理）、哪个UI脚本（显示与交互）。
    *   **遵循QFramework规则**:
        *   **获取数据**: UI和System通常通过 `this.GetModel<MyModel>()` 获取数据模型。
        *   **执行操作**: UI通常通过 `this.SendCommand(new MyCommand())` 发送命令来请求改变游戏状态或执行操作。Command内部会获取System来执行具体逻辑。
        *   **响应变化**: UI通常通过监听Model或System中的 `BindableProperty` 或通过 `this.RegisterEvent<MyEvent>()` 监听事件来更新显示。System之间也可能通过事件通信。
    *   **从小处着手**: 先尝试修改现有内容的参数，再尝试添加与现有内容类似的新条目（如新资源、新科技），逐步熟悉流程。
    *   **查看控制台**: Unity的Console窗口会显示错误和警告信息，这是排查问题的重要工具。
    *   **备份项目**: 在进行较大修改前，最好备份您的项目。

## 第六部分：故障排除 (常见问题)

在您探索和修改项目的过程中，可能会遇到一些常见问题。以下是一些排查方向：

*   **UI不显示预期数据 / 列表为空 / 数值不更新**:
    1.  **控制台错误**: 首先检查Unity的 `Console` 窗口是否有红色的错误信息或黄色的警告信息。这些信息通常会指出问题的根源（如脚本未找到、对象为空等）。
    2.  **脚本是否正确挂载**: 确保对应的UI Display脚本 (如 `SurvivorDisplay.cs`) 已经附加到场景中正确的GameObject上 (例如 `SurvivorDisplay_Panel`)。
    3.  **预制件链接**: 如果是动态列表，检查 `XXXDisplay` 脚本中的预制件字段 (如 `survivorItemPrefab`) 是否已经在Inspector中正确链接了对应的预制件。检查预制件本身是否配置正确（子UI元素是否链接到其 `XXXListItemUI` 脚本的字段上）。
    4.  **Inspector字段赋值**: 检查相关的UI Text、Slider、Button等组件是否已经从Hierarchy拖拽到对应脚本在Inspector中暴露的公共字段上。如果字段是空的(None)，脚本就无法控制它们。
    5.  **初始化流程**: 确认 `GameInitializer` 是否正确执行，相关的Model和System是否被成功注册和初始化。如果Model没有数据，UI自然无法显示。
    6.  **Model中是否有数据**: 使用 `Debug.Log()`（如前文所述）在 `Start()` 或相关事件回调中打印Model的内容，确认数据源是否正确。
    7.  **事件/绑定是否正常工作**:
        *   对于 `BindableProperty`：确保UI脚本中的回调方法被正确注册，并且在 `OnDestroy` 中解注册。
        *   对于QFramework事件：确保事件发送方 (`this.SendEvent()`) 和接收方 (`this.RegisterEvent()`) 使用的是完全相同的事件类型，并且接收方已正确注册。

*   **点击UI按钮没有反应**:
    1.  **控制台错误**: 同样，先检查Console。
    2.  **按钮OnClick链接**: 选中按钮GameObject，在Inspector中查看 `Button` 组件的 `OnClick()` 事件列表。确保您期望调用的方法（例如 `MyDisplayScript.OnMyButtonClick`) 已经被添加到列表中，并且其Object字段指向了挂载该脚本的GameObject。
    3.  **Command发送**: 如果按钮点击应该发送一个Command，确认 `this.SendCommand(new MyCommand(...));` 是否被正确调用。
    4.  **Command注册与处理**: 虽然QFramework的Command通常不需要显式“注册”其执行逻辑（它们是自包含的），但要确保Command的 `OnExecute()` 方法内的逻辑是正确的，例如它能正确获取到所需的System。
    5.  **System方法执行**: Command内部调用的System方法是否执行了预期的操作？System方法是否有自己的前置条件检查导致操作未执行？System内部是否有 `Debug.Log` 可以帮助追踪？

*   **编译错误 (代码无法运行，Console显示红色错误)**:
    1.  **仔细阅读错误信息**: Console中的编译错误通常会指明哪个脚本的哪一行出了问题，以及错误类型（如找不到类型、方法签名不匹配、变量未声明等）。
    2.  **检查拼写和大小写**: C#是大小写敏感的。确保变量名、方法名、类名、命名空间等的拼写和大小写与定义它们时完全一致。
    3.  **`using`语句是否缺失**: 如果使用了某个命名空间下的类型（如 `List<T>` 需要 `using System.Collections.Generic;`，或者项目自定义的命名空间如 `YourGameNamespace.Survivors`），确保文件顶部有对应的 `using` 语句。
    4.  **方法签名匹配**: 如果您正在实现一个接口方法或重写一个基类方法，确保方法签名（返回类型、方法名、参数列表）完全匹配。
    5.  **访问权限**: 检查是否有尝试访问 `private` 或 `protected` 成员而导致的错误。
    6.  **分号、花括号**: 检查是否遗漏了分号 `；` 或花括号 `{}` 是否配对正确。

## 第七部分：结语

恭喜您完成了本用户指南的初步阅读！希望这份文档为您提供了一个清晰的起点，帮助您理解和上手这个生存管理游戏项目。

游戏开发是一个不断学习和探索的过程。我们鼓励您：

*   **动手实践**: 尝试修改一些参数，按照指引添加一些简单的内容。
*   **深入代码**: 在熟悉了基本操作后，尝试阅读和理解核心脚本的逻辑。
*   **查阅QFramework文档**: 如果您想更深入地了解QFramework的强大功能和设计理念，可以查阅其官方文档或社区资源，这将对您理解项目架构和进行更复杂的开发非常有帮助。

祝您在本项目中的探索旅程愉快且富有成效！
