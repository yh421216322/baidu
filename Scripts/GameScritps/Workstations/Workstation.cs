using System;
using System.Collections.Generic;
using YourGameNamespace.Survivors; // 用于 SurvivorModel 引用（即使此处不直接使用 Survivor 类）
using UnityEngine; // 用于 Debug.Log

namespace YourGameNamespace.Workstations
{
    public class Workstation
    {
        public Guid Id { get; private set; }
        public WorkstationType Type { get; private set; }
        public List<Guid> AssignedSurvivorIds { get; private set; } = new List<Guid>();
        public float ProductionProgress { get; set; } = 0f;
        public float BaseProductionRate { get; private set; } 
        public float ProductionCycleTime { get; private set; } 
        public GameResourceType OutputResourceType { get; private set; }
        public int OutputQuantity { get; private set; } = 1; // 基础产出数量
        public GameResourceType? InputResourceType { get; private set; } = null;
        public int InputQuantity { get; private set; } = 0;

        // 用于可修改属性的新属性
        public float ProductionBonusMultiplier { get; set; } = 1.0f; // 初始化为1.0（无加成）
        public int FlatProductionBonus { get; set; } = 0;    // 初始化为0

        public Workstation(WorkstationType type)
        {
            Id = Guid.NewGuid();
            Type = type;

            switch (type)
            {
                case WorkstationType.Farm:
                    OutputResourceType = GameResourceType.Food;
                    OutputQuantity = 5;
                    // 提示：BaseProductionRate = 1f; ProductionCycleTime = 10f;
                    // 这意味着它需要1个“工作单位”，耗时10秒完成。
                    // 如果 GetCurrentProductionRatePerSecond 是 BaseProductionRate / ProductionCycleTime，
                    // 那么有效速率是 1/10 = 0.1 “工作单位”/秒。
                    // 并且 ProductionProgress 累积“工作单位”。
                    // 所以 ProductionProgress 应该达到 BaseProductionRate。
                    // 让我们调整以匹配 UpdateProduction 逻辑：Progress 累积到 CycleTime。
                    // 如果速率是 BaseProductionRate / ProductionCycleTime，那么 progress = rate * deltaTime。
                    // 如果 progress >= CycleTime。这令人困惑。

                    // 让我们遵循提示的示例：
                    // 农场：产出：食物5，BaseProductionRate：1f，CycleTime：10f。（基础设置下每10秒产出5食物）
                    // 这意味着 BaseProductionRate 是一个速度因子，而不是总工作量。
                    // 如果 ProductionProgress 从0增长到 ProductionCycleTime，那么 GetCurrentProductionRatePerSecond 就是“每秒完成 ProductionCycleTime 的多少”。
                    // 如果 BaseProductionRate 是1，这意味着它精确地花费 ProductionCycleTime 秒。所以速率是1。
                    // GetCurrentProductionRatePerSecond 的提示：`BaseProductionRate / ProductionCycleTime`。
                    // 如果 BaseProductionRate = 1f，CycleTime = 10f，那么速率 = 0.1f。
                    // ProductionProgress += 0.1f * deltaTime。
                    // 如果 ProductionProgress >= ProductionCycleTime (10)，这将花费100秒。这是不对的。

                    // 让我们重新解释：
                    // ProductionProgress 累积“工作量”。最大“工作量”是 BaseProductionRate。
                    // 工作速度是每秒1单位“工作量”（受幸存者影响）。
                    // CycleTime 是以1单位/秒工作时所需的时间。
                    // 不，UpdateProduction 的提示是：ProductionProgress += effectiveRate * deltaTime; if (ProductionProgress >= ProductionCycleTime)
                    // 这意味着 ProductionProgress 从0增长到 ProductionCycleTime。
                    // 而 effectiveRate 是“每秒向 ProductionCycleTime 推进的单位数”。
                    // 所以，如果一个周期是10秒，并且 BaseProductionRate 是1f（意味着标准速度），那么 effectiveRate 应该是1f。
                    // 如果 BaseProductionRate 是2f（快一倍），effectiveRate 是2f，5秒内完成。
                    // 所以 GetCurrentProductionRatePerSecond 应该只返回 BaseProductionRate。

                    BaseProductionRate = 1f; // 标准速度因子
                    ProductionCycleTime = 10f; // 标准速度下一个完整周期的时间（秒）
                    break;

                case WorkstationType.PowerPlant:
                    OutputResourceType = GameResourceType.Power;
                    OutputQuantity = 10;
                    BaseProductionRate = 1f; // 标准速度因子
                    ProductionCycleTime = 12f;
                    break;

                case WorkstationType.Workshop:
                    OutputResourceType = GameResourceType.Ammo;
                    OutputQuantity = 2;
                    InputResourceType = GameResourceType.Power;
                    InputQuantity = 1;
                    // 提示：BaseProductionRate = 0.5f; ProductionCycleTime = 15f;
                    // 这意味着它以半速工作，花费15秒。
                    // 所以，有效时间将是 15秒 / 0.5 = 30秒。
                    BaseProductionRate = 0.5f; // 半速因子
                    ProductionCycleTime = 15f; // 如果速率为1.0时一个完整周期的时间（秒）。
                                               // 所以实际时间是 15 / 0.5 = 30 秒。
                    break;

                case WorkstationType.Clinic:
                    OutputResourceType = GameResourceType.Medicine;
                    OutputQuantity = 1;
                    InputResourceType = GameResourceType.Food;
                    InputQuantity = 2;
                    BaseProductionRate = 1f; // 标准速度因子
                    ProductionCycleTime = 20f;
                    break;
                case WorkstationType.ResearchLab:
                    this.OutputResourceType = GameResourceType.ResearchPoints;
                    this.OutputQuantity = 1;       // 每个周期产出1个研究点
                    this.BaseProductionRate = 1f;  // 名义速率
                    this.ProductionCycleTime = 20f;// 产生1个研究点需要20秒
                    this.InputResourceType = GameResourceType.Power; 
                    this.InputQuantity = 1;        // 每个周期消耗1单位电力
                    Debug.Log($"已初始化研究实验室：每 {this.ProductionCycleTime}秒 产出 {this.OutputQuantity} {this.OutputResourceType}，消耗 {this.InputQuantity} {this.InputResourceType}");
                    break;
            }
        }

