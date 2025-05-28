using QFramework;
using System.Collections.Generic;
using System.Linq;
using YourGameNamespace.Workstations; // 用于示例任务中的 WorkstationType
using YourGameNamespace.Research; // 用于示例任务中的 Technology (特指技术ID)


namespace YourGameNamespace.Quests
{
    public class QuestModel : AbstractModel
    {
        public Dictionary<string, Quest> AllQuests { get; private set; } = new Dictionary<string, Quest>();
        public List<Quest> ActiveQuests { get; private set; } = new List<Quest>(); // 当前正在进行的任务

        protected override void OnInit()
        {
     
            PopulateInitialQuests();
        }

        private void PopulateInitialQuests()
        {
            // 任务1：“巩固基础”
            var q1Objectives = new List<QuestObjective>
            {
                new QuestObjective("Collect 100 Food", ObjectiveType.CollectResource, GameResourceType.Food.ToString(), 100),
                new QuestObjective("Collect 50 Power", ObjectiveType.CollectResource, GameResourceType.Power.ToString(), 50)
            };
            var q1Rewards = new List<QuestReward> { new QuestReward(QuestRewardType.Resource, GameResourceType.Ammo.ToString(), 20) };
            AllQuests.Add("Q_FOUNDATION", new Quest("Q_FOUNDATION", "Secure the Foundation", "Establish basic resource production to ensure initial survival.", q1Objectives, rewards: q1Rewards));

            // 任务2：“仰望星空” - 依赖于 Q_FOUNDATION
            // 假设技术“TECH_RADIO_BASIC”和POI“POI_RADIO_TOWER”将在其他地方定义（根据计划步骤4）
            var q2Objectives = new List<QuestObjective>
            {
                new QuestObjective("Research 'Basic Radio Communications'", ObjectiveType.ResearchTech, "TECH_RADIO_BASIC", 1),
                new QuestObjective("Scout the 'Old Radio Tower'", ObjectiveType.ExplorePOI, "POI_RADIO_TOWER", 1)
            };
            var q2Rewards = new List<QuestReward> { new QuestReward(QuestRewardType.Resource, GameResourceType.ResearchPoints.ToString(), 50) };
            var q2Prereqs = new List<string> { "Q_FOUNDATION" };
            AllQuests.Add("Q_REACH_SKY", new Quest("Q_REACH_SKY", "Reach for the Sky", "We need to find a way to signal for help. A radio tower might be our best bet.", q2Objectives, prereqQuestIds: q2Prereqs, rewards: q2Rewards));
            
            // 任务3：“灯塔计划：第一部分” - 依赖于 Q_REACH_SKY
            // 假设资源“ElectronicParts”将在其他地方定义（根据计划步骤4）
            var q3Objectives = new List<QuestObjective>
            {
                new QuestObjective("Collect 50 Electronic Parts", ObjectiveType.CollectResource, GameResourceType.ElectronicParts.ToString(), 50),
                new QuestObjective("Collect 100 Power", ObjectiveType.CollectResource, GameResourceType.Power.ToString(), 100)
            };
            var q3Prereqs = new List<string> { "Q_REACH_SKY" };
            AllQuests.Add("Q_BEACON_1", new Quest("Q_BEACON_1", "The Beacon Project: Part 1", "Gather components to repair and upgrade the radio tower beacon.", q3Objectives, prereqQuestIds: q3Prereqs));

            // 第一个任务的初始状态（可由QuestSystem检查前置条件来处理）
            // AllQuests["Q_FOUNDATION"].Status = QuestStatus.Available; 
        }

        public Quest GetQuest(string questId)
        {
            AllQuests.TryGetValue(questId, out var quest);
            return quest;
        }
        
        public void UpdateQuestStatus(string questId, QuestStatus status)
        {
            if (AllQuests.TryGetValue(questId, out var quest))
            {
                quest.Status = status;
                if (status == QuestStatus.Active && !ActiveQuests.Contains(quest))
                {
                    ActiveQuests.Add(quest);
                }
                else if (status != QuestStatus.Active && ActiveQuests.Contains(quest))
                {
                    ActiveQuests.Remove(quest);
                }
            }
        }
    }
}
