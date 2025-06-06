using UnityEngine; // 用于 Mathf.Clamp
using QFramework;  // 用于 BindableProperty

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

        // 将 CurrentAmount 修改为 BindableProperty，并设其 set 访问器为 private
        public BindableProperty<int> CurrentAmount { get; private set; }

        // 修改 IsComplete 属性的getter以依赖 CurrentAmount.Value
        public bool IsComplete => CurrentAmount.Value >= RequiredAmount;

        // 构造函数
        public QuestObjective(string description, ObjectiveType type, string targetId, int requiredAmount)
        {
            Description = description; // 任务目标的具体描述，应为中文
            Type = type;
            TargetId = targetId;
            RequiredAmount = requiredAmount;
            // 在构造函数中初始化 CurrentAmount BindableProperty
            this.CurrentAmount = new BindableProperty<int>(0); // 初始时，当前数量为0
        }

        // 用于直接设置当前进度值 (替换旧的 UpdateProgress，如果其行为是设置总量)
        public void SetCurrentProgress(int progress)
        {
            CurrentAmount.Value = Mathf.Clamp(progress, 0, RequiredAmount);
        }

        // 用于增加/调整进度 (如果旧的 UpdateProgress 行为是增量式的，则使用此方法)
        public void AddProgress(int amount)
        {
            CurrentAmount.Value = Mathf.Clamp(CurrentAmount.Value + amount, 0, RequiredAmount);
        }

        // 旧的 UpdateProgress 方法根据其原有逻辑，似乎更接近 SetCurrentProgress 的行为（因为它直接用新值，并用Min确保不超过上限）
        // 因此，我们将保留 SetCurrentProgress 并考虑移除 UpdateProgress，或将其调整为 AddProgress 的别名（如果需要）。
        // 为了清晰，这里移除旧的 UpdateProgress 方法，推荐使用 SetCurrentProgress 或 AddProgress。
        // public void UpdateProgress(int newCurrentAmount)
        // {
        //     CurrentAmount.Value = Mathf.Min(newCurrentAmount, RequiredAmount); // 旧逻辑现在由 SetCurrentProgress(Mathf.Min(...)) 或 AddProgress(Mathf.Min(...)-oldValue) 实现
        // }
    }
}
