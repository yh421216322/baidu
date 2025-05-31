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
            var survivorManagerSystem = architecture.GetSystem<ISurvivorManagerSystem>(); // 获取ISurvivorManagerSystem

            if (survivorModel == null || survivorManagerSystem == null)
            {
                Description = "幸存者疾病事件执行失败：未找到SurvivorModel或ISurvivorManagerSystem。";
                Debug.LogError(Description);
                return;
            }

            // 获取所有非受伤且非需要关注状态的幸存者列表
            // 注意: Survivor.Status 现在是 BindableProperty，需要访问 .Value
            var healthySurvivors = survivorModel.GetAllSurvivors()
                .Where(s => s.Status.Value != SurvivorStatus.Injured && s.Status.Value != SurvivorStatus.NeedsAttention)
                .ToList();

            if (healthySurvivors.Any()) // 如果存在健康的幸存者
            {
                // 从健康幸存者中随机选择一个“倒霉蛋”
                var unluckySurvivor = healthySurvivors[Random.Range(0, healthySurvivors.Count)];

                // 通过 ISurvivorManagerSystem 更新幸存者状态
                survivorManagerSystem.UpdateSurvivorStatus(unluckySurvivor.Id, SurvivorStatus.Injured);

                // 注意: Survivor.Name 现在是 BindableProperty，需要访问 .Value
                Description = $"{unluckySurvivor.Name.Value} 突然生病，现在处于受伤状态！";
                Debug.LogWarning(Description);
            }
            else 
            { 
                Description = "一阵疾病袭来，但幸运的是，当前没有健康的幸存者可以被感染（所有人都已处于特殊状态）。";
                Debug.Log(Description); 
            }
        }
    }
}
