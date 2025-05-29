namespace YourGameNamespace.Quests
{
    // 任务奖励的类型枚举
    public enum QuestRewardType
    {
        Resource,       // 奖励资源
        Item,           // 奖励物品 (当前为占位符，待物品系统实现后具体化)
        UnlockTech,     // 解锁特定技术，使其可供研究
        UnlockPOI,      // 解锁兴趣点(POI)，使其可被发现或可进行探索
        SpawnSurvivor   // 生成新的幸存者作为奖励
        // 未来可能添加的奖励类型：解锁新的建筑蓝图等。
    }

    // 代表任务完成后给予玩家的奖励
    // [System.Serializable] 属性允许该类的实例在Unity检视面板中被序列化和编辑
    [System.Serializable]
    public class QuestReward
    {
        public QuestRewardType Type { get; private set; } // 奖励的类型
        // 奖励目标的标识符 (例如：对于Resource类型，是GameResourceType.ToString()；对于UnlockTech，是TechID；对于UnlockPOI，是POI_ID)
        public string TargetId { get; private set; } 
        // 奖励的数量 (例如：奖励资源的数量，生成幸存者的数量)
        public int Amount { get; private set; } 

        // 构造函数
        public QuestReward(QuestRewardType type, string targetId = null, int amount = 0)
        {
            Type = type;
            // 如果奖励类型不需要目标ID (例如，一个通用的士气提升事件或固定奖励)，TargetId可以为null
            TargetId = targetId; 
            Amount = amount;
        }
    }
}
