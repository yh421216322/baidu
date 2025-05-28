

namespace YourGameNamespace.Exploration
{
    [System.Serializable]
    public class POIReward
    {
        public GameResourceType ResourceType;
        public int MinQuantity;
        public int MaxQuantity;
        public float Probability; // 0.0 到 1.0 (1.0 表示如果POI成功搜刮则保证获得)

        public POIReward(GameResourceType resource, int min, int max, float probability = 1.0f)
        {
            ResourceType = resource;
            MinQuantity = min;
            MaxQuantity = max;
            Probability = probability;
        }
    }
}
