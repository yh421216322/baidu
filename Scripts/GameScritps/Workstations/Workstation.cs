using System;
using System.Collections.Generic;
using YourGameNamespace.Survivors; // 用于 SurvivorModel 的引用 (即使此处不直接使用 Survivor 类，也可能在未来用于根据幸存者技能调整生产效率)
using UnityEngine;                 // 用于 Debug.Log 和 Mathf 数学函数

namespace YourGameNamespace.Workstations
{
    // 代表游戏中的一个工作站（例如：农场、工坊）
    public class Workstation
    {
        public Guid Id { get; private set; } // 工作站的唯一ID
        public WorkstationType Type { get; private set; } // 工作站的类型 (例如：农场, 发电厂)
        public List<Guid> AssignedSurvivorIds { get; private set; } = new List<Guid>(); // 分配到此工作站的幸存者ID列表
        public float ProductionProgress { get; set; } = 0f; // 当前生产周期的进度 (0 到 ProductionCycleTime)
        public float BaseProductionRate { get; private set; }  // 基础生产速率因子 (例如：1.0代表标准速度)
        public float ProductionCycleTime { get; private set; } // 完成一个生产周期所需的时间（秒），在基础速率为1.0时
        public GameResourceType OutputResourceType { get; private set; } // 此工作站产出的资源类型
        public int OutputQuantity { get; private set; } = 1; // 每个生产周期的基础产出数量
        public GameResourceType? InputResourceType { get; private set; } = null; // 可选：生产所需的输入资源类型
        public int InputQuantity { get; private set; } = 0; // 如果需要输入资源，则为所需的数量

        // 新增属性：用于通过研究或其他方式修改工作站效率
        public float ProductionBonusMultiplier { get; set; } = 1.0f; // 生产加成乘数，默认为1.0 (无加成)
        public int FlatProductionBonus { get; set; } = 0;    // 固定生产加成值，默认为0

        // 构造函数，根据工作站类型初始化其属性
        public Workstation(WorkstationType type)
        {
            Id = Guid.NewGuid(); // 生成新的唯一ID
            Type = type;

            // 根据不同的工作站类型设置其特定的生产参数
            switch (type)
            {
                case WorkstationType.Farm: // 农场
                    OutputResourceType = GameResourceType.Food; // 产出食物
                    OutputQuantity = 5;                         // 基础产量5单位
                    // 关于生产速率和周期的解释与调整：
                    // 原注释中对 BaseProductionRate 和 ProductionCycleTime 的理解存在一些混淆。
                    // 采纳后的逻辑：ProductionProgress 从0累积到 ProductionCycleTime。
                    // effectiveRate (由 GetCurrentProductionRatePerSecond 计算，当前简化为 BaseProductionRate)
                    // 表示“每真实秒数，ProductionProgress 推进多少单位”。
                    // 因此，如果 BaseProductionRate = 1f，ProductionCycleTime = 10f，则完成一个周期需要 10 / 1 = 10 秒。
                    // 如果 BaseProductionRate = 2f (快一倍)，则需要 10 / 2 = 5 秒。
                    BaseProductionRate = 1f;  // 标准生产速率因子
                    ProductionCycleTime = 10f; // 在标准速率下一个完整生产周期所需的时间（秒）
                    break;

                case WorkstationType.PowerPlant: // 发电厂
                    OutputResourceType = GameResourceType.Power; // 产出电力
                    OutputQuantity = 10;
                    BaseProductionRate = 1f;  // 标准速率因子
                    ProductionCycleTime = 12f;
                    break;

                case WorkstationType.Workshop: // 工坊
                    OutputResourceType = GameResourceType.Ammo; // 产出弹药
                    OutputQuantity = 2;
                    InputResourceType = GameResourceType.Power; // 需要电力作为输入
                    InputQuantity = 1;
                    // 原注释提示：BaseProductionRate = 0.5f; ProductionCycleTime = 15f;
                    // 这意味着它以半速工作。如果ProductionCycleTime仍然是“标准速率下的周期时间”，
                    // 那么实际时间将是 15秒 / 0.5 = 30秒。这个逻辑是合理的。
                    BaseProductionRate = 0.5f; // 生产速率因子为0.5（半速）
                    ProductionCycleTime = 15f; // 如果速率为1.0时，一个完整周期所需的时间（秒）。
                                               // 因此，实际完成时间是 15 / 0.5 = 30 秒。
                    break;

                case WorkstationType.Clinic: // 诊所
                    OutputResourceType = GameResourceType.Medicine; // 产出药品
                    OutputQuantity = 1;
                    InputResourceType = GameResourceType.Food; // 需要食物作为输入
                    InputQuantity = 2;
                    BaseProductionRate = 1f;  // 标准速率因子
                    ProductionCycleTime = 20f;
                    break;
                case WorkstationType.ResearchLab: // 研究实验室
                    this.OutputResourceType = GameResourceType.ResearchPoints; // 产出研究点
                    this.OutputQuantity = 1;       // 每个周期产出1个研究点
                    this.BaseProductionRate = 1f;  // 名义生产速率
                    this.ProductionCycleTime = 20f;// 在名义速率下，产生1个研究点需要20秒
                    this.InputResourceType = GameResourceType.Power; // 需要电力作为输入
                    this.InputQuantity = 1;        // 每个周期消耗1单位电力
                    Debug.Log($"已初始化研究实验室：每 {this.ProductionCycleTime}秒 产出 {this.OutputQuantity} 单位 {this.OutputResourceType}，消耗 {this.InputQuantity} 单位 {this.InputResourceType}");
                    break;
            }
        }