        public bool AssignSurvivor(Guid survivorId)
        {
            if (!AssignedSurvivorIds.Contains(survivorId))
            {
                // 如果需要，以后添加容量逻辑
                AssignedSurvivorIds.Add(survivorId);
                return true;
            }
            return false;
        }

        public bool UnassignSurvivor(Guid survivorId)
        {
            return AssignedSurvivorIds.Remove(survivorId);
        }

        // 计算有效生产率。
        // 此速率表示“每秒实际时间完成 ProductionCycleTime 的单位数”。
        // 如果 BaseProductionRate 为 1.0，则表示每秒1单位进度。
        // 如果 BaseProductionRate 为 2.0，则表示每秒2单位进度（完成速度快一倍）。
        public float GetCurrentProductionRatePerSecond(SurvivorModel survivorModel)
        {
            // 目前仅为 BaseProductionRate。以后会考虑幸存者数量和来自survivorModel的技能。
            // 如果有多个幸存者，此速率可以求和、平均或使用最佳值。
            // 目前，如果有任何幸存者被分配，则使用工作站的基础速率。
            if (AssignedSurvivorIds.Count > 0)
            {
                return BaseProductionRate;
            }
            return 0f;
        }

        public void UpdateProduction(float deltaTime, SurvivorModel survivorModel, ResourceModel resourceModel)
        {
            if (AssignedSurvivorIds.Count == 0)
            {
                // 可选：重置进度或让其暂停
                // ProductionProgress = 0; 
                return;
            }

            float effectiveRate = GetCurrentProductionRatePerSecond(survivorModel); // 例如 1.0f 或 0.5f
            ProductionProgress += effectiveRate * deltaTime;

            if (ProductionProgress >= ProductionCycleTime)
            {
                // 计算已完成的完整周期数
                int cyclesCompleted = (int)(ProductionProgress / ProductionCycleTime);
                ProductionProgress -= cyclesCompleted * ProductionCycleTime; // 结转多余进度

                for (int i = 0; i < cyclesCompleted; i++)
                {
                    bool canProduce = true;
                    if (InputResourceType.HasValue)
                    {
                        // ConsumeResource 成功时返回 true
                        canProduce = resourceModel.ConsumeResource(InputResourceType.Value, InputQuantity);
                    }

                    if (canProduce)
                    {
                        // 用加成计算最终产出数量
                        int baseOutput = this.OutputQuantity; 
                        int finalOutputQuantity = Mathf.Max(1, (int)(baseOutput * this.ProductionBonusMultiplier) + this.FlatProductionBonus);
                        if (baseOutput <= 0) finalOutputQuantity = 0; // 如果基础为0，则确保为0

                        resourceModel.AddResource(OutputResourceType, finalOutputQuantity);
                        Debug.Log($"{Type} 产出了 {finalOutputQuantity} 单位的 {OutputResourceType} (基础: {baseOutput}, 倍率: {ProductionBonusMultiplier:F2}, 固定加成: {FlatProductionBonus}, 周期 {i+1}/{cyclesCompleted})");
                    }
                    else
                    {
                        Debug.LogWarning($"{Type} 在周期 {i+1}/{cyclesCompleted} 生产失败：{InputResourceType.Value} 不足");
                        break; 
                    }
                }
            }
        }
    }
}
