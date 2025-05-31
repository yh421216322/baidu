using QFramework;
using System.Collections.Generic;
using System; // For Guid
using YourGameNamespace.Exploration; // For IExplorationSystem, ExplorationSystem, etc.
// Potentially using YourGameNamespace.Survivors; if survivor validation logic moves into command

namespace YourGameNamespace.Commands
{
    public class StartExpeditionCommand : AbstractCommand
    {
        private readonly string poiId;
        private readonly List<Guid> survivorIds;

        public StartExpeditionCommand(string poiId, List<Guid> survivorIds)
        {
            this.poiId = poiId;
            this.survivorIds = survivorIds ?? new List<Guid>();
        }

        protected override void OnExecute()
        {
            var explorationSystem = this.GetSystem<IExplorationSystem>();
            // 在QFramework中，Command可以直接获取System/Model

            // 调用ExplorationSystem中原有的开始远征逻辑
            // StartExpedition方法内部已经处理了CanStartExpeditionToPOI的检查
            bool success = explorationSystem.StartExpedition(this.poiId, this.survivorIds);

            if (success)
            {
                // 远征成功开始。
                // UI层会通过监听 ExplorationModel 的 Model_ActiveExpeditionAddedEvent 来更新界面。
                UnityEngine.Debug.Log($"命令：远征已开始，目标POI: {this.poiId}, 参与者数量: {this.survivorIds.Count}");
            }
            else
            {
                string failureReason = "未知错误"; // Default reason
                // 尝试获取更具体的失败原因
                // CanStartExpeditionToPOI 需要是 IExplorationSystem 接口的一部分
                // Ensure YourGameNamespace.Events is imported if Command_StartExpeditionFailedEvent is in that namespace
                if (explorationSystem.CanStartExpeditionToPOI(this.poiId, this.survivorIds, out string reasonFromSystem))
                {
                    // 如果CanStart返回true但StartExpedition返回false，说明在检查和执行之间状态变了，或有其他内部逻辑失败
                    failureReason = reasonFromSystem + " (或执行时发生意外)";
                } else {
                    failureReason = reasonFromSystem; // This will be the reason why CanStartExpeditionToPOI returned false
                }
                UnityEngine.Debug.LogWarning($"命令：远征开始失败，目标POI: {this.poiId}。原因: {failureReason}");
                this.SendEvent(new YourGameNamespace.Events.Command_StartExpeditionFailedEvent
                {
                    PoiId = this.poiId,
                    SurvivorIds = this.survivorIds,
                    Reason = failureReason
                });
            }
        }
    }
}
