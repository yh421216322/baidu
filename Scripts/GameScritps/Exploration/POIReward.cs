namespace YourGameNamespace.Exploration
{
    // POI（兴趣点）奖励的数据结构
    // 使其可序列化，方便在Unity编辑器中配置或通过其他方式（如JSON）加载
    [System.Serializable] 
    public class POIReward
    {
        public GameResourceType ResourceType; // 奖励的资源类型
        public int MinQuantity;               // 可能获得的最小数量
        public int MaxQuantity;               // 可能获得的最大数量
        public float Probability;             // 获得此奖励的概率 (0.0 到 1.0, 其中1.0表示如果POI探索成功则必定获得)

        // 构造函数
        public POIReward(GameResourceType resource, int min, int max, float probability = 1.0f)
        {
            ResourceType = resource;
            MinQuantity = min;
            MaxQuantity = max;
            Probability = probability;
        }
    }
}
