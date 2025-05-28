using System.Collections.Generic;
using System.Linq; // 用于 All()

namespace YourGameNamespace.Quests
{
    public enum QuestStatus // 定义在 Quest.cs 文件中，但在 Quest 类之外
    {
        Locked,     // 前置条件未满足
        Available,  // 前置条件已满足，可以开始（或自动开始）
        Active,     // 进行中
        Success,    // 所有目标已达成
        Failed      // (可选) 如果任务有失败条件
    }

    [System.Serializable]
    public class Quest
    {
        public string Id { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; } // 任务总体描述
        public QuestStatus Status { get; set; }
        public List<QuestObjective> Objectives { get; private set; }
        
        // 前置条件
        public List<string> PrerequisiteQuestIds { get; private set; } // 必须为“成功”状态的任务ID列表
        public int RequiredDay { get; private set; } = 0; // 任务可用时的天数（0 = 无天数要求）
        public string RequiredTechId { get; private set; } = null; // 必须为“已完成”状态的技术ID

        public List<QuestReward> Rewards { get; private set; }

        public Quest(string id, string title, string description, List<QuestObjective> objectives, 
                     List<string> prereqQuestIds = null, int requiredDay = 0, string requiredTechId = null,
                     List<QuestReward> rewards = null)
        {
            Id = id;
            Title = title;
            Description = description;
            Objectives = objectives ?? new List<QuestObjective>();
            Status = QuestStatus.Locked; // 默认为锁定状态

            PrerequisiteQuestIds = prereqQuestIds ?? new List<string>();
            RequiredDay = requiredDay;
            RequiredTechId = requiredTechId;
            Rewards = rewards ?? new List<QuestReward>();
        }

        public bool AreAllObjectivesComplete()
        {
            if (!Objectives.Any()) return true; // 没有目标意味着它是无任务的，或者是基于标志的
            return Objectives.All(o => o.IsComplete);
        }
    }
}
