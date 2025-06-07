using YourGameNamespace.Workstations; // 用于 WorkstationType 枚举
using YourGameNamespace.Survivors;  // 用于 Survivor 类型
using YourGameNamespace.Enemies;    // 用于 Zombie 类型
using System;                       // 用于 Guid 类型
using YourGameNamespace.Buildings; // For Building class and BuildingType enum

// 注意：以下事件定义中的注释主要解释事件的用途和包含的数据。
// 事件类本身不直接依赖于 ExplorationOutcome 或 Technology 的完整对象，通常只需要ID。

namespace YourGameNamespace.Events
{
    // 当任何资源的数量发生重大变化时发送的事件
    public class ResourceChangedEvent 
    {
        public GameResourceType Type; // 发生变化的资源类型
        public int NewTotalAmount;    // 变化后该资源的当前总量
        public int ChangeAmount;      // 本次变化的数量（正数为增加，负数为减少）
        public ResourceChangedEvent(GameResourceType type, int newTotal, int change) 
            { Type = type; NewTotalAmount = newTotal; ChangeAmount = change; }
    }

    // 当一项技术研究完成时发送的事件
    public class TechnologyCompletedEvent 
    {
        public string TechId; // 已完成技术的ID
        public TechnologyCompletedEvent(string techId) { TechId = techId; }
    }

    // 当一个兴趣点(POI)探索成功并结束时发送的事件
    public class POIExploredEvent 
    {
        public string PoiId; // 被探索的POI的ID
        // public YourGameNamespace.Exploration.ExpeditionOutcome Outcome; // 可选：如果除了QuestSystem之外的其他系统也需要完整的探索结果，则可以包含此字段
        public POIExploredEvent(string poiId/*, YourGameNamespace.Exploration.ExpeditionOutcome outcome = null*/) 
            { PoiId = poiId; /*Outcome = outcome;*/ }
    }

    // 当一个新的工作站建成并投入使用时发送的事件
    public class WorkstationBuiltEvent
    {
        public WorkstationType Type;      // 新建工作站的类型
        public System.Guid WorkstationId; // 新建工作站的唯一ID
        public WorkstationBuiltEvent(WorkstationType type, System.Guid id) { Type = type; WorkstationId = id; }
    }

    // 当游戏内的天数发生变化时发送的事件
    public class DayChangedEvent
    {
        public int CurrentDay; // 变化后的当前天数
        public DayChangedEvent(int day) { CurrentDay = day; }
    }
    
    // 当一个自定义的游戏标志（通常用于任务系统）被设置或触发时发送的事件
    public class CustomQuestFlagEvent
    {
        public string FlagName; // 自定义标志的名称
        public CustomQuestFlagEvent(string flagName) { FlagName = flagName; }
    }

    // --- 以下是与任务系统状态相关的特定事件 ---

    // 当一个任务被激活时发送的事件
    public class QuestActivatedEvent 
    {
        public YourGameNamespace.Quests.Quest ActivatedQuest; // 被激活的任务对象
        public QuestActivatedEvent(YourGameNamespace.Quests.Quest q) { ActivatedQuest = q; }
    }

    // 当一个任务的目标完成时发送的事件
    public class QuestObjectiveCompletedEvent 
    { 
        public YourGameNamespace.Quests.Quest ParentQuest; // 该目标所属的任务
        public YourGameNamespace.Quests.QuestObjective Objective; // 已完成的目标对象
        public QuestObjectiveCompletedEvent(YourGameNamespace.Quests.Quest parent, YourGameNamespace.Quests.QuestObjective obj) { ParentQuest = parent; Objective = obj; }
    }

    // 当一个任务成功完成所有目标时发送的事件
    public class QuestSucceededEvent 
    {
        public YourGameNamespace.Quests.Quest SucceededQuest; // 已成功完成的任务对象
        public QuestSucceededEvent(YourGameNamespace.Quests.Quest q) { SucceededQuest = q; }
    }

    // --- 以下是与敌人模型 (EnemyModel) 相关的事件 ---

    // 当一个新的敌人（例如僵尸）被添加到 EnemyModel 时发送的事件
    public struct Model_EnemyAddedEvent
    {
        public YourGameNamespace.Enemies.Zombie Enemy; // 包含被添加的敌人对象
    }

    // 当一个敌人从 EnemyModel 中移除时发送的事件
    public struct Model_EnemyRemovedEvent
    {
        public System.Guid ZombieId; // 包含被移除的敌人的ID
    }

    // 当 EnemyModel 中的所有敌人都被清除时发送的事件
    public struct Model_AllEnemiesClearedEvent
    {
        // 此事件通常不需要额外数据，它的发生本身即为信息
    }

    // --- 以下是与幸存者模型 (SurvivorModel) 相关的事件 ---

    // 当一个新的幸存者被添加到 SurvivorModel 时发送的事件
    public struct Model_SurvivorAddedEvent
    {
        public YourGameNamespace.Survivors.Survivor SurvivorData; // 包含被添加的幸存者数据
    }

    // --- 以下是与工作站模型 (WorkstationModel) 相关的事件 ---

    // 当一个新的工作站被添加到 WorkstationModel 并注册时发送的事件
    public struct Model_WorkstationRegisteredEvent
    {
        public YourGameNamespace.Workstations.Workstation WorkstationData; // 包含被注册的工作站数据
    }

    // --- 以下是与研究模型 (ResearchModel) 相关的事件 ---

    // 当一项科技在 ResearchModel 中的状态发生更新时发送的事件
    public struct Model_TechnologyStatusUpdatedEvent
    {
        public string TechId;                                                   // 发生状态更新的科技ID
        public YourGameNamespace.Research.ResearchStatus NewStatus; // 科技的新状态
        public YourGameNamespace.Research.ResearchStatus OldStatus; // 科技的旧状态
    }

