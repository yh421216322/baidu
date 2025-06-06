# Game Design Document

This document provides a detailed overview of the existing features and systems in the game.

## 1. Core Game Loop & Data

### Overview

The Core Game Loop & Data system forms the backbone of the game, managing the overall game state, initialization, and the progression of time. It orchestrates the updates of other systems and holds central game data like the current day and base health.

### Key Classes Involved

*   **`GameArchitecture.cs`**: Implements the QFramework `Architecture`, serving as a central registry for all game models and systems. It ensures that different parts of the game can access each other in a structured way.
*   **`GameInitializer.cs`**: A MonoBehaviour responsible for initializing the `GameArchitecture` and setting up the initial game state when the game starts. This includes creating default workstations, assigning initial survivors, and setting up critical system parameters.
*   **`GameLoop.cs`**: A MonoBehaviour that drives the main game updates. In its `Update()` method, it calls the update methods of various registered systems (Workstation, Combat, DayNight, Survivor, Research, Exploration), ensuring each system processes its logic every frame.
*   **`GameDataModel.cs`**: An `AbstractModel` (QFramework) that stores fundamental game data. Currently, it holds `CurrentDay` (integer representing the number of days survived) and `BaseHealth` (float representing the health of the player's base).
*   **`DayNightSystem.cs`**: An `AbstractSystem` (QFramework) that manages the passage of time within the game, specifically the day/night cycle. It tracks the current time of day and triggers new day events.

### Core Functionalities

*   **Initialization**:
    *   `GameArchitecture` registers all necessary models and systems (e.g., `ResourceModel`, `SurvivorModel`, `CombatSystem`, `DayNightSystem`) during its `Init()` phase.
    *   `GameInitializer` ensures the `GameArchitecture` is set up. It then performs specific initial setup tasks, such as:
        *   Building default workstations (Farm, PowerPlant, Workshop, ResearchLab) if they don't exist.
        *   Assigning available survivors to some of these workstations.
        *   Setting the base position for the `CombatSystem`.
*   **Game Loop Update**:
    *   `GameLoop.cs` in its `Update()` method, systematically calls the `Update` methods of various core systems:
        *   `WorkstationSystem.UpdateAllWorkstations(deltaTime)`: Updates the production and status of all workstations.
        *   `CombatSystem.UpdateCombat(deltaTime)`: Manages ongoing combat scenarios.
        *   `DayNightSystem.UpdateDayCycle(deltaTime)`: Advances the in-game time and handles day transitions.
        *   `SurvivorManagerSystem.UpdateSurvivorNeeds(deltaTime)`: Updates survivor status, hunger, thirst, etc.
        *   `ResearchSystem.UpdateResearchProcess(deltaTime)`: Advances ongoing research projects.
        *   `ExplorationSystem.UpdateActiveExpeditions(deltaTime)`: Updates the progress of active expeditions.
*   **Time Progression (Day/Night Cycle)**:
    *   `DayNightSystem` tracks `mCurrentTimeInDay` against `SecondsPerDay`.
    *   When `mCurrentTimeInDay` exceeds `SecondsPerDay`, a new day begins:
        *   `GameDataModel.CurrentDay` is incremented.
        *   A `DayChangedEvent` is sent.
        *   `TriggerNewDayEvents()` is called, which in turn:
            *   Instructs `CombatSystem` to `SpawnZombieWaveForDay(day)`.
            *   Instructs `GEventSystem` to `TryTriggerRandomEvent()`.
    *   The game has a win condition if `CurrentDay` exceeds 100.
    *   The game loop stops if `BaseHealth` in `GameDataModel` drops to 0 or below.
*   **Central Data Management**:
    *   `GameDataModel` provides a central place to store and access `CurrentDay` and `BaseHealth`. Other systems can query this model for essential game state information.

### Interactions with Other Game Systems

*   **All Systems**: `GameArchitecture` acts as a service locator, allowing any system or model to retrieve instances of other registered systems and models.
*   **`GameLoop` interacts with**:
    *   `WorkstationSystem`: To update workstation states.
    *   `CombatSystem`: To update combat states.
    *   `DayNightSystem`: To update the game's time.
    *   `SurvivorManagerSystem`: To update survivor needs.
    *   `ResearchSystem`: To update research progress.
    *   `ExplorationSystem`: To update expedition progress.
*   **`DayNightSystem` interacts with**:
    *   `GameDataModel`: To get and set `CurrentDay`, and to check `BaseHealth` to determine if the game loop should continue.
    *   `CombatSystem`: To trigger zombie waves at the start of each new day.
    *   `GEventSystem` (via `GameArchitecture`): To trigger random events at the start of each new day.
*   **`GameInitializer` interacts with (among others)**:
    *   `WorkstationSystem`: To create initial workstations.
    *   `SurvivorModel`: To get available survivors for initial assignment.
    *   `CombatSystem`: To set initial parameters like base position.

## 2. Combat System

### Overview

The Combat System is responsible for managing all aspects of combat between player-controlled entities (indirectly, via base defense) and hostile entities (zombies). This includes spawning enemies, handling their behavior, processing attacks, and determining the outcome of combat encounters, primarily focused on base defense.

### Key Classes Involved

*   **`CombatSystem.cs`**: An `AbstractSystem` that orchestrates combat. It handles zombie spawning, updates zombie actions, and manages survivor attacks against zombies.
*   **`EnemyModel.cs`**: An `AbstractModel` that stores and manages the list of active zombies (`mZombies`). It provides methods to add, remove, and retrieve zombie data.
*   **`Zombie.cs`**: A class representing an individual zombie enemy. It contains the zombie's `Stats`, `Position`, `TargetPosition` (the base), and methods for movement (`Move`), attacking (`AttackTarget`), and receiving damage (`TakeDamage`). Each zombie has a unique `Id`.
*   **`ZombieStats.cs`**: A data class holding the statistics for a zombie, including `MaxHealth`, `CurrentHealth`, `AttackPower`, and `MovementSpeed`.

### Core Functionalities

*   **Zombie Spawning (`SpawnZombieWaveForDay`)**:
    *   Triggered by `DayNightSystem` at the start of each new day.
    *   The number of zombies to spawn (`zombiesToSpawn`) increases with the `day` (`baseZombieCount + (day / 2)`).
    *   Zombie stats (`healthMultiplier`, `attackMultiplier`) also scale with the `day`, making them progressively tougher.
    *   Zombies are spawned at `spawnAreaCenter`, which moves further away from the base (`new Vector2(10 + day * 0.5f, 0)`) as days progress. The `spawnRadius` also increases.
    *   New `Zombie` objects are created with randomized base stats (within a range) modified by the day's multipliers, and added to the `EnemyModel`.
*   **Zombie Behavior (`UpdateCombat` and `Zombie.Move`, `Zombie.AttackTarget`)**:
    *   In `CombatSystem.UpdateCombat()`:
        *   Iterates through all zombies in `EnemyModel`.
        *   If a zombie is not dead, its `Move()` method is called.
            *   `Zombie.Move()`: Updates the zombie's `Position` by moving it towards its `TargetPosition` (the base) based on its `MovementSpeed` and `deltaTime`.
        *   If a zombie is within `ZombieAttackRange` of the `BasePosition`:
            *   The zombie performs an attack (`zombie.AttackTarget()`, which logs the attack).
            *   The `GameDataModel.BaseHealth` is reduced by the zombie's `Stats.AttackPower`.
            *   If `BaseHealth` drops to 0 or below, a game over condition is logged.
*   **Survivor (Base) Defense (`UpdateCombat`)**:
    *   A cooldown (`mSurvivorAttackCooldown`) manages the frequency of survivor attacks.
    *   If the cooldown has passed, there are available defenders (`SurvivorModel.GetAllSurvivors()` who are `Idle` or `Soldier` profession), and living zombies exist:
        *   Checks if there is enough `AmmoType` resource (`mResourceModel.HasEnough`).
        *   If ammo is sufficient, it's consumed (`mResourceModel.ConsumeResource`).
        *   `mTimeSinceLastSurvivorAttack` is reset.
        *   Each available defender targets the `FindClosestZombie()` that is not dead and is within `SurvivorAttackRange`.
            *   `BaseSurvivorAttackPower` is used, potentially modified by `SurvivorAttackPowerMultiplier`.
            *   Survivors with the `Soldier` profession get a 1.5x damage multiplier on top of the current attack power.
            *   The target zombie's `TakeDamage()` method is called.
*   **Damage and Death (`Zombie.TakeDamage`, `CombatSystem.UpdateCombat`)**:
    *   `Zombie.TakeDamage(amount)`: Reduces `CurrentHealth` by `amount`. If `CurrentHealth` drops to 0 or below, the zombie is considered dead (`IsDead` becomes true).
    *   In `CombatSystem.UpdateCombat()`, zombies that `IsDead` are added to a `zombiesToRemove` list and then removed from the `EnemyModel`.
*   **Targeting (`FindClosestZombie`)**:
    *   Iterates through all zombies in `EnemyModel` that are not dead.
    *   Returns the zombie with the minimum distance to the given `position` (typically `BasePosition`).

### Interactions with Other Game Systems

*   **`DayNightSystem`**:
    *   Calls `CombatSystem.SpawnZombieWaveForDay()` at the start of each new day.
*   **`GameDataModel`**:
    *   `CombatSystem` reads `BaseHealth` to check for game over conditions.
    *   `CombatSystem` writes to `BaseHealth` when zombies attack the base.
*   **`SurvivorModel`**:
    *   `CombatSystem` reads the list of all survivors and their status/profession to determine available defenders.
*   **`ResourceModel`**:
    *   `CombatSystem` checks for and consumes `AmmoType` resources when survivors attack.
*   **`EnemyModel`**:
    *   `CombatSystem` adds new zombies to `EnemyModel` during spawning.
    *   `CombatSystem` reads zombie data from `EnemyModel` for updates and targeting.
    *   `CombatSystem` removes dead zombies from `EnemyModel`.

## 3. Survivor Management System

### Overview

The Survivor Management System is responsible for the creation, tracking, and well-being of all survivors in the game. This includes managing their attributes, professions, current status (idle, working, resting, etc.), and their basic needs like food and rest.

### Key Classes Involved

*   **`SurvivorManagerSystem.cs`**: An `AbstractSystem` that handles the logic for survivor needs, status changes, and actions like eating or resting.
*   **`SurvivorModel.cs`**: An `AbstractModel` that stores and manages the list of all survivors (`mSurvivors`). It provides methods to add, retrieve, and query survivors (e.g., get available survivors). It initializes with a couple of default survivors ("Bob" and "Alice").
*   **`Survivor.cs`**: A class representing an individual survivor. It holds:
    *   `Id` (Guid), `Name` (string)
    *   `Attributes` (`SurvivorAttributes`)
    *   `Profession` (`SurvivorProfession`)
    *   `Status` (`SurvivorStatus`)
    *   `WorkstationId` (Guid?): ID of the workstation they are assigned to, if any.
    *   `FoodLevel` (float, default 100, max 100)
    *   `RestLevel` (float, default 100, max 100)
*   **`SurvivorAttributes.cs`**: A data class holding a survivor's core attributes: `Strength`, `Dexterity`, and `Intelligence`.
*   **`SurvivorProfession.cs`**: An enum defining possible survivor professions (`Unassigned`, `Doctor`, `Engineer`, `Soldier`, `Farmer`).
*   **`SurvivorStatus.cs`**: An enum defining the possible states a survivor can be in (`Idle`, `Working`, `Resting`, `Injured`, `NeedsAttention`, `OnExpedition`).

### Core Functionalities

*   **Survivor Creation (`CreateNewSurvivor`)**:
    *   Takes a `name`, `attributes`, and `profession` as input.
    *   Creates a new `Survivor` object with a unique `Id`.
    *   Adds the new survivor to the `SurvivorModel`.
*   **Survivor Needs Management (`UpdateSurvivorNeeds`)**:
    *   Called every frame by the `GameLoop`.
    *   Iterates through all survivors in `SurvivorModel`.
    *   **Food Consumption**: Decreases `FoodLevel` based on `mFoodConsumptionRate` and `deltaTime`. `FoodLevel` cannot go below 0.
    *   **Rest Management**:
        *   If `Status` is `Working`, `RestLevel` decreases based on `mRestDecreaseRateWorking`.
        *   If `Status` is `Idle` or `NeedsAttention`, `RestLevel` decreases based on `mRestDecreaseRateIdle`. (Injured survivors also currently use this rate).
        *   If `Status` is `Resting`, `RestLevel` increases based on `mRestIncreaseRate` up to `MaxRestLevel`.
        *   `RestLevel` cannot go below 0.
    *   **Needs Attention Status**:
        *   If a survivor's `FoodLevel` or `RestLevel` reaches 0, their `Status` is set to `NeedsAttention`, provided they are not already in a non-interruptible state (like `Injured` or `Resting`).
        *   A warning is logged.
        *   If a survivor's needs are met (e.g., `FoodLevel > 10` and `RestLevel > 10`) while they are in `NeedsAttention` status, their `Status` is changed back to `Idle`.
*   **Survivor Actions**:
    *   **Eating (`SurvivorTryEat`)**:
        *   Takes a `survivorId` and `foodToEat` amount.
        *   Checks if the specified survivor exists and if there is enough `Food` resource in `ResourceModel`.
        *   If successful, consumes the `Food` resource and increases the survivor's `FoodLevel` (1 unit of food resource provides 5 food points), capped at `MaxFoodLevel`.
    *   **Resting (`SurvivorSetResting`)**:
        *   Takes a `survivorId` and a boolean `isResting`.
        *   If `isResting` is true, sets the survivor's `Status` to `Resting`.
        *   If `isResting` is false, sets the survivor's `Status` to `Idle`. This allows `UpdateSurvivorNeeds` to potentially set them to `NeedsAttention` if their levels are critical.
        *   Survivors who are `Injured` might have restrictions on resting (currently allowed).
*   **Querying Survivors**:
    *   `SurvivorModel.GetSurvivorById(id)`: Retrieves a specific survivor.
    *   `SurvivorModel.GetAllSurvivors()`: Returns a list of all survivors.
    *   `SurvivorModel.GetAvailableSurvivors()`: Returns a list of survivors whose `Status` is `Idle`.

### Interactions with Other Game Systems

*   **`GameLoop`**:
    *   Calls `SurvivorManagerSystem.UpdateSurvivorNeeds(deltaTime)` every frame.
*   **`ResourceModel`**:
    *   `SurvivorManagerSystem` reads from `ResourceModel` to check for available `Food`.
    *   `SurvivorManagerSystem` calls `ConsumeResource` on `ResourceModel` when survivors eat.
*   **`WorkstationSystem`**:
    *   (Implicit) `WorkstationSystem` likely assigns survivors to workstations, changing their `Status` to `Working` and setting their `WorkstationId`.
    *   `SurvivorManagerSystem` checks the `WorkstationId` when determining if a survivor in `NeedsAttention` status should be pulled from work (though current implementation notes this as a complex interaction point).
    *   `WorkstationSystem` might need to check a survivor's `Status` to ensure they are capable of working (e.g., not `NeedsAttention`, `Injured`, or `Resting`).
*   **`CombatSystem`**:
    *   `CombatSystem` queries `SurvivorModel` for available defenders (survivors who are `Idle` or `Soldier`).
*   **`ExplorationSystem`**:
    *   Survivors participating in expeditions will have their `Status` set to `OnExpedition`. `SurvivorManagerSystem` needs to account for this status, likely by not applying standard food/rest decay or by having the `ExplorationSystem` manage it. (The `OnExpedition` status is present in `SurvivorStatus.cs` but its handling in `UpdateSurvivorNeeds` is not explicitly detailed in the provided `SurvivorManagerSystem.cs`).

## 4. Resource Management System

### Overview

The Resource Management System is responsible for tracking and managing all game resources. It provides functionalities to add, consume, and query the amounts of various resources available to the player.

### Key Classes Involved

*   **`ResourceManagerSystem.cs`**: An `AbstractSystem`. In the current provided code, this system is very minimal and does not have explicit functionalities beyond its `OnInit` method. The core logic for resource manipulation resides primarily in the `ResourceModel`.
*   **`ResourceModel.cs`**: An `AbstractModel` that contains the actual storage and logic for managing resources. It uses a `ResourceStorage` object internally.
    *   **`ResourceStorage` (internal class/structure, inferred from `ResourceModel.cs`)**: This class (though not explicitly shown as a separate file) is instantiated within `ResourceModel` and is responsible for directly holding the quantities of different resources, likely in a dictionary or similar data structure mapping `GameResourceType` to an integer amount.
*   **`GameResourceType.cs`**: An enum defining all types of resources available in the game (e.g., `Food`, `Power`, `Ammo`, `Medicine`, `ResearchPoints`, `ElectronicParts`).

### Core Functionalities

*   **Resource Initialization**:
    *   When `ResourceModel` is initialized (`OnInit`), it creates an instance of `ResourceStorage`.
    *   The `ResourceStorage` likely initializes all known `GameResourceType` values to a starting amount (e.g., 0 or some default).
*   **Adding Resources (`ResourceModel.AddResource`)**:
    *   Takes a `GameResourceType` and an `amount` to add.
    *   Does nothing if `amount` is zero or negative.
    *   Calls the internal `resourceStorage.AddResource(type, amount)` to update the stored quantity.
    *   Sends a `ResourceChangedEvent` with the resource type, the new total amount, and the amount added.
*   **Consuming Resources (`ResourceModel.ConsumeResource`)**:
    *   Takes a `GameResourceType`, an `amount` to consume, and an optional `allowForceConsume` boolean.
    *   Does nothing if `amount` is zero or negative.
    *   Calls the internal `resourceStorage.ConsumeResource(type, amount, allowForceConsume)`.
        *   The `ResourceStorage.ConsumeResource` method (inferred) would check if there are enough resources. If not, and `allowForceConsume` is false, the consumption fails. If `allowForceConsume` is true, it might allow the resource count to go negative or simply consume what's available.
    *   If the consumption is successful (returns true from `resourceStorage.ConsumeResource`):
        *   Calculates the `amountActuallyConsumed`.
        *   If `amountActuallyConsumed` is greater than 0, sends a `ResourceChangedEvent` with the resource type, the new total amount, and the negative of `amountActuallyConsumed` (to indicate consumption).
*   **Querying Resource Amounts (`ResourceModel.GetAmount`)**:
    *   Takes a `GameResourceType`.
    *   Returns the current stored quantity of that resource by calling `resourceStorage.GetAmount(type)`.
*   **Checking Resource Availability (`ResourceModel.HasEnough`)**:
    *   Takes a `GameResourceType` and an `amount`.
    *   Returns true if the stored quantity of the resource is greater than or equal to the specified `amount`, by calling `resourceStorage.HasEnough(type, amount)`.

### Interactions with Other Game Systems

The `ResourceModel` is a central point for resource information and is interacted with by many other systems:

*   **`WorkstationSystem`**:
    *   Likely adds resources to `ResourceModel` when workstations complete production cycles (e.g., Farms producing `Food`).
    *   May consume resources from `ResourceModel` if workstations require input materials for production.
*   **`CombatSystem`**:
    *   Consumes `Ammo` from `ResourceModel` when survivors attack zombies.
*   **`SurvivorManagerSystem`**:
    *   Consumes `Food` from `ResourceModel` when `SurvivorTryEat` is called.
*   **`ResearchSystem`**:
    *   May consume resources (e.g., `ResearchPoints`, `ElectronicParts`) from `ResourceModel` to start or progress research projects.
    *   May add `ResearchPoints` to `ResourceModel` via specialized workstations or events.
*   **`ExplorationSystem`**:
    *   May add various resources to `ResourceModel` as rewards from successful expeditions.
    *   May consume resources from `ResourceModel` to initiate expeditions (e.g., supplies).
*   **`EventSystem`**:
    *   Random events could add or remove resources from `ResourceModel` (e.g., `ResourceDiscoveryEvent` adding resources, `FoodSpoilageEvent` removing `Food`).
*   **`QuestSystem`**:
    *   Quest rewards might include adding resources to `ResourceModel`.
    *   Quest objectives might involve gathering or spending specific resources.
*   **UI System (e.g., `ResourceDisplay.cs`)**:
    *   Listens for `ResourceChangedEvent` sent by `ResourceModel`.
    *   Updates the UI to reflect the current amounts of resources by querying `ResourceModel.GetAmount()`.

## 5. Workstation System

### Overview

The Workstation System manages the functionality of all production and utility buildings (workstations) in the game. This includes building new workstations, assigning survivors to them, and processing their production cycles to generate resources or other outputs.

### Key Classes Involved

*   **`WorkstationSystem.cs`**: An `AbstractSystem` that handles the creation of workstations, assignment/unassignment of survivors to workstations, and calls the update logic for all workstations.
*   **`WorkstationModel.cs`**: An `AbstractModel` that stores and manages a list of all existing `Workstation` objects in the game.
*   **`Workstation.cs`**: A class representing an individual workstation. It holds:
    *   `Id` (Guid), `Type` (`WorkstationType`)
    *   `AssignedSurvivorIds` (List<Guid>): List of survivor IDs currently assigned to this workstation.
    *   `ProductionProgress` (float): Current progress towards completing a production cycle.
    *   `BaseProductionRate` (float): A factor determining the base speed of production (e.g., 1.0 for standard speed, 0.5 for half speed).
    *   `ProductionCycleTime` (float): The time (in seconds) required to complete one production cycle if the `BaseProductionRate` were 1.0 and one survivor was assigned. The actual time is `ProductionCycleTime / effectiveRate`.
    *   `OutputResourceType` (`GameResourceType`): The type of resource this workstation produces.
    *   `OutputQuantity` (int): The base amount of resource produced per cycle.
    *   `InputResourceType` (`GameResourceType?`): Optional input resource type required for production.
    *   `InputQuantity` (int): Amount of input resource required if `InputResourceType` is set.
    *   `ProductionBonusMultiplier` (float, default 1.0): A multiplier applied to `OutputQuantity`.
    *   `FlatProductionBonus` (int, default 0): A flat bonus added to `OutputQuantity` after the multiplier.
*   **`WorkstationType.cs`**: An enum defining the different types of workstations available (e.g., `Farm`, `PowerPlant`, `Workshop`, `Clinic`, `ResearchLab`). Each type has pre-defined production parameters (output, input, cycle time, base rate) set in the `Workstation` constructor.

### Core Functionalities

*   **Building Workstations (`BuildWorkstation`)**:
    *   Takes a `WorkstationType`.
    *   Creates a new `Workstation` object of the specified type.
    *   Adds the new workstation to `WorkstationModel`.
    *   Sends a `WorkstationBuiltEvent`.
*   **Assigning Survivors (`AssignSurvivorToWorkstation`)**:
    *   Takes a `survivorId` and `workstationId`.
    *   Retrieves the `Survivor` and `Workstation` objects from their respective models.
    *   Checks if the survivor is `Idle`.
    *   If the survivor was previously assigned to a different workstation, they are unassigned from it first.
    *   Calls `workstation.AssignSurvivor(survivorId)`.
        *   `Workstation.AssignSurvivor()`: Adds the `survivorId` to its `AssignedSurvivorIds` list (future capacity checks could be added here).
    *   Updates the survivor's `WorkstationId` and sets their `Status` to `Working`.
*   **Unassigning Survivors (Implicit and `Workstation.UnassignSurvivor`)**:
    *   `Workstation.UnassignSurvivor(survivorId)`: Removes the `survivorId` from `AssignedSurvivorIds`. This is called when a survivor is reassigned or potentially when they become unable to work.
*   **Production Update (`UpdateAllWorkstations` and `Workstation.UpdateProduction`)**:
    *   `WorkstationSystem.UpdateAllWorkstations()`: Called by `GameLoop` every frame. Iterates through all workstations in `WorkstationModel` and calls their `UpdateProduction` method.
    *   `Workstation.UpdateProduction(deltaTime, survivorModel, resourceModel)`:
        *   If no survivors are assigned (`AssignedSurvivorIds.Count == 0`), production typically halts or `ProductionProgress` might be reset (currently, it just returns).
        *   Calculates `effectiveRate` using `GetCurrentProductionRatePerSecond(survivorModel)`.
            *   `Workstation.GetCurrentProductionRatePerSecond()`: Currently returns `BaseProductionRate` if any survivors are assigned, otherwise 0. This is where survivor skills or multiple survivor bonuses could be factored in.
        *   Increments `ProductionProgress` by `effectiveRate * deltaTime`.
        *   If `ProductionProgress` >= `ProductionCycleTime`:
            *   Calculates how many full `cyclesCompleted` occurred.
            *   Reduces `ProductionProgress` by `cyclesCompleted * ProductionCycleTime` (carries over excess progress).
            *   For each completed cycle:
                *   If `InputResourceType` is set, attempts to consume `InputQuantity` from `resourceModel`. If consumption fails, production for that cycle (and subsequent ones in this update) is halted.
                *   If input is met (or not required), calculates `finalOutputQuantity` using `OutputQuantity`, `ProductionBonusMultiplier`, and `FlatProductionBonus`. Ensures output is at least 1 unless base is 0.
                *   Adds `finalOutputQuantity` of `OutputResourceType` to `resourceModel`.
                *   Logs the production.

### Pre-defined Workstation Types and Parameters (Examples from `Workstation.cs` constructor)

*   **Farm**: Produces 5 `Food`. Cycle: 10s. Rate: 1.0.
*   **PowerPlant**: Produces 10 `Power`. Cycle: 12s. Rate: 1.0.
*   **Workshop**: Produces 2 `Ammo`. Consumes 1 `Power`. Cycle: 15s. Rate: 0.5 (effectively 30s).
*   **Clinic**: Produces 1 `Medicine`. Consumes 2 `Food`. Cycle: 20s. Rate: 1.0.
*   **ResearchLab**: Produces 1 `ResearchPoints`. Consumes 1 `Power`. Cycle: 20s. Rate: 1.0.

### Interactions with Other Game Systems

*   **`GameLoop`**:
    *   Calls `WorkstationSystem.UpdateAllWorkstations(deltaTime)` every frame.
*   **`SurvivorModel` & `SurvivorManagerSystem`**:
    *   `WorkstationSystem` reads survivor status from `SurvivorModel` when assigning.
    *   `WorkstationSystem` updates survivor status to `Working` and sets `WorkstationId` on the `Survivor` object (managed by `SurvivorModel`).
    *   `Workstation.GetCurrentProductionRatePerSecond()` takes `SurvivorModel` as a parameter, indicating future potential to use survivor data (skills, count) to modify production rates.
*   **`ResourceModel`**:
    *   `Workstation.UpdateProduction()` adds output resources to `ResourceModel`.
    *   `Workstation.UpdateProduction()` consumes input resources from `ResourceModel`.
*   **`GameInitializer`**:
    *   Calls `WorkstationSystem.BuildWorkstation()` to create default workstations at the start of the game.
*   **`GEventSystem`**:
    *   `WorkstationSystem.BuildWorkstation()` sends a `WorkstationBuiltEvent`. This could be used by UI or other systems to react to new workstations.
*   **`ResearchSystem`**:
    *   Technology effects (from `ResearchSystem`) might modify `Workstation` parameters like `ProductionBonusMultiplier` or `FlatProductionBonus`. (This interaction is not explicitly coded in the provided snippets but is a common pattern).

## 6. Research System

### Overview

The Research System allows players to unlock new abilities, bonuses, and game elements (like workstations) by investing research points and time into technologies. It manages the availability, progress, and application of various technologies.

### Key Classes Involved

*   **`ResearchSystem.cs`**: An `AbstractSystem` that manages the active research process. It handles starting, updating, and completing research projects. It also applies the effects of completed technologies.
*   **`ResearchModel.cs`**: An `AbstractModel` that stores all defined technologies (`mAllTechnologies`) and their current statuses. It provides methods to query technologies and update their statuses based on prerequisites and completions. It includes a `PopulateInitialTechnologies()` method to define the initial tech tree.
*   **`Technology.cs`**: A class representing a single researchable technology. It holds:
    *   `Id` (string), `Name` (string), `Description` (string)
    *   `ResearchPointCost` (int): The number of research points required to *start* the research, and also implicitly the "effort" or "time" needed to complete it after starting.
    *   `PrerequisiteTechIds` (List<string>): A list of technology IDs that must be completed before this technology becomes available.
    *   `Effects` (List<`TechnologyEffectData`>): A list of effects that are applied to the game when this technology is completed.
    *   `Status` (`ResearchStatus`): The current state of the technology (`Locked`, `Available`, `InProgress`, `Completed`).
*   **`TechnologyEffectData.cs`**: A class defining a single effect of a technology. It includes:
    *   `EffectType` (`TechnologyEffectType`): The type of effect (e.g., increase production, modify survivor stat, unlock workstation).
    *   `Value` (float): The magnitude of the effect.
    *   `TargetResource` (`GameResourceType`): Optional target resource for the effect.
    *   `TargetWorkstationType` (`WorkstationType`): Optional target workstation type for the effect.
*   **`ResearchStatus.cs` (enum within `Technology.cs`)**: An enum defining the possible states of a technology: `Locked`, `Available`, `InProgress`, `Completed`.
*   **`TechnologyEffectType.cs` (enum within `TechnologyEffectData.cs`)**: An enum defining the types of effects a technology can have (e.g., `IncreaseProductionOutput`, `IncreaseProductionMultiplier`, `ModifySurvivorStat`, `UnlockWorkstation`).

### Core Functionalities

*   **Technology Definition & Initialization (`ResearchModel.PopulateInitialTechnologies`)**:
    *   A predefined set of technologies is created and stored in `ResearchModel.mAllTechnologies`. Each technology includes its ID, name, description, cost, prerequisites, and effects.
    *   Examples: "TECH_FARM_1" (improves farm output), "TECH_AMMO_1" (improves ammo workshop output), "TECH_UNLOCK_LAB" (unlocks Research Lab workstation), "TECH_BALLISTICS_1" (increases survivor attack power).
*   **Status Updates (`ResearchModel.UpdateAllTechnologyStatuses`, `ResearchModel.UpdateTechnologyStatus`)**:
    *   `UpdateAllTechnologyStatuses()`: Iterates through all technologies. If a technology is `Locked` and all its `PrerequisiteTechIds` have been `Completed`, its status is changed to `Available`. Notifies `ResearchSystem` if status changes.
    *   `UpdateTechnologyStatus(techId, newStatus)`: Directly sets the status of a specific technology.
*   **Starting Research (`ResearchSystem.StartResearch`)**:
    *   Takes a `techId`.
    *   Checks if another research is already in progress.
    *   Validates if the technology exists and is `Available`.
    *   Consumes the `ResearchPointCost` from `ResourceModel`.
    *   If all checks pass, sets `mCurrentResearch` to the selected technology, resets `mCurrentResearchTimeAccumulated`, and updates the technology's status to `InProgress`.
*   **Research Progress (`ResearchSystem.UpdateResearchProcess`)**:
    *   Called by `GameLoop` every frame.
    *   If `mCurrentResearch` is not null, increments `mCurrentResearchTimeAccumulated` by `deltaTime`. (Note: The provided code has `deltaTime + 50`, which seems like a bug and would make research complete extremely fast. It should likely be just `deltaTime` or `deltaTime` scaled by some efficiency factor).
    *   If `mCurrentResearchTimeAccumulated` >= `mCurrentResearch.ResearchPointCost` (meaning the "effort" or "time" matches the initial point cost), calls `CompleteResearch`.
*   **Completing Research (`ResearchSystem.CompleteResearch`)**:
    *   Sets the completed technology's status to `Completed` in `ResearchModel`.
    *   Iterates through the `Effects` of the completed technology:
        *   Applies effects based on `TechnologyEffectType`:
            *   `IncreaseProductionMultiplier`: Modifies `ProductionBonusMultiplier` of targeted workstations.
            *   `IncreaseProductionOutput`: Modifies `FlatProductionBonus` of targeted workstations.
            *   `ModifySurvivorStat`: Example shown modifies `SurvivorAttackPowerMultiplier` in `CombatSystem`.
            *   `UnlockWorkstation`: Logs that a workstation type is unlocked (actual unlocking might involve another system or UI update).
        *   This requires `ResearchSystem` to get other models/systems (e.g., `WorkstationModel`, `CombatSystem`).
    *   Resets `mCurrentResearch` and `mCurrentResearchTimeAccumulated`.
    *   Calls `ResearchModel.UpdateAllTechnologyStatuses()` to potentially unlock new technologies.
    *   Sends a `TechnologyCompletedEvent`.
*   **Querying Research State**:
    *   `ResearchModel.GetTechnology(techId)`, `GetAllTechnologies()`, `GetAvailableTechnologies()`, `GetCompletedTechnologies()`, `GetInProgressTechnologies()`.
    *   `ResearchSystem.GetCurrentResearch()`, `GetCurrentResearchProgressNormalized()`, `IsResearching()`.

### Interactions with Other Game Systems

*   **`GameLoop`**:
    *   Calls `ResearchSystem.UpdateResearchProcess(deltaTime)` every frame.
*   **`ResourceModel`**:
    *   `ResearchSystem` consumes `ResearchPoints` from `ResourceModel` when starting research.
    *   Workstations (like `ResearchLab`) produce `ResearchPoints` and add them to `ResourceModel`.
*   **`WorkstationModel` & `WorkstationSystem`**:
    *   Technology effects can modify `ProductionBonusMultiplier` and `FlatProductionBonus` of `Workstation` instances managed by `WorkstationModel`.
    *   `UnlockWorkstation` effects change the availability of workstation types for building (likely handled by UI listening to an event or `WorkstationSystem` checking unlocked techs).
    *   `ResearchLab` workstation is essential for generating `ResearchPoints`.
*   **`CombatSystem`**:
    *   Technology effects can modify combat-related parameters, such as `SurvivorAttackPowerMultiplier` in `CombatSystem`.
*   **`GEventSystem`**:
    *   `ResearchSystem` sends `TechnologyCompletedEvent` when research is finished. This can be used by the `QuestSystem` or UI.
    *   `ResearchSystem` can trigger `OnTechnologyStatusChanged` events via `ResearchModel` to update UI elements like the tech tree.
*   **UI System (e.g., `ResearchDisplay.cs`)**:
    *   Displays the technology tree, available research, progress of current research.
    *   Allows the player to select and start research.
    *   Updates based on events like `TechnologyCompletedEvent` or `OnTechnologyStatusChanged`.

## 7. Exploration System

### Overview

The Exploration System allows players to send survivors on expeditions to various Points of Interest (POIs) to find resources, trigger events, or discover new information. It manages the discovery of POIs, the process of expeditions (travel, exploration, return), and the resolution of expedition outcomes.

### Key Classes Involved

*   **`ExplorationSystem.cs`**: An `AbstractSystem` responsible for managing active expeditions. It handles starting expeditions, updating their progress through different phases (departing, exploring, returning), and resolving their outcomes.
*   **`ExplorationModel.cs`**: An `AbstractModel` that stores all defined POIs (`mPointsOfInterest`) and currently active expeditions (`mActiveExpeditions`). It includes methods to populate initial POIs, query POIs and expeditions, and update POI statuses.
*   **`Expedition.cs`**: A class representing an active expedition. It holds:
    *   `ExpeditionId` (Guid), `TargetPoiId` (string)
    *   `AssignedSurvivorIds` (List<Guid>)
    *   `TravelTimeToPoi` (float): Time taken to reach the POI.
    *   `ExplorationTimeAtPoi` (float): Time spent exploring the POI.
    *   `TravelTimeBackToBase` (float): Time taken to return from the POI.
    *   `TimeElapsedOnCurrentPhase` (float): Tracks time spent in the current phase.
    *   `Status` (`ExpeditionStatus`): Current state of the expedition (e.g., `Departing`, `Exploring`, `Returning`, `Completed`).
    *   `Outcome` (`ExpeditionOutcome`): Stores the results of the expedition upon completion.
*   **`ExplorationPointOfInterest.cs` (POI)**: A class representing a discoverable location. It holds:
    *   `Id` (string), `Name` (string), `Description` (string)
    *   `Difficulty` (int): Affects risks and potentially rewards.
    *   `BaseExplorationTime` (float): Base time required to explore this POI.
    *   `MaxSurvivorSlots` (int): Maximum number of survivors that can be assigned.
    *   `PotentialRewards` (List<`POIReward`>): A list of possible rewards that can be found.
    *   `Status` (`POIStatus`): Current state of the POI (e.g., `Unexplored`, `BeingExplored`, `Explored`, `Depleted`).
*   **`POIReward.cs`**: A class defining a potential reward from a POI, including `ResourceType`, `MinQuantity`, `MaxQuantity`, and `Probability` of finding it.
*   **`ExpeditionStatus.cs` (enum within `Expedition.cs`)**: Defines the phases of an expedition: `Preparing`, `Departing`, `Exploring`, `Returning`, `Completed`, `Failed`.
*   **`POIStatus.cs` (enum within `ExplorationPointOfInterest.cs`)**: Defines the states of a POI: `Unexplored`, `Scouted`, `BeingExplored`, `Explored`, `Depleted`.
*   **`ExpeditionOutcome.cs` (structure implicitly used by `Expedition.cs`)**: A data structure to store the results of an expedition, including `WasSuccessful` (bool), `NarrativeLog` (string), `ResourcesFound` (Dictionary<`GameResourceType`, int>), and `SurvivorStatusChanges` (List<string>).

### Core Functionalities

*   **POI Definition & Initialization (`ExplorationModel.PopulateInitialPOIs`)**:
    *   A predefined set of POIs is created and stored in `ExplorationModel.mPointsOfInterest`. Each POI includes its ID, name, description, difficulty, base exploration time, survivor slots, and potential rewards.
    *   Examples: "POI_SUPERMARKET_1" (food, medicine), "POI_POLICE_1" (ammo), "POI_LIBRARY_1" (research points), "POI_RADIO_TOWER" (research points, electronic parts).
*   **Starting an Expedition (`ExplorationSystem.StartExpedition`)**:
    *   Validates if an expedition can be started using `CanStartExpeditionToPOI` (checks POI status, survivor availability/status, and slot limits).
    *   If valid, creates a new `Expedition` object.
        *   `actualExplorationTimeAtPoi` is currently set to `poi.BaseExplorationTime` but could be modified by survivor skills in the future.
        *   Default travel times (`TravelTimeToPoi`, `TravelTimeBackToBase`) are currently fixed (e.g., 10s each).
    *   Adds the expedition to `ExplorationModel.mActiveExpeditions`.
    *   Updates the POI's status to `BeingExplored`.
    *   Sets the status of assigned survivors to `OnExpedition`.
*   **Updating Active Expeditions (`ExplorationSystem.UpdateActiveExpeditions`)**:
    *   Called by `GameLoop` every frame.
    *   Iterates through `mActiveExpeditions` in `ExplorationModel`.
    *   Increments `TimeElapsedOnCurrentPhase` for each expedition.
    *   Manages phase transitions based on time elapsed:
        *   `Departing` -> `Exploring` (when `TimeElapsedOnCurrentPhase` >= `TravelTimeToPoi`)
        *   `Exploring` -> `Returning` (when `TimeElapsedOnCurrentPhase` >= `ExplorationTimeAtPoi`)
        *   `Returning` -> `Completed` (when `TimeElapsedOnCurrentPhase` >= `TravelTimeBackToBase`)
    *   When an expedition `Completed`, calls `ResolveExpeditionOutcome`.
    *   Removes completed expeditions from `mActiveExpeditions`.
*   **Resolving Expedition Outcome (`ExplorationSystem.ResolveExpeditionOutcome`)**:
    *   Retrieves the `POI` related to the expedition.
    *   Populates the `ExpeditionOutcome` object:
        *   **Resource Rewards**: Iterates through `poi.PotentialRewards`. For each, rolls against `Probability`. If successful, grants a random amount between `MinQuantity` and `MaxQuantity` of the `ResourceType`. Adds these to `Outcome.ResourcesFound` and to the global `ResourceModel`.
        *   **Survivor Risks**: For each survivor, calculates an `injuryRisk` (currently `baseInjuryChance * poi.Difficulty`). If a random roll is below this risk, the survivor's status is set to `Injured`. Otherwise, set to `Idle`. Changes are logged in `Outcome.SurvivorStatusChanges` and `Outcome.NarrativeLog`.
        *   Sets `Outcome.WasSuccessful` (currently always true if completed).
        *   Updates the POI's status to `Explored` (could become `Depleted` in more complex scenarios).
    *   Logs the detailed outcome.
    *   Notifies the UI (`ExplorationDisplay.DisplayOutcome`).
    *   Sends a `POIExploredEvent` if successful.
*   **Querying Exploration State**:
    *   `ExplorationModel`: `GetPOI(id)`, `GetAllPOIs()`, `GetAvailablePOIs()`, `GetActiveExpeditions()`, `GetExpedition(id)`.

### Interactions with Other Game Systems

*   **`GameLoop`**:
    *   Calls `ExplorationSystem.UpdateActiveExpeditions(deltaTime)` every frame.
*   **`SurvivorModel` & `SurvivorManagerSystem`**:
    *   `ExplorationSystem` reads survivor status from `SurvivorModel` to check availability for expeditions.
    *   `ExplorationSystem` updates survivor status to `OnExpedition` when an expedition starts, and to `Idle` or `Injured` when it resolves. The `SurvivorManagerSystem` needs to correctly handle the `OnExpedition` status (e.g., by pausing needs decay or applying different rules).
*   **`ResourceModel`**:
    *   `ExplorationSystem` adds resources found during expeditions to `ResourceModel`.
*   **`GEventSystem`**:
    *   `ExplorationSystem` sends `POIExploredEvent` upon successful exploration. This can be used by the `QuestSystem` or other systems.
*   **UI System (e.g., `ExplorationDisplay.cs`)**:
    *   Displays available POIs, allows players to assign survivors and start expeditions.
    *   Shows progress of active expeditions.
    *   Displays the `ExpeditionOutcome` when an expedition is completed.
*   **`QuestSystem`**:
    *   Quests might require exploring specific POIs (listening for `POIExploredEvent`).
    *   Discovering new POIs could be a quest objective or trigger new quests.

## 8. Event System

### Overview

The Event System is responsible for introducing random occurrences and managing a global event bus for inter-system communication. Random events add unpredictability and challenges to the game, while the global event bus allows different game systems to react to significant occurrences without direct dependencies.

### Key Classes Involved

*   **`GEventSystem.cs` (referred to as EventSystem in some contexts)**: An `AbstractSystem` (QFramework) that manages the triggering of random events. It holds a list of "event factories" (delegates that create specific random event instances).
*   **`EventModel.cs`**: An `AbstractModel` (QFramework) that primarily stores the `CurrentEvent` (the active random event, if any) so that UI or other systems can access its details for display or processing.
*   **`RandomEvent.cs` (abstract base class, inferred)**: While not explicitly provided as a separate file, the derived event classes like `FoodSpoilageEvent` imply a base class `RandomEvent` which likely defines a common interface or properties, such as `Title`, `Description`, and an `Execute(IArchitecture architecture)` method.
*   **Specific Event Classes (e.g., `FoodSpoilageEvent.cs`, `ResourceDiscoveryEvent.cs`, `SurvivorSicknessEvent.cs`)**: These classes inherit from `RandomEvent`. Each defines a specific random event:
    *   They set their `Title` and `Description` (often dynamically based on execution).
    *   Their `Execute()` method contains the logic for applying the event's effects to the game state by interacting with various models (e.g., `ResourceModel`, `SurvivorModel`) obtained through the `IArchitecture` interface.
*   **`GameEvents.cs`**: This file defines a collection of plain C# classes that serve as event data structures for the global event bus (QFramework's event system). These are not random events but specific notifications that systems can send and listen to. Examples include:
    *   `ResourceChangedEvent`: Sent when a resource amount changes.
    *   `TechnologyCompletedEvent`: Sent when research is completed.
    *   `POIExploredEvent`: Sent when a POI is explored.
    *   `WorkstationBuiltEvent`: Sent when a new workstation is built.
    *   `DayChangedEvent`: Sent when the game day changes.
    *   `CustomQuestFlagEvent`, `QuestActivatedEvent`, `QuestObjectiveCompletedEvent`, `QuestSucceededEvent`: Used by the Quest system.

### Core Functionalities

*   **Random Event Triggering (`GEventSystem.TryTriggerRandomEvent`)**:
    *   Called by `DayNightSystem` at the start of each new day.
    *   Has a chance (e.g., 25%) to trigger a new random event.
    *   If the chance roll succeeds:
        *   Selects a random event factory from `mEventFactories`.
        *   Creates an instance of a `RandomEvent` using the factory.
        *   Calls the `Execute()` method of the chosen `RandomEvent` instance, passing the game's `IArchitecture` so the event can access models and systems.
        *   Stores the executed event instance in `EventModel.CurrentEvent` for potential UI display.
    *   If no new event is triggered, `EventModel.CurrentEvent` is set to `null` to clear any previous event from display.
*   **Random Event Execution (within each specific event class's `Execute` method)**:
    *   **`FoodSpoilageEvent`**:
        *   Accesses `ResourceModel`.
        *   Calculates a random amount of `Food` to lose (e.g., up to 20% of current food, or a minimum range).
        *   Consumes the calculated `Food` amount from `ResourceModel`.
        *   Sets its `Description` based on the outcome.
    *   **`ResourceDiscoveryEvent`**:
        *   Accesses `ResourceModel`.
        *   Randomly selects a `GameResourceType`.
        *   Randomly determines an `amountFound` (e.g., 15-50).
        *   Adds the `amountFound` of the `foundType` to `ResourceModel`.
        *   Sets its `Description`.
    *   **`SurvivorSicknessEvent`**:
        *   Accesses `SurvivorModel`.
        *   Finds a list of healthy survivors (not `Injured` or `NeedsAttention`).
        *   If healthy survivors exist, randomly selects one and changes their `Status` to `Injured`.
        *   Sets its `Description` based on the outcome.
*   **Global Event Bus (QFramework)**:
    *   Systems can send instances of event classes defined in `GameEvents.cs` using `this.SendEvent(new MyEvent(...))`.
    *   Other systems, models, or UI components can register listeners for these specific event types to react accordingly (e.g., UI updating on `ResourceChangedEvent`, Quest system listening for `TechnologyCompletedEvent`). This is part of QFramework's architecture and not explicitly detailed in the provided event system files but is a common usage pattern with such event definitions.

### Interactions with Other Game Systems

*   **`DayNightSystem`**:
    *   Calls `GEventSystem.TryTriggerRandomEvent()` at the start of each new day.
*   **`ResourceModel`**:
    *   Random events like `FoodSpoilageEvent` consume resources from `ResourceModel`.
    *   Random events like `ResourceDiscoveryEvent` add resources to `ResourceModel`.
    *   `ResourceModel` itself sends `ResourceChangedEvent` via the global event bus when resources are added or consumed through its methods.
*   **`SurvivorModel`**:
    *   Random events like `SurvivorSicknessEvent` can change the `Status` of survivors managed by `SurvivorModel`.
*   **UI System (e.g., `EventDisplay.cs`)**:
    *   Likely reads `EventModel.CurrentEvent` to display the title and description of the active random event.
    *   Other UI elements (like `ResourceDisplay.cs`) listen to global events like `ResourceChangedEvent` to update themselves.
*   **Various Systems (as event publishers/subscribers)**:
    *   `WorkstationSystem` sends `WorkstationBuiltEvent`.
    *   `ResearchSystem` sends `TechnologyCompletedEvent`.
    *   `ExplorationSystem` sends `POIExploredEvent`.
    *   `QuestSystem` sends and listens to various quest-related events.
    *   Any system might listen to events relevant to its function (e.g., `QuestSystem` listening to `TechnologyCompletedEvent` or `POIExploredEvent` to complete objectives).

### Note on "GEventSystem" vs "EventSystem"
The class is named `GEventSystem` in `EventSystem.cs`, but `GameArchitecture.cs` registers it as `this.RegisterSystem(new GEventSystem());`. Other systems like `DayNightSystem` get it using `this.GetSystem<GEventSystem>()`. The documentation will use `GEventSystem` for clarity referring to this specific class for random event management, distinct from QFramework's general event bus mechanism.

## 9. Quest System

### Overview

The Quest System manages all aspects of quests, including their activation, tracking objectives, and delivering rewards upon completion. It provides a structured way to guide player progression and introduce narrative elements.

### Key Classes Involved

*   **`QuestSystem.cs`**: An `AbstractSystem` responsible for managing the lifecycle of quests. It listens to game events to update quest objective progress, checks for quest activation conditions, and processes quest completion including reward application.
*   **`QuestModel.cs`**: An `AbstractModel` that stores all defined quests (`AllQuests`) and a list of currently `ActiveQuests`. It includes `PopulateInitialQuests()` to define the game's quests.
*   **`Quest.cs`**: A class representing a single quest. It holds:
    *   `Id` (string), `Title` (string), `Description` (string)
    *   `Status` (`QuestStatus` enum: `Locked`, `Available`, `Active`, `Success`, `Failed`)
    *   `Objectives` (List<`QuestObjective`>)
    *   Prerequisites: `PrerequisiteQuestIds` (List<string>), `RequiredDay` (int), `RequiredTechId` (string)
    *   `Rewards` (List<`QuestReward`>)
*   **`QuestObjective.cs`**: A class defining a single objective within a quest. It includes:
    *   `Description` (string)
    *   `Type` (`ObjectiveType` enum: `CollectResource`, `ResearchTech`, `ExplorePOI`, `BuildWorkstation`, `CustomFlag`)
    *   `TargetId` (string): Identifier for the objective target (e.g., resource name, tech ID).
    *   `RequiredAmount` (int), `CurrentAmount` (int)
    *   `IsComplete` (bool property based on `CurrentAmount` >= `RequiredAmount`)
*   **`QuestReward.cs`**: A class defining a reward for completing a quest. It includes:
    *   `Type` (`QuestRewardType` enum: `Resource`, `Item`, `UnlockTech`, `UnlockPOI`, `SpawnSurvivor`)
    *   `TargetId` (string): Identifier for the reward (e.g., resource name, tech ID).
    *   `Amount` (int): Quantity for resource rewards.
*   **Enums (defined in their respective files or `Quest.cs`)**:
    *   `QuestStatus`: `Locked`, `Available`, `Active`, `Success`, `Failed`.
    *   `ObjectiveType`: `CollectResource`, `ResearchTech`, `ExplorePOI`, `BuildWorkstation`, `CustomFlag`.
    *   `QuestRewardType`: `Resource`, `Item`, `UnlockTech`, `UnlockPOI`, `SpawnSurvivor`.

### Core Functionalities

*   **Quest Definition (`QuestModel.PopulateInitialQuests`)**:
    *   Quests are predefined with their IDs, titles, descriptions, objectives, prerequisites (other quests, day number, technology completed), and rewards.
    *   Example quests: "Q_FOUNDATION" (collect basic resources), "Q_REACH_SKY" (research radio tech, scout radio tower POI), "Q_BEACON_1" (collect electronic parts and power).
*   **Quest Activation (`QuestSystem.CheckForAllQuestActivations`)**:
    *   Called during `OnInit` and after a quest is completed. Also triggered by `DayChangedEvent`.
    *   Iterates through all quests in `QuestModel.AllQuests`.
    *   For quests with `Status == QuestStatus.Locked`:
        *   Checks if all `PrerequisiteQuestIds` have `Status == QuestStatus.Success`.
        *   Checks if `GameDataModel.CurrentDay` >= `quest.RequiredDay`.
        *   Checks if `quest.RequiredTechId` (if any) has been completed (status `ResearchStatus.Completed` in `ResearchModel`).
        *   If all conditions are met, calls `ActivateQuest(quest)`.
*   **Activating a Quest (`QuestSystem.ActivateQuest`)**:
    *   Sets the quest's `Status` to `Active` in `QuestModel` and adds it to `ActiveQuests`.
    *   For `CollectResource` objectives, initializes `CurrentAmount` based on the current amount in `ResourceModel`.
    *   Checks if the quest is already completed upon activation (e.g., objectives met by existing game state). If so, calls `CompleteQuest`.
*   **Tracking Objective Progress (`QuestSystem.OnGameEvent`)**:
    *   `QuestSystem` registers listeners for various game events (e.g., `ResourceChangedEvent`, `TechnologyCompletedEvent`, `POIExploredEvent`, `WorkstationBuiltEvent`, `CustomQuestFlagEvent`).
    *   When a relevant game event occurs:
        *   Iterates through objectives of all `ActiveQuests`.
        *   If an objective's `Type` and `TargetId` match the event:
            *   For `CollectResource`, updates `CurrentAmount` to the new total amount of that resource.
            *   For other types (which are typically count-based like "research 1 tech"), it increments `CurrentAmount`.
        *   After updating an objective, checks if `quest.AreAllObjectivesComplete()`. If true, calls `CompleteQuest(quest)`.
*   **Completing a Quest (`QuestSystem.CompleteQuest`)**:
    *   Sets the quest's `Status` to `Success` in `QuestModel` and removes it from `ActiveQuests`.
    *   Logs the completion.
    *   Iterates through `quest.Rewards` and calls `ApplyReward` for each.
    *   Calls `CheckForAllQuestActivations()` again, as completing this quest might unlock new ones.
*   **Applying Rewards (`QuestSystem.ApplyReward`)**:
    *   Based on `QuestRewardType`:
        *   `Resource`: Adds the specified `Amount` of the `TargetId` (resource type) to `ResourceModel`.
        *   `UnlockTech`: Marks a technology as unlockable. (Current implementation logs this and relies on `ResearchModel.UpdateAllTechnologyStatuses()` to handle actual availability if other prerequisites are met).
        *   `UnlockPOI`: Changes a POI's status from `Unexplored` to `Scouted` in `ExplorationModel`.
        *   Other types like `Item` or `SpawnSurvivor` are noted as future implementations.

### Interactions with Other Game Systems

*   **`GEventSystem` (QFramework's Event Bus)**:
    *   `QuestSystem` subscribes to various events (`ResourceChangedEvent`, `TechnologyCompletedEvent`, etc.) to track objective progress.
    *   `QuestSystem` sends its own events (e.g., `QuestActivatedEvent`, `QuestObjectiveCompletedEvent`, `QuestSucceededEvent`, defined in `GameEvents.cs`) which UI or other systems can listen to.
*   **`ResourceModel`**:
    *   `QuestSystem` reads resource amounts from `ResourceModel` to initialize and update `CollectResource` objectives.
    *   Quest rewards can add resources to `ResourceModel`.
*   **`ResearchModel`**:
    *   `QuestSystem` checks `Technology.Status` in `ResearchModel` for `ResearchTech` objectives and prerequisites.
    *   Quest rewards can unlock technologies (conceptually, by making them `Available` if other prerequisites are met).
*   **`ExplorationModel`**:
    *   `QuestSystem` checks POI exploration status for `ExplorePOI` objectives.
    *   Quest rewards can unlock POIs (change their status to `Scouted`).
*   **`WorkstationModel`**:
    *   `QuestSystem` tracks `BuildWorkstation` objectives (likely by listening to `WorkstationBuiltEvent`).
*   **`GameDataModel`**:
    *   `QuestSystem` checks `CurrentDay` from `GameDataModel` for day-based prerequisites.
*   **UI System (e.g., `QuestLogDisplay.cs`)**:
    *   Displays active quests and their objectives.
    *   Notifies the player of quest activation, progress, and completion by listening to quest-specific events sent by `QuestSystem`.

## 10. UI System

### Overview

The UI (User Interface) System is responsible for presenting game state information to the player and allowing player interaction (though the interaction part is less detailed in the provided scripts, the display part is evident). It visualizes data from various game models and systems, ensuring the player is informed about day/time, resources, events, research, quests, survivor status, etc.

### General Approach

The UI scripts are typically Unity MonoBehaviours attached to UI GameObjects (Text, Buttons, Panels, etc.). They follow a pattern of:

1.  **Referencing UI Elements**: Public variables (e.g., `public Text dayText;`) are declared and assigned in the Unity Inspector.
2.  **Accessing Game Systems/Models**: In their `Start()` or `Awake()` methods, UI scripts retrieve instances of necessary Models or Systems from the `GameArchitecture.Interface`.
3.  **Displaying Data**:
    *   **Polling/Direct Update**: Many UI scripts update their displayed text or state in their `Update()` method by continuously polling data from the models (e.g., `ResourceDisplay` getting amounts from `ResourceModel` every frame).
    *   **Event-Driven Updates (Implied/Recommended)**: While not always explicitly implemented in the provided snippets for all displays, a more optimized approach involves UI components listening to specific events (e.g., `ResourceChangedEvent` for `ResourceDisplay`) and updating only when relevant data changes. `CombatLogDisplay` uses `Application.logMessageReceived`, which is a form of event listening. `ExplorationDisplay` mentions the desirability of event-driven updates for expedition outcomes.
4.  **Handling Player Input (Basic)**: Some UI scripts, like `ExplorationDisplay`, include `Button` references and `InputField` references to capture player input and trigger game actions (e.g., starting an expedition).

### Key Classes Involved (Examples)

*   **`DayDisplay.cs`**:
    *   **Purpose**: Displays the current game day, time of day (as a percentage), and base health. Also shows "GAME OVER" or "VICTORY" states.
    *   **Key UI Elements**: `dayText` (Text), `timeText` (Text), `baseHealthText` (Text).
    *   **Interactions**:
        *   Retrieves `GameDataModel` to get `CurrentDay` and `BaseHealth`.
        *   Retrieves `DayNightSystem` to get `TimeOfDayNormalized`.
        *   Updates text fields in its `Update()` method based on these values.
*   **`ResourceDisplay.cs`**:
    *   **Purpose**: Shows the current quantities of various game resources.
    *   **Key UI Elements**: `foodText`, `powerText`, `ammoText`, `medicineText`, `researchPointsText` (all Text).
    *   **Interactions**:
        *   Retrieves `ResourceModel`.
        *   Updates text fields in its `Update()` method by calling `resourceModel.GetAmount()` for each `GameResourceType`.
*   **`CombatLogDisplay.cs`**:
    *   **Purpose**: Displays a filtered log of combat-related messages.
    *   **Key UI Elements**: `combatLogText` (Text).
    *   **Interactions**:
        *   Subscribes to `Application.logMessageReceived`.
        *   Filters log messages based on keywords (e.g., "Zombie", "attacks", "ammo") and type (Log or Warning).
        *   Maintains a queue (`mLogMessages`) of the most recent `maxLogLines` messages.
        *   Updates `combatLogText` when a relevant new message is received.
*   **`EventDisplay.cs`**:
    *   **Purpose**: Displays information about the currently active random event.
    *   **Key UI Elements**: `eventText` (Text).
    *   **Interactions**:
        *   Retrieves `EventModel`.
        *   In its `Update()` method, checks `mEventModel.CurrentEvent`.
        *   If an event is active, displays its `Title` and `Description`.
        *   Includes logic to clear the event message after a `mDisplayDuration` or if `CurrentEvent` becomes null.
*   **`ExplorationDisplay.cs`**:
    *   **Purpose**: Displays available POIs, active expeditions, results of the last expedition, and provides input fields/button to start new expeditions.
    *   **Key UI Elements**: `availablePOIsText`, `activeExpeditionsText`, `expeditionReportText` (all Text), `poiIdInput` (InputField), `survivorIdsInput` (InputField), `startExpeditionButton` (Button).
    *   **Interactions**:
        *   Retrieves `ExplorationModel`, `ExplorationSystem`, and `SurvivorModel`.
        *   `RefreshDisplay()` (called periodically in `Update()`):
            *   Gets POI data from `ExplorationModel` to list available POIs.
            *   Gets active expedition data from `ExplorationModel` and survivor names from `SurvivorModel`.
            *   Formats and displays this information.
            *   Displays `s_lastOutcome` (a static variable storing the outcome of the last completed expedition, set by `ExplorationSystem` via `DisplayOutcome` static method).
        *   `TryStartExpeditionFromInput()`:
            *   Reads `poiIdInput` and `survivorIdsInput`.
            *   Calls `mExplorationSystem.CanStartExpeditionToPOI` and `mExplorationSystem.StartExpedition`.
            *   Updates `expeditionReportText` with success or error messages.
*   **Other UI Scripts (General Pattern)**:
    *   **`QuestLogDisplay.cs`**: Would interact with `QuestModel` to display active quests and their objectives, likely updating based on events from `QuestSystem`.
    *   **`ResearchDisplay.cs`**: Would interact with `ResearchModel` and `ResearchSystem` to show the tech tree, research progress, and allow starting new research.
    *   **`SurvivorDisplay.cs`**: Would interact with `SurvivorModel` to list survivors, their stats, status, and professions.
    *   **`WorkstationDisplay.cs`**: Would interact with `WorkstationModel` to show built workstations, their status, assigned survivors, and potentially allow assignment.

### Core Functionalities

*   **Data Presentation**: Displaying textual and numerical game state information (day, resources, health, logs, event descriptions, POI details, expedition status, quest details, etc.).
*   **State Indication**: Reflecting game states like "Game Over", "Victory", or specific POI/Quest/Technology statuses.
*   **Basic Input Handling**: Allowing players to input text (e.g., POI ID for exploration) and click buttons to initiate actions.
*   **Dynamic Updates**: UI elements refresh either through polling in `Update()` or, in some cases, by reacting to game events or direct calls from other systems (like `ExplorationDisplay.DisplayOutcome`).

### Interactions with Other Game Systems

The UI system is primarily a *consumer* of data from most other game systems and a *trigger* for actions in some systems via player input.

*   **Models (Primary Data Source)**: `GameDataModel`, `ResourceModel`, `EventModel`, `ExplorationModel`, `QuestModel`, `ResearchModel`, `SurvivorModel`, `WorkstationModel`, `EnemyModel` (indirectly via logs) are all read by various UI components.
*   **Systems (For Actions & Some Data)**:
    *   `DayNightSystem`: For normalized time of day.
    *   `ExplorationSystem`: To start expeditions and receive outcome data.
    *   `ResearchSystem`: To start research projects.
    *   (Potentially) `SurvivorManagerSystem`, `WorkstationSystem` for actions like assigning survivors, initiating resting, etc., though these interactions are not explicitly detailed in the provided UI scripts for direct action triggers.
*   **`GameArchitecture.Interface`**: Used universally by UI scripts to gain access to any model or system.
*   **`Application.logMessageReceived` (Unity Event)**: Used by `CombatLogDisplay` to capture relevant log entries.
*   **QFramework Event System (Implied for future/robustness)**: While direct polling is common in the provided scripts, a more robust UI would heavily rely on listening to events like `ResourceChangedEvent`, `TechnologyCompletedEvent`, `QuestActivatedEvent`, etc., to trigger updates more efficiently. `ExplorationDisplay` explicitly mentions this as a better approach.

This concludes the documentation of the existing game systems.
