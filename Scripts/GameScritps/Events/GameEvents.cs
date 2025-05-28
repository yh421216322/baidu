
using YourGameNamespace.Workstations; // 用于 WorkstationType
// 事件本身不直接依赖于 ExplorationOutcome，只需要ID。
// 事件本身不直接依赖于 Technology，只需要ID。

namespace YourGameNamespace.Events
{
    // 当任何资源数量发生重大变化时发送的事件
    public class ResourceChangedEvent 
    {
        public GameResourceType Type;
        public int NewTotalAmount; // 此资源的当前总量
        public int ChangeAmount;   // 变化的数量（+ 或 -）
        public ResourceChangedEvent(GameResourceType type, int newTotal, int change) 
            { Type = type; NewTotalAmount = newTotal; ChangeAmount = change; }
    }

    // 当技术研究完成时发送的事件
    public class TechnologyCompletedEvent 
    {
        public string TechId;
        public TechnologyCompletedEvent(string techId) { TechId = techId; }
    }

    // 当POI探索成功解决时发送的事件
    public class POIExploredEvent 
    {
        public string PoiId;
        // public YourGameNamespace.Exploration.ExpeditionOutcome Outcome; // 可选：如果除了QuestSystem之外的其他系统需要，则包含结果
        public POIExploredEvent(string poiId/*, YourGameNamespace.Exploration.ExpeditionOutcome outcome = null*/) 
            { PoiId = poiId; /*Outcome = outcome;*/ }
    }

    // 当新工作站建成并投入使用时发送的事件
    public class WorkstationBuiltEvent
    {
        public WorkstationType Type;
        public System.Guid WorkstationId; // 新工作站的特定ID
        public WorkstationBuiltEvent(WorkstationType type, System.Guid id) { Type = type; WorkstationId = id; }
    }

    // 当天数变化时发送的事件
    public class DayChangedEvent
    {
        public int CurrentDay;
        public DayChangedEvent(int day) { CurrentDay = day; }
    }
    
    // 当设置自定义标志时触发的事件（更通用于任务系统）
    public class CustomQuestFlagEvent
    {
        public string FlagName;
        public CustomQuestFlagEvent(string flagName) { FlagName = flagName; }
    }

    // 任务状态事件
    public class QuestActivatedEvent 
    {
        public YourGameNamespace.Quests.Quest ActivatedQuest;
        public QuestActivatedEvent(YourGameNamespace.Quests.Quest q) { ActivatedQuest = q; }
    }

    public class QuestObjectiveCompletedEvent 
    { 
        public YourGameNamespace.Quests.Quest ParentQuest; 
        public YourGameNamespace.Quests.QuestObjective Objective;
        public QuestObjectiveCompletedEvent(YourGameNamespace.Quests.Quest parent, YourGameNamespace.Quests.QuestObjective obj) { ParentQuest = parent; Objective = obj; }
    }

    public class QuestSucceededEvent 
    {
        public YourGameNamespace.Quests.Quest SucceededQuest;
        public QuestSucceededEvent(YourGameNamespace.Quests.Quest q) { SucceededQuest = q; }
    }
}
