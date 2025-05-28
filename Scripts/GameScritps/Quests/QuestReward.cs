

namespace YourGameNamespace.Quests
{
    public enum QuestRewardType
    {
        Resource,
        Item, // 物品实现后的占位符
        UnlockTech, // 使特定技术可供研究
        UnlockPOI,  // 使POI可被发现或可用
        SpawnSurvivor // 添加新的幸存者
        // 未来可能添加：解锁建筑等。
    }

    [System.Serializable]
    public class QuestReward
    {
        public QuestRewardType Type { get; private set; }
        public string TargetId { get; private set; } // 例如：GameResourceType.ToString(), TechID, POI_ID
        public int Amount { get; private set; } // 用于资源或幸存者数量

        public QuestReward(QuestRewardType type, string targetId = null, int amount = 0)
        {
            Type = type;
            TargetId = targetId; // 如果奖励类型不需要，可以为null（例如：通用的士气提升事件）
            Amount = amount;
        }
    }
}
