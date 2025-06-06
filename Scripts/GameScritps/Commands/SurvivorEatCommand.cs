using QFramework;
using System; // For Guid
using YourGameNamespace.Survivors; // For ISurvivorManagerSystem

namespace YourGameNamespace.Commands
{
    public class SurvivorEatCommand : AbstractCommand
    {
        private readonly Guid survivorId;
        private readonly int foodAmountToConsume;

        public SurvivorEatCommand(Guid survivorId, int amount)
        {
            this.survivorId = survivorId;
            this.foodAmountToConsume = amount;
        }

        protected override void OnExecute()
        {
            var survivorManagerSystem = this.GetSystem<ISurvivorManagerSystem>();

            bool success = survivorManagerSystem.SurvivorTryEat(this.survivorId, this.foodAmountToConsume);

            if (success)
            {
                UnityEngine.Debug.Log($"命令：幸存者 {this.survivorId} 尝试进食 {this.foodAmountToConsume} 单位食物成功。");
                // UI 更新将通过 Survivor 实体的 FoodLevel BindableProperty 自动触发
            }
            else
            {
                UnityEngine.Debug.LogWarning($"命令：幸存者 {this.survivorId} 尝试进食 {this.foodAmountToConsume} 单位食物失败 (可能食物不足)。");
                // 可选：发送一个 System_SurvivorEatFailedEvent(survivorId, reason)
            }
        }
    }
}
