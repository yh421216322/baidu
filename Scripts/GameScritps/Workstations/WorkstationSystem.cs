using QFramework;
using UnityEngine;
using YourGameNamespace.Survivors;
using YourGameNamespace.Events;
using System;
using YourGameNamespace.Research;
using System.Collections.Generic; // Added for Dictionary
// GameResourceType is in YourGameNamespace. Assuming YourGameNamespace is implicitly included or GameResourceType is defined in YourGameNamespace.
// using YourGameNamespace; // If GameResourceType is directly in YourGameNamespace

namespace YourGameNamespace.Workstations
{
    // IWorkstationSystem interface is now in its own file IWorkstationSystem.cs

    public class WorkstationSystem : AbstractSystem, IController, IWorkstationSystem
    {
        private WorkstationModel mWorkstationModel;
        private SurvivorModel mSurvivorModel;
        private ResourceModel mResourceModel;
        private ISurvivorManagerSystem mSurvivorManagerSystem;

        private Dictionary<WorkstationType, (GameResourceType resource, int amount)> mWorkstationBuildCosts;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        protected override void OnInit()
        {
            mWorkstationModel = this.GetModel<WorkstationModel>();
            mSurvivorModel = this.GetModel<SurvivorModel>();
            mResourceModel = this.GetModel<ResourceModel>();
            mSurvivorManagerSystem = this.GetSystem<ISurvivorManagerSystem>();

            if (mSurvivorManagerSystem == null)
            {
                Debug.LogError("工作站系统 (WorkstationSystem)：未能获取幸存者管理系统 (ISurvivorManagerSystem)！");
            }

            // Initialize build costs
            mWorkstationBuildCosts = new Dictionary<WorkstationType, (GameResourceType resource, int amount)>
            {
                { WorkstationType.Farm, (GameResourceType.Food, 50) }, // Assuming Food is a valid GameResourceType
                { WorkstationType.PowerPlant, (GameResourceType.ElectronicParts, 20) }, // Assuming ElectronicParts is valid
                { WorkstationType.Workshop, (GameResourceType.ElectronicParts, 15) },
                { WorkstationType.Clinic, (GameResourceType.Food, 30) }, // Using Food for Clinic as Medicine resource type might not exist
                { WorkstationType.ResearchLab, (GameResourceType.ElectronicParts, 25) }
            };
        }

        public void ApplyResearchEffectToWorkstation(WorkstationType targetStationType, TechnologyEffectType effectType, float effectValue, GameResourceType targetAffectedResource)
        {
            bool effectApplied = false;
            foreach (var station in this.GetModel<WorkstationModel>().GetAllWorkstations())
            {
                if (station.Type == targetStationType)
                {
                    if (effectType == TechnologyEffectType.IncreaseProductionMultiplier)
                    {
                        station.ProductionBonusMultiplier += effectValue;
                        effectApplied = true;
                        Debug.Log($"科技效果：工作站 {station.Type} (ID: {station.Id}) 的产出乘数增加了 {effectValue}，新乘数: {station.ProductionBonusMultiplier}");
                    }
                    else if (effectType == TechnologyEffectType.IncreaseProductionOutput)
                    {
                        station.FlatProductionBonus += (int)effectValue;
                        effectApplied = true;
                        Debug.Log($"科技效果：工作站 {station.Type} (ID: {station.Id}) 的固定产出增加了 {(int)effectValue}，新固定加成: {station.FlatProductionBonus}");
                    }
                }
            }
            if (!effectApplied)
            {
                Debug.LogWarning($"尝试应用科技效果 {effectType} 到工作站类型 {targetStationType}，但未找到匹配的工作站或效果未被处理。");
            }
        }

        public bool BuildWorkstation(WorkstationType type)
        {
            if (!mWorkstationBuildCosts.TryGetValue(type, out var cost))
            {
                Debug.LogError($"工作站类型 {type} 的建造成本未定义！");
                return false;
            }

            if (!mResourceModel.HasEnough(cost.resource, cost.amount))
            {
                Debug.LogWarning($"建造工作站 {type} 失败：资源 {cost.resource} 不足。需要: {cost.amount}, 当前: {mResourceModel.GetAmount(cost.resource)}");
                return false;
            }

            if (!mResourceModel.ConsumeResource(cost.resource, cost.amount))
            {
                Debug.LogError($"建造工作站 {type} 失败：消耗资源 {cost.resource} ({cost.amount}单位) 失败。");
                return false;
            }
            Debug.Log($"为建造工作站 {type} 已消耗 {cost.amount} 单位 {cost.resource}。");

            Workstation newStation = new Workstation(type);
            mWorkstationModel.AddWorkstation(newStation);
            Debug.Log($"已成功建造新的工作站：类型为 {type} (ID: {newStation.Id.ToString().Substring(0,4)})"); // Localized
            this.SendEvent(new WorkstationBuiltEvent(newStation.Type, newStation.Id));
            return true;
        }

