using UnityEngine; // 用于 Mathf.Min

namespace YourGameNamespace.Quests
{
    // 任务目标的类型枚举
    public enum ObjectiveType
    {
        CollectResource,    // 收集资源：TargetId = GameResourceType.ToString(), 例如："Food" (食物)
        ResearchTech,       // 研究技术：TargetId = Technology.Id, 例如："TECH_RADIO_1" (基础无线电技术)
        ExplorePOI,         // 探索兴趣点：TargetId = ExplorationPointOfInterest.Id, 例如："POI_RADIO_TOWER" (旧无线电塔)
        BuildWorkstation,   // 建造工作站：TargetId = WorkstationType.ToString(), 例如："Farm" (农场)
        CustomFlag          // 自定义标记：TargetId = CustomFlagName (自定义标记名称), 例如："BeaconActivated" (信标已激活)
        // 未来可能添加的目标类型：到达特定天数、提升幸存者属性等。
    }

    // 代表任务中的一个具体目标
    // [System.Serializable] 属性允许该类的实例在Unity检视面板中被序列化和编辑
    [System.Serializable]
    public class QuestObjective
    {
        public string Description { get; private set; } // 目标的描述 (例如，“收集100单位食物”)
        public ObjectiveType Type { get; private set; } // 目标的类型
        public string TargetId { get; private set; }    // 目标的具体标识符 (例如：资源类型的字符串名称，技术ID，POI的ID等)
        public int RequiredAmount { get; private set; } // 完成目标所需的数量
        public int CurrentAmount { get; set; }          // 当前已完成的数量
        public bool IsComplete { get { return CurrentAmount >= RequiredAmount; } } // 目标是否已完成

        // 构造函数
        public QuestObjective(string description, ObjectiveType type, string targetId, int requiredAmount)
        {
            Description = description; // 任务目标的具体描述，应为中文
            Type = type;
            TargetId = targetId;
            RequiredAmount = requiredAmount;
            CurrentAmount = 0; // 初始时，当前数量为0
        }

        // 更新目标进度的方法
        // 确保当前数量不会超过所需数量 (主要用于收集资源类型的目标，其newCurrentAmount是总量)
        public void UpdateProgress(int newCurrentAmount)
        {
            CurrentAmount = Mathf.Min(newCurrentAmount, RequiredAmount);
        }
    }
}
