using UnityEngine; // Mathf 所需

namespace YourGameNamespace.Quests
{
    public enum ObjectiveType
    {
        CollectResource,    // TargetId = GameResourceType.ToString(), 例如："Food"
        ResearchTech,       // TargetId = Technology.Id, 例如："TECH_RADIO_1"
        ExplorePOI,         // TargetId = ExplorationPointOfInterest.Id, 例如："POI_RADIO_TOWER"
        BuildWorkstation,   // TargetId = WorkstationType.ToString(), 例如："Farm"
        CustomFlag          // TargetId = CustomFlagName, 例如："BeaconActivated"
        // 未来可能添加：到达某天，幸存者属性等。
    }

    [System.Serializable]
    public class QuestObjective
    {
        public string Description { get; private set; }
        public ObjectiveType Type { get; private set; }
        public string TargetId { get; private set; } // 目标的标识符（例如：资源名称，技术ID）
        public int RequiredAmount { get; private set; }
        public int CurrentAmount { get; set; }
        public bool IsComplete { get { return CurrentAmount >= RequiredAmount; } }

        public QuestObjective(string description, ObjectiveType type, string targetId, int requiredAmount)
        {
            Description = description;
            Type = type;
            TargetId = targetId;
            RequiredAmount = requiredAmount;
            CurrentAmount = 0;
        }

        // 更新进度的辅助方法，确保显示时不超过所需数量
        public void UpdateProgress(int newCurrentAmount)
        {
            CurrentAmount = Mathf.Min(newCurrentAmount, RequiredAmount);
        }
    }
}
