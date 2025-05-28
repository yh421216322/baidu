using System.Collections.Generic;
using UnityEngine; // 用于 Vector2，如果以后用于地图位置

namespace YourGameNamespace.Exploration
{
    public enum POIStatus
    {
        Unexplored,
        Scouted, // 已发现但尚未探索
        BeingExplored,
        Explored,   // 已成功搜刮，可能已枯竭或冷却后可再次搜刮
        Depleted    // 资源已耗尽
    }

    public class ExplorationPointOfInterest
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int Difficulty { get; private set; } // 例如，1-5级，影响风险和奖励
        public float BaseExplorationTime { get; private set; } // 基础时间（秒）
        public int MaxSurvivorSlots { get; private set; }
        public List<POIReward> PotentialRewards { get; private set; }
        public POIStatus Status { get; set; }
        // public Vector2 MapLocation { get; private set; } // 未来地图功能的可选属性

        public ExplorationPointOfInterest(string id, string name, string description, int difficulty, float baseTime, int slots, List<POIReward> rewards)
        {
            Id = id;
            Name = name;
            Description = description;
            Difficulty = Mathf.Clamp(difficulty, 1, 5);
            BaseExplorationTime = baseTime;
            MaxSurvivorSlots = Mathf.Max(1, slots); // 至少1个槽位
            PotentialRewards = rewards ?? new List<POIReward>();
            Status = POIStatus.Unexplored; // 或者如果是需要先发现，则为 Scouted
        }
    }
}
