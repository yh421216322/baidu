using System;
using System.Collections.Generic;
using YourGameNamespace.Survivors; // 用于 SurvivorModel 的引用
using UnityEngine;                 // 用于 Debug.Log 和 Mathf 数学函数
using QFramework;                  // 用于 BindableProperty

namespace YourGameNamespace.Workstations
{
    // 代表游戏中的一个工作站（例如：农场、工坊）
    public class Workstation
    {
        public Guid Id { get; private set; } // 工作站的唯一ID
        public WorkstationType Type { get; private set; } // 工作站的类型 (例如：农场, 发电厂)

        // 将 AssignedSurvivorIds 的 set 访问器设为 private，确保只能通过方法修改
        public List<Guid> AssignedSurvivorIds { get; private set; }
        // 使用 BindableProperty 包装 ProductionProgress
        public BindableProperty<float> ProductionProgress { get; private set; }
        // 新增 BindableProperty 用于已分配幸存者数量
        public BindableProperty<int> AssignedSurvivorCount { get; private set; }

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
            AssignedSurvivorIds = new List<Guid>(); // 正确初始化列表
            ProductionProgress = new BindableProperty<float>(0f); // 初始化BindableProperty
            AssignedSurvivorCount = new BindableProperty<int>(0);   // 初始化BindableProperty

            // 根据不同的工作站类型设置其特定的生产参数
            switch (type)
            {
                case WorkstationType.Farm:
                    OutputResourceType = GameResourceType.Food;
                    OutputQuantity = 5;
                    BaseProductionRate = 1f;
                    ProductionCycleTime = 10f;
                    break;

                case WorkstationType.PowerPlant:
                    OutputResourceType = GameResourceType.Power;
                    OutputQuantity = 10;
                    BaseProductionRate = 1f;
                    ProductionCycleTime = 12f;
                    break;

                case WorkstationType.Workshop:
                    OutputResourceType = GameResourceType.Ammo;
                    OutputQuantity = 2;
                    InputResourceType = GameResourceType.Power;
                    InputQuantity = 1;
                    BaseProductionRate = 0.5f;
                    ProductionCycleTime = 15f;
                    break;

                case WorkstationType.Clinic:
                    OutputResourceType = GameResourceType.Medicine;
                    OutputQuantity = 1;
                    InputResourceType = GameResourceType.Food;
                    InputQuantity = 2;
                    BaseProductionRate = 1f;
                    ProductionCycleTime = 20f;
                    break;
                case WorkstationType.ResearchLab:
                    this.OutputResourceType = GameResourceType.ResearchPoints;
                    this.OutputQuantity = 1;
                    this.BaseProductionRate = 1f;
                    this.ProductionCycleTime = 20f;
                    this.InputResourceType = GameResourceType.Power;
                    this.InputQuantity = 1;
                    Debug.Log($"已初始化研究实验室：每 {this.ProductionCycleTime}秒 产出 {this.OutputQuantity} 单位 {this.OutputResourceType}，消耗 {this.InputQuantity} 单位 {this.InputResourceType}");
                    break;
            }
        }

        // 分配一个幸存者到此工作站
        public bool AssignSurvivor(Guid survivorId)
        {
            if (!AssignedSurvivorIds.Contains(survivorId))
            {
                AssignedSurvivorIds.Add(survivorId);
                AssignedSurvivorCount.Value = AssignedSurvivorIds.Count; // 更新计数
                return true;
            }
            return false;
        }

        // 从此工作站取消分配一个幸存者
        public bool UnassignSurvivor(Guid survivorId)
        {
            bool removed = AssignedSurvivorIds.Remove(survivorId);
            if (removed)
            {
                AssignedSurvivorCount.Value = AssignedSurvivorIds.Count; // 更新计数
            }
            return removed;
        }

        public float GetCurrentProductionRatePerSecond(SurvivorModel survivorModel)
        {
            if (AssignedSurvivorIds.Count > 0)
            {
                return BaseProductionRate;
            }
            return 0f;
        }

        // 更新工作站的生产进度，由 WorkstationSystem 每帧调用
        public void UpdateProduction(float deltaTime, SurvivorModel survivorModel, ResourceModel resourceModel)
        {
            if (AssignedSurvivorIds.Count == 0)
            {
                // ProductionProgress.Value = 0f; // 如果需要，在没有幸存者时重置进度
                return;
            }

            float effectiveRate = GetCurrentProductionRatePerSecond(survivorModel);
            ProductionProgress.Value += effectiveRate * deltaTime; // 更新 .Value

            if (ProductionProgress.Value >= ProductionCycleTime)
            {
                int cyclesCompleted = (int)(ProductionProgress.Value / ProductionCycleTime);
                ProductionProgress.Value -= cyclesCompleted * ProductionCycleTime; // 更新 .Value

                for (int i = 0; i < cyclesCompleted; i++)
                {
                    bool canProduce = true;
                    if (InputResourceType.HasValue)
                    {
                        canProduce = resourceModel.ConsumeResource(InputResourceType.Value, InputQuantity);
                    }

                    if (canProduce)
                    {
                        int baseOutput = this.OutputQuantity;
                        int finalOutputQuantity = Mathf.Max(1, (int)(baseOutput * this.ProductionBonusMultiplier) + this.FlatProductionBonus);
                        if (baseOutput <= 0) finalOutputQuantity = 0;

                        resourceModel.AddResource(OutputResourceType, finalOutputQuantity);
                        Debug.Log($"工作站 {Type} (ID: {Id.ToString().Substring(0,4)}) 产出了 {finalOutputQuantity} 单位的 {OutputResourceType} (基础产量: {baseOutput}, 生产倍率: {ProductionBonusMultiplier:F2}, 固定加成: {FlatProductionBonus}, 完成周期 {i+1}/{cyclesCompleted})");
                    }
                    else
                    {
                        Debug.LogWarning($"工作站 {Type} (ID: {Id.ToString().Substring(0,4)}) 在尝试完成周期 {i+1}/{cyclesCompleted} 的生产时失败：所需的输入资源 {InputResourceType.Value} 不足。");
                        break;
                    }
                }
            }
        }
    }
}
