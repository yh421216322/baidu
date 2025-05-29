using QFramework; 

using UnityEngine;

namespace YourGameNamespace.Events
{
    // 食物腐败随机事件
    public class FoodSpoilageEvent : RandomEvent
    {
        public FoodSpoilageEvent() 
        { 
            Title = "食物腐败！"; // 事件标题
        }

        public override void Execute(IArchitecture architecture)
        {
            var resourceModel = architecture.GetModel<ResourceModel>();
            if (resourceModel == null)
            {
                Description = "食物腐败事件执行失败：未找到ResourceModel。"; // 事件描述：失败情况
                Debug.LogError(Description);
                return;
            }

            int currentFood = resourceModel.GetAmount(GameResourceType.Food); // 获取当前食物量
            int foodLost = 0; // 损失的食物量
            if (currentFood > 0) // 只有在有食物的情况下才会发生损失
            {
                // 随机损失10到当前食物量20%之间的食物（至少损失10，但如果20%小于10则取较大者，即至少11）
                foodLost = Random.Range(10, Mathf.Max(11, (int)(currentFood * 0.2f))); 
                foodLost = Mathf.Min(currentFood, foodLost); // 确保损失量不超过当前拥有的食物量
                
                // 使用 allowForceConsume 参数，在此上下文中表示“消耗可用量，直至达到指定数量”
                // 实际上是强制消耗，因为我们已经计算了实际能损失的最大量
                resourceModel.ConsumeResource(GameResourceType.Food, foodLost, true); 
            }
            
            if (foodLost > 0)
            {
                Description = $"一部分食物储备已腐败！损失了 {foodLost} 单位食物。"; // 事件描述：成功损失食物
                Debug.LogWarning(Description);
            }
            else
            {
                Description = "食物腐败警报！但幸运的是，没有食物损失（或者当前没有食物可供损失）。"; // 事件描述：没有实际损失
                Debug.Log(Description);
            }
        }
    }
}
