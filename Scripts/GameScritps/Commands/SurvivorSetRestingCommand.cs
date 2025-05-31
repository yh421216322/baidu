using QFramework;
using System; // For Guid
using YourGameNamespace.Survivors; // For ISurvivorManagerSystem

namespace YourGameNamespace.Commands
{
    public class SurvivorSetRestingCommand : AbstractCommand
    {
        private readonly Guid survivorId;
        private readonly bool isResting;

        public SurvivorSetRestingCommand(Guid survivorId, bool isResting)
        {
            this.survivorId = survivorId;
            this.isResting = isResting;
        }

        protected override void OnExecute()
        {
            var survivorManagerSystem = this.GetSystem<ISurvivorManagerSystem>();

            survivorManagerSystem.SurvivorSetResting(this.survivorId, this.isResting);

            UnityEngine.Debug.Log($"命令：设置幸存者 {this.survivorId} 的休息状态为 {this.isResting} 的指令已执行。");
            // UI 更新将通过 Survivor 实体的 Status BindableProperty 自动触发
        }
    }
}
