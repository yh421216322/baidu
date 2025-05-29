# 游戏设计文档

本文档详细概述了游戏中现有的功能和系统。

## 1. 核心游戏循环与数据 (Core Game Loop & Data)

### 概述 (Overview)

核心游戏循环与数据系统构成了游戏的支柱，管理整体游戏状态、初始化以及时间的推进。它协调其他系统的更新，并持有诸如当前天数和基地健康状况等核心游戏数据。

### 关键类参与 (Key Classes Involved)

*   **`GameArchitecture.cs`**: 实现了 QFramework 的 `Architecture`，作为所有游戏模型和系统的中央注册表。它确保游戏的不同部分能够以结构化的方式相互访问。
*   **`GameInitializer.cs`**: 一个 MonoBehaviour，负责在游戏开始时初始化 `GameArchitecture` 并设置初始游戏状态。这包括创建默认工作站、分配初始幸存者以及设置关键系统参数。
*   **`GameLoop.cs`**: 一个 MonoBehaviour，驱动主游戏更新。在其 `Update()` 方法中，它调用各种已注册系统（工作站、战斗、昼夜、幸存者、研究、探索）的更新方法，确保每个系统每帧处理其逻辑。
*   **`GameDataModel.cs`**: 一个 `AbstractModel` (QFramework)，存储基础游戏数据。目前，它持有 `CurrentDay`（代表已存活天数的整数）和 `BaseHealth`（代表玩家基地健康状况的浮点数）。
*   **`DayNightSystem.cs`**: 一个 `AbstractSystem` (QFramework)，管理游戏内时间的流逝，特别是昼夜循环。它跟踪一天中的当前时间并触发新的一天事件。

### 核心功能 (Core Functionalities)

*   **初始化 (Initialization)**:
    *   `GameArchitecture` 在其 `Init()` 阶段注册所有必要的模型和系统（例如 `ResourceModel`, `SurvivorModel`, `CombatSystem`, `DayNightSystem`）。
    *   `GameInitializer` 确保 `GameArchitecture` 已设置。然后它执行特定的初始设置任务，例如：
        *   如果默认工作站（农场、发电厂、工坊、研究实验室）不存在，则建造它们。
        *   将可用的幸存者分配给其中一些工作站。
        *   为 `CombatSystem` 设置基地位置。
*   **游戏循环更新 (Game Loop Update)**:
    *   `GameLoop.cs` 在其 `Update()` 方法中，系统地调用各个核心系统的 `Update` 方法：
        *   `WorkstationSystem.UpdateAllWorkstations(deltaTime)`: 更新所有工作站的生产和状态。
        *   `CombatSystem.UpdateCombat(deltaTime)`: 管理正在进行的战斗场景。
        *   `DayNightSystem.UpdateDayCycle(deltaTime)`:推进游戏内时间并处理昼夜转换。
        *   `SurvivorManagerSystem.UpdateSurvivorNeeds(deltaTime)`: 更新幸存者状态、饥饿度、口渴度等。
        *   `ResearchSystem.UpdateResearchProcess(deltaTime)`: 推进正在进行的研究项目。
        *   `ExplorationSystem.UpdateActiveExpeditions(deltaTime)`: 更新活动远征的进度。
*   **时间推进（昼夜循环）(Time Progression (Day/Night Cycle))**:
    *   `DayNightSystem` 根据 `SecondsPerDay` 跟踪 `mCurrentTimeInDay`。
    *   当 `mCurrentTimeInDay` 超过 `SecondsPerDay` 时，新的一天开始：
        *   `GameDataModel.CurrentDay` 增加。
        *   发送一个 `DayChangedEvent` 事件。
        *   调用 `TriggerNewDayEvents()`，该方法会：
            *   指示 `CombatSystem` 执行 `SpawnZombieWaveForDay(day)`。
            *   指示 `GEventSystem` 执行 `TryTriggerRandomEvent()`。
    *   如果 `CurrentDay` 超过100天，游戏有胜利条件。
    *   如果 `GameDataModel` 中的 `BaseHealth` 降至0或以下，游戏循环将停止。
*   **中央数据管理 (Central Data Management)**:
    *   `GameDataModel` 提供了一个中央位置来存储和访问 `CurrentDay` 和 `BaseHealth`。其他系统可以查询此模型以获取基本的游戏状态信息。

### 与其他游戏系统的交互 (Interactions with Other Game Systems)

*   **所有系统 (All Systems)**: `GameArchitecture` 充当服务定位器，允许任何系统或模型检索其他已注册系统和模型的实例。
*   **`GameLoop` 交互对象**:
    *   `WorkstationSystem`: 更新工作站状态。
    *   `CombatSystem`: 更新战斗状态。
    *   `DayNightSystem`: 更新游戏时间。
    *   `SurvivorManagerSystem`: 更新幸存者需求。
    *   `ResearchSystem`: 更新研究进度。
    *   `ExplorationSystem`: 更新远征进度。
*   **`DayNightSystem` 交互对象**:
    *   `GameDataModel`: 获取和设置 `CurrentDay`，并检查 `BaseHealth` 以确定游戏循环是否应继续。
    *   `CombatSystem`: 在每个新的一天开始时触发僵尸潮。
    *   `GEventSystem` (通过 `GameArchitecture`): 在每个新的一天开始时触发随机事件。
*   **`GameInitializer` 交互对象 (部分列举)**:
    *   `WorkstationSystem`: 创建初始工作站。
    *   `SurvivorModel`: 获取可用的幸存者进行初始分配。
    *   `CombatSystem`: 设置初始参数，如基地位置。

## 2. 战斗系统 (Combat System)

### 概述 (Overview)

战斗系统负责管理玩家控制实体（间接通过基地防御）与敌对实体（僵尸）之间所有方面的战斗。这包括生成敌人、处理敌人行为、处理攻击以及确定战斗遭遇的结果，主要集中在基地防御上。

### 关键类参与 (Key Classes Involved)

*   **`CombatSystem.cs`**: 一个 `AbstractSystem`，负责协调战斗。它处理僵尸生成、更新僵尸行动以及管理幸存者对僵尸的攻击。
*   **`EnemyModel.cs`**: 一个 `AbstractModel`，存储和管理活动僵尸列表 (`mZombies`)。它提供添加、移除和检索僵尸数据的方法。
*   **`Zombie.cs`**: 代表单个僵尸敌人的类。它包含僵尸的 `Stats`（属性）、`Position`（位置）、`TargetPosition`（目标位置，即基地），以及移动 (`Move`)、攻击 (`AttackTarget`) 和受到伤害 (`TakeDamage`) 的方法。每个僵尸都有一个唯一的 `Id`。
*   **`ZombieStats.cs`**: 一个数据类，包含僵尸的统计数据，包括 `MaxHealth`（最大生命值）、`CurrentHealth`（当前生命值）、`AttackPower`（攻击力）和 `MovementSpeed`（移动速度）。

### 核心功能 (Core Functionalities)

*   **僵尸生成 (`SpawnZombieWaveForDay`)**:
    *   由 `DayNightSystem` 在每个新的一天开始时触发。
    *   要生成的僵尸数量 (`zombiesToSpawn`) 随 `day` 增加 (`baseZombieCount + (day / 2)`)。
    *   僵尸属性 (`healthMultiplier`, `attackMultiplier`) 也随 `day` 调整，使它们逐渐变强。
    *   僵尸在 `spawnAreaCenter` 生成，该中心点随天数进展离基地越来越远 (`new Vector2(10 + day * 0.5f, 0)`)。`spawnRadius`（生成半径）也会增加。
    *   新的 `Zombie` 对象以随机基础属性（在一定范围内）创建，这些属性会根据当天的乘数进行修改，并添加到 `EnemyModel` 中。
