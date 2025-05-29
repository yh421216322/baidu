using QFramework; 
using YourGameNamespace.Survivors; 
using UnityEngine; 
using System.Linq;

namespace YourGameNamespace.Events
{
    // 幸存者生病随机事件
    public class SurvivorSicknessEvent : RandomEvent
    {
        public SurvivorSicknessEvent() 
        { 
            Title = "突发疾病"; // 事件标题
        }

        public override void Execute(IArchitecture architecture)
        {
            var survivorModel = architecture.GetModel<SurvivorModel>();
            if (survivorModel == null)
            {
                Description = "幸存者疾病事件执行失败：未找到SurvivorModel。"; // 事件描述：失败情况
                Debug.LogError(Description);
                return;
            }

            // 获取所有非受伤且非需要关注状态的幸存者列表
            var healthySurvivors = survivorModel.GetAllSurvivors()
                .Where(s => s.Status != SurvivorStatus.Injured && s.Status != SurvivorStatus.NeedsAttention)
                .ToList();

            if (healthySurvivors.Any()) // 如果存在健康的幸存者
            {
                // 从健康幸存者中随机选择一个“倒霉蛋”
                var unluckySurvivor = healthySurvivors[Random.Range(0, healthySurvivors.Count)];
                
                // 将其状态更改为受伤。
                // 注意：如果该幸存者正在工作，理想情况下应有逻辑使其停止工作并从工作站解绑。
                // 当前实现仅更改状态。理论上，受伤状态应能阻止其在其他地方工作或被分配。
                unluckySurvivor.Status = SurvivorStatus.Injured; 
                Description = $"{unluckySurvivor.Name} 突然生病，现在处于受伤状态！"; // 事件描述：有幸存者生病
                Debug.LogWarning(Description);
            }
            else 
            { 
                Description = "一阵疾病袭来，但幸运的是，当前没有健康的幸存者可以被感染（所有人都已处于特殊状态）。"; // 事件描述：没有幸存者生病
                Debug.Log(Description); 
            }
        }
    }
}
