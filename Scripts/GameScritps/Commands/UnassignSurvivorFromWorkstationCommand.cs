using QFramework;
using System; // For Guid
using YourGameNamespace.Workstations;
using YourGameNamespace.Events;   // Added for the result event
using UnityEngine;                // Added for Debug.Log

namespace YourGameNamespace.Commands
{
    public class UnassignSurvivorFromWorkstationCommand : AbstractCommand
    {
        private readonly Guid survivorId;
        private readonly Guid workstationId;

        // Optional: QFramework convention for command-specific completion event
        public struct CompletedEvent {
            public Guid SurvivorId;
            public Guid WorkstationId;
            public bool Success; // Assuming success unless system throws an error
        }

        public UnassignSurvivorFromWorkstationCommand(Guid survivorId, Guid workstationId)
        {
            this.survivorId = survivorId;
            this.workstationId = workstationId;
        }

        protected override void OnExecute()
        {
            var workstationSystem = this.GetSystem<WorkstationSystem>(); // Changed to concrete type

            // WorkstationSystem.UnassignSurvivorFromWorkstation is void,
            // so we assume success if no exceptions are thrown.
            // For more robust error handling, the system method could return a bool or throw specific exceptions.
            bool success = true;
            try
            {
                workstationSystem.UnassignSurvivorFromWorkstation(this.survivorId, this.workstationId);
                Debug.Log($"命令：已执行从工作站 {this.workstationId} 解除分配幸存者 {this.survivorId} 的操作。");
            }
            catch (Exception ex)
            {
                Debug.LogError($"命令：解除分配幸存者 {this.survivorId} 从工作站 {this.workstationId} 时发生错误: {ex.Message}");
                success = false;
            }

            // Send the general result event
            this.SendEvent(new UnassignSurvivorFromWorkstationResultEvent {
                SurvivorId = this.survivorId,
                WorkstationId = this.workstationId,
                Success = success
            });

            // Send the command-specific completed event (QFramework convention)
            this.SendEvent(new CompletedEvent { SurvivorId = this.survivorId, WorkstationId = this.workstationId, Success = success });
        }
    }
}
