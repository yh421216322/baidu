using QFramework;
using UnityEngine; // 用于 Debug.Log
using System.Linq; // 用于 Linq 操作，例如 .ToList(), .Where(), .Values
using YourGameNamespace.Research;    // 用于访问研究相关的模型和枚举
using YourGameNamespace.Exploration; // 用于访问探索相关的模型和枚举
using YourGameNamespace.Workstations;// 用于访问工作站相关的模型和枚举
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

        // private EventSystem mEventSystem; // QFramework的事件系统，当前注释掉因其在此处不直接用于发送事件，而是用于上下文理解

        // 系统初始化时调用
        protected override void OnInit()
        {
            // 获取所需模型的实例
            mQuestModel = this.GetModel<QuestModel>();
            mResourceModel = this.GetModel<ResourceModel>();
            mResearchModel = this.GetModel<ResearchModel>();
            mExplorationModel = this.GetModel<ExplorationModel>();
            mWorkstationModel = this.GetModel<WorkstationModel>(); // 获取工作站模型实例
            mGameDataModel = this.GetModel<GameDataModel>();
            // mEventSystem = this.GetSystem<EventSystem>(); // 如果需要，可以获取事件系统实例（通常已在GameArchitecture中注册）

            // 注册对各种游戏事件的监听，以便更新任务进度
            this.RegisterEvent<ResourceChangedEvent>(e => OnGameEvent(ObjectiveType.CollectResource, e.Type.ToString(), e.NewTotalAmount)); // 监听资源变化事件
            this.RegisterEvent<TechnologyCompletedEvent>(e => OnGameEvent(ObjectiveType.ResearchTech, e.TechId, 1)); // 监听技术研究完成事件
            this.RegisterEvent<POIExploredEvent>(e => OnGameEvent(ObjectiveType.ExplorePOI, e.PoiId, 1));             // 监听兴趣点探索完成事件
            this.RegisterEvent<WorkstationBuiltEvent>(e => OnGameEvent(ObjectiveType.BuildWorkstation, e.Type.ToString(), 1)); // 监听工作站建造事件
            this.RegisterEvent<DayChangedEvent>(e => { // 监听天数变化事件
                CheckForAllQuestActivations(); // 检查是否有任务可以激活
            });
            this.RegisterEvent<CustomQuestFlagEvent>(e => OnGameEvent(ObjectiveType.CustomFlag, e.FlagName, 1)); // 监听自定义任务标记事件

            // 游戏启动时，立即检查一次所有任务的激活状态
            CheckForAllQuestActivations();
        }

        // 处理游戏事件，更新相关任务目标进度
        public void OnGameEvent(ObjectiveType type, string targetId, int amount = 1)
        {
            if (mQuestModel == null) return; // 如果任务模型不存在，则不执行任何操作
            bool questCompletedThisCycle = false; // 标记本轮事件处理中是否有任务完成

            // 迭代活动任务列表的副本，因为在迭代过程中任务状态可能改变（例如，任务完成并从活动列表移除）
            foreach (var quest in mQuestModel.ActiveQuests.ToList()) 
            {
                if (quest.Status != QuestStatus.Active) continue; // 只处理活动状态的任务

                // 遍历任务中的每个目标
                foreach (var objective in quest.Objectives)
                {
                    // 如果目标已完成或类型不匹配，则跳过
                    if (objective.IsComplete || objective.Type != type) continue;

                    // 如果目标ID与事件中的目标ID匹配
                    if (objective.TargetId == targetId)
                    {
                        if (type == ObjectiveType.CollectResource) // 如果是收集资源类型的目标
                        {
                            objective.UpdateProgress(amount); // 'amount' 代表资源的新总量
                        }
                        else // 对于其他类型的目标 (如研究技术、探索POI)，'amount' 通常是增量 (例如，完成1项研究)
                        {
                            objective.UpdateProgress(objective.CurrentAmount + amount);
                        }
                        Debug.Log($"任务 '{quest.Title}'：目标 '{objective.Description}' 进度更新为：{objective.CurrentAmount}/{objective.RequiredAmount}");
                    }
                }

                // 检查任务的所有目标是否都已完成
                if (quest.AreAllObjectivesComplete())
                {
                    CompleteQuest(quest); // 完成任务
                    questCompletedThisCycle = true; // 标记有任务在本轮完成
                }
            }
            // 如果在本轮事件处理中有任务完成，这可能解锁了新的任务，因此需要重新检查任务激活状态
            if(questCompletedThisCycle) CheckForAllQuestActivations();
        }

        // 检查所有任务的激活条件，激活符合条件的任务
        public void CheckForAllQuestActivations()
        {
            if (mQuestModel == null || mGameDataModel == null || mResearchModel == null) return; // 确保所需模型存在

            // 遍历所有已定义的任务
            foreach (var quest in mQuestModel.AllQuests.Values)
            {
                if (quest.Status == QuestStatus.Locked) // 只检查处于“锁定”状态的任务
                {
                    bool prerequisitesMet = true; // 标记所有前置条件是否满足
                    
                    // 1. 检查前置任务条件
                    if (quest.PrerequisiteQuestIds != null)
                    {
                        foreach (string prereqQuestId in quest.PrerequisiteQuestIds)
                        {
                            var prereqQuest = mQuestModel.GetQuest(prereqQuestId);
                            // 如果前置任务不存在或未成功完成，则条件不满足
                            if (prereqQuest == null || prereqQuest.Status != QuestStatus.Success)
                            {
                                prerequisitesMet = false;
                                break;
                            }
                        }
                    }
                    if (!prerequisitesMet) continue; // 如果前置任务条件不满足，则跳过此任务的后续检查

                    // 2. 检查天数条件
                    if (quest.RequiredDay > 0 && mGameDataModel.CurrentDay < quest.RequiredDay)
                    {
                        prerequisitesMet = false; // 如果当前天数未达到要求，则条件不满足
                    }
                    if (!prerequisitesMet) continue;

                    // 3. 检查技术条件
                    if (!string.IsNullOrEmpty(quest.RequiredTechId))
                    {
                        var tech = mResearchModel.GetTechnology(quest.RequiredTechId);
                        // 如果所需技术不存在或未研究完成，则条件不满足
                        if (tech == null || tech.Status != ResearchStatus.Completed)
                        {
                            prerequisitesMet = false;
                        }
                    }
                    if (!prerequisitesMet) continue;
                    
                    // 如果所有前置条件均满足，则激活任务
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
            if (quest.Status != QuestStatus.Locked) return; // 确保只激活处于“锁定”状态的任务

            mQuestModel.UpdateQuestStatus(quest.Id, QuestStatus.Active); // 更新任务状态为“活动”
            Debug.Log($"任务已激活：'{quest.Title}'");

            // 对于“收集资源”类型的目标，根据当前已拥有的资源量初始化其进度
            foreach (var objective in quest.Objectives.Where(o => o.Type == ObjectiveType.CollectResource))
            {
                if (System.Enum.TryParse<GameResourceType>(objective.TargetId, out GameResourceType resourceType))
                {
                    objective.UpdateProgress(mResourceModel.GetAmount(resourceType));
                }
            }

            // 检查任务在激活时是否已经满足了所有目标（例如，玩家在任务激活前已收集足够资源）
            if (quest.AreAllObjectivesComplete())
            {
                CompleteQuest(quest); // 如果是，则直接完成任务
            }
        }

        // 完成指定的任务
        private void CompleteQuest(Quest quest)
        {
            if (quest.Status == QuestStatus.Success) return; // 如果任务已经成功，则不执行任何操作

            mQuestModel.UpdateQuestStatus(quest.Id, QuestStatus.Success); // 更新任务状态为“成功”
            Debug.LogWarning($"任务已完成：'{quest.Title}'！");

            // 应用任务奖励
            if (quest.Rewards != null)
            {
                foreach (var reward in quest.Rewards)
                {
                    ApplyReward(reward);
                }
            }
            // 任务完成后，重新检查是否有新的任务可以激活（因为此任务的完成可能是其他任务的前置条件）
            CheckForAllQuestActivations(); 
        }

        // 应用单个任务奖励
        private void ApplyReward(QuestReward reward)
        {
            Debug.Log($"正在应用奖励：类型={reward.Type}，目标ID={reward.TargetId}，数量={reward.Amount}");
            switch (reward.Type)
            {
                case QuestRewardType.Resource: // 如果奖励类型是资源
                    if (System.Enum.TryParse<GameResourceType>(reward.TargetId, out GameResourceType resourceType))
                    {
                        mResourceModel.AddResource(resourceType, reward.Amount); // 添加资源
                        Debug.Log($"作为任务奖励，已添加 {reward.Amount} 单位 {resourceType}。");
                    }
                    break;
                // 其他奖励类型（如解锁技术、解锁POI、生成幸存者等）将在此处处理
                case QuestRewardType.UnlockTech: // 如果奖励类型是解锁技术
                    var researchModel = this.GetModel<ResearchModel>();
                    var tech = researchModel?.GetTechnology(reward.TargetId);
                    if(tech != null && tech.Status == ResearchStatus.Locked) // 确保技术存在且当前为锁定状态
                    {
                        // 注意：直接将技术设为Available可能会绕过其自身其他前置条件。
                        // 一个更稳妥的做法是依赖 ResearchModel.UpdateAllTechnologyStatuses() 在任务完成后自动处理。
                        // 此处仅记录日志，表示该技术已被标记为可以通过任务奖励解锁。
                        // researchModel.UpdateTechnologyStatus(reward.TargetId, ResearchStatus.Available); // 避免直接修改，让其自然解锁
                        Debug.Log($"技术 {reward.TargetId} 已被标记为可以通过任务奖励解锁。稍后将重新检查其所有前置条件以确定是否变为可用。");
                    } else if (tech != null) {
                        Debug.Log($"技术 {reward.TargetId} 当前状态为 {tech.Status}。任务奖励未产生状态变更。");
                    }
                    break;
                case QuestRewardType.UnlockPOI: // 如果奖励类型是解锁兴趣点
                    var explorationModel = this.GetModel<ExplorationModel>();
                    var poi = explorationModel?.GetPOI(reward.TargetId);
                    if(poi != null && poi.Status == POIStatus.Unexplored) // 确保POI存在且当前为未探索状态
                    {
                        explorationModel.UpdatePOIStatus(reward.TargetId, POIStatus.Scouted); // 将POI状态更新为已侦察
                        Debug.Log($"兴趣点 {reward.TargetId} 由于任务奖励，现已标记为“已侦察”。");
                    }
                    break;
                // TODO: 添加对 SpawnSurvivor (生成幸存者) 等其他奖励类型的处理逻辑
                default:
                    Debug.LogWarning($"奖励类型 {reward.Type} 的应用逻辑尚未完全实现。");
                    break;
            }
        }
    }
}
