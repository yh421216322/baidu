using System;
using QFramework;
using UnityEngine; // 用于 Debug.Log 和 Time.deltaTime

using YourGameNamespace.Events;   // 用于 TechnologyCompletedEvent (技术完成事件)

namespace YourGameNamespace.Research
{
    // 研究系统，负责管理技术的研发过程和效果应用
    public class ResearchSystem : AbstractSystem
    {
        private ResearchModel mResearchModel; // 研究数据模型
        private ResourceModel mResourceModel; // 资源数据模型

        private Technology mCurrentResearch = null; // 当前正在研究的技术
        private float mCurrentResearchTimeAccumulated = 0f; // 当前研究已累积的时间（或理解为“研究点投入”）
        public event Action<Technology> OnTechnologyStatusChanged; // 当技术状态（例如，从锁定变为可用）发生变化时触发的事件

        // 系统初始化
        protected override void OnInit()
        {
            mResearchModel = this.GetModel<ResearchModel>();
            mResourceModel = this.GetModel<ResourceModel>();
            if (mResearchModel == null) Debug.LogError("研究系统：未能找到研究数据模型 (ResearchModel)！初始化失败。");
            if (mResourceModel == null) Debug.LogError("研究系统：未能找到资源数据模型 (ResourceModel)！初始化失败。");
        }

        // 开始一项新的技术研究
        public bool StartResearch(string techId)
        {
            if (mCurrentResearch != null) // 如果当前已有研究项目在进行中
            {
                Debug.LogWarning($"无法开始研究技术 {techId}。因为 {mCurrentResearch.Name} 的研究已在进行中。");
                return false;
            }

            Technology techToResearch = mResearchModel.GetTechnology(techId); // 获取要研究的技术对象
            if (techToResearch == null) // 如果技术ID无效
            {
                Debug.LogError($"未能找到ID为 {techId} 的技术。");
                return false;
            }

            if (techToResearch.Status != ResearchStatus.Available) // 如果技术当前不可研究（例如，前置条件未满足或已完成）
            {
                Debug.LogWarning($"技术 {techToResearch.Name} (ID: {techId}) 当前不可研究。其状态为：{techToResearch.Status}");
                return false;
            }

            // 尝试消耗所需的研究点数
            if (!mResourceModel.ConsumeResource(GameResourceType.ResearchPoints, techToResearch.ResearchPointCost))
            {
                Debug.LogWarning($"研究点不足，无法开始研究 {techToResearch.Name}。需要研究点：{techToResearch.ResearchPointCost}，当前可用：{mResourceModel.GetAmount(GameResourceType.ResearchPoints)}");
                return false;
            }

            // 设置当前研究项目
            mCurrentResearch = techToResearch;
            mCurrentResearchTimeAccumulated = 0f; // 重置累积研究时间/点数
            mResearchModel.UpdateTechnologyStatus(techId, ResearchStatus.InProgress); // 更新技术状态为“进行中”
            Debug.Log($"开始研究：{mCurrentResearch.Name}。所需研究点/时间：{mCurrentResearch.ResearchPointCost}。");
            // 如果UI需要实时更新当前研究项目，可以在此处发送一个事件，例如：
            // this.SendEvent<CurrentResearchChangedEvent>(new CurrentResearchChangedEvent(mCurrentResearch)); 
            return true;
        }

        // 更新当前研究项目的进度
        public void UpdateResearchProcess(float deltaTime)
        {
            if (mCurrentResearch == null) // 如果没有正在进行的研究，则直接返回
            {
                return;
            }

            // 假设研究点数代表了研究所需的时间（或“努力”）。
            // deltaTime 代表真实时间的流逝。
            // 注意：原始代码中 `deltaTime+50` 会导致研究瞬间完成，这里修正为仅使用 `deltaTime`。
            // 如果需要加速或基于其他因素调整研究速度，应引入一个速率乘数。
            mCurrentResearchTimeAccumulated += deltaTime; 

            // 如果累积的研究时间/点数已达到或超过所需成本
            if (mCurrentResearchTimeAccumulated >= mCurrentResearch.ResearchPointCost)
            {
                CompleteResearch(mCurrentResearch.Id); // 完成研究
            }
        }
        
        /// <summary>
        /// 当某个技术的状态在ResearchModel中被更新时（例如，从锁定变为可用），
        /// ResearchModel会调用此方法，以便ResearchSystem可以触发一个外部事件通知UI等其他观察者。
        /// </summary>
        public void NotifyTechnologyStatusChanged(Technology tech)
        {
            OnTechnologyStatusChanged?.Invoke(tech); // 触发事件，传递已改变状态的技术对象
        }
        
