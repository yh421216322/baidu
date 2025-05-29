using System.Collections.Generic;
using UnityEngine; // 如果将来要使用Vector2表示地图位置，则需要此引用

namespace YourGameNamespace.Exploration
{
    // 兴趣点(POI)的状态枚举
    public enum POIStatus
    {
        Unexplored,     // 未探索：完全未知或未被发现
        Scouted,        // 已侦察：已发现，但尚未进行探索
        BeingExplored,  // 探索中：当前有远征队正在此POI进行探索
        Explored,       // 已探索：已被成功搜刮过，根据游戏设计，可能已资源枯竭，或在冷却后可再次搜刮
        Depleted        // 已耗尽：资源已完全耗尽，无法再从此POI获取任何东西
    }

    // 代表一个可探索的兴趣点(POI)的类
    public class ExplorationPointOfInterest
    {
        public string Id { get; private set; } // POI的唯一ID
        public string Name { get; private set; } // POI的名称 (例如，“废弃超市”)
        public string Description { get; private set; } // POI的描述信息
        public int Difficulty { get; private set; } // POI的难度等级 (例如，1-5级)，影响风险和潜在奖励
        public float BaseExplorationTime { get; private set; } // 探索此POI所需的基础时间（秒）
        public int MaxSurvivorSlots { get; private set; } // 此POI允许同时探索的最大幸存者数量
        public List<POIReward> PotentialRewards { get; private set; } // 此POI可能产出的潜在奖励列表
        public POIStatus Status { get; set; } // POI的当前状态
        // public Vector2 MapLocation { get; private set; } // 可选属性：POI在游戏地图上的坐标 (为未来地图功能预留)

        // 构造函数
        public ExplorationPointOfInterest(string id, string name, string description, int difficulty, float baseTime, int slots, List<POIReward> rewards)
        {
            Id = id;
            Name = name;
            Description = description;
            Difficulty = Mathf.Clamp(difficulty, 1, 5); // 确保难度在1到5之间
            BaseExplorationTime = baseTime;
            MaxSurvivorSlots = Mathf.Max(1, slots); // 确保至少有1个幸存者槽位
            PotentialRewards = rewards ?? new List<POIReward>(); // 如果传入null则初始化为空列表
            Status = POIStatus.Unexplored; // 初始状态通常为未探索 (或者，如果游戏设计需要先“发现”POI，则可以是Scouted)
        }
    }
}
