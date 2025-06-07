using QFramework;
using UnityEngine;
using YourGameNamespace.Survivors;
using YourGameNamespace.Events;
using System;
using YourGameNamespace.Research;
using System.Collections.Generic;
using MyGameNamespace; // Assuming RegisterManager and concrete models/systems might be here or global
using YourGameNamespace.Buildings;
using YourGameNamespace.Framework; // For concrete GameDataModel if not covered by MyGameNamespace

namespace YourGameNamespace.Workstations
{
    public interface IWorkstationSystem : QFramework.QFISystem
    {
        void ApplyResearchEffectToWorkstation(WorkstationType targetStationType, TechnologyEffectType effectType, float effectValue, GameResourceType targetAffectedResource);

        /// <summary>
        /// 尝试建造或注册一个工作站。
        /// </summary>
        /// <param name="type">要建造的工作站类型。</param>
        /// <param name="costsAlreadyHandled">指示建造成本是否已在别处处理 (例如由BuildingSystem处理)。</param>
        /// <param name="associatedBuildingId">如果此工作站关联一个Building实体，则提供其ID。</param>
        /// <returns>如果成功则返回true，否则返回false。</returns>
        bool BuildWorkstation(WorkstationType type, bool costsAlreadyHandled = false, Guid? associatedBuildingId = null);

        bool AssignSurvivorToWorkstation(Guid survivorId, Guid workstationId);
        void UnassignSurvivorFromWorkstation(Guid survivorId, Guid workstationId);
        void UpdateAllWorkstations(float deltaTime);
        // List<(GameResourceType resource, int amount)> GetWorkstationBuildCosts(WorkstationType type); // Conceptual, for BuildingSystem
    }

    // Removed IController as Systems are primarily for logic, not direct UI control.
    public class WorkstationSystem : AbstractSystem, IWorkstationSystem
    {
        private WorkstationModel mWorkstationModel;
        private SurvivorModel mSurvivorModel;
        private ResourceModel mResourceModel;
        private SurvivorManagerSystem mSurvivorManagerSystem;

        private Dictionary<WorkstationType, (GameResourceType resource, int amount)> mWorkstationBuildCosts;

        // GetArchitecture is implicitly provided by AbstractSystem if RegisterManager.Interface is set up as the architecture.
        // If direct architecture access is needed and RegisterManager is the way:
        // private IArchitecture architecture => RegisterManager.Interface;

        protected override void OnInit()
        {
            mWorkstationModel = this.GetModel<WorkstationModel>();
            mSurvivorModel = this.GetModel<SurvivorModel>();
            mResourceModel = this.GetModel<ResourceModel>();
            mSurvivorManagerSystem = this.GetSystem<SurvivorManagerSystem>();

            if (mSurvivorManagerSystem == null)
            {
                Debug.LogError("工作站系统 (WorkstationSystem)：未能获取幸存者管理系统 (SurvivorManagerSystem)！");
            }

            InitializeBuildCosts();

            // 注册监听建筑建造完成事件
            this.RegisterEvent<Model_BuildingConstructedEvent>(OnBuildingConstructed)
                .UnRegisterWhenDisposed(this); // Corrected unregistration for AbstractSystem
        }

        private void InitializeBuildCosts()
        {
            // Initialize build costs
            mWorkstationBuildCosts = new Dictionary<WorkstationType, (GameResourceType resource, int amount)>
            {
                { WorkstationType.Farm, (GameResourceType.Food, 50) },
                { WorkstationType.PowerPlant, (GameResourceType.ElectronicParts, 20) },
                { WorkstationType.Workshop, (GameResourceType.ElectronicParts, 15) },
                { WorkstationType.Clinic, (GameResourceType.Food, 30) },
                { WorkstationType.ResearchLab, (GameResourceType.ElectronicParts, 25) }
            };
        }

        private void OnBuildingConstructed(Model_BuildingConstructedEvent e)
        {
            if (BuildingSystem.IsWorkstationEquivalent(e.BuildingData.Type, out WorkstationType workstationType))
            {
                Debug.Log($"工作站系统：检测到建筑 {e.BuildingData.Type} (ID: {e.BuildingData.Id}) 已建造，将创建对应的工作站实体 (成本已处理)。");
                bool success = BuildWorkstation(workstationType, true, e.BuildingData.Id);
                if (!success) {
                   Debug.LogError($"工作站系统：为建筑 {e.BuildingData.Type} (BuildingID: {e.BuildingData.Id}) 创建对应的工作站实体失败！这可能表示逻辑错误，因为成本已处理。");
                }
            }
        }

