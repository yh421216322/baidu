using QFramework;
using System; // For Guid
using YourGameNamespace.Workstations; // For IWorkstationSystem

namespace YourGameNamespace.Commands
{
    public class UnassignSurvivorFromWorkstationCommand : AbstractCommand
    {
        private readonly Guid survivorId;
        private readonly Guid workstationId;

        public UnassignSurvivorFromWorkstationCommand(Guid survivorId, Guid workstationId)
        {
            this.survivorId = survivorId;
            this.workstationId = workstationId;
        }

        protected override void OnExecute()
        {
            var workstationSystem = this.GetSystem<IWorkstationSystem>();

            // WorkstationSystem.UnassignSurvivorFromWorkstation 方法内部会处理
            // Workstation实体内部列表的更新和与SurvivorManagerSystem的交互。
            workstationSystem.UnassignSurvivorFromWorkstation(this.survivorId, this.workstationId);

            UnityEngine.Debug.Log($"命令：尝试从工作站 {this.workstationId} 解除分配幸存者 {this.survivorId} 的指令已执行。");
        }
    }
}
