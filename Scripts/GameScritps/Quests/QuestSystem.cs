using QFramework;
using UnityEngine; // 用于 Debug.Log
using System.Linq; // 用于 Linq 操作，例如 .ToList(), .Where(), .Values
using YourGameNamespace.Research;    // 用于访问研究相关的模型和枚举 (Technology, ResearchStatus)
using YourGameNamespace.Exploration; // 用于访问探索相关的模型和枚举 (ExplorationPointOfInterest, POIStatus)
using YourGameNamespace.Workstations;// 用于访问工作站相关的模型和枚举 (WorkstationType)
using YourGameNamespace.Framework;   // 用于 GameDataModel (核心游戏数据模型)
using YourGameNamespace.Events;      // 用于 GameEvents (游戏事件定义)

namespace YourGameNamespace.Quests
{
    // 任务系统，负责管理任务的整个生命周期：激活、进度更新、完成及奖励发放
    public class QuestSystem : AbstractSystem
    {
        // 各种所需模型的引用
        private QuestModel mQuestModel;           // 任务数据模型
        private ResourceModel mResourceModel;       // 资源数据模型
        private ResearchModel mResearchModel;       // 研究数据模型
        private ExplorationModel mExplorationModel; // 探索数据模型
        private WorkstationModel mWorkstationModel; // 工作站数据模型 (当前未使用，但可能用于未来任务类型)
        private GameDataModel mGameDataModel;       // 核心游戏数据模型

        // 系统初始化时调用
        protected override void OnInit()
        {
            // 获取所需模型的实例
            mQuestModel = this.GetModel<QuestModel>();
            mResourceModel = this.GetModel<ResourceModel>();
            mResearchModel = this.GetModel<ResearchModel>();
            mExplorationModel = this.GetModel<ExplorationModel>();
            mWorkstationModel = this.GetModel<WorkstationModel>();
            mGameDataModel = this.GetModel<GameDataModel>();

            // 注册对各种游戏事件的监听，以便更新任务进度
            this.RegisterEvent<ResourceChangedEvent>(e => OnGameEvent(ObjectiveType.CollectResource, e.Type.ToString(), e.NewTotalAmount));
            this.RegisterEvent<TechnologyCompletedEvent>(e => OnGameEvent(ObjectiveType.ResearchTech, e.TechId, 1));
            this.RegisterEvent<POIExploredEvent>(e => OnGameEvent(ObjectiveType.ExplorePOI, e.PoiId, 1));
            this.RegisterEvent<WorkstationBuiltEvent>(e => OnGameEvent(ObjectiveType.BuildWorkstation, e.Type.ToString(), 1));
            this.RegisterEvent<DayChangedEvent>(e => {
                CheckForAllQuestActivations();
            });
            this.RegisterEvent<CustomQuestFlagEvent>(e => OnGameEvent(ObjectiveType.CustomFlag, e.FlagName, 1));

            // 游戏启动时，立即检查一次所有任务的激活状态
            CheckForAllQuestActivations();
        }

        // 处理游戏事件，更新相关任务目标进度
        public void OnGameEvent(ObjectiveType type, string targetId, int amount = 1)
        {
            if (mQuestModel == null) return;
            bool questCompletedThisCycle = false;

            foreach (var quest in mQuestModel.ActiveQuests.ToList()) 
            {
                // 使用 .Value 访问 BindableProperty
                if (quest.Status.Value != QuestStatus.Active) continue;

                foreach (var objective in quest.Objectives)
                {
                    if (objective.IsComplete || objective.Type != type) continue;

                    if (objective.TargetId == targetId)
                    {
                        if (type == ObjectiveType.CollectResource)
                        {
                            objective.SetCurrentProgress(amount); // 'amount' 代表资源的新总量
                        }
                        else
                        {
                            objective.AddProgress(amount); // 'amount' 通常是增量
                        }
                        // 使用 .Value 访问 BindableProperty
                        Debug.Log($"任务 '{quest.Title}'：目标 '{objective.Description}' 进度更新为：{objective.CurrentAmount.Value}/{objective.RequiredAmount}");
                    }
                }

                if (quest.AreAllObjectivesComplete())
                {
                    CompleteQuest(quest);
                    questCompletedThisCycle = true;
                }
            }
            if(questCompletedThisCycle) CheckForAllQuestActivations();
        }

