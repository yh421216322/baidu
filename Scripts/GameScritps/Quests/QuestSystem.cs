using QFramework;
using UnityEngine; // 用于 Debug.Log
using System.Linq; // 用于 Linq 操作
using YourGameNamespace.Research;
using YourGameNamespace.Exploration;
using YourGameNamespace.Workstations;
using YourGameNamespace.Framework; // 用于 GameDataModel
using YourGameNamespace.Events;   // 用于 GameEvents

namespace YourGameNamespace.Quests
{
    public class QuestSystem : AbstractSystem
    {
        private QuestModel mQuestModel;
        private ResourceModel mResourceModel;
        private ResearchModel mResearchModel;
        private ExplorationModel mExplorationModel;
        private WorkstationModel mWorkstationModel;
        private GameDataModel mGameDataModel;
        // private EventSystem mEventSystem; // 不是直接用于发送，而是用于上下文

        protected override void OnInit()
        {
       
            mQuestModel = this.GetModel<QuestModel>();
            mResourceModel = this.GetModel<ResourceModel>();
            mResearchModel = this.GetModel<ResearchModel>();
            mExplorationModel = this.GetModel<ExplorationModel>();
            mWorkstationModel = this.GetModel<WorkstationModel>();
            mGameDataModel = this.GetModel<GameDataModel>();
            // mEventSystem = this.GetSystem<EventSystem>(); // 如果其他地方需要，已在 GameArchitecture 中注册

            // 注册游戏事件
            this.RegisterEvent<ResourceChangedEvent>(e => OnGameEvent(ObjectiveType.CollectResource, e.Type.ToString(), e.NewTotalAmount));
            this.RegisterEvent<TechnologyCompletedEvent>(e => OnGameEvent(ObjectiveType.ResearchTech, e.TechId, 1));
            this.RegisterEvent<POIExploredEvent>(e => OnGameEvent(ObjectiveType.ExplorePOI, e.PoiId, 1));
            this.RegisterEvent<WorkstationBuiltEvent>(e => OnGameEvent(ObjectiveType.BuildWorkstation, e.Type.ToString(), 1));
            this.RegisterEvent<DayChangedEvent>(e => { 
                CheckForAllQuestActivations(); 
            });
            this.RegisterEvent<CustomQuestFlagEvent>(e => OnGameEvent(ObjectiveType.CustomFlag, e.FlagName, 1));

            // 初始检查任务激活状态
            CheckForAllQuestActivations();
        }

        public void OnGameEvent(ObjectiveType type, string targetId, int amount = 1)
        {
            if (mQuestModel == null) return;
            bool questCompletedThisCycle = false;

            // 如果在迭代过程中可能修改列表（例如任务完成会移除自身），则迭代活动任务的副本
            foreach (var quest in mQuestModel.ActiveQuests.ToList()) 
            {
                if (quest.Status != QuestStatus.Active) continue;

                foreach (var objective in quest.Objectives)
                {
                    if (objective.IsComplete || objective.Type != type) continue;

                    if (objective.TargetId == targetId)
                    {
                        if (type == ObjectiveType.CollectResource)
                        {
                            objective.UpdateProgress(amount); // 此处的 'amount' 是资源的新总量
                        }
                        else // 对于其他类型，amount 通常是增量（例如，一个已研究的技术为1）
                        {
                            objective.UpdateProgress(objective.CurrentAmount + amount);
                        }
                        Debug.Log($"任务 '{quest.Title}'：目标 '{objective.Description}' 进度：{objective.CurrentAmount}/{objective.RequiredAmount}");
                    }
                }

                if (quest.AreAllObjectivesComplete())
                {
                    CompleteQuest(quest);
                    questCompletedThisCycle = true;
                }
            }
            // 如果一个任务的完成可能解锁另一个任务，则重新检查
            if(questCompletedThisCycle) CheckForAllQuestActivations();
        }

