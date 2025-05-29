using QFramework;
using YourGameNamespace.Events; // 用于 ResourceChangedEvent (资源变更事件)
using UnityEngine;             // 用于 Debug.Log

namespace YourGameNamespace
{
    // 资源数据模型，负责存储和管理游戏中所有类型的资源
    public class ResourceModel : AbstractModel
    {
        private ResourceStorage resourceStorage; // 内部使用的资源存储对象

        // 模型初始化时调用
        protected override void OnInit()
        {
            resourceStorage = new ResourceStorage(); // 创建资源存储实例
            // 可以在此处设置资源的初始值，例如：
            // resourceStorage.AddResource(GameResourceType.Food, 100);
            // resourceStorage.AddResource(GameResourceType.Power, 50);
        }
        
        // 向模型中添加指定数量的某种资源
        public void AddResource(GameResourceType type, int amount)
        {
            if (amount <= 0) return; // 如果尝试添加零或负数数量的资源，则不执行任何操作

            resourceStorage.AddResource(type, amount); // 调用存储对象的添加方法
            int newTotal = resourceStorage.GetAmount(type); // 获取更新后的资源总量
            this.SendEvent(new ResourceChangedEvent(type, newTotal, amount)); // 发送资源变更事件
            // Debug.Log($"资源模型：已添加 {amount} 单位 {type}。新的总量为：{newTotal}"); // 可选的调试日志
        }

        // 从模型中消耗指定数量的某种资源
        // allowForceConsume 参数表示是否允许在资源不足时也执行消耗（例如，消耗到0或负数，取决于ResourceStorage的实现）
        public bool ConsumeResource(GameResourceType type, int amount, bool allowForceConsume = false)
        {
            if (amount <= 0) return false; // 不能消耗零或负数数量的资源
            
            int amountActuallyConsumed = 0; // 实际消耗的数量
            int initialAmount = resourceStorage.GetAmount(type); // 获取消耗前的初始数量

            // 调用存储对象的消耗方法
            bool success = resourceStorage.ConsumeResource(type, amount, allowForceConsume); 
            
            if (success) // 如果消耗操作成功（或部分成功）
            {
                int finalAmount = resourceStorage.GetAmount(type); // 获取消耗后的最终数量
                amountActuallyConsumed = initialAmount - finalAmount; // 计算实际消耗掉的数量

                if (amountActuallyConsumed > 0) // 仅当实际消耗量大于0时才发送事件（避免不必要的事件）
                {
                    // 发送资源变更事件，注意消耗时 ChangeAmount 为负数
                    this.SendEvent(new ResourceChangedEvent(type, finalAmount, -amountActuallyConsumed)); 
                    // Debug.Log($"资源模型：已消耗 {amountActuallyConsumed} 单位 {type}。剩余：{finalAmount}。请求消耗量：{amount}，是否强制消耗：{allowForceConsume}"); // 可选的调试日志
                }
            }
            return success; // 返回消耗操作是否成功
        }

        // 获取指定类型资源的当前数量
        public int GetAmount(GameResourceType type)
        {
            return resourceStorage.GetAmount(type);
        }

        // 检查是否有足够数量的指定类型资源
        public bool HasEnough(GameResourceType type, int amount)
        {
            return resourceStorage.HasEnough(type, amount);
        }
    }
}
