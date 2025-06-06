using QFramework;
using YourGameNamespace.Workstations; // For IWorkstationSystem, WorkstationType

namespace YourGameNamespace.Commands
{
    public class BuildWorkstationCommand : AbstractCommand
    {
        private readonly WorkstationType stationType;

        public BuildWorkstationCommand(WorkstationType type)
        {
            this.stationType = type;
        }

        protected override void OnExecute()
        {
            var workstationSystem = this.GetSystem<IWorkstationSystem>();

            bool success = workstationSystem.BuildWorkstation(this.stationType);

            if (success)
            {
                // 工作站成功建造。
                // UI 层将通过监听 WorkstationModel 的 Model_WorkstationRegisteredEvent
                // (在 WorkstationSystem.BuildWorkstation 内部的 AddWorkstation 调用后触发) 来更新界面。
                // WorkstationSystem.BuildWorkstation 内部已经有成功建造的日志。
                UnityEngine.Debug.Log($"命令：建造工作站 {this.stationType} 的指令已成功执行。");
            }
            else
            {
                // 建造失败 (原因由 WorkstationSystem.BuildWorkstation 内部的Debug.LogWarning说明，例如资源不足)
                UnityEngine.Debug.LogWarning($"命令：建造工作站 {this.stationType} 的指令执行失败（例如资源不足或类型未定义成本）。");
            }
            // 可选：如果需要，可以在此发送一个 BuildWorkstationResultEvent(bool success, WorkstationType type, string reason)
            // 以便UI可以给出更直接的反馈，特别是失败原因。
            // 目前依赖WorkstationSystem的日志和Model的事件。
        }
    }
}