        // 分配一个幸存者到此工作站
        public bool AssignSurvivor(Guid survivorId)
        {
            if (!AssignedSurvivorIds.Contains(survivorId)) // 如果该幸存者尚未分配到此工作站
            {
                // 未来可以根据工作站容量 (MaxSurvivorSlots) 添加逻辑来限制分配人数
                AssignedSurvivorIds.Add(survivorId); // 添加到已分配列表
                return true; // 分配成功
            }
            return false; // 已分配或分配失败
        }

        // 从此工作站取消分配一个幸存者
        public bool UnassignSurvivor(Guid survivorId)
        {
            return AssignedSurvivorIds.Remove(survivorId); // 尝试从列表中移除，并返回操作是否成功
        }

        // 计算当前工作站的有效生产速率。
        // 此速率表示“每真实秒数，ProductionProgress 应该推进多少单位”。
        // 例如，如果 BaseProductionRate 为 1.0，则表示每秒进度推进1单位。
        // 如果 BaseProductionRate 为 2.0，则表示每秒进度推进2单位（即完成速度快一倍）。
        public float GetCurrentProductionRatePerSecond(SurvivorModel survivorModel)
        {
            // 当前实现：只要有幸存者被分配，就使用工作站的基础生产速率。
            // 未来扩展：可以考虑分配的幸存者数量、他们的技能等级（从survivorModel获取）等因素来调整有效速率。
            // 例如，多个幸存者可能会叠加效率，或者取最高技能值等。
            if (AssignedSurvivorIds.Count > 0) // 如果至少有一个幸存者被分配
            {
                return BaseProductionRate; // 返回基础生产速率
            }
            return 0f; // 如果没有幸存者分配，则生产速率为0
        }

        // 更新工作站的生产进度，由 WorkstationSystem 每帧调用
        public void UpdateProduction(float deltaTime, SurvivorModel survivorModel, ResourceModel resourceModel)
        {
            if (AssignedSurvivorIds.Count == 0) // 如果没有幸存者分配到此工作站
            {
                // 可选行为：可以选择在没有幸存者时重置生产进度，或让其暂停在当前进度。
                // ProductionProgress = 0; // 例如，重置进度
                return; // 直接返回，不进行生产
            }

            float effectiveRate = GetCurrentProductionRatePerSecond(survivorModel); // 获取当前有效生产速率 (例如 1.0f 或 0.5f)
            ProductionProgress += effectiveRate * deltaTime; // 根据速率和时间差累加生产进度

            if (ProductionProgress >= ProductionCycleTime) // 如果生产进度已达到或超过一个完整周期所需的时间
            {
                // 计算已完成的完整生产周期数量
                int cyclesCompleted = (int)(ProductionProgress / ProductionCycleTime);
                ProductionProgress -= cyclesCompleted * ProductionCycleTime; // 从当前进度中减去已完成周期的部分，剩余部分作为下一周期的起始进度 (结转)

                // 为每个完成的周期尝试产出资源
                for (int i = 0; i < cyclesCompleted; i++)
                {
                    bool canProduce = true; // 标记是否可以进行生产（主要用于检查输入资源）
                    if (InputResourceType.HasValue) // 如果此工作站需要输入资源
                    {
                        // 尝试消耗所需数量的输入资源。ConsumeResource 方法在成功时返回 true。
                        canProduce = resourceModel.ConsumeResource(InputResourceType.Value, InputQuantity);
                    }

                    if (canProduce) // 如果满足输入资源条件（或无需输入资源）
                    {
                        // 计算考虑了各种加成后的最终产出数量
                        int baseOutput = this.OutputQuantity; // 获取基础产出量
                        // 最终产量 = (基础产量 * 乘数加成) + 固定加成，且至少为1（除非基础产量为0或负数）
                        int finalOutputQuantity = Mathf.Max(1, (int)(baseOutput * this.ProductionBonusMultiplier) + this.FlatProductionBonus);
                        if (baseOutput <= 0) finalOutputQuantity = 0; // 如果基础产量本身就不是正数，则最终产量为0

                        resourceModel.AddResource(OutputResourceType, finalOutputQuantity); // 将产出的资源添加到全局资源模型
                        Debug.Log($"工作站 {Type} (ID: {Id.ToString().Substring(0,4)}) 产出了 {finalOutputQuantity} 单位的 {OutputResourceType} (基础产量: {baseOutput}, 生产倍率: {ProductionBonusMultiplier:F2}, 固定加成: {FlatProductionBonus}, 完成周期 {i+1}/{cyclesCompleted})");
                    }
                    else // 如果输入资源不足，导致无法生产
                    {
                        Debug.LogWarning($"工作站 {Type} (ID: {Id.ToString().Substring(0,4)}) 在尝试完成周期 {i+1}/{cyclesCompleted} 的生产时失败：所需的输入资源 {InputResourceType.Value} 不足。");
                        break; // 中断后续周期的生产尝试，因为资源已不足
                    }
                }
            }
        }
    }
}
