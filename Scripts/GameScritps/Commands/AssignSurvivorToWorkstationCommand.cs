using QFramework;
using System; // For Guid
using YourGameNamespace.Workstations;
using YourGameNamespace.Events; // For the new event
using UnityEngine; // For Debug.Log

namespace YourGameNamespace.Commands
{
    public class AssignSurvivorToWorkstationCommand : AbstractCommand
    {
        private readonly Guid survivorId;
        private readonly Guid workstationId;

        // Optional: QFramework convention for command-specific completion event
        public struct CompletedEvent {
            public Guid SurvivorId;
            public Guid WorkstationId;
            public bool Success;
        }

        public AssignSurvivorToWorkstationCommand(Guid survivorId, Guid workstationId)
        {
            this.survivorId = survivorId;
            this.workstationId = workstationId;
        }

        protected override void OnExecute()
        {
            var workstationSystem = this.GetSystem<IWorkstationSystem>();
            // AssignSurvivorToWorkstation now returns a bool
            bool success = workstationSystem.AssignSurvivorToWorkstation(this.survivorId, this.workstationId);

            string logMessage;
            string failureKey = ""; // Used for localization key if needed

            if (success)
            {
                logMessage = $"命令：成功分配幸存者 {this.survivorId} 到工作站 {this.workstationId}。";
                Debug.Log(logMessage);
            }
            else
            {
                logMessage = $"命令：分配幸存者 {this.survivorId} 到工作站 {this.workstationId} 失败。具体原因请查看WorkstationSystem或Workstation类的日志。";
                failureKey = "ASSIGN_FAIL_GENERAL"; // Example general failure key
                // More specific keys could be set by WorkstationSystem if it returned a detailed result object
                Debug.LogWarning(logMessage);
            }

            // Send the general result event for broader system listening
            this.SendEvent(new AssignSurvivorToWorkstationResultEvent {
                SurvivorId = this.survivorId,
                WorkstationId = this.workstationId,
                Success = success,
                FailureReasonKey = failureKey
            });

            // Send the command-specific completed event (QFramework convention, optional)
            this.SendEvent(new CompletedEvent { SurvivorId = this.survivorId, WorkstationId = this.workstationId, Success = success });
        }
    }
}
