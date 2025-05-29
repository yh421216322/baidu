using QFramework;
using UnityEngine; // 用于 Time.deltaTime 和 Debug.Log 等 Unity API
using YourGameNamespace.Survivors;   // 用于 SurvivorModel, SurvivorStatus 等幸存者相关类
using YourGameNamespace.Events;      // 用于 WorkstationBuiltEvent (工作站建成事件)

namespace YourGameNamespace.Workstations
{
    // 工作站系统，负责管理工作站的建造、幸存者分配以及驱动所有工作站的生产更新逻辑
    public class WorkstationSystem : AbstractSystem
    {
        // 对所需数据模型的引用
        private WorkstationModel mWorkstationModel; // 工作站数据模型
        private SurvivorModel mSurvivorModel;     // 幸存者数据模型
        private ResourceModel mResourceModel;     // 资源数据模型 (虽然在此脚本的当前版本中未直接使用，但通常工作站生产会与资源消耗/产出相关，故保留)

        // 系统初始化时调用
        protected override void OnInit()
        {
            // 从 GameArchitecture 获取所需数据模型的实例
            mWorkstationModel = this.GetModel<WorkstationModel>();
            mSurvivorModel = this.GetModel<SurvivorModel>();
            mResourceModel = this.GetModel<ResourceModel>(); // 获取资源模型实例
        }

        // 建造一个新的工作站
        public void BuildWorkstation(WorkstationType type)
        {
            Workstation newStation = new Workstation(type); // 创建新的工作站实例
            mWorkstationModel.AddWorkstation(newStation);  // 将新工作站添加到数据模型中
            // 注意：WorkstationType枚举值的ToString()结果是其英文名。如果日志需要显示中文名，需额外处理。
            Debug.Log($"已成功建造新的工作站：类型为 {type} (ID: {newStation.Id.ToString().Substring(0,4)})");
            this.SendEvent(new WorkstationBuiltEvent(newStation.Type, newStation.Id)); // 发送工作站建成事件
        }

        // 将指定的幸存者分配到指定的工作站
        public void AssignSurvivorToWorkstation(System.Guid survivorId, System.Guid workstationId)
        {
            var survivor = mSurvivorModel.GetSurvivorById(survivorId);         // 获取幸存者对象
            var workstation = mWorkstationModel.GetWorkstationById(workstationId); // 获取工作站对象

            if (survivor != null && workstation != null) // 确保幸存者和工作站都存在
            {
                if (survivor.Status == SurvivorStatus.Idle) // 检查幸存者是否处于空闲状态，可以被分配
                {
                    // 在分配到新工作站之前，首先尝试从该幸存者当前可能分配的任何旧工作站中取消分配
                    if (survivor.WorkstationId.HasValue && survivor.WorkstationId.Value != workstationId)
                    {
                        var previousWorkstation = mWorkstationModel.GetWorkstationById(survivor.WorkstationId.Value);
                        if (previousWorkstation != null)
                        {
                            previousWorkstation.UnassignSurvivor(survivorId); // 从旧工作站取消分配
                            Debug.Log($"幸存者 {survivor.Name} 在被分配到新工作站前，已从其先前所在的工作站 {previousWorkstation.Type} 取消分配。");
                        }
                    }
                    
                    // 尝试将幸存者分配到目标工作站
                    if (workstation.AssignSurvivor(survivorId))
                    {
                        survivor.WorkstationId = workstationId; // 更新幸存者数据中的 WorkstationId
                        survivor.Status = SurvivorStatus.Working; // 更新幸存者状态为“工作中”
                        Debug.Log($"幸存者 {survivor.Name} 已成功分配到工作站 {workstation.Type}。");
                    }
                    else
                    {
                        // 分配失败，可能是因为工作站已满员或该幸存者已被分配到此工作站等原因（具体逻辑在Workstation.AssignSurvivor中）
                        Debug.LogWarning($"未能将幸存者 {survivor.Name} 分配到工作站 {workstation.Type}。工作站拒绝了此次分配（例如：容量已满或该幸存者已被分配）。");
                    }
                }
                else // 如果幸存者当前不处于空闲状态
                {
                     Debug.LogWarning($"未能将幸存者 {survivor.Name} 分配到工作站 {workstation.Type}：该幸存者当前状态为 {survivor.Status}，不是空闲状态。");
                }
            }
            else // 如果幸存者ID或工作站ID无效，导致未能找到对应的对象
            {
                Debug.LogWarning($"未能将幸存者分配到工作站：无法找到指定的幸存者 (ID: {survivorId}) 或工作站 (ID: {workstationId})。");
            }
        }

        // 更新所有工作站的生产状态，由 GameLoop 每帧调用
        public void UpdateAllWorkstations(float deltaTime)
        {
            // 安全检查，确保所有必要的模型都已初始化
            if (mWorkstationModel == null || mSurvivorModel == null || mResourceModel == null)
            {
                Debug.LogError("工作站系统 (WorkstationSystem) 在尝试更新所有工作站时，发现一个或多个必要的模型引用为空。请检查初始化过程。");
                return;
            }
            // 遍历所有已建造的工作站，并调用其各自的生产更新方法
            foreach (var station in mWorkstationModel.GetAllWorkstations())
            {
                // UpdateProduction 方法需要 deltaTime, SurvivorModel (可能用于根据幸存者技能调整效率), 和 ResourceModel (用于消耗输入资源和添加产出资源)
                station.UpdateProduction(deltaTime, mSurvivorModel, mResourceModel);
            }
        }
    }
}