        public void CheckForAllQuestActivations()
        {
            if (mQuestModel == null || mGameDataModel == null || mResearchModel == null) return;

            foreach (var quest in mQuestModel.AllQuests.Values)
            {
                if (quest.Status == QuestStatus.Locked)
                {
                    bool prerequisitesMet = true;
                    // 检查任务前置条件
                    if (quest.PrerequisiteQuestIds != null)
                    {
                        foreach (string prereqQuestId in quest.PrerequisiteQuestIds)
                        {
                            var prereqQuest = mQuestModel.GetQuest(prereqQuestId);
                            if (prereqQuest == null || prereqQuest.Status != QuestStatus.Success)
                            {
                                prerequisitesMet = false;
                                break;
                            }
                        }
                    }
                    if (!prerequisitesMet) continue;

                    // 检查天数前置条件
                    if (quest.RequiredDay > 0 && mGameDataModel.CurrentDay < quest.RequiredDay)
                    {
                        prerequisitesMet = false;
                    }
                    if (!prerequisitesMet) continue;

                    // 检查技术前置条件
                    if (!string.IsNullOrEmpty(quest.RequiredTechId))
                    {
                        var tech = mResearchModel.GetTechnology(quest.RequiredTechId);
                        if (tech == null || tech.Status != ResearchStatus.Completed)
                        {
                            prerequisitesMet = false;
                        }
                    }
                    if (!prerequisitesMet) continue;
                    
                    // 如果所有前置条件都满足，则激活任务
                    if (prerequisitesMet)
                    {
                        ActivateQuest(quest);
                    }
                }
            }
        }

        private void ActivateQuest(Quest quest)
        {
            if (quest.Status != QuestStatus.Locked) return; // 只应激活锁定的任务

            mQuestModel.UpdateQuestStatus(quest.Id, QuestStatus.Active);
            Debug.Log($"任务已激活：'{quest.Title}'");

            // 根据当前资源量初始化收集资源目标
            foreach (var objective in quest.Objectives.Where(o => o.Type == ObjectiveType.CollectResource))
            {
                if (System.Enum.TryParse<GameResourceType>(objective.TargetId, out GameResourceType resourceType))
                {
                    objective.UpdateProgress(mResourceModel.GetAmount(resourceType));
                }
            }

            // 检查激活时是否已完成
            if (quest.AreAllObjectivesComplete())
            {
                CompleteQuest(quest);
            }
        }

        private void CompleteQuest(Quest quest)
        {
            if (quest.Status == QuestStatus.Success) return; // 已完成

            mQuestModel.UpdateQuestStatus(quest.Id, QuestStatus.Success);
            Debug.LogWarning($"任务已完成：'{quest.Title}'！");

            // 应用奖励
            if (quest.Rewards != null)
            {
                foreach (var reward in quest.Rewards)
                {
                    ApplyReward(reward);
                }
            }
            // 可能触发新任务或游戏事件
            CheckForAllQuestActivations(); // 检查此完成是否解锁其他任务
        }

        private void ApplyReward(QuestReward reward)
        {
            Debug.Log($"应用奖励：类型={reward.Type}，目标ID={reward.TargetId}，数量={reward.Amount}");
            switch (reward.Type)
            {
                case QuestRewardType.Resource:
                    if (System.Enum.TryParse<GameResourceType>(reward.TargetId, out GameResourceType resourceType))
                    {
                        mResourceModel.AddResource(resourceType, reward.Amount);
                        Debug.Log($"作为任务奖励添加了 {reward.Amount} {resourceType}。");
                    }
                    break;
                // 其他奖励类型（解锁技术，解锁POI，生成幸存者）将在此处处理
                case QuestRewardType.UnlockTech:
                    var researchModel = this.GetModel<ResearchModel>();
                    var tech = researchModel?.GetTechnology(reward.TargetId);
                    if(tech != null && tech.Status == ResearchStatus.Locked) // 确保在设为可用前是锁定的
                    {
                        // 如果ResearchModel.UpdateAllTechnologyStatuses处理此问题，则可以简化。
                        // 目前，如果所有其他前置条件都满足，则直接使其可用。
                        // researchModel.UpdateTechnologyStatus(reward.TargetId, ResearchStatus.Available); // 这可能会绕过其他前置条件。
                        // 最好在此任务完成后让UpdateAllTechnologyStatuses处理。
                        Debug.Log($"技术 {reward.TargetId} 已被标记为可通过任务奖励解锁。将重新检查前置条件。");
                    } else if (tech != null) {
                        Debug.Log($"技术 {reward.TargetId} 已处于状态 {tech.Status}。任务奖励无变更。");
                    }
                    break;
                case QuestRewardType.UnlockPOI:
                    var explorationModel = this.GetModel<ExplorationModel>();
                    var poi = explorationModel?.GetPOI(reward.TargetId);
                    if(poi != null && poi.Status == POIStatus.Unexplored)
                    {
                        explorationModel.UpdatePOIStatus(reward.TargetId, POIStatus.Scouted);
                        Debug.Log($"POI {reward.TargetId} 由于任务奖励现已标记为已侦察。");
                    }
                    break;
                // 添加生成幸存者等情况的处理
                default:
                    Debug.LogWarning($"奖励类型 {reward.Type} 的应用尚未完全实现。");
                    break;
            }
        }
    }
}
