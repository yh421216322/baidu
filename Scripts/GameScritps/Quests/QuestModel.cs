using QFramework;
using System.Collections.Generic;
using System.Linq;
using YourGameNamespace.Workstations; // 用于示例任务中引用 WorkstationType
using YourGameNamespace.Research;    // 用于示例任务中引用 Technology (特指技术ID)
using YourGameNamespace.Events;      // 用于 Model_QuestStatusUpdatedEvent

namespace YourGameNamespace.Quests
{
    // 任务数据模型，存储所有任务信息及当前活动任务
    public class QuestModel : AbstractModel
    {
        // 存储游戏中所有已定义的任务，键为任务ID
        public Dictionary<string, Quest> AllQuests { get; private set; } = new Dictionary<string, Quest>();
        // 当前正在进行的活动任务列表
        public List<Quest> ActiveQuests { get; private set; } = new List<Quest>();

        // 模型初始化时调用
        protected override void OnInit()
        {
            // 填充初始的任务数据
            PopulateInitialQuests();
        }

        // 填充初始任务数据的方法
        private void PopulateInitialQuests()
        {
            // 任务1：“巩固基础”
            var q1Objectives = new List<QuestObjective> // 任务目标列表
            {
                new QuestObjective("收集100单位食物", ObjectiveType.CollectResource, GameResourceType.Food.ToString(), 100),
                new QuestObjective("收集50单位电力", ObjectiveType.CollectResource, GameResourceType.Power.ToString(), 50)
            };
            var q1Rewards = new List<QuestReward> { new QuestReward(QuestRewardType.Resource, GameResourceType.Ammo.ToString(), 20) }; // 任务奖励：20单位弹药
            AllQuests.Add("Q_FOUNDATION", new Quest("Q_FOUNDATION", "巩固基础", "建立基础资源生产，确保初期生存。", q1Objectives, rewards: q1Rewards));

            // 任务2：“仰望星空” - 依赖于任务 "Q_FOUNDATION" 的完成
            // 假设技术“TECH_RADIO_BASIC”(基础无线电通讯)和POI“POI_RADIO_TOWER”(旧无线电塔)已在其他地方定义
            var q2Objectives = new List<QuestObjective>
            {
                new QuestObjective("研究“基础无线电通讯”技术", ObjectiveType.ResearchTech, "TECH_RADIO_BASIC", 1),
                new QuestObjective("侦察“旧无线电塔”", ObjectiveType.ExplorePOI, "POI_RADIO_TOWER", 1)
            };
            var q2Rewards = new List<QuestReward> { new QuestReward(QuestRewardType.Resource, GameResourceType.ResearchPoints.ToString(), 50) }; // 奖励：50研究点
            var q2Prereqs = new List<string> { "Q_FOUNDATION" }; // 前置任务ID列表
            AllQuests.Add("Q_REACH_SKY", new Quest("Q_REACH_SKY", "仰望星空", "我们需要找到一种方式发送求救信号。无线电塔可能是我们最好的选择。", q2Objectives, prereqQuestIds: q2Prereqs, rewards: q2Rewards));
            
            // 任务3：“灯塔计划：第一部分” - 依赖于任务 "Q_REACH_SKY" 的完成
            // 假设资源类型 “ElectronicParts”(电子零件) 已在其他地方定义
            var q3Objectives = new List<QuestObjective>
            {
                new QuestObjective("收集50单位电子零件", ObjectiveType.CollectResource, GameResourceType.ElectronicParts.ToString(), 50),
                new QuestObjective("收集100单位电力", ObjectiveType.CollectResource, GameResourceType.Power.ToString(), 100)
            };
            var q3Prereqs = new List<string> { "Q_REACH_SKY" };
            AllQuests.Add("Q_BEACON_1", new Quest("Q_BEACON_1", "灯塔计划：第一部分", "收集修理和升级无线电塔信标所需的部件。", q3Objectives, prereqQuestIds: q3Prereqs));

            // 关于第一个任务初始状态的说明：
            // 此处注释掉的代码 `AllQuests["Q_FOUNDATION"].Status = QuestStatus.Available;`
            // 表明任务的初始状态通常应由 QuestSystem 根据其前置条件逻辑（例如检查是否有前置任务、天数要求等）来确定，
            // 而不是在这里硬编码。
        }

        // 根据任务ID获取任务对象
        public Quest GetQuest(string questId)
        {
            AllQuests.TryGetValue(questId, out var quest);
            return quest;
        }
        
        // 更新任务状态，并同步维护活动任务列表和发送事件
        public void UpdateQuestStatus(string questId, QuestStatus newStatus) // 显式使用 Quest.QuestStatus
        {
            if (AllQuests.TryGetValue(questId, out var quest)) // 如果任务存在
            {
                var oldStatus = quest.Status; // 获取旧状态
                if (oldStatus != newStatus) // 仅当状态实际改变时才更新并发送事件
                {
                    quest.Status.Value = newStatus; // 更新任务状态

                    // 管理 ActiveQuests 列表
                    if (newStatus == QuestStatus.Active)
                    {
                        if (!ActiveQuests.Contains(quest))
                        {
                            ActiveQuests.Add(quest); // 添加到活动列表
                        }
                    }
                    else // 如果新状态不是 Active (例如 Success, Failed, Locked, Available)
                    {
                        if (ActiveQuests.Contains(quest))
                        {
                            ActiveQuests.Remove(quest); // 从活动列表中移除
                        }
                    }
                    // 注意: mCompletedQuests 或 mFailedQuests 列表的管理不在此方法中。
                    // QuestSystem 在调用此方法后，可能会根据新状态将任务移至已完成/失败列表（如果需要此类列表）。
                    // 或者，UI可以直接从 AllQuests 筛选已完成/失败的任务进行显示。

                    this.SendEvent(new Model_QuestStatusUpdatedEvent()
                    {
                        QuestId = questId,
                        NewStatus = newStatus,
                        OldStatus = oldStatus
                    });
                }
            }
        }
    }
}
