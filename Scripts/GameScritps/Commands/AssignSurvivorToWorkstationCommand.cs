using QFramework;
using System; // For Guid
using YourGameNamespace.Workstations; // For IWorkstationSystem

namespace YourGameNamespace.Commands
{
    public class AssignSurvivorToWorkstationCommand : AbstractCommand
    {
        private readonly Guid survivorId;
        private readonly Guid workstationId;

        public AssignSurvivorToWorkstationCommand(Guid survivorId, Guid workstationId)
        {
            this.survivorId = survivorId;
            this.workstationId = workstationId;
        }

        protected override void OnExecute()
        {
            var workstationSystem = this.GetSystem<IWorkstationSystem>();

            // WorkstationSystem.AssignSurvivorToWorkstation 方法内部会处理幸存者状态检查、
            // 与SurvivorManagerSystem的交互等。
            // 目前该方法没有bool返回值，Command执行后，UI依赖Model/Entity的事件/BindableProperty更新。
            workstationSystem.AssignSurvivorToWorkstation(this.survivorId, this.workstationId);

            // 可以添加一个简单的日志确认Command已被执行
            UnityEngine.Debug.Log($"命令：尝试分配幸存者 {this.survivorId} 到工作站 {this.workstationId} 的指令已执行。");

            // 如果 AssignSurvivorToWorkstation 未来返回bool或抛出特定异常，可以在此处理并发送结果事件。
        }
    }
}