        // 检查所有任务的激活条件，激活符合条件的任务
        public void CheckForAllQuestActivations()
        {
            if (mQuestModel == null || mGameDataModel == null || mResearchModel == null) return;

            foreach (var quest in mQuestModel.AllQuests.Values)
            {
                // 使用 .Value 访问 BindableProperty
                if (quest.Status.Value == QuestStatus.Locked)
                {
                    bool prerequisitesMet = true;

                    if (quest.PrerequisiteQuestIds != null)
                    {
                        foreach (string prereqQuestId in quest.PrerequisiteQuestIds)
                        {
                            var prereqQuest = mQuestModel.GetQuest(prereqQuestId);
                            // 使用 .Value 访问 BindableProperty
                            if (prereqQuest == null || prereqQuest.Status.Value != QuestStatus.Success)
                            {
                                prerequisitesMet = false;
                                break;
                            }
                        }
                    }
                    if (!prerequisitesMet) continue;

                    // 使用 .Value 访问 BindableProperty
                    if (quest.RequiredDay > 0 && mGameDataModel.CurrentDay.Value < quest.RequiredDay)
                    {
                        prerequisitesMet = false;
                    }
                    if (!prerequisitesMet) continue;

                    if (!string.IsNullOrEmpty(quest.RequiredTechId))
                    {
                        var tech = mResearchModel.GetTechnology(quest.RequiredTechId);
                        // 使用 .Value 访问 BindableProperty
                        if (tech == null || tech.Status.Value != ResearchStatus.Completed)
                        {
                            prerequisitesMet = false;
                        }
                    }
                    if (!prerequisitesMet) continue;
                    
                    if (prerequisitesMet)
                    {
                        ActivateQuest(quest);
                    }
                }
            }
        }

        // 激活指定的任务
        private void ActivateQuest(Quest quest)
        {
            // 使用 .Value 访问 BindableProperty
            if (quest.Status.Value != QuestStatus.Locked) return;

            // QuestModel的UpdateQuestStatus内部会调用quest.UpdateStatus
            mQuestModel.UpdateQuestStatus(quest.Id, QuestStatus.Active);
            Debug.Log($"任务已激活：'{quest.Title}'");

            foreach (var objective in quest.Objectives.Where(o => o.Type == ObjectiveType.CollectResource))
            {
                if (System.Enum.TryParse<GameResourceType>(objective.TargetId, out GameResourceType resourceType))
                {
                    objective.SetCurrentProgress(mResourceModel.GetAmount(resourceType));
                }
            }

            if (quest.AreAllObjectivesComplete())
            {
                CompleteQuest(quest);
            }
        }

        // 完成指定的任务
        private void CompleteQuest(Quest quest)
        {
            // 使用 .Value 访问 BindableProperty
            if (quest.Status.Value == QuestStatus.Success) return;

            // QuestModel的UpdateQuestStatus内部会调用quest.UpdateStatus
            mQuestModel.UpdateQuestStatus(quest.Id, QuestStatus.Success);
            Debug.LogWarning($"任务已完成：'{quest.Title}'！");

            if (quest.Rewards != null)
            {
                foreach (var reward in quest.Rewards)
                {
                    ApplyReward(reward);
                }
            }
            CheckForAllQuestActivations();
        }

        // 应用单个任务奖励
        private void ApplyReward(QuestReward reward)
        {
            Debug.Log($"正在应用奖励：类型={reward.Type}，目标ID={reward.TargetId}，数量={reward.Amount}");
            switch (reward.Type)
            {
                case QuestRewardType.Resource:
                    if (System.Enum.TryParse<GameResourceType>(reward.TargetId, out GameResourceType resourceType))
                    {
                        mResourceModel.AddResource(resourceType, reward.Amount);
                        Debug.Log($"作为任务奖励，已添加 {reward.Amount} 单位 {resourceType}。");
                    }
                    break;
                case QuestRewardType.UnlockTech:
                    var researchModel = this.GetModel<ResearchModel>();
                    var tech = researchModel?.GetTechnology(reward.TargetId);
                    // 使用 .Value 访问 BindableProperty
                    if(tech != null && tech.Status.Value == ResearchStatus.Locked)
                    {
                        // ResearchModel的UpdateTechnologyStatus会处理tech.Status.Value的更新和事件发送
                        researchModel.UpdateTechnologyStatus(reward.TargetId, ResearchStatus.Available);
                        Debug.Log($"技术 {reward.TargetId} 已通过任务奖励解锁，现在状态为 Available。");
                    } else if (tech != null) {
                        Debug.Log($"技术 {reward.TargetId} 当前状态为 {tech.Status.Value}。任务奖励未产生状态变更。");
                    }
                    break;
                case QuestRewardType.UnlockPOI:
                    var explorationModel = this.GetModel<ExplorationModel>();
                    var poi = explorationModel?.GetPOI(reward.TargetId);
                    if(poi != null && poi.Status == POIStatus.Unexplored)
                    {
                        // ExplorationModel的UpdatePOIStatus会处理poi.Status的更新和事件发送
                        explorationModel.UpdatePOIStatus(reward.TargetId, POIStatus.Scouted);
                        Debug.Log($"兴趣点 {reward.TargetId} 由于任务奖励，现已标记为“已侦察”。");
                    }
                    break;
                default:
                    Debug.LogWarning($"奖励类型 {reward.Type} 的应用逻辑尚未完全实现。");
                    break;
            }
        }
    }
}
