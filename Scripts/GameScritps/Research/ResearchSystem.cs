using System;
using QFramework;
using UnityEngine; // 用于 Debug.Log 和 deltaTime

using YourGameNamespace.Events;   // 用于 TechnologyCompletedEvent

namespace YourGameNamespace.Research
{
    public class ResearchSystem : AbstractSystem
    {
        private ResearchModel mResearchModel;
        private ResourceModel mResourceModel;

        private Technology mCurrentResearch = null;
        private float mCurrentResearchTimeAccumulated = 0f; // 当前研究已累积的时间
        public event Action<Technology> OnTechnologyStatusChanged;
        protected override void OnInit()
        {
         
            mResearchModel = this.GetModel<ResearchModel>();
            mResourceModel = this.GetModel<ResourceModel>();
            if (mResearchModel == null) Debug.LogError("研究系统：未找到研究模型 (ResearchModel)！");
            if (mResourceModel == null) Debug.LogError("研究系统：未找到资源模型 (ResourceModel)！");
        }

        public bool StartResearch(string techId)
        {
            if (mCurrentResearch != null)
            {
                Debug.LogWarning($"无法开始研究 {techId}。{mCurrentResearch.Name} 的研究已在进行中。");
                return false;
            }

            Technology techToResearch = mResearchModel.GetTechnology(techId);
            if (techToResearch == null)
            {
                Debug.LogError($"未找到技术 {techId}。");
                return false;
            }

            if (techToResearch.Status != ResearchStatus.Available)
            {
                Debug.LogWarning($"技术 {techToResearch.Name} 当前不可研究。状态：{techToResearch.Status}");
                return false;
            }

            if (!mResourceModel.ConsumeResource(GameResourceType.ResearchPoints, techToResearch.ResearchPointCost))
            {
                Debug.LogWarning($"研究点不足，无法开始 {techToResearch.Name}。需要：{techToResearch.ResearchPointCost}，可用：{mResourceModel.GetAmount(GameResourceType.ResearchPoints)}");
                return false;
            }

            mCurrentResearch = techToResearch;
            mCurrentResearchTimeAccumulated = 0f;
            mResearchModel.UpdateTechnologyStatus(techId, ResearchStatus.InProgress);
            Debug.Log($"开始研究 {mCurrentResearch.Name}。成本：{mCurrentResearch.ResearchPointCost} 研究点。这将花费 {mCurrentResearch.ResearchPointCost} “秒”的研究努力。");
            // 如果UI需要更新，则发送事件
            // this.SendEvent<CurrentResearchChangedEvent>(); 
            return true;
        }

        public void UpdateResearchProcess(float deltaTime)
        {
            if (mCurrentResearch == null)
            {
                return;
            }

            // 假设每秒1单位“研究努力”。
            // 技术的“成本”也定义了其在花费点数后的“研究时间”。
            mCurrentResearchTimeAccumulated += deltaTime+50; 

            if (mCurrentResearchTimeAccumulated >= mCurrentResearch.ResearchPointCost)
            {
                CompleteResearch(mCurrentResearch.Id);
            }
        }

        
        /// <summary>
        /// 当某个技术状态更新时调用此方法，并通知 UI 更新
        /// </summary>
        public void NotifyTechnologyStatusChanged(Technology tech)
        {
            OnTechnologyStatusChanged?.Invoke(tech);
        }
        
        private void CompleteResearch(string techId)
        {
            Technology completedTech = mResearchModel.GetTechnology(techId); // 应该是 mCurrentResearch
            if (completedTech == null) return;

            Debug.LogWarning($"研究完成：{completedTech.Name}！");
            mResearchModel.UpdateTechnologyStatus(techId, ResearchStatus.Completed);

            // 应用效果
            Debug.Log($"正在应用 {completedTech.Name} 的效果：");
            var workstationModel = this.GetModel<YourGameNamespace.Workstations.WorkstationModel>(); // 获取 WorkstationModel

            foreach (var effect in completedTech.Effects)
            {
                Debug.Log($"应用 -> 效果类型：{effect.EffectType}，值：{effect.Value}，目标工作站：{effect.TargetWorkstationType}，目标资源：{effect.TargetResource}");
                switch (effect.EffectType)
                {
                    case TechnologyEffectType.IncreaseProductionMultiplier:
                        if (workstationModel != null)
                        {
                            foreach (var station in workstationModel.GetAllWorkstations().FindAll(ws => ws.Type == effect.TargetWorkstationType))
                            {
                                station.ProductionBonusMultiplier += effect.Value; // 对乘数进行累加
                                Debug.Log($"已对 {station.Type} (ID: {station.Id}) 应用 {effect.Value} 生产乘数。新乘数：{station.ProductionBonusMultiplier}");
                            }
                        }
                        break;

                    case TechnologyEffectType.IncreaseProductionOutput:
                        if (workstationModel != null)
                        {
                            foreach (var station in workstationModel.GetAllWorkstations().FindAll(ws => ws.Type == effect.TargetWorkstationType))
                            {
                                station.FlatProductionBonus += (int)effect.Value;
                                Debug.Log($"已对 {station.Type} (ID: {station.Id}) 应用 {(int)effect.Value} 固定生产加成。新固定加成：{station.FlatProductionBonus}");
                            }
                        }
                        break;

                    case TechnologyEffectType.ModifySurvivorStat: 
                        var combatSystem = this.GetSystem<YourGameNamespace.Combat.CombatSystem>();
                        if (combatSystem != null)
                        {
                            combatSystem.SurvivorAttackPowerMultiplier += effect.Value; // 对乘数进行累加
                            Debug.Log($"已对幸存者攻击力乘数应用 {effect.Value}。新乘数：{combatSystem.SurvivorAttackPowerMultiplier}");
                        }
                        break;

                    case TechnologyEffectType.UnlockWorkstation:
                        Debug.Log($"工作站类型 {(YourGameNamespace.Workstations.WorkstationType)effect.Value} 现在被视为由研究 {completedTech.Name} 解锁！");
                        // 可选：发送一个UI可以监听的事件，以刷新建造菜单
                        // this.SendEvent(new WorkstationUnlockedEvent((WorkstationType)effect.Value));
                        break;
            
                    default:
                        Debug.LogWarning($"技术 {completedTech.Name} 的效果类型 {effect.EffectType} 的应用尚未实现。");
                        break;
                }
            }

            mCurrentResearch = null;
            mCurrentResearchTimeAccumulated = 0f;

            // 更新可能以此技术为前置条件的其他技术的状态
            mResearchModel.UpdateAllTechnologyStatuses();
            
            // 为任务系统发送事件
            this.SendEvent(new TechnologyCompletedEvent(completedTech.Id));

            // 如果UI需要更新，则发送事件（其他UI的示例，不直接属于此任务的一部分）
            // this.SendEvent<ResearchCompletedEvent>(new ResearchCompletedEvent(completedTech)); 
            // this.SendEvent<AvailableTechsChangedEvent>();
        }

        public Technology GetCurrentResearch()
        {
            return mCurrentResearch;
        }

        public float GetCurrentResearchProgressNormalized()
        {
            if (mCurrentResearch == null || mCurrentResearch.ResearchPointCost == 0) // 避免除以零
            {
                return 0f;
            }
            return Mathf.Clamp01(mCurrentResearchTimeAccumulated / mCurrentResearch.ResearchPointCost);
        }
         public bool IsResearching() => mCurrentResearch != null;
    }
}
