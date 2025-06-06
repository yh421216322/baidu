using QFramework;
using UnityEngine;
using YourGameNamespace.Survivors;
using YourGameNamespace.Events;
using System;
using YourGameNamespace.Research;
using System.Collections.Generic;
using MyGameNamespace; // Added for Dictionary
// GameResourceType is in YourGameNamespace. Assuming YourGameNamespace is implicitly included or GameResourceType is defined in YourGameNamespace.
// using YourGameNamespace; // If GameResourceType is directly in YourGameNamespace

namespace YourGameNamespace.Workstations
{
    // Interface definition moved here
using YourGameNamespace.Buildings; // Required for BuildingSystem.IsWorkstationEquivalent and Model_BuildingConstructedEvent

namespace YourGameNamespace.Workstations
{
    // Interface definition moved here
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
        bool AssignSurvivorToWorkstation(Guid survivorId, Guid workstationId); // Changed to return bool
        void UnassignSurvivorFromWorkstation(Guid survivorId, Guid workstationId);
        void UpdateAllWorkstations(float deltaTime);
    }

    public class WorkstationSystem : AbstractSystem, IController, IWorkstationSystem
    {
        private WorkstationModel mWorkstationModel;
        private SurvivorModel mSurvivorModel;
        private ResourceModel mResourceModel;
        private SurvivorManagerSystem mSurvivorManagerSystem;

        private Dictionary<WorkstationType, (GameResourceType resource, int amount)> mWorkstationBuildCosts;

        public IArchitecture GetArchitecture() => RegisterManager.Interface;

        protected override void OnInit()
        {
            mWorkstationModel = this.GetModel<WorkstationModel>();
            mSurvivorModel = this.GetModel<SurvivorModel>();
            mResourceModel = this.GetModel<ResourceModel>();
            mSurvivorManagerSystem = this.GetSystem<SurvivorManagerSystem>();

            if (mSurvivorManagerSystem == null)
            {
                Debug.LogError("工作站系统 (WorkstationSystem)：未能获取幸存者管理系统 (ISurvivorManagerSystem)！");
            }

            // Initialize build costs
            mWorkstationBuildCosts = new Dictionary<WorkstationType, (GameResourceType resource, int amount)>
            {
                { WorkstationType.Farm, (GameResourceType.Food, 50) },
                { WorkstationType.PowerPlant, (GameResourceType.ElectronicParts, 20) },
                { WorkstationType.Workshop, (GameResourceType.ElectronicParts, 15) },
                { WorkstationType.Clinic, (GameResourceType.Food, 30) },
                { WorkstationType.ResearchLab, (GameResourceType.ElectronicParts, 25) }
            };

            // 注册监听建筑建造完成事件
            this.RegisterEvent<Model_BuildingConstructedEvent>(OnBuildingConstructed)
                .UnRegisterWhenGameObjectDestroyed(ArchBindable.gameObject); // Assuming ArchBindable gives a context or use a placeholder GO if system is not MonoBehaviour
        }

        private void OnBuildingConstructed(Model_BuildingConstructedEvent e)
        {
            if (BuildingSystem.IsWorkstationEquivalent(e.BuildingData.Type, out WorkstationType workstationType))
            {
                Debug.Log($"工作站系统：检测到建筑 {e.BuildingData.Type} (ID: {e.BuildingData.Id}) 已建造，将创建对应的工作站实体 (成本已处理)。");
                // 调用BuildWorkstation，并标记成本已被BuildingSystem处理，传递关联的Building ID
                bool success = BuildWorkstation(workstationType, true, e.BuildingData.Id);
                if (!success) {
                   Debug.LogError($"工作站系统：为建筑 {e.BuildingData.Type} (BuildingID: {e.BuildingData.Id}) 创建对应的工作站实体失败！这可能表示逻辑错误，因为成本已处理。");
                }
            }
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

        public bool BuildWorkstation(WorkstationType type, bool costsAlreadyHandled = false, Guid? associatedBuildingId = null)
        {
            if (!costsAlreadyHandled) // 仅当成本未被处理时，才检查和消耗资源
            {
                if (!mWorkstationBuildCosts.TryGetValue(type, out var cost))
                {
                    Debug.LogError($"工作站类型 {type} 的建造成本未定义！");
                    return false;
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

            // 将 associatedBuildingId 传递给 Workstation 构造函数
            Workstation newStation = new Workstation(type, associatedBuildingId);
            mWorkstationModel.AddWorkstation(newStation);
            Debug.Log($"已成功注册新的工作站逻辑实体：类型为 {type} (ID: {newStation.Id.ToString().Substring(0,4)}, 关联建筑ID: {newStation.AssociatedBuildingId?.ToString() ?? "无"})");
            this.SendEvent(new WorkstationBuiltEvent(newStation.Type, newStation.Id)); // 事件依旧发送，表明一个可工作的站台已就绪
            return true;
        }

        public bool AssignSurvivorToWorkstation(System.Guid survivorId, System.Guid workstationId)
        {
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

            // 使用 .Value 访问 BindableProperty 的值
            if (survivor.Status.Value != SurvivorStatus.Idle)
            {
                Debug.LogWarning($"未能将幸存者 {survivor.Name.Value} 分配到工作站 {workstation.Type}：该幸存者当前状态为 {survivor.Status.Value}，不是空闲状态。");
                return false;
            }

            // 处理幸存者已在其他工作站的情况
            if (survivor.WorkstationId.Value.HasValue && survivor.WorkstationId.Value.Value != workstationId)
            {
                var previousWorkstation = mWorkstationModel.GetWorkstationById(survivor.WorkstationId.Value.Value);
                if (previousWorkstation != null)
                {
                    previousWorkstation.UnassignSurvivor(survivorId); // This already updates AssignedSurvivorCount
                    Debug.Log($"幸存者 {survivor.Name.Value} 在被分配到新工作站前，已从其先前所在的工作站 {previousWorkstation.Type} 取消分配。");
                }
                // ClearSurvivorWorkAssignment will set status to Idle, which is fine before re-assignment
                mSurvivorManagerSystem.ClearSurvivorWorkAssignment(survivorId);
            }

            // workstation.AssignSurvivor 现在返回 bool 并处理容量检查
            if (workstation.AssignSurvivor(survivorId))
            {
                // 通过 SurvivorManagerSystem 来更新幸存者的工作状态和工作站ID
                mSurvivorManagerSystem.AssignSurvivorToWork(survivorId, workstation.Id, workstation.Type);
                Debug.Log($"幸存者 {survivor.Name.Value} 已成功分配到工作站 {workstation.Type}。");
                // this.SendEvent(new WorkstationAssignmentsChangedEvent(workstationId)); // Send specific event (optional, covered by general result event now)
                return true;
            }
            else
            {
                // workstation.AssignSurvivor 内部会记录具体原因 (如已满或已分配)
                // Debug.LogWarning($"未能将幸存者 {survivor.Name.Value} 分配到工作站 {workstation.Type}。工作站拒绝了此次分配。"); // Redundant if Workstation.AssignSurvivor logs
                return false;
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
