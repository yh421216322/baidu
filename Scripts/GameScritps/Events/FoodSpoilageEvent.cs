using QFramework; 

using UnityEngine;

namespace YourGameNamespace.Events
{
    public class FoodSpoilageEvent : RandomEvent
    {
        public FoodSpoilageEvent() 
        { 
            Title = "Food Spoilage!"; 
        }

        public override void Execute(IArchitecture architecture)
        {
            var resourceModel = architecture.GetModel<ResourceModel>();
            if (resourceModel == null)
            {
                Description = "食物腐败事件失败：未找到ResourceModel。";
                Debug.LogError(Description);
                return;
            }

            int currentFood = resourceModel.GetAmount(GameResourceType.Food);
            int foodLost = 0;
            if (currentFood > 0) // 只有在有食物时才会损失
            {
                foodLost = Random.Range(10, Mathf.Max(11, (int)(currentFood * 0.2f)));
                foodLost = Mathf.Min(currentFood, foodLost); // 确保损失量不超过现有量
                
                // 使用 allowForceConsume 参数，在此上下文中表示“消耗可用量，直至达到指定数量”
                resourceModel.ConsumeResource(GameResourceType.Food, foodLost, true); 
            }
            
            if (foodLost > 0)
            {
                Description = $"一部分食物储备已腐败！损失 {foodLost} 食物。";
                Debug.LogWarning(Description);
            }
            else
            {
                Description = "食物腐败警报，但幸运的是没有食物损失（或者没有食物可损失）！";
                Debug.Log(Description);
            }
        }
    }
}