        public void AssignSurvivorToWorkstation(System.Guid survivorId, System.Guid workstationId)
        {
            var survivor = mSurvivorModel.GetSurvivorById(survivorId);
            var workstation = mWorkstationModel.GetWorkstationById(workstationId);

            if (survivor != null && workstation != null)
            {
                // 使用 .Value 访问 BindableProperty 的值
                if (survivor.Status.Value == SurvivorStatus.Idle)
                {
                    // 处理幸存者已在其他工作站的情况
                    if (survivor.WorkstationId.Value.HasValue && survivor.WorkstationId.Value.Value != workstationId)
                    {
                        var previousWorkstation = mWorkstationModel.GetWorkstationById(survivor.WorkstationId.Value.Value);
                        if (previousWorkstation != null)
                        {
                            previousWorkstation.UnassignSurvivor(survivorId);
                            Debug.Log($"幸存者 {survivor.Name.Value} 在被分配到新工作站前，已从其先前所在的工作站 {previousWorkstation.Type} 取消分配。");
                            // 注意：在调用 ClearSurvivorWorkAssignment 之前，确保 previousWorkstation 的 UnassignSurvivor 已完成
                            // 因为 ClearSurvivorWorkAssignment 可能会改变幸存者状态为 Idle
                        }
                        // 清除幸存者在旧工作站的记录并将其状态设为Idle
                        mSurvivorManagerSystem.ClearSurvivorWorkAssignment(survivorId);
                    }
                    
                    if (workstation.AssignSurvivor(survivorId))
                    {
                        // 通过 SurvivorManagerSystem 来更新幸存者的工作状态和工作站ID
                        mSurvivorManagerSystem.AssignSurvivorToWork(survivorId, workstation.Id, workstation.Type);
                        // Debug.Log($"幸存者 {survivor.Name.Value} 已成功分配到工作站 {workstation.Type}。"); // 这条日志现在由AssignSurvivorToWork处理
                    }
                    else
                    {
                        Debug.LogWarning($"未能将幸存者 {survivor.Name.Value} 分配到工作站 {workstation.Type}。工作站拒绝了此次分配（例如：容量已满或该幸存者已被分配）。");
                    }
                }
                else
                {
                     Debug.LogWarning($"未能将幸存者 {survivor.Name.Value} 分配到工作站 {workstation.Type}：该幸存者当前状态为 {survivor.Status.Value}，不是空闲状态。");
                }
            }
            else
            {
                Debug.LogWarning($"未能将幸存者分配到工作站：无法找到指定的幸存者 (ID: {survivorId}) 或工作站 (ID: {workstationId})。");
            }
        }

        // 可选：添加一个显式的解除分配方法，如果需要从外部触发（例如UI按钮）
        public void UnassignSurvivorFromWorkstation(Guid survivorId, Guid workstationId)
        {
            var survivor = mSurvivorModel.GetSurvivorById(survivorId);
            var workstation = mWorkstationModel.GetWorkstationById(workstationId);

            if (survivor != null && workstation != null)
            {
                if (workstation.AssignedSurvivorIds.Contains(survivorId)) // 确保幸存者确实分配在此工作站
                {
                    workstation.UnassignSurvivor(survivorId);
                    mSurvivorManagerSystem.ClearSurvivorWorkAssignment(survivorId); // 更新幸存者状态
                    Debug.Log($"幸存者 {survivor.Name.Value} 已从工作站 {workstation.Type} 手动解除分配。");
                }
                else
                {
                    Debug.LogWarning($"幸存者 {survivor.Name.Value} 并未分配到工作站 {workstation.Type}。");
                }
            }
        }


        // 更新所有工作站的生产状态，由 GameLoop 每帧调用
        public void UpdateAllWorkstations(float deltaTime)
        {
            if (mWorkstationModel == null || mSurvivorModel == null || mResourceModel == null)
            {
                Debug.LogError("工作站系统 (WorkstationSystem) 在尝试更新所有工作站时，发现一个或多个必要的模型引用为空。请检查初始化过程。");
                return;
            }
            foreach (var station in mWorkstationModel.GetAllWorkstations())
            {
                station.UpdateProduction(deltaTime, mSurvivorModel, mResourceModel);
            }
        }
    }
}