*   **僵尸行为 (`UpdateCombat` 和 `Zombie.Move`, `Zombie.AttackTarget`)**:
    *   在 `CombatSystem.UpdateCombat()` 中:
        *   遍历 `EnemyModel` 中的所有僵尸。
        *   如果僵尸未死亡，则调用其 `Move()` 方法。
            *   `Zombie.Move()`: 根据僵尸的 `MovementSpeed` 和 `deltaTime`，将其 `Position` 朝其 `TargetPosition`（基地）移动。
        *   如果僵尸在 `BasePosition` 的 `ZombieAttackRange`（僵尸攻击范围）内：
            *   僵尸执行攻击 (`zombie.AttackTarget()`，该方法会记录攻击日志）。
            *   `GameDataModel.BaseHealth` 因僵尸的 `Stats.AttackPower` 而减少。
            *   如果 `BaseHealth` 降至0或以下，则记录游戏结束条件。
*   **幸存者（基地）防御 (`UpdateCombat`)**:
    *   一个冷却时间 (`mSurvivorAttackCooldown`) 管理幸存者攻击的频率。
    *   如果冷却时间已过，有可用的防御者（`SurvivorModel.GetAllSurvivors()` 中状态为 `Idle` 或职业为 `Soldier` 的幸存者），并且存在活着的僵尸：
        *   检查是否有足够的 `AmmoType` 资源 (`mResourceModel.HasEnough`)。
        *   如果弹药充足，则消耗弹药 (`mResourceModel.ConsumeResource`)。
        *   重置 `mTimeSinceLastSurvivorAttack`。
        *   每个可用的防御者都会以未死亡且在 `SurvivorAttackRange`（幸存者攻击范围）内的 `FindClosestZombie()`（最近的僵尸）为目标。
            *   使用 `BaseSurvivorAttackPower`（基础幸存者攻击力），可能会受到 `SurvivorAttackPowerMultiplier`（幸存者攻击力乘数）的修正。
            *   职业为 `Soldier` 的幸存者在当前攻击力的基础上获得1.5倍的伤害乘数。
            *   调用目标僵尸的 `TakeDamage()` 方法。
*   **伤害与死亡 (`Zombie.TakeDamage`, `CombatSystem.UpdateCombat`)**:
    *   `Zombie.TakeDamage(amount)`: 将 `CurrentHealth` 减少 `amount`。如果 `CurrentHealth` 降至0或以下，则认为僵尸已死亡 (`IsDead` 变为 true)。
    *   在 `CombatSystem.UpdateCombat()` 中，`IsDead` 的僵尸会被添加到一个 `zombiesToRemove` 列表中，然后从 `EnemyModel` 中移除。
*   **目标选择 (`FindClosestZombie`)**:
    *   遍历 `EnemyModel` 中所有未死亡的僵尸。
    *   返回与给定 `position`（通常是 `BasePosition`）距离最小的僵尸。

### 与其他游戏系统的交互 (Interactions with Other Game Systems)

*   **`DayNightSystem`**:
    *   在每个新的一天开始时调用 `CombatSystem.SpawnZombieWaveForDay()`。
*   **`GameDataModel`**:
    *   `CombatSystem` 读取 `BaseHealth` 以检查游戏结束条件。
    *   当僵尸攻击基地时，`CombatSystem` 会写入 `BaseHealth`。
*   **`SurvivorModel`**:
    *   `CombatSystem` 读取所有幸存者及其状态/职业的列表，以确定可用的防御者。
*   **`ResourceModel`**:
    *   当幸存者攻击时，`CombatSystem` 会检查并消耗 `ResourceModel` 中的 `AmmoType` 资源。
*   **`EnemyModel`**:
    *   `CombatSystem` 在生成僵尸时将其添加到 `EnemyModel`。
    *   `CombatSystem` 从 `EnemyModel` 读取僵尸数据以进行更新和目标选择。
    *   `CombatSystem` 从 `EnemyModel` 中移除死亡的僵尸。

## 3. 幸存者管理系统 (Survivor Management System)

### 概述 (Overview)

幸存者管理系统负责游戏中所有幸存者的创建、跟踪和福祉。这包括管理他们的属性、职业、当前状态（空闲、工作、休息等）以及他们的基本需求，如食物和休息。

### 关键类参与 (Key Classes Involved)

*   **`SurvivorManagerSystem.cs`**: 一个 `AbstractSystem`，处理幸存者需求、状态变化以及诸如进食或休息等行动的逻辑。
*   **`SurvivorModel.cs`**: 一个 `AbstractModel`，存储和管理所有幸存者 (`mSurvivors`) 的列表。它提供添加、检索和查询幸存者的方法（例如，获取可用的幸存者）。它会初始化几个默认幸存者（“Bob”和“Alice”）。
*   **`Survivor.cs`**: 代表单个幸存者的类。它包含：
    *   `Id` (Guid), `Name` (字符串)
    *   `Attributes` (`SurvivorAttributes`)
    *   `Profession` (`SurvivorProfession`)
    *   `Status` (`SurvivorStatus`)
    *   `WorkstationId` (Guid?): 他们被分配到的工作站ID（如果有）。
    *   `FoodLevel` (浮点数, 默认 100, 最大 100)
    *   `RestLevel` (浮点数, 默认 100, 最大 100)
*   **`SurvivorAttributes.cs`**: 一个数据类，包含幸存者的核心属性：`Strength`（力量）、`Dexterity`（敏捷）和 `Intelligence`（智力）。
*   **`SurvivorProfession.cs`**: 一个枚举，定义可能的幸存者职业 (`Unassigned`, `Doctor`, `Engineer`, `Soldier`, `Farmer`)。
*   **`SurvivorStatus.cs`**: 一个枚举，定义幸存者可能处于的各种状态 (`Idle`, `Working`, `Resting`, `Injured`, `NeedsAttention`, `OnExpedition`)。

### 核心功能 (Core Functionalities)

*   **幸存者创建 (`CreateNewSurvivor`)**:
    *   接收一个 `name`、`attributes` 和 `profession` 作为输入。
    *   创建一个具有唯一 `Id` 的新 `Survivor` 对象。
    *   将新幸存者添加到 `SurvivorModel`。
*   **幸存者需求管理 (`UpdateSurvivorNeeds`)**:
    *   由 `GameLoop` 每帧调用。
    *   遍历 `SurvivorModel` 中的所有幸存者。
    *   **食物消耗**: 根据 `mFoodConsumptionRate` 和 `deltaTime` 减少 `FoodLevel`。`FoodLevel` 不能低于0。
    *   **休息管理**:
        *   如果 `Status` 是 `Working`，则根据 `mRestDecreaseRateWorking` 减少 `RestLevel`。
        *   如果 `Status` 是 `Idle` 或 `NeedsAttention`，则根据 `mRestDecreaseRateIdle` 减少 `RestLevel`。（受伤的幸存者目前也使用此速率）。
        *   如果 `Status` 是 `Resting`，则根据 `mRestIncreaseRate` 增加 `RestLevel`，最高不超过 `MaxRestLevel`。
        *   `RestLevel` 不能低于0。
    *   **需要关注状态 (Needs Attention Status)**:
        *   如果幸存者的 `FoodLevel` 或 `RestLevel` 达到0，他们的 `Status` 将设置为 `NeedsAttention`，前提是他们目前未处于不可中断的状态（如 `Injured` 或 `Resting`）。
        *   记录一条警告日志。
        *   如果幸存者在 `NeedsAttention` 状态下其需求得到满足（例如 `FoodLevel > 10` 且 `RestLevel > 10`），他们的 `Status` 将改回 `Idle`。
*   **幸存者行动 (Survivor Actions)**:
    *   **进食 (`SurvivorTryEat`)**:
        *   接收一个 `survivorId` 和 `foodToEat` 的数量。
        *   检查指定的幸存者是否存在，以及 `ResourceModel` 中是否有足够的 `Food` 资源。
        *   如果成功，则消耗 `Food` 资源并增加幸存者的 `FoodLevel`（1单位食物资源提供5点食物值），最高不超过 `MaxFoodLevel`。
    *   **休息 (`SurvivorSetResting`)**:
        *   接收一个 `survivorId` 和一个布尔值 `isResting`。
        *   如果 `isResting` 为 true，则将幸存者的 `Status` 设置为 `Resting`。
        *   如果 `isResting` 为 false，则将幸存者的 `Status` 设置为 `Idle`。这允许 `UpdateSurvivorNeeds` 在其需求水平危急时可能将其设置为 `NeedsAttention`。
        *   `Injured`（受伤）的幸存者在休息方面可能会有限制（目前允许）。
*   **查询幸存者 (Querying Survivors)**:
    *   `SurvivorModel.GetSurvivorById(id)`: 检索特定幸存者。
    *   `SurvivorModel.GetAllSurvivors()`: 返回所有幸存者的列表。
    *   `SurvivorModel.GetAvailableSurvivors()`: 返回状态为 `Idle` 的幸存者列表。

### 与其他游戏系统的交互 (Interactions with Other Game Systems)

*   **`GameLoop`**:
    *   每帧调用 `SurvivorManagerSystem.UpdateSurvivorNeeds(deltaTime)`。
*   **`ResourceModel`**:
    *   `SurvivorManagerSystem` 从 `ResourceModel` 读取数据以检查可用的 `Food`。
    *   当幸存者进食时，`SurvivorManagerSystem` 调用 `ResourceModel` 的 `ConsumeResource` 方法。
*   **`WorkstationSystem`**:
    *   （隐式）`WorkstationSystem` 可能会将幸存者分配到工作站，将其 `Status` 更改为 `Working` 并设置其 `WorkstationId`。
    *   `SurvivorManagerSystem` 在确定处于 `NeedsAttention` 状态的幸存者是否应停止工作时会检查 `WorkstationId`（尽管当前实现将此标记为一个复杂的交互点）。
    *   `WorkstationSystem` 可能需要检查幸存者的 `Status` 以确保他们能够工作（例如，不是 `NeedsAttention`、`Injured` 或 `Resting`）。
*   **`CombatSystem`**:
    *   `CombatSystem` 从 `SurvivorModel` 查询可用的防御者（状态为 `Idle` 或职业为 `Soldier` 的幸存者）。
*   **`ExplorationSystem`**:
    *   参与远征的幸存者其 `Status` 将设置为 `OnExpedition`。`SurvivorManagerSystem` 需要考虑此状态，可能通过不应用标准的食物/休息衰减，或由 `ExplorationSystem` 管理。（`OnExpedition` 状态存在于 `SurvivorStatus.cs` 中，但在提供的 `SurvivorManagerSystem.cs` 中未明确详述其处理方式）。

## 4. 资源管理系统 (Resource Management System)

### 概述 (Overview)

资源管理系统负责跟踪和管理所有游戏资源。它提供添加、消耗和查询玩家可用各种资源数量的功能。

### 关键类参与 (Key Classes Involved)

*   **`ResourceManagerSystem.cs`**: 一个 `AbstractSystem`。在当前提供的代码中，该系统非常精简，除了其 `OnInit` 方法外没有明确的功能。资源操作的核心逻辑主要存在于 `ResourceModel` 中。
*   **`ResourceModel.cs`**: 一个 `AbstractModel`，包含管理资源的实际存储和逻辑。它内部使用一个 `ResourceStorage` 对象。
    *   **`ResourceStorage` (内部类/结构, 从 `ResourceModel.cs` 推断)**: 这个类（虽然没有明确显示为单独的文件）在 `ResourceModel` 内部实例化，并负责直接持有不同资源的数量，可能使用字典或类似的数据结构将 `GameResourceType` 映射到一个整数值。
*   **`GameResourceType.cs`**: 一个枚举，定义游戏中所有可用的资源类型（例如 `Food`, `Power`, `Ammo`, `Medicine`, `ResearchPoints`, `ElectronicParts`）。

### 核心功能 (Core Functionalities)

*   **资源初始化 (Resource Initialization)**:
    *   当 `ResourceModel` 初始化时 (`OnInit`)，它会创建一个 `ResourceStorage` 的实例。
    *   `ResourceStorage` 可能会将所有已知的 `GameResourceType` 值初始化为一个起始数量（例如0或某个默认值）。
*   **添加资源 (`ResourceModel.AddResource`)**:
    *   接收一个 `GameResourceType` 和一个要添加的 `amount`。
    *   如果 `amount` 为零或负数，则不执行任何操作。
    *   调用内部的 `resourceStorage.AddResource(type, amount)` 来更新存储的数量。
    *   发送一个 `ResourceChangedEvent`，包含资源类型、新的总数量以及添加的数量。
*   **消耗资源 (`ResourceModel.ConsumeResource`)**:
    *   接收一个 `GameResourceType`、一个要消耗的 `amount` 以及一个可选的 `allowForceConsume` 布尔值。
    *   如果 `amount` 为零或负数，则不执行任何操作。
    *   调用内部的 `resourceStorage.ConsumeResource(type, amount, allowForceConsume)`。
        *   （推断）`ResourceStorage.ConsumeResource` 方法会检查是否有足够的资源。如果没有，并且 `allowForceConsume` 为 false，则消耗失败。如果 `allowForceConsume` 为 true，它可能允许资源计数变为负数或简单地消耗可用部分。
    *   如果消耗成功（从 `resourceStorage.ConsumeResource` 返回 true）：
        *   计算 `amountActuallyConsumed`（实际消耗量）。
        *   如果 `amountActuallyConsumed` 大于0，则发送一个 `ResourceChangedEvent`，包含资源类型、新的总数量以及 `amountActuallyConsumed` 的负值（表示消耗）。
*   **查询资源数量 (`ResourceModel.GetAmount`)**:
    *   接收一个 `GameResourceType`。
    *   通过调用 `resourceStorage.GetAmount(type)` 返回该资源的当前存储数量。
*   **检查资源可用性 (`ResourceModel.HasEnough`)**:
    *   接收一个 `GameResourceType` 和一个 `amount`。
    *   通过调用 `resourceStorage.HasEnough(type, amount)`，如果资源的存储数量大于或等于指定 `amount`，则返回 true。

### 与其他游戏系统的交互 (Interactions with Other Game Systems)

`ResourceModel` 是资源信息的中心点，并与许多其他系统交互：

*   **`WorkstationSystem`**:
    *   当工作站完成生产周期时（例如农场生产 `Food`），可能会向 `ResourceModel` 添加资源。
    *   如果工作站生产需要输入材料，可能会从 `ResourceModel` 消耗资源。
*   **`CombatSystem`**:
    *   当幸存者攻击僵尸时，从 `ResourceModel` 消耗 `Ammo`。
*   **`SurvivorManagerSystem`**:
    *   当调用 `SurvivorTryEat` 时，从 `ResourceModel` 消耗 `Food`。
*   **`ResearchSystem`**:
    *   可能从 `ResourceModel` 消耗资源（例如 `ResearchPoints`, `ElectronicParts`）以开始或推进研究项目。
    *   可能通过专门的工作站或事件向 `ResourceModel` 添加 `ResearchPoints`。
*   **`ExplorationSystem`**:
    *   可能将成功远征的奖励（各种资源）添加到 `ResourceModel`。
    *   可能从 `ResourceModel` 消耗资源以启动远征（例如补给品）。
*   **`EventSystem`**:
    *   随机事件可能会从 `ResourceModel` 添加或移除资源（例如 `ResourceDiscoveryEvent` 添加资源，`FoodSpoilageEvent` 移除 `Food`）。
*   **`QuestSystem`**:
    *   任务奖励可能包括向 `ResourceModel` 添加资源。
    *   任务目标可能涉及收集或花费特定资源。
*   **UI 系统 (例如 `ResourceDisplay.cs`)**:
    *   监听由 `ResourceModel` 发送的 `ResourceChangedEvent`。
    *   通过查询 `ResourceModel.GetAmount()` 更新UI以反映当前的资源数量。

## 5. 工作站系统 (Workstation System)

### 概述 (Overview)

工作站系统管理游戏中所有生产和公用设施建筑（工作站）的功能。这包括建造新的工作站、为其分配幸存者以及处理其生产周期以生成资源或其他产出。

### 关键类参与 (Key Classes Involved)

*   **`WorkstationSystem.cs`**: 一个 `AbstractSystem`，处理工作站的创建、幸存者到工作站的分配/取消分配，并调用所有工作站的更新逻辑。
*   **`WorkstationModel.cs`**: 一个 `AbstractModel`，存储和管理游戏中所有现有 `Workstation` 对象的列表。
*   **`Workstation.cs`**: 代表单个工作站的类。它包含：
    *   `Id` (Guid), `Type` (`WorkstationType`)
    *   `AssignedSurvivorIds` (List<Guid>): 当前分配给此工作站的幸存者ID列表。
    *   `ProductionProgress` (浮点数): 完成生产周期的当前进度。
    *   `BaseProductionRate` (浮点数): 决定基础生产速度的因子（例如，标准速度为1.0，半速为0.5）。
    *   `ProductionCycleTime` (浮点数): 如果 `BaseProductionRate` 为1.0且分配了一名幸存者，则完成一个生产周期所需的时间（秒）。实际时间为 `ProductionCycleTime / effectiveRate`。
    *   `OutputResourceType` (`GameResourceType`): 此工作站生产的资源类型。
    *   `OutputQuantity` (整数): 每个周期的基础资源产出量。
    *   `InputResourceType` (`GameResourceType?`): 生产所需的可选输入资源类型。
    *   `InputQuantity` (整数): 如果设置了 `InputResourceType`，则为所需的输入资源数量。
    *   `ProductionBonusMultiplier` (浮点数, 默认 1.0): 应用于 `OutputQuantity` 的乘数。
    *   `FlatProductionBonus` (整数, 默认 0): 在乘数之后添加到 `OutputQuantity` 的固定加成。
*   **`WorkstationType.cs`**: 一个枚举，定义可用的不同类型的工作站（例如 `Farm`, `PowerPlant`, `Workshop`, `Clinic`, `ResearchLab`）。每种类型在 `Workstation` 构造函数中都有预定义的生产参数（产出、投入、周期时间、基础速率）。

### 核心功能 (Core Functionalities)

*   **建造工作站 (`BuildWorkstation`)**:
    *   接收一个 `WorkstationType`。
    *   创建一个指定类型的新 `Workstation` 对象。
    *   将新工作站添加到 `WorkstationModel`。
    *   发送一个 `WorkstationBuiltEvent`。
*   **分配幸存者 (`AssignSurvivorToWorkstation`)**:
    *   接收一个 `survivorId` 和一个 `workstationId`。
    *   从各自的模型中检索 `Survivor` 和 `Workstation` 对象。
    *   检查幸存者是否为 `Idle`（空闲）状态。
    *   如果幸存者之前被分配到不同的工作站，则首先将其从先前的工作站取消分配。
    *   调用 `workstation.AssignSurvivor(survivorId)`。
        *   `Workstation.AssignSurvivor()`: 将 `survivorId` 添加到其 `AssignedSurvivorIds` 列表（将来可以添加容量检查）。
    *   更新幸存者的 `WorkstationId` 并将其 `Status` 设置为 `Working`（工作中）。
*   **取消分配幸存者 (隐式及 `Workstation.UnassignSurvivor`)**:
    *   `Workstation.UnassignSurvivor(survivorId)`: 从 `AssignedSurvivorIds` 中移除 `survivorId`。当幸存者被重新分配或可能无法工作时调用此方法。
*   **生产更新 (`UpdateAllWorkstations` 和 `Workstation.UpdateProduction`)**:
    *   `WorkstationSystem.UpdateAllWorkstations()`: 由 `GameLoop` 每帧调用。遍历 `WorkstationModel` 中的所有工作站并调用其 `UpdateProduction` 方法。
    *   `Workstation.UpdateProduction(deltaTime, survivorModel, resourceModel)`:
        *   如果没有分配幸存者 (`AssignedSurvivorIds.Count == 0`)，生产通常会停止，或者 `ProductionProgress` 可能会被重置（目前只是返回）。
        *   使用 `GetCurrentProductionRatePerSecond(survivorModel)` 计算 `effectiveRate`（有效速率）。
            *   `Workstation.GetCurrentProductionRatePerSecond()`: 如果有任何幸存者被分配，则当前返回 `BaseProductionRate`，否则返回0。这里可以考虑幸存者技能或多个幸存者加成。
        *   通过 `effectiveRate * deltaTime` 增加 `ProductionProgress`。
        *   如果 `ProductionProgress` >= `ProductionCycleTime`:
            *   计算发生了多少个完整的 `cyclesCompleted`（周期）。
            *   通过 `cyclesCompleted * ProductionCycleTime` 减少 `ProductionProgress`（结转多余进度）。
            *   对于每个完成的周期：
                *   如果设置了 `InputResourceType`，则尝试从 `resourceModel` 消耗 `InputQuantity`。如果消耗失败，则该周期（以及本次更新中的后续周期）的生产将停止。
                *   如果满足输入（或不需要），则使用 `OutputQuantity`、`ProductionBonusMultiplier` 和 `FlatProductionBonus` 计算 `finalOutputQuantity`。确保产出至少为1，除非基础为0。
                *   将 `finalOutputQuantity` 的 `OutputResourceType` 添加到 `resourceModel`。
                *   记录生产日志。

### 预定义工作站类型和参数 (来自 `Workstation.cs` 构造函数的示例)

*   **Farm (农场)**: 生产 5 `Food`。周期: 10秒。速率: 1.0。
*   **PowerPlant (发电厂)**: 生产 10 `Power`。周期: 12秒。速率: 1.0。
*   **Workshop (工坊)**: 生产 2 `Ammo`。消耗 1 `Power`。周期: 15秒。速率: 0.5 (实际30秒)。
*   **Clinic (诊所)**: 生产 1 `Medicine`。消耗 2 `Food`。周期: 20秒。速率: 1.0。
*   **ResearchLab (研究实验室)**: 生产 1 `ResearchPoints`。消耗 1 `Power`。周期: 20秒。速率: 1.0。

### 与其他游戏系统的交互 (Interactions with Other Game Systems)

*   **`GameLoop`**:
    *   每帧调用 `WorkstationSystem.UpdateAllWorkstations(deltaTime)`。
*   **`SurvivorModel` & `SurvivorManagerSystem`**:
    *   `WorkstationSystem` 在分配时从 `SurvivorModel` 读取幸存者状态。
    *   `WorkstationSystem` 更新幸存者状态为 `Working` 并设置 `Survivor` 对象（由 `SurvivorModel` 管理）上的 `WorkstationId`。
    *   `Workstation.GetCurrentProductionRatePerSecond()` 接收 `SurvivorModel` 作为参数，表明未来可能使用幸存者数据（技能、数量）来修改生产率。
*   **`ResourceModel`**:
    *   `Workstation.UpdateProduction()` 向 `ResourceModel` 添加产出资源。
    *   `Workstation.UpdateProduction()` 从 `ResourceModel` 消耗输入资源。
*   **`GameInitializer`**:
    *   在游戏开始时调用 `WorkstationSystem.BuildWorkstation()` 来创建默认工作站。
*   **`GEventSystem`**:
    *   `WorkstationSystem.BuildWorkstation()` 发送一个 `WorkstationBuiltEvent`。UI或其他系统可以使用它来响应新的工作站。
*   **`ResearchSystem`**:
    *   技术效果（来自 `ResearchSystem`）可能会修改 `Workstation` 参数，如 `ProductionBonusMultiplier` 或 `FlatProductionBonus`。（这种交互在提供的代码片段中没有明确编码，但是一种常见的模式）。

## 6. 研究系统 (Research System)

### 概述 (Overview)

研究系统允许玩家通过投入研究点数和时间到技术中，来解锁新的能力、加成和游戏元素（如工作站）。它管理各种技术的可用性、进度和应用。

### 关键类参与 (Key Classes Involved)

*   **`ResearchSystem.cs`**: 一个 `AbstractSystem`，管理活动的研究过程。它处理开始、更新和完成研究项目。它还应用已完成技术的效果。
*   **`ResearchModel.cs`**: 一个 `AbstractModel`，存储所有定义的技术 (`mAllTechnologies`) 及其当前状态。它提供查询技术和根据前置条件及完成情况更新其状态的方法。它包含一个 `PopulateInitialTechnologies()` 方法来定义初始科技树。
*   **`Technology.cs`**: 代表单个可研究技术的类。它包含：
    *   `Id` (字符串), `Name` (字符串), `Description` (字符串)
    *   `ResearchPointCost` (整数): *开始*研究所需的研究点数，也隐含地表示开始后完成所需的“努力”或“时间”。
    *   `PrerequisiteTechIds` (List<字符串>): 在此技术可用之前必须完成的技术ID列表。
    *   `Effects` (List<`TechnologyEffectData`>): 此技术完成时应用于游戏的效果列表。
    *   `Status` (`ResearchStatus`): 技术的当前状态 (`Locked`, `Available`, `InProgress`, `Completed`)。
*   **`TechnologyEffectData.cs`**: 定义技术单个效果的类。它包含：
    *   `EffectType` (`TechnologyEffectType`): 效果类型（例如，增加产量、修改幸存者属性、解锁工作站）。
    *   `Value` (浮点数): 效果的大小。
    *   `TargetResource` (`GameResourceType`): 效果的可选目标资源。
    *   `TargetWorkstationType` (`WorkstationType`): 效果的可选目标工作站类型。
*   **`ResearchStatus.cs` (`Technology.cs` 内的枚举)**: 定义技术可能状态的枚举：`Locked`（锁定）、`Available`（可用）、`InProgress`（进行中）、`Completed`（已完成）。
*   **`TechnologyEffectType.cs` (`TechnologyEffectData.cs` 内的枚举)**: 定义技术可以产生的效果类型的枚举（例如 `IncreaseProductionOutput`, `IncreaseProductionMultiplier`, `ModifySurvivorStat`, `UnlockWorkstation`）。

### 核心功能 (Core Functionalities)

*   **技术定义与初始化 (`ResearchModel.PopulateInitialTechnologies`)**:
    *   在 `ResearchModel.mAllTechnologies` 中创建并存储一组预定义的技术。每项技术包括其ID、名称、描述、成本、前置条件和效果。
    *   示例：“TECH_FARM_1”（提高农场产量）、“TECH_AMMO_1”（提高弹药工坊产量）、“TECH_UNLOCK_LAB”（解锁研究实验室工作站）、“TECH_BALLISTICS_1”（增加幸存者攻击力）。
*   **状态更新 (`ResearchModel.UpdateAllTechnologyStatuses`, `ResearchModel.UpdateTechnologyStatus`)**:
    *   `UpdateAllTechnologyStatuses()`: 遍历所有技术。如果一项技术处于 `Locked` 状态且其所有 `PrerequisiteTechIds` 都已 `Completed`，则其状态更改为 `Available`。如果状态更改，则通知 `ResearchSystem`。
    *   `UpdateTechnologyStatus(techId, newStatus)`: 直接设置特定技术的状态。
*   **开始研究 (`ResearchSystem.StartResearch`)**:
    *   接收一个 `techId`。
    *   检查是否已有其他研究正在进行中。
    *   验证技术是否存在且为 `Available`。
    *   从 `ResourceModel` 消耗 `ResearchPointCost`。
    *   如果所有检查通过，则将 `mCurrentResearch` 设置为所选技术，重置 `mCurrentResearchTimeAccumulated`，并将技术状态更新为 `InProgress`。
*   **研究进度 (`ResearchSystem.UpdateResearchProcess`)**:
    *   由 `GameLoop` 每帧调用。
    *   如果 `mCurrentResearch` 不为null，则通过 `deltaTime` 增加 `mCurrentResearchTimeAccumulated`。（注意：提供的代码中有 `deltaTime + 50`，这似乎是一个错误，会使研究完成得非常快。它可能应该是 `deltaTime` 或由某个效率因子缩放的 `deltaTime`）。
    *   如果 `mCurrentResearchTimeAccumulated` >= `mCurrentResearch.ResearchPointCost`（意味着“努力”或“时间”与初始点数成本相匹配），则调用 `CompleteResearch`。
*   **完成研究 (`ResearchSystem.CompleteResearch`)**:
    *   在 `ResearchModel` 中将已完成技术的状态设置为 `Completed`。
    *   遍历已完成技术的 `Effects`：
        *   根据 `TechnologyEffectType` 应用效果：
            *   `IncreaseProductionMultiplier`: 修改目标工作站的 `ProductionBonusMultiplier`。
            *   `IncreaseProductionOutput`: 修改目标工作站的 `FlatProductionBonus`。
            *   `ModifySurvivorStat`: 示例显示修改 `CombatSystem` 中的 `SurvivorAttackPowerMultiplier`。
            *   `UnlockWorkstation`: 记录工作站类型已解锁（实际解锁可能涉及另一个系统或UI更新）。
        *   这需要 `ResearchSystem` 获取其他模型/系统（例如 `WorkstationModel`, `CombatSystem`）。
    *   重置 `mCurrentResearch` 和 `mCurrentResearchTimeAccumulated`。
    *   调用 `ResearchModel.UpdateAllTechnologyStatuses()` 以可能解锁新技术。
    *   发送一个 `TechnologyCompletedEvent`。
*   **查询研究状态 (Querying Research State)**:
    *   `ResearchModel.GetTechnology(techId)`, `GetAllTechnologies()`, `GetAvailableTechnologies()`, `GetCompletedTechnologies()`, `GetInProgressTechnologies()`。
    *   `ResearchSystem.GetCurrentResearch()`, `GetCurrentResearchProgressNormalized()`, `IsResearching()`。

### 与其他游戏系统的交互 (Interactions with Other Game Systems)

*   **`GameLoop`**:
    *   每帧调用 `ResearchSystem.UpdateResearchProcess(deltaTime)`。
*   **`ResourceModel`**:
    *   `ResearchSystem` 在开始研究时从 `ResourceModel` 消耗 `ResearchPoints`。
    *   工作站（如 `ResearchLab`）生产 `ResearchPoints` 并将其添加到 `ResourceModel`。
*   **`WorkstationModel` & `WorkstationSystem`**:
    *   技术效果可以修改由 `WorkstationModel` 管理的 `Workstation` 实例的 `ProductionBonusMultiplier` 和 `FlatProductionBonus`。
    *   `UnlockWorkstation` 效果会改变可建造工作站类型的可用性（可能由监听事件的UI或检查已解锁技术的 `WorkstationSystem` 处理）。
    *   `ResearchLab` 工作站对于生成 `ResearchPoints` 至关重要。
*   **`CombatSystem`**:
    *   技术效果可以修改与战斗相关的参数，例如 `CombatSystem` 中的 `SurvivorAttackPowerMultiplier`。
*   **`GEventSystem`**:
    *   研究完成后，`ResearchSystem` 发送 `TechnologyCompletedEvent`。`QuestSystem` 或 UI 可以使用此事件。
    *   `ResearchSystem` 可以通过 `ResearchModel` 触发 `OnTechnologyStatusChanged` 事件来更新UI元素，如科技树。
*   **UI 系统 (例如 `ResearchDisplay.cs`)**:
    *   显示科技树、可用研究、当前研究进度。
    *   允许玩家选择并开始研究。
    *   根据 `TechnologyCompletedEvent` 或 `OnTechnologyStatusChanged` 等事件进行更新。

## 7. 探索系统 (Exploration System)

### 概述 (Overview)

探索系统允许玩家派遣幸存者前往各个兴趣点（POI）进行远征，以寻找资源、触发事件或发现新信息。它管理POI的发现、远征过程（行进、探索、返回）以及远征结果的解析。

### 关键类参与 (Key Classes Involved)

*   **`ExplorationSystem.cs`**: 一个 `AbstractSystem`，负责管理活动的远征。它处理开始远征、更新其不同阶段（出发、探索、返回）的进度以及解析其结果。
*   **`ExplorationModel.cs`**: 一个 `AbstractModel`，存储所有定义的POI (`mPointsOfInterest`) 和当前活动的远征 (`mActiveExpeditions`)。它包含填充初始POI、查询POI和远征以及更新POI状态的方法。
*   **`Expedition.cs`**: 代表一次活动远征的类。它包含：
    *   `ExpeditionId` (Guid), `TargetPoiId` (字符串)
    *   `AssignedSurvivorIds` (List<Guid>)
    *   `TravelTimeToPoi` (浮点数): 到达POI所需的时间。
    *   `ExplorationTimeAtPoi` (浮点数): 在POI探索所花费的时间。
    *   `TravelTimeBackToBase` (浮点数): 从POI返回所需的时间。
    *   `TimeElapsedOnCurrentPhase` (浮点数): 跟踪在当前阶段花费的时间。
    *   `Status` (`ExpeditionStatus`): 远征的当前状态（例如 `Departing`, `Exploring`, `Returning`, `Completed`）。
    *   `Outcome` (`ExpeditionOutcome`): 完成后存储远征结果。
*   **`ExplorationPointOfInterest.cs` (POI)**: 代表一个可发现位置的类。它包含：
    *   `Id` (字符串), `Name` (字符串), `Description` (字符串)
    *   `Difficulty` (整数): 影响风险并可能影响奖励。
    *   `BaseExplorationTime` (浮点数): 探索此POI所需的基础时间。
    *   `MaxSurvivorSlots` (整数): 可分配的最大幸存者数量。
    *   `PotentialRewards` (List<`POIReward`>): 可能找到的奖励列表。
    *   `Status` (`POIStatus`): POI的当前状态（例如 `Unexplored`, `BeingExplored`, `Explored`, `Depleted`）。
*   **`POIReward.cs`**: 定义POI潜在奖励的类，包括 `ResourceType`、`MinQuantity`、`MaxQuantity` 和找到它的 `Probability`（概率）。
*   **`ExpeditionStatus.cs` (`Expedition.cs` 内的枚举)**: 定义远征的阶段：`Preparing`（准备中）、`Departing`（出发中）、`Exploring`（探索中）、`Returning`（返回中）、`Completed`（已完成）、`Failed`（失败）。
*   **`POIStatus.cs` (`ExplorationPointOfInterest.cs` 内的枚举)**: 定义POI的状态：`Unexplored`（未探索）、`Scouted`（已侦察）、`BeingExplored`（探索中）、`Explored`（已探索）、`Depleted`（已耗尽）。
*   **`ExpeditionOutcome.cs` (`Expedition.cs` 隐式使用的结构)**: 用于存储远征结果的数据结构，包括 `WasSuccessful` (布尔值)、`NarrativeLog` (字符串)、`ResourcesFound` (Dictionary<`GameResourceType`, 整数>) 和 `SurvivorStatusChanges` (List<字符串>)。

### 核心功能 (Core Functionalities)

*   **POI 定义与初始化 (`ExplorationModel.PopulateInitialPOIs`)**:
    *   在 `ExplorationModel.mPointsOfInterest` 中创建并存储一组预定义的POI。每个POI包括其ID、名称、描述、难度、基础探索时间、幸存者槽位和潜在奖励。
    *   示例：“POI_SUPERMARKET_1”（食物、药品）、“POI_POLICE_1”（弹药）、“POI_LIBRARY_1”（研究点数）、“POI_RADIO_TOWER”（研究点数、电子零件）。
*   **开始远征 (`ExplorationSystem.StartExpedition`)**:
    *   使用 `CanStartExpeditionToPOI` 验证是否可以开始远征（检查POI状态、幸存者可用性/状态以及槽位限制）。
    *   如果有效，则创建一个新的 `Expedition` 对象。
        *   `actualExplorationTimeAtPoi` 当前设置为 `poi.BaseExplorationTime`，但将来可能会根据幸存者技能进行修改。
        *   默认行进时间 (`TravelTimeToPoi`, `TravelTimeBackToBase`) 当前是固定的（例如，各10秒）。
    *   将远征添加到 `ExplorationModel.mActiveExpeditions`。
    *   将POI的状态更新为 `BeingExplored`。
    *   将被分配幸存者的状态设置为 `OnExpedition`。
*   **更新活动远征 (`ExplorationSystem.UpdateActiveExpeditions`)**:
    *   由 `GameLoop` 每帧调用。
    *   遍历 `ExplorationModel` 中的 `mActiveExpeditions`。
    *   为每个远征增加 `TimeElapsedOnCurrentPhase`。
    *   根据已用时间管理阶段转换：
        *   `Departing` -> `Exploring` (当 `TimeElapsedOnCurrentPhase` >= `TravelTimeToPoi`)
        *   `Exploring` -> `Returning` (当 `TimeElapsedOnCurrentPhase` >= `ExplorationTimeAtPoi`)
        *   `Returning` -> `Completed` (当 `TimeElapsedOnCurrentPhase` >= `TravelTimeBackToBase`)
    *   当远征 `Completed` 时，调用 `ResolveExpeditionOutcome`。
    *   从 `mActiveExpeditions` 中移除已完成的远征。
*   **解析远征结果 (`ExplorationSystem.ResolveExpeditionOutcome`)**:
    *   检索与远征相关的 `POI`。
    *   填充 `ExpeditionOutcome` 对象：
        *   **资源奖励**: 遍历 `poi.PotentialRewards`。对每个奖励，根据 `Probability` 进行判定。如果成功，则授予 `MinQuantity` 和 `MaxQuantity` 之间的随机数量的 `ResourceType`。将这些添加到 `Outcome.ResourcesFound` 和全局 `ResourceModel`。
        *   **幸存者风险**: 对每个幸存者，计算 `injuryRisk`（当前为 `baseInjuryChance * poi.Difficulty`）。如果随机判定低于此风险，则幸存者的状态设置为 `Injured`。否则，设置为 `Idle`。更改记录在 `Outcome.SurvivorStatusChanges` 和 `Outcome.NarrativeLog` 中。
        *   设置 `Outcome.WasSuccessful`（目前如果完成则始终为true）。
        *   将POI的状态更新为 `Explored`（在更复杂的场景中可能变为 `Depleted`）。
    *   记录详细结果。
    *   通知UI (`ExplorationDisplay.DisplayOutcome`)。
    *   如果成功，则发送一个 `POIExploredEvent`。
*   **查询探索状态 (Querying Exploration State)**:
    *   `ExplorationModel`: `GetPOI(id)`, `GetAllPOIs()`, `GetAvailablePOIs()`, `GetActiveExpeditions()`, `GetExpedition(id)`。

### 与其他游戏系统的交互 (Interactions with Other Game Systems)

*   **`GameLoop`**:
    *   每帧调用 `ExplorationSystem.UpdateActiveExpeditions(deltaTime)`。
*   **`SurvivorModel` & `SurvivorManagerSystem`**:
    *   `ExplorationSystem` 从 `SurvivorModel` 读取幸存者状态以检查远征的可用性。
    *   `ExplorationSystem` 在远征开始时将幸存者状态更新为 `OnExpedition`，并在解析结果时更新为 `Idle` 或 `Injured`。`SurvivorManagerSystem` 需要正确处理 `OnExpedition` 状态（例如，暂停需求衰减或应用不同规则）。
*   **`ResourceModel`**:
    *   `ExplorationSystem` 将远征期间找到的资源添加到 `ResourceModel`。
*   **`GEventSystem`**:
    *   成功探索后，`ExplorationSystem` 发送 `POIExploredEvent`。`QuestSystem` 或其他系统可以使用此事件。
*   **UI 系统 (例如 `ExplorationDisplay.cs`)**:
    *   显示可用的POI，允许玩家分配幸存者并开始远征。
    *   显示活动远征的进度。
    *   远征完成后显示 `ExpeditionOutcome`。
*   **`QuestSystem`**:
    *   任务可能需要探索特定的POI（监听 `POIExploredEvent`）。
    *   发现新的POI可能成为任务目标或触发新任务。

## 8. 事件系统 (Event System)

### 概述 (Overview)

事件系统负责引入随机事件并通过全局事件总线管理系统间的通信。随机事件为游戏增加了不可预测性和挑战性，而全局事件总线允许不同的游戏系统在没有直接依赖关系的情况下对重要事件做出反应。

### 关键类参与 (Key Classes Involved)

*   **`GEventSystem.cs` (在某些上下文中称为 EventSystem)**: 一个 `AbstractSystem` (QFramework)，管理随机事件的触发。它持有一系列“事件工厂”（创建特定随机事件实例的委托）。
*   **`EventModel.cs`**: 一个 `AbstractModel` (QFramework)，主要存储 `CurrentEvent`（当前活动的随机事件，如果有的话），以便UI或其他系统可以访问其详细信息以进行显示或处理。
*   **`RandomEvent.cs` (抽象基类, 推断)**: 虽然没有明确作为单独文件提供，但像 `FoodSpoilageEvent` 这样的派生事件类暗示了一个基类 `RandomEvent`，该基类可能定义了通用接口或属性，例如 `Title`、`Description` 和一个 `Execute(IArchitecture architecture)` 方法。
*   **特定事件类 (例如 `FoodSpoilageEvent.cs`, `ResourceDiscoveryEvent.cs`, `SurvivorSicknessEvent.cs`)**: 这些类继承自 `RandomEvent`。每个类定义一个特定的随机事件：
    *   它们设置自己的 `Title` 和 `Description`（通常根据执行结果动态设置）。
    *   它们的 `Execute()` 方法包含通过 `IArchitecture` 接口与各种模型（例如 `ResourceModel`, `SurvivorModel`）交互来将事件效果应用于游戏状态的逻辑。
*   **`GameEvents.cs`**: 此文件定义了一系列普通的C#类，作为全局事件总线（QFramework的事件系统）的事件数据结构。这些不是随机事件，而是系统可以发送和监听的特定通知。示例包括：
    *   `ResourceChangedEvent`: 当资源数量发生变化时发送。
    *   `TechnologyCompletedEvent`: 当研究完成时发送。
    *   `POIExploredEvent`: 当POI被探索时发送。
    *   `WorkstationBuiltEvent`: 当建造新工作站时发送。
    *   `DayChangedEvent`: 当游戏天数变化时发送。
    *   `CustomQuestFlagEvent`, `QuestActivatedEvent`, `QuestObjectiveCompletedEvent`, `QuestSucceededEvent`: 由任务系统使用。

### 核心功能 (Core Functionalities)

*   **随机事件触发 (`GEventSystem.TryTriggerRandomEvent`)**:
    *   由 `DayNightSystem` 在每个新的一天开始时调用。
    *   有一定几率（例如25%）触发新的随机事件。
    *   如果几率判定成功：
        *   从 `mEventFactories` 中选择一个随机事件工厂。
        *   使用工厂创建一个 `RandomEvent` 的实例。
        *   调用所选 `RandomEvent` 实例的 `Execute()` 方法，并传递游戏的 `IArchitecture`，以便事件可以访问模型和系统。
        *   将执行的事件实例存储在 `EventModel.CurrentEvent` 中，以供潜在的UI显示。
    *   如果没有触发新事件，则将 `EventModel.CurrentEvent` 设置为 `null`，以清除显示中的任何先前事件。
*   **随机事件执行 (在每个特定事件类的 `Execute` 方法内)**:
    *   **`FoodSpoilageEvent`**:
        *   访问 `ResourceModel`。
        *   计算要损失的随机数量的 `Food`（例如，当前食物的20%，或一个最小范围）。
        *   从 `ResourceModel` 消耗计算出的 `Food` 数量。
        *   根据结果设置其 `Description`。
    *   **`ResourceDiscoveryEvent`**:
        *   访问 `ResourceModel`。
        *   随机选择一个 `GameResourceType`。
        *   随机确定一个 `amountFound`（例如15-50）。
        *   将 `amountFound` 的 `foundType` 添加到 `ResourceModel`。
        *   设置其 `Description`。
    *   **`SurvivorSicknessEvent`**:
        *   访问 `SurvivorModel`。
        *   查找健康幸存者列表（非 `Injured` 或 `NeedsAttention` 状态）。
        *   如果存在健康幸存者，则随机选择一个并将其 `Status` 更改为 `Injured`。
        *   根据结果设置其 `Description`。
*   **全局事件总线 (QFramework)**:
    *   系统可以使用 `this.SendEvent(new MyEvent(...))` 发送在 `GameEvents.cs` 中定义的事件类的实例。
    *   其他系统、模型或UI组件可以注册这些特定事件类型的监听器以做出相应反应（例如，UI在 `ResourceChangedEvent` 上更新，任务系统监听 `TechnologyCompletedEvent`）。这是QFramework架构的一部分，在提供的事件系统文件中没有明确详述，但与此类事件定义一起是常见的使用模式。

### 与其他游戏系统的交互 (Interactions with Other Game Systems)

*   **`DayNightSystem`**:
    *   在每个新的一天开始时调用 `GEventSystem.TryTriggerRandomEvent()`。
*   **`ResourceModel`**:
    *   像 `FoodSpoilageEvent` 这样的随机事件会从 `ResourceModel` 消耗资源。
    *   像 `ResourceDiscoveryEvent` 这样的随机事件会向 `ResourceModel` 添加资源。
    *   当通过其方法添加或消耗资源时，`ResourceModel` 本身会通过全局事件总线发送 `ResourceChangedEvent`。
*   **`SurvivorModel`**:
    *   像 `SurvivorSicknessEvent` 这样的随机事件可以更改由 `SurvivorModel` 管理的幸存者的 `Status`。
*   **UI 系统 (例如 `EventDisplay.cs`)**:
    *   可能会读取 `EventModel.CurrentEvent` 以显示活动随机事件的标题和描述。
    *   其他UI元素（如 `ResourceDisplay.cs`）监听像 `ResourceChangedEvent` 这样的全局事件以更新自身。
*   **各种系统 (作为事件发布者/订阅者)**:
    *   `WorkstationSystem` 发送 `WorkstationBuiltEvent`。
    *   `ResearchSystem` 发送 `TechnologyCompletedEvent`。
    *   `ExplorationSystem` 发送 `POIExploredEvent`。
    *   `QuestSystem` 发送和监听各种与任务相关的事件。
    *   任何系统都可能监听与其功能相关的事件（例如，`QuestSystem` 监听 `TechnologyCompletedEvent` 或 `POIExploredEvent` 以完成目标）。

### 关于 "GEventSystem" 与 "EventSystem" 的说明
该类在 `EventSystem.cs` 中名为 `GEventSystem`，但 `GameArchitecture.cs` 将其注册为 `this.RegisterSystem(new GEventSystem());`。其他系统如 `DayNightSystem` 使用 `this.GetSystem<GEventSystem>()` 获取它。本文档将使用 `GEventSystem` 以明确指代这个用于随机事件管理的特定类，区别于QFramework的通用事件总线机制。

## 9. 任务系统 (Quest System)

### 概述 (Overview)

任务系统管理任务的所有方面，包括其激活、目标跟踪以及完成后的奖励发放。它提供了一种结构化的方式来引导玩家进程并引入叙事元素。

### 关键类参与 (Key Classes Involved)

*   **`QuestSystem.cs`**: 一个 `AbstractSystem`，负责管理任务的生命周期。它监听游戏事件以更新任务目标进度，检查任务激活条件，并处理任务完成，包括奖励应用。
*   **`QuestModel.cs`**: 一个 `AbstractModel`，存储所有定义的任务 (`AllQuests`) 和当前 `ActiveQuests`（活动任务）的列表。它包含 `PopulateInitialQuests()` 来定义游戏中的任务。
*   **`Quest.cs`**: 代表单个任务的类。它包含：
    *   `Id` (字符串), `Title` (字符串), `Description` (字符串)
    *   `Status` (`QuestStatus` 枚举: `Locked`, `Available`, `Active`, `Success`, `Failed`)
    *   `Objectives` (List<`QuestObjective`>)
    *   前置条件: `PrerequisiteQuestIds` (List<字符串>), `RequiredDay` (整数), `RequiredTechId` (字符串)
    *   `Rewards` (List<`QuestReward`>)
*   **`QuestObjective.cs`**: 定义任务中单个目标的类。它包含：
    *   `Description` (字符串)
    *   `Type` (`ObjectiveType` 枚举: `CollectResource`, `ResearchTech`, `ExplorePOI`, `BuildWorkstation`, `CustomFlag`)
    *   `TargetId` (字符串): 目标对象的标识符（例如，资源名称、技术ID）。
    *   `RequiredAmount` (整数), `CurrentAmount` (整数)
    *   `IsComplete` (布尔属性，基于 `CurrentAmount` >= `RequiredAmount`)
*   **`QuestReward.cs`**: 定义完成任务奖励的类。它包含：
    *   `Type` (`QuestRewardType` 枚举: `Resource`, `Item`, `UnlockTech`, `UnlockPOI`, `SpawnSurvivor`)
    *   `TargetId` (字符串): 奖励对象的标识符（例如，资源名称、技术ID）。
    *   `Amount` (整数): 用于资源奖励的数量。
*   **枚举 (在其各自的文件或 `Quest.cs` 中定义)**:
    *   `QuestStatus`: `Locked`（锁定）、`Available`（可用）、`Active`（活动）、`Success`（成功）、`Failed`（失败）。
    *   `ObjectiveType`: `CollectResource`（收集资源）、`ResearchTech`（研究技术）、`ExplorePOI`（探索兴趣点）、`BuildWorkstation`（建造工作站）、`CustomFlag`（自定义标记）。
    *   `QuestRewardType`: `Resource`（资源）、`Item`（物品）、`UnlockTech`（解锁技术）、`UnlockPOI`（解锁兴趣点）、`SpawnSurvivor`（生成幸存者）。

### 核心功能 (Core Functionalities)

*   **任务定义 (`QuestModel.PopulateInitialQuests`)**:
    *   预先定义任务及其ID、标题、描述、目标、前置条件（其他任务、天数、已完成的技术）和奖励。
    *   示例任务：“Q_FOUNDATION”（收集基础资源）、“Q_REACH_SKY”（研究无线电技术、侦察无线电塔POI）、“Q_BEACON_1”（收集电子零件和电力）。
*   **任务激活 (`QuestSystem.CheckForAllQuestActivations`)**:
    *   在 `OnInit` 期间和任务完成后调用。也由 `DayChangedEvent` 触发。
    *   遍历 `QuestModel.AllQuests` 中的所有任务。
    *   对于状态为 `QuestStatus.Locked` 的任务：
        *   检查所有 `PrerequisiteQuestIds` 是否具有 `QuestStatus.Success` 状态。
        *   检查 `GameDataModel.CurrentDay` 是否 >= `quest.RequiredDay`。
        *   检查 `quest.RequiredTechId`（如果有）是否已在 `ResearchModel` 中完成（状态为 `ResearchStatus.Completed`）。
        *   如果满足所有条件，则调用 `ActivateQuest(quest)`。
*   **激活任务 (`QuestSystem.ActivateQuest`)**:
    *   在 `QuestModel` 中将任务的 `Status` 设置为 `Active`，并将其添加到 `ActiveQuests`。
    *   对于 `CollectResource` 目标，根据 `ResourceModel` 中的当前数量初始化 `CurrentAmount`。
    *   检查任务在激活时是否已完成（例如，目标已由现有游戏状态满足）。如果是，则调用 `CompleteQuest`。
*   **跟踪目标进度 (`QuestSystem.OnGameEvent`)**:
    *   `QuestSystem` 注册各种游戏事件的监听器（例如 `ResourceChangedEvent`, `TechnologyCompletedEvent`, `POIExploredEvent`, `WorkstationBuiltEvent`, `CustomQuestFlagEvent`）。
    *   当相关的游戏事件发生时：
        *   遍历所有 `ActiveQuests` 的目标。
        *   如果目标的 `Type` 和 `TargetId` 与事件匹配：
            *   对于 `CollectResource`，将 `CurrentAmount` 更新为该资源的新总量。
            *   对于其他类型（通常是基于计数的，如“研究1项技术”），则增加 `CurrentAmount`。
        *   更新目标后，检查 `quest.AreAllObjectivesComplete()`。如果为true，则调用 `CompleteQuest(quest)`。
*   **完成任务 (`QuestSystem.CompleteQuest`)**:
    *   在 `QuestModel` 中将任务的 `Status` 设置为 `Success`，并将其从 `ActiveQuests` 中移除。
    *   记录完成日志。
    *   遍历 `quest.Rewards` 并为每个奖励调用 `ApplyReward`。
    *   再次调用 `CheckForAllQuestActivations()`，因为完成此任务可能会解锁新任务。
*   **应用奖励 (`QuestSystem.ApplyReward`)**:
    *   根据 `QuestRewardType`:
        *   `Resource`: 将指定 `Amount` 的 `TargetId`（资源类型）添加到 `ResourceModel`。
        *   `UnlockTech`: 将技术标记为可解锁。（当前实现会记录此信息，并依赖 `ResearchModel.UpdateAllTechnologyStatuses()` 来处理实际可用性，如果其他前置条件已满足）。
        *   `UnlockPOI`: 在 `ExplorationModel` 中将POI的状态从 `Unexplored` 更改为 `Scouted`。
        *   其他类型如 `Item` 或 `SpawnSurvivor` 被标记为未来实现。

### 与其他游戏系统的交互 (Interactions with Other Game Systems)

*   **`GEventSystem` (QFramework的事件总线)**:
    *   `QuestSystem` 订阅各种事件（`ResourceChangedEvent`, `TechnologyCompletedEvent` 等）以跟踪目标进度。
    *   `QuestSystem` 发送其自身的事件（例如在 `GameEvents.cs` 中定义的 `QuestActivatedEvent`, `QuestObjectiveCompletedEvent`, `QuestSucceededEvent`），UI或其他系统可以监听这些事件。
*   **`ResourceModel`**:
    *   `QuestSystem` 从 `ResourceModel` 读取资源数量以初始化和更新 `CollectResource` 目标。
    *   任务奖励可以向 `ResourceModel` 添加资源。
*   **`ResearchModel`**:
    *   `QuestSystem` 检查 `ResearchModel` 中的 `Technology.Status` 以用于 `ResearchTech` 目标和前置条件。
    *   任务奖励可以解锁技术（概念上，如果其他前置条件已满足，则使其 `Available`）。
*   **`ExplorationModel`**:
    *   `QuestSystem` 检查 `ExplorePOI` 目标的POI探索状态。
    *   任务奖励可以解锁POI（将其状态更改为 `Scouted`）。
*   **`WorkstationModel`**:
    *   `QuestSystem` 跟踪 `BuildWorkstation` 目标（可能通过监听 `WorkstationBuiltEvent`）。
*   **`GameDataModel`**:
    *   `QuestSystem` 检查 `GameDataModel` 中的 `CurrentDay` 以用于基于天数的前置条件。
*   **UI 系统 (例如 `QuestLogDisplay.cs`)**:
    *   显示活动任务及其目标。
    *   通过监听 `QuestSystem` 发送的特定于任务的事件，通知玩家任务激活、进度和完成情况。

## 10. UI 系统 (UI System)

### 概述 (Overview)

UI（用户界面）系统负责向玩家呈现游戏状态信息并允许玩家交互（尽管交互部分在提供的脚本中细节较少，但显示部分很明显）。它可视化来自各种游戏模型和系统的数据，确保玩家了解日期/时间、资源、事件、研究、任务、幸存者状态等信息。

### 通用方法 (General Approach)

UI脚本通常是附加到UI GameObject（如Text、Button、Panel等）的Unity MonoBehaviour。它们遵循以下模式：

1.  **引用UI元素**: 声明公共变量（例如 `public Text dayText;`）并在Unity检查器中分配。
2.  **访问游戏系统/模型**: 在其 `Start()` 或 `Awake()` 方法中，UI脚本从 `GameArchitecture.Interface` 检索必要模型或系统的实例。
3.  **显示数据**:
    *   **轮询/直接更新**: 许多UI脚本在其 `Update()` 方法中通过持续轮询模型数据来更新其显示的文本或状态（例如，`ResourceDisplay` 每帧从 `ResourceModel` 获取数量）。
    *   **事件驱动更新 (隐含/推荐)**: 虽然在提供的片段中并非所有显示都明确实现了此方法，但更优化的方法是UI组件监听特定事件（例如，`ResourceDisplay` 的 `ResourceChangedEvent`）并在相关数据更改时才更新。`CombatLogDisplay` 使用 `Application.logMessageReceived`，这是一种事件监听形式。`ExplorationDisplay` 提到事件驱动更新对于远征结果是更好的方法。
4.  **处理玩家输入 (基础)**: 一些UI脚本，如 `ExplorationDisplay`，包含 `Button` 引用和 `InputField` 引用以捕获玩家输入并触发游戏动作（例如，开始远征）。

### 关键类参与 (示例) (Key Classes Involved (Examples))

*   **`DayDisplay.cs`**:
    *   **用途**: 显示当前游戏日、一天中的时间（百分比）和基地生命值。还显示“游戏结束”或“胜利”状态。
    *   **关键UI元素**: `dayText` (Text), `timeText` (Text), `baseHealthText` (Text)。
    *   **交互**:
        *   检索 `GameDataModel` 以获取 `CurrentDay` 和 `BaseHealth`。
        *   检索 `DayNightSystem` 以获取 `TimeOfDayNormalized`。
        *   在其 `Update()` 方法中根据这些值更新文本字段。
*   **`ResourceDisplay.cs`**:
    *   **用途**: 显示各种游戏资源的当前数量。
    *   **关键UI元素**: `foodText`, `powerText`, `ammoText`, `medicineText`, `researchPointsText` (均为 Text)。
    *   **交互**:
        *   检索 `ResourceModel`。
        *   在其 `Update()` 方法中通过为每个 `GameResourceType` 调用 `resourceModel.GetAmount()` 来更新文本字段。
*   **`CombatLogDisplay.cs`**:
    *   **用途**: 显示与战斗相关的过滤日志消息。
    *   **关键UI元素**: `combatLogText` (Text)。
    *   **交互**:
        *   订阅 `Application.logMessageReceived`。
        *   根据关键字（例如“Zombie”、“attacks”、“ammo”）和类型（Log或Warning）过滤日志消息。
        *   维护最近 `maxLogLines` 条消息的队列 (`mLogMessages`)。
        *   在收到相关新消息时更新 `combatLogText`。
*   **`EventDisplay.cs`**:
    *   **用途**: 显示有关当前活动随机事件的信息。
    *   **关键UI元素**: `eventText` (Text)。
    *   **交互**:
        *   检索 `EventModel`。
        *   在其 `Update()` 方法中，检查 `mEventModel.CurrentEvent`。
        *   如果事件处于活动状态，则显示其 `Title` 和 `Description`。
        *   包含在 `mDisplayDuration` 之后或如果 `CurrentEvent` 变为null时清除事件消息的逻辑。
*   **`ExplorationDisplay.cs`**:
    *   **用途**: 显示可用的POI、活动远征、上次远征的结果，并提供输入字段/按钮以开始新的远征。
    *   **关键UI元素**: `availablePOIsText`, `activeExpeditionsText`, `expeditionReportText` (均为 Text), `poiIdInput` (InputField), `survivorIdsInput` (InputField), `startExpeditionButton` (Button)。
    *   **交互**:
        *   检索 `ExplorationModel`, `ExplorationSystem`, 和 `SurvivorModel`。
        *   `RefreshDisplay()` (在 `Update()` 中定期调用):
            *   从 `ExplorationModel` 获取POI数据以列出可用的POI。
            *   从 `ExplorationModel` 获取活动远征数据，并从 `SurvivorModel` 获取幸存者名称。
            *   格式化并显示此信息。
            *   显示 `s_lastOutcome`（一个静态变量，存储上次完成远征的结果，由 `ExplorationSystem` 通过 `DisplayOutcome` 静态方法设置）。
        *   `TryStartExpeditionFromInput()`:
            *   读取 `poiIdInput` 和 `survivorIdsInput`。
            *   调用 `mExplorationSystem.CanStartExpeditionToPOI` 和 `mExplorationSystem.StartExpedition`。
            *   使用成功或错误消息更新 `expeditionReportText`。
*   **其他UI脚本 (通用模式)**:
    *   **`QuestLogDisplay.cs`**: 将与 `QuestModel` 交互以显示活动任务及其目标，可能基于 `QuestSystem` 的事件进行更新。
    *   **`ResearchDisplay.cs`**: 将与 `ResearchModel` 和 `ResearchSystem` 交互以显示科技树、研究进度，并允许开始新的研究。
    *   **`SurvivorDisplay.cs`**: 将与 `SurvivorModel` 交互以列出幸存者、他们的统计数据、状态和职业。
    *   **`WorkstationDisplay.cs`**: 将与 `WorkstationModel` 交互以显示已建造的工作站、其状态、已分配的幸存者，并可能允许分配。

### 核心功能 (Core Functionalities)

*   **数据呈现**: 显示文本和数字游戏状态信息（日期、资源、生命值、日志、事件描述、POI详细信息、远征状态、任务详细信息等）。
*   **状态指示**: 反映游戏状态，如“游戏结束”、“胜利”或特定的POI/任务/技术状态。
*   **基本输入处理**: 允许玩家输入文本（例如，用于探索的POI ID）并单击按钮以启动操作。
*   **动态更新**: UI元素通过在 `Update()` 中轮询刷新，或者在某些情况下，通过响应游戏事件或来自其他系统的直接调用（如 `ExplorationDisplay.DisplayOutcome`）来刷新。

### 与其他游戏系统的交互 (Interactions with Other Game Systems)

UI系统主要是大多数其他游戏系统数据的*消费者*，并通过玩家输入成为某些系统中动作的*触发器*。

*   **模型 (主要数据源)**: `GameDataModel`, `ResourceModel`, `EventModel`, `ExplorationModel`, `QuestModel`, `ResearchModel`, `SurvivorModel`, `WorkstationModel`, `EnemyModel` (通过日志间接) 都被各种UI组件读取。
*   **系统 (用于动作和一些数据)**:
    *   `DayNightSystem`: 用于规范化的一天中的时间。
    *   `ExplorationSystem`: 开始远征并接收结果数据。
    *   `ResearchSystem`: 开始研究项目。
    *   （可能）`SurvivorManagerSystem`, `WorkstationSystem` 用于诸如分配幸存者、开始休息等动作，尽管这些交互在提供的UI脚本中没有明确详述直接的动作触发器。
*   **`GameArchitecture.Interface`**: UI脚本普遍使用它来访问任何模型或系统。
*   **`Application.logMessageReceived` (Unity 事件)**: `CombatLogDisplay` 使用它来捕获相关的日志条目。
*   **QFramework 事件系统 (隐含用于未来/稳健性)**: 虽然在提供的脚本中直接轮询很常见，但更稳健的UI将严重依赖监听诸如 `ResourceChangedEvent`、`TechnologyCompletedEvent`、`QuestActivatedEvent` 等事件来更有效地触发更新。`ExplorationDisplay` 明确提到这是一种更好的方法。

本文档对现有游戏系统的描述至此结束。
