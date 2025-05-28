using QFramework;
using UnityEngine; // 用于 Time.deltaTime 和 Debug
using YourGameNamespace.Survivors;
using YourGameNamespace.Events;   // 用于 WorkstationBuiltEvent

namespace YourGameNamespace.Workstations
{
    public class WorkstationSystem : AbstractSystem
    {
        private WorkstationModel mWorkstationModel;
        private SurvivorModel mSurvivorModel;
        private ResourceModel mResourceModel;

        protected override void OnInit()
        {
           
            mWorkstationModel = this.GetModel<WorkstationModel>();
            mSurvivorModel = this.GetModel<SurvivorModel>();
            mResourceModel = this.GetModel<ResourceModel>();
        }

        public void BuildWorkstation(WorkstationType type)
        {
            Workstation newStation = new Workstation(type);
            mWorkstationModel.AddWorkstation(newStation);
            Debug.Log($"已建造新的 {type} (ID: {newStation.Id.ToString().Substring(0,4)})");
            this.SendEvent(new WorkstationBuiltEvent(newStation.Type, newStation.Id)); // 发送事件
        }

        public void AssignSurvivorToWorkstation(System.Guid survivorId, System.Guid workstationId)
        {
            var survivor = mSurvivorModel.GetSurvivorById(survivorId);
            var workstation = mWorkstationModel.GetWorkstationById(workstationId);

            if (survivor != null && workstation != null)
            {
                if (survivor.Status == SurvivorStatus.Idle)
                {
                    // 首先尝试从任何先前的工作站取消分配
                    if (survivor.WorkstationId.HasValue && survivor.WorkstationId.Value != workstationId)
                    {
                        var previousWorkstation = mWorkstationModel.GetWorkstationById(survivor.WorkstationId.Value);
                        if (previousWorkstation != null)
                        {
                            previousWorkstation.UnassignSurvivor(survivorId);
                            Debug.Log($"{survivor.Name} 在移动前已从 {previousWorkstation.Type} 取消分配。");
                        }
                    }
                    
                    if (workstation.AssignSurvivor(survivorId))
                    {
                        survivor.WorkstationId = workstationId; // 在 Survivor.cs 中更新
                        survivor.Status = SurvivorStatus.Working;
                        Debug.Log($"{survivor.Name} 已分配到 {workstation.Type}。");
                    }
                    else
                    {
                        Debug.LogWarning($"未能将 {survivor.Name} 分配到 {workstation.Type} - 工作站拒绝分配（例如容量已满或已被分配）。");
                    }
                }
                else
                {
                     Debug.LogWarning($"未能将 {survivor.Name} 分配到 {workstation.Type}：幸存者不是空闲状态 (状态：{survivor.Status})。");
                }
            }
            else
            {
                Debug.LogWarning($"未能将幸存者分配到工作站：未找到幸存者 (ID: {survivorId}) 或工作站 (ID: {workstationId})。");
            }
        }

        public void UpdateAllWorkstations(float deltaTime)
        {
            if (mWorkstationModel == null || mSurvivorModel == null || mResourceModel == null)
            {
                Debug.LogError("工作站系统 (WorkstationSystem) 缺少模型引用。");
                return;
            }
            foreach (var station in mWorkstationModel.GetAllWorkstations())
            {
                station.UpdateProduction(deltaTime, mSurvivorModel, mResourceModel);
            }
        }
    }
}