    // --- 以下是与探索模型 (ExplorationModel) 相关的事件 ---

    // 当一个兴趣点 (POI) 在 ExplorationModel 中的状态发生更新时发送的事件
    public struct Model_POIStatusUpdatedEvent
    {
        public string PoiId;                                                              // POI的ID
        public YourGameNamespace.Exploration.POIStatus NewStatus; // POI的新状态
        public YourGameNamespace.Exploration.POIStatus OldStatus; // POI的旧状态
    }

    // 当一个新的活动远征被添加到 ExplorationModel 时发送的事件
    public struct Model_ActiveExpeditionAddedEvent
    {
        public YourGameNamespace.Exploration.Expedition ExpeditionData; // 包含被添加的远征数据
    }

    // 当一个活动远征从 ExplorationModel 中移除时发送的事件
    public struct Model_ActiveExpeditionRemovedEvent
    {
        public System.Guid ExpeditionId; // 被移除远征的ID
    }

    // --- 以下是与任务模型 (QuestModel) 相关的事件 ---

    // 当一个任务在 QuestModel 中的状态发生更新时发送的事件
    public struct Model_QuestStatusUpdatedEvent
    {
        public string QuestId;                                          // 任务的ID
        public YourGameNamespace.Quests.QuestStatus NewStatus;    // 任务的新状态
        public YourGameNamespace.Quests.QuestStatus OldStatus;    // 任务的旧状态
    }

    // --- 以下是与战斗系统 (CombatSystem) 相关的事件 ---

    // 当一个僵尸在战斗中死亡时由 CombatSystem 发送的事件
    public struct Combat_ZombieDiedEvent
    {
        public System.Guid ZombieId;         // 死亡僵尸的ID
        // public string ZombieTypeName;      // (可选) 僵尸的类型名称，如果需要区分
        // public Vector2 Position;         // (可选) 僵尸死亡时的位置
        // public bool KilledBySurvivor;    // (可选) 是否由幸存者击杀
    }

    // 当幸存者的全局攻击力乘数在 CombatSystem 中发生变化时发送的事件
    public struct Combat_SurvivorAttackPowerMultiplierChangedEvent
    {
        public float NewMultiplier;       // 新的攻击力乘数值
        public float OldMultiplier;       // 旧的攻击力乘数值
    }

    // --- 以下是与幸存者管理系统 (SurvivorManagerSystem) 相关的事件 ---

    // 当一个幸存者因需求未能满足而进入“需要关注”状态时，由 SurvivorManagerSystem 发送的事件
    public struct System_SurvivorNeedsAttentionEvent
    {
        public System.Guid SurvivorId;    // 需要关注的幸存者的ID
        public string SurvivorName;       // 需要关注的幸存者的名字 (方便UI直接显示)
        // 可以根据需要添加其他信息，例如具体是食物还是休息不足
    }

    // --- 以下是与命令执行结果相关的事件 ---

    // 当尝试开始远征但失败时由 StartExpeditionCommand 发送的事件
    public struct Command_StartExpeditionFailedEvent
    {
        public string PoiId;
        public System.Collections.Generic.List<System.Guid> SurvivorIds; // Ensure System.Collections.Generic is used or List<Guid>
        public string Reason; // 失败的原因
    }

    /// <summary>
    /// 当尝试分配幸存者到工作站操作完成后的事件
    /// </summary>
    public struct AssignSurvivorToWorkstationResultEvent {
        public Guid SurvivorId;
        public Guid WorkstationId;
        public bool Success;
        public string FailureReasonKey; // 可选，用于UI本地化错误信息
    }

    // --- 以下是与建筑模型 (BuildingModel) 相关的事件 ---

    /// <summary>
    /// 当一个新建筑在模型中注册（通常表示建造完成）时发送
    /// </summary>
    public struct Model_BuildingConstructedEvent {
        public Building BuildingData; // 包含新建造建筑的完整数据
    }

    /// <summary>
    /// 当一个建筑从模型中移除（通常表示被拆除）时发送
    /// </summary>
    public struct Model_BuildingDemolishedEvent {
        public Guid BuildingId; // 被移除建筑的ID
        public BuildingType BuildingType; // 被移除建筑的类型
    }

    /// <summary>
    /// 当通过 BuildBuildingCommand 尝试建造建筑操作完成后的事件
    /// </summary>
    public struct BuildBuildingResultEvent {
        public BuildingType BuildingTypeAttempted; // 尝试建造的建筑类型
        public bool Success;                     // 是否成功
        public string FailureReasonKey;          // 可选，用于UI本地化错误信息 (例如 "INSUFFICIENT_RESOURCES")
    }

    /// <summary>
    /// 当尝试从工作站解除分配幸存者操作完成后的事件
    /// </summary>
    public struct UnassignSurvivorFromWorkstationResultEvent {
        public Guid SurvivorId;     // 相关幸存者的ID
        public Guid WorkstationId;  // 相关工作站的ID
        public bool Success;        // 操作是否成功 (通常为true，除非发生意外错误)
    }
}
// 确保文件顶部有:
// using YourGameNamespace.Enemies;
// using YourGameNamespace.Survivors; // 如果尚未添加
// using YourGameNamespace.Workstations; // 如果尚未添加 (实际上已存在)
// using YourGameNamespace.Research; // 如果尚未添加
// using YourGameNamespace.Exploration; // 如果尚未添加
// using YourGameNamespace.Quests; // 如果尚未添加
// using UnityEngine; // 如果需要Vector2等Unity类型
// using System;
// using System.Collections.Generic; // For List<Guid> in Command_StartExpeditionFailedEvent
