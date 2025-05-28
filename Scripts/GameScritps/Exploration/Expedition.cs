using System;
using System.Collections.Generic;

namespace YourGameNamespace.Exploration
{
    public enum ExpeditionStatus
    {
        Preparing,      // 分配幸存者，尚未出发
        Departing,      // 前往POI
        Exploring,      // 在POI，计时器运行中
        Returning,      // 从POI返回
        Completed,      // 已返回，结果已处理
        Failed         // 例如，提前召回或严重失败
    }

    public class Expedition
    {
        public Guid ExpeditionId { get; private set; }
        public string TargetPoiId { get; private set; }
        public List<Guid> AssignedSurvivorIds { get; private set; }
        public float TravelTimeToPoi { get; private set; } = 10f; // 示例固定行程时间（秒）
        public float ExplorationTimeAtPoi { get; private set; } // 根据POI的基础探索时间和幸存者技能计算
        public float TravelTimeBackToBase { get; private set; } = 10f; // 示例固定行程时间（秒）

        public float TimeElapsedOnCurrentPhase { get; set; } = 0f;
        public ExpeditionStatus Status { get; set; }
        public ExpeditionOutcome Outcome { get; set; } // 完成或失败后设置

        public Expedition(string poiId, List<Guid> survivorIds, float explorationTimeAtPoi)
        {
            ExpeditionId = Guid.NewGuid();
            TargetPoiId = poiId;
            AssignedSurvivorIds = survivorIds ?? new List<Guid>();
            ExplorationTimeAtPoi = explorationTimeAtPoi; // 这应由ExplorationSystem根据POI和幸存者确定
            Status = ExpeditionStatus.Preparing;
            Outcome = new ExpeditionOutcome(); // 初始化结果
        }
    }
}
