using QFramework; 
using YourGameNamespace.Survivors; 
using UnityEngine; 
using System.Linq;

namespace YourGameNamespace.Events
{
    public class SurvivorSicknessEvent : RandomEvent
    {
        public SurvivorSicknessEvent() 
        { 
            Title = "Sudden Sickness"; 
        }

        public override void Execute(IArchitecture architecture)
        {
            var survivorModel = architecture.GetModel<SurvivorModel>();
            if (survivorModel == null)
            {
                Description = "幸存者疾病事件失败：未找到SurvivorModel。";
                Debug.LogError(Description);
                return;
            }

            var healthySurvivors = survivorModel.GetAllSurvivors().Where(s => s.Status != SurvivorStatus.Injured && s.Status != SurvivorStatus.NeedsAttention).ToList();
            if (healthySurvivors.Any())
            {
                var unluckySurvivor = healthySurvivors[Random.Range(0, healthySurvivors.Count)];
                // 将状态更改为受伤。考虑如果他们正在工作，则应停止。
                // 这可能需要更复杂的逻辑，例如需要将他们从工作站中取消分配。
                // 目前，仅更改状态。受伤状态应能阻止其在别处工作/被分配。
                unluckySurvivor.Status = SurvivorStatus.Injured; 
                Description = $"{unluckySurvivor.Name} 突然生病，现在受伤了！";
                Debug.LogWarning(Description);
            }
            else 
            { 
                Description = "一阵疾病袭来，但所有人要么已经身体不适，要么正在休息，或者需要关注。"; 
                Debug.Log(Description); 
            }
        }
    }
}
