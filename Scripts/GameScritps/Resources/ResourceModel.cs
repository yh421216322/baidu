using QFramework;
using YourGameNamespace.Events; // 为 ResourceChangedEvent 添加
using UnityEngine; // 用于 Debug.Log

namespace YourGameNamespace
{
    public class ResourceModel : AbstractModel
    {
        private ResourceStorage resourceStorage;

        protected override void OnInit()
        {
            resourceStorage = new ResourceStorage();
            
            
        }
        
        public void AddResource(GameResourceType type, int amount)
        {
            if (amount <= 0) return; // 如果添加零或负数则无变化
            resourceStorage.AddResource(type, amount);
            int newTotal = resourceStorage.GetAmount(type);
            this.SendEvent(new ResourceChangedEvent(type, newTotal, amount));
            // Debug.Log($"资源模型：已添加 {amount} {type}。新的总量：{newTotal}"); // 可选：用于调试
        }

        public bool ConsumeResource(GameResourceType type, int amount, bool allowForceConsume = false)
        {
            if (amount <= 0) return false; // 不能消耗零或负数
            
            int amountActuallyConsumed = 0;
            int initialAmount = resourceStorage.GetAmount(type);

            bool success = resourceStorage.ConsumeResource(type, amount, allowForceConsume);
            
            if (success)
            {
                int finalAmount = resourceStorage.GetAmount(type);
                amountActuallyConsumed = initialAmount - finalAmount; // 计算实际变化量

                if (amountActuallyConsumed > 0) // 仅当实际消耗量大于0时才发送事件
                {
                    this.SendEvent(new ResourceChangedEvent(type, finalAmount, -amountActuallyConsumed)); // 消耗为负数
                    // Debug.Log($"资源模型：已消耗 {amountActuallyConsumed} {type}。剩余：{finalAmount}。请求：{amount}，强制：{allowForceConsume}"); // 可选：用于调试
                }
            }
            return success;
        }

        public int GetAmount(GameResourceType type)
        {
            return resourceStorage.GetAmount(type);
        }

        public bool HasEnough(GameResourceType type, int amount)
        {
            return resourceStorage.HasEnough(type, amount);
        }
    }
}