        // 完成指定ID的技术研究
        private void CompleteResearch(string techId)
        {
            Technology completedTech = mResearchModel.GetTechnology(techId); // 获取已完成的技术对象
            if (completedTech == null) return; // 安全检查

            Debug.LogWarning($"研究完成：{completedTech.Name}！");
            mResearchModel.UpdateTechnologyStatus(techId, ResearchStatus.Completed); // 更新技术状态为“已完成”

            // 应用该技术的效果
            Debug.Log($"正在应用技术 {completedTech.Name} 的效果：");
            var workstationModel = this.GetModel<YourGameNamespace.Workstations.WorkstationModel>(); // 获取工作站模型以便应用效果

            foreach (var effect in completedTech.Effects) // 遍历技术的所有效果
            {
                Debug.Log($"应用效果 -> 类型：{effect.EffectType}，值：{effect.Value}，目标工作站：{effect.TargetWorkstationType}，目标资源：{effect.TargetResource}");
                switch (effect.EffectType)
                {
                    case TechnologyEffectType.IncreaseProductionMultiplier: // 增加产量乘数
                        if (workstationModel != null)
                        {
                            // 找到所有类型匹配的工作站并应用效果
                            foreach (var station in workstationModel.GetAllWorkstations().FindAll(ws => ws.Type == effect.TargetWorkstationType))
                            {
                                station.ProductionBonusMultiplier += effect.Value; // 注意：这里是累加乘数，确保这是期望行为
                                Debug.Log($"已对工作站 {station.Type} (ID: {station.Id}) 应用生产乘数加成 {effect.Value}。新的总生产乘数：{station.ProductionBonusMultiplier}");
                            }
                        }
                        break;

                    case TechnologyEffectType.IncreaseProductionOutput: // 增加固定产量
                        if (workstationModel != null)
                        {
                            foreach (var station in workstationModel.GetAllWorkstations().FindAll(ws => ws.Type == effect.TargetWorkstationType))
                            {
                                station.FlatProductionBonus += (int)effect.Value; // 累加固定产量加成
                                Debug.Log($"已对工作站 {station.Type} (ID: {station.Id}) 应用固定产量加成 {(int)effect.Value}。新的总固定加成：{station.FlatProductionBonus}");
                            }
                        }
                        break;

                    case TechnologyEffectType.ModifySurvivorStat: // 修改幸存者属性
                        var combatSystem = this.GetSystem<YourGameNamespace.Combat.CombatSystem>(); // 获取战斗系统
                        if (combatSystem != null)
                        {
                            // 示例：增加幸存者攻击力乘数
                            combatSystem.SurvivorAttackPowerMultiplier += effect.Value; // 注意：累加乘数
                            Debug.Log($"已对幸存者攻击力乘数应用加成 {effect.Value}。新的总攻击力乘数：{combatSystem.SurvivorAttackPowerMultiplier}");
                        }
                        break;

                    case TechnologyEffectType.UnlockWorkstation: // 解锁新工作站类型
                        Debug.Log($"工作站类型 {(YourGameNamespace.Workstations.WorkstationType)effect.Value} 现在因完成研究 {completedTech.Name} 而被视为已解锁！");
                        // 可选：发送一个UI可以监听的事件，以刷新建筑菜单，例如：
                        // this.SendEvent(new WorkstationUnlockedEvent((WorkstationType)effect.Value));
                        break;
            
                    default: // 其他未实现的效果类型
                        Debug.LogWarning($"技术 {completedTech.Name} 的效果类型 {effect.EffectType} 的应用逻辑尚未实现。");
                        break;
                }
            }

            mCurrentResearch = null; // 清除当前研究项目
            mCurrentResearchTimeAccumulated = 0f; // 重置累积时间

            // 研究完成后，更新所有技术的状态（因为此项技术的完成可能满足了其他技术的前置条件）
            mResearchModel.UpdateAllTechnologyStatuses();
            
            // 发送技术完成事件，供任务系统等其他系统监听
            this.SendEvent(new TechnologyCompletedEvent(completedTech.Id));

            // 如果UI需要根据技术完成情况进行特定更新，可以发送更具体的事件
            // 例如: this.SendEvent<ResearchCompletedEvent>(new ResearchCompletedEvent(completedTech)); 
            // 或: this.SendEvent<AvailableTechsChangedEvent>(); // 通知可用技术列表已改变
        }

        // 获取当前正在研究的技术对象
        public Technology GetCurrentResearch()
        {
            return mCurrentResearch;
        }

        // 获取当前研究的标准化进度 (0到1之间)
        public float GetCurrentResearchProgressNormalized()
        {
            if (mCurrentResearch == null || mCurrentResearch.ResearchPointCost == 0) // 避免除以零错误
            {
                return 0f;
            }
            return Mathf.Clamp01(mCurrentResearchTimeAccumulated / mCurrentResearch.ResearchPointCost); // 确保进度在0和1之间
        }
        // 判断当前是否有技术正在研究中
         public bool IsResearching() => mCurrentResearch != null;
    }
}