        public void ApplyResearchEffectToWorkstation(WorkstationType targetStationType, TechnologyEffectType effectType, float effectValue, GameResourceType targetAffectedResource)
        {
            bool effectApplied = false;
            // Assuming GetModel<WorkstationModel>() is the correct way if mWorkstationModel is not directly used here for some reason
            // However, it's a field, so it should be used.
            foreach (var station in mWorkstationModel.GetAllWorkstations())
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

        public bool BuildWorkstation(WorkstationType type, bool costsAlreadyHandled = false, Guid? associatedBuildingId = null)
        {
            if (!costsAlreadyHandled)
            {
                if (!mWorkstationBuildCosts.TryGetValue(type, out var cost))
                {
                    Debug.LogError($"工作站类型 {type} 的建造成本未定义！");
                    return false;
                }

                if (mResourceModel == null) { // Added null check for mResourceModel
                     Debug.LogError($"建造工作站 {type} 失败: ResourceModel 未初始化。"); return false;
                }

                if (!mResourceModel.HasEnough(cost.resource, cost.amount))
                {
                    Debug.LogWarning($"建造工作站 {type} 失败：资源 {cost.resource} 不足。需要: {cost.amount}, 当前: {mResourceModel.GetAmount(cost.resource)}。");
                    return false;
                }

                if (!mResourceModel.ConsumeResource(cost.resource, cost.amount))
                {
                    Debug.LogError($"建造工作站 {type} 失败：消耗资源 {cost.resource} ({cost.amount}单位) 失败。");
                    return false;
                }
                Debug.Log($"为建造工作站 {type} 已消耗 {cost.amount} 单位 {cost.resource} (由工作站系统处理)。");
            }
            else
            {
                Debug.Log($"为工作站 {type} (关联建筑ID: {associatedBuildingId?.ToString() ?? "N/A"}) 创建逻辑实体，成本已由建筑系统处理。");
            }

            if (mWorkstationModel == null) { // Added null check for mWorkstationModel
                 Debug.LogError($"注册工作站 {type} 失败: WorkstationModel 未初始化。"); return false;
            }
            Workstation newStation = new Workstation(type, associatedBuildingId);
            mWorkstationModel.AddWorkstation(newStation);
            Debug.Log($"已成功注册新的工作站逻辑实体：类型为 {type} (ID: {newStation.Id.ToString().Substring(0,4)}, 关联建筑ID: {newStation.AssociatedBuildingId?.ToString() ?? "无"})");
            this.SendEvent(new WorkstationBuiltEvent(newStation.Type, newStation.Id));
            return true;
        }

        public bool AssignSurvivorToWorkstation(System.Guid survivorId, System.Guid workstationId)
        {
            if (mSurvivorModel == null || mWorkstationModel == null || mSurvivorManagerSystem == null) {
                Debug.LogError("AssignSurvivorToWorkstation 失败: 核心模型或系统未初始化。"); return false;
            }

            var survivor = mSurvivorModel.GetSurvivorById(survivorId);
            var workstation = mWorkstationModel.GetWorkstationById(workstationId);

            if (survivor == null)
            {
                Debug.LogWarning($"分配失败：未能找到幸存者 (ID: {survivorId})。");
                return false;
            }
            if (workstation == null)
            {
                Debug.LogWarning($"分配失败：未能找到工作站 (ID: {workstationId})。");
                return false;
            }

            if (survivor.Status.Value != SurvivorStatus.Idle)
            {
                Debug.LogWarning($"未能将幸存者 {survivor.Name.Value} 分配到工作站 {workstation.Type}：该幸存者当前状态为 {survivor.Status.Value}，不是空闲状态。");
                return false;
            }

            if (survivor.WorkstationId.Value.HasValue && survivor.WorkstationId.Value.Value != workstationId)
            {
                var previousWorkstation = mWorkstationModel.GetWorkstationById(survivor.WorkstationId.Value.Value);
                if (previousWorkstation != null)
                {
                    previousWorkstation.UnassignSurvivor(survivorId);
                    Debug.Log($"幸存者 {survivor.Name.Value} 在被分配到新工作站前，已从其先前所在的工作站 {previousWorkstation.Type} 取消分配。");
                }
                mSurvivorManagerSystem.ClearSurvivorWorkAssignment(survivorId);
            }

            if (workstation.AssignSurvivor(survivorId))
            {
                mSurvivorManagerSystem.AssignSurvivorToWork(survivorId, workstation.Id, workstation.Type);
                Debug.Log($"幸存者 {survivor.Name.Value} 已成功分配到工作站 {workstation.Type}。");
                return true;
            }
            else
            {
                return false;
            }
        }

        public void UnassignSurvivorFromWorkstation(Guid survivorId, Guid workstationId)
        {
             if (mSurvivorModel == null || mWorkstationModel == null || mSurvivorManagerSystem == null) {
                Debug.LogError("UnassignSurvivorFromWorkstation 失败: 核心模型或系统未初始化。"); return;
            }
            var survivor = mSurvivorModel.GetSurvivorById(survivorId);
            var workstation = mWorkstationModel.GetWorkstationById(workstationId);

            if (survivor != null && workstation != null)
            {
                if (workstation.AssignedSurvivorIds.Contains(survivorId))
                {
                    workstation.UnassignSurvivor(survivorId);
                    mSurvivorManagerSystem.ClearSurvivorWorkAssignment(survivorId);
                    Debug.Log($"幸存者 {survivor.Name.Value} 已从工作站 {workstation.Type} 手动解除分配。");
                }
                else
                {
                    Debug.LogWarning($"幸存者 {survivor.Name.Value} 并未分配到工作站 {workstation.Type}。");
                }
            } else {
                 Debug.LogWarning($"解除分配失败: 未找到幸存者(ID:{survivorId})或工作站(ID:{workstationId})。");
            }
        }

        public void UpdateAllWorkstations(float deltaTime)
        {
            if (mWorkstationModel == null || mSurvivorModel == null || mResourceModel == null)
            {
                Debug.LogError("工作站系统 (WorkstationSystem) 在尝试更新所有工作站时，发现一个或多个必要的模型引用为空。请检查初始化过程。");
                return;
            }
            foreach (var station in mWorkstationModel.GetAllWorkstations()) // Use field instead of GetModel
            {
                station.UpdateProduction(deltaTime, mSurvivorModel, mResourceModel);
            }
        }

        // Conceptual method for BuildingSystem to fetch costs - needs to be added to IWorkstationSystem if used by BuildingSystem
        // public List<(GameResourceType resource, int amount)> GetWorkstationBuildCosts(WorkstationType type)
        // {
        //     if (mWorkstationBuildCosts.TryGetValue(type, out var costs))
        //     {
        //         return costs; // Consider returning a copy if external modification is a concern
        //     }
        //     Debug.LogWarning($"工作站类型 {type} 的建造成本在WorkstationSystem中未定义。");
        //     return null;
        // }
    }
}
