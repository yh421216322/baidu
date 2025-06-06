using System;
using System.Collections.Generic;

namespace YourGameNamespace.Exploration
{
    // 远征状态枚举
    public enum ExpeditionStatus
    {
        Preparing,      // 准备中：分配幸存者，尚未出发
        Departing,      // 出发中：正在前往POI
        Exploring,      // 探索中：正在POI探索，计时器运行中
        Returning,      // 返回中：正在从POI返回
        Completed,      // 已完成：已返回基地，结果已处理
        Failed         // 失败：例如，被提前召回或发生严重失败事件
    }

    // 代表一次远征的类
    public class Expedition
    {
        public Guid ExpeditionId { get; private set; } // 远征的唯一ID
        public string TargetPoiId { get; private set; } // 目标兴趣点(POI)的ID
        public List<Guid> AssignedSurvivorIds { get; private set; } // 分配给此次远征的幸存者ID列表
        public float TravelTimeToPoi { get; private set; } = 10f; // 前往POI的行程时间（秒），此处为示例固定值
        public float ExplorationTimeAtPoi { get; private set; } // 在POI的探索时间（秒），应根据POI基础探索时间和幸存者技能计算
        public float TravelTimeBackToBase { get; private set; } = 10f; // 返回基地的行程时间（秒），此处为示例固定值

        public float TimeElapsedOnCurrentPhase { get; set; } = 0f; // 当前阶段已用时间（秒）
        public ExpeditionStatus Status { get; set; } // 远征的当前状态
        public ExpeditionOutcome Outcome { get; set; } // 远征的结果，在完成或失败后设置

        // 构造函数
        public Expedition(string poiId, List<Guid> survivorIds, float explorationTimeAtPoi)
        {
            ExpeditionId = Guid.NewGuid(); // 生成新的唯一ID
            TargetPoiId = poiId;
            AssignedSurvivorIds = survivorIds ?? new List<Guid>(); // 如果传入null则初始化为空列表
            ExplorationTimeAtPoi = explorationTimeAtPoi; // 此时间应由ExplorationSystem根据POI和幸存者具体情况确定
            Status = ExpeditionStatus.Preparing; // 初始状态为准备中
            Outcome = new ExpeditionOutcome(); // 初始化远征结果对象
        }
    }
}
