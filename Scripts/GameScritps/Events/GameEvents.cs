using YourGameNamespace.Workstations; // 用于 WorkstationType 枚举

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
}
