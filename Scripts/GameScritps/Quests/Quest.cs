using System.Collections.Generic;
using System.Linq; // 用于 Linq 的 All() 方法

namespace YourGameNamespace.Quests
{
    // 任务状态枚举 (定义在 Quest.cs 文件中，但在 Quest 类之外，以便其他相关类可以访问)
    public enum QuestStatus 
    {
        Locked,     // 锁定：前置条件未满足，任务不可用
        Available,  // 可用：前置条件已满足，任务可以开始（或已自动开始，等待玩家查看）
        Active,     // 活动：任务正在进行中
        Success,    // 成功：所有目标均已达成
        Failed      // 失败：(可选状态) 如果任务存在失败条件且已被触发
    }

    // 代表一个任务的数据类
    // [System.Serializable] 属性允许该类的实例在Unity检视面板中被序列化和编辑（如果用作MonoBehaviour的公共字段）
    [System.Serializable] 
    public class Quest
    {
        public string Id { get; private set; } // 任务的唯一ID
        public string Title { get; private set; } // 任务的标题 (例如，“收集食物”)
        public string Description { get; private set; } // 任务的详细描述
        public QuestStatus Status { get; set; } // 任务的当前状态
        public List<QuestObjective> Objectives { get; private set; } // 任务包含的目标列表
        
        // --- 任务的前置条件 ---
        // 需要先成功完成的其他任务的ID列表
        public List<string> PrerequisiteQuestIds { get; private set; } 
        // 任务变为可用状态所需的最低游戏天数 (0 表示没有天数要求)
        public int RequiredDay { get; private set; } = 0; 
        // 任务变为可用状态所需完成的特定技术的ID (null 或空字符串表示没有技术要求)
        public string RequiredTechId { get; private set; } = null; 

        // 任务完成后给予的奖励列表
        public List<QuestReward> Rewards { get; private set; }

        // 构造函数
        public Quest(string id, string title, string description, List<QuestObjective> objectives, 
                     List<string> prereqQuestIds = null, int requiredDay = 0, string requiredTechId = null,
                     List<QuestReward> rewards = null)
        {
            Id = id;
            Title = title;
            Description = description;
            Objectives = objectives ?? new List<QuestObjective>(); // 如果传入null则初始化为空列表
            Status = QuestStatus.Locked; // 新创建的任务默认为锁定状态

            PrerequisiteQuestIds = prereqQuestIds ?? new List<string>();
            RequiredDay = requiredDay;
            RequiredTechId = requiredTechId;
            Rewards = rewards ?? new List<QuestReward>();
        }

        // 检查此任务的所有目标是否均已完成
        public bool AreAllObjectivesComplete()
        {
            // 如果任务没有任何目标，则视为空目标任务，直接算作完成 (例如，某些仅用于触发剧情或检查状态的任务)
            if (!Objectives.Any()) return true; 
            // 使用Linq的All()方法检查是否所有目标(o)的IsComplete属性都为true
            return Objectives.All(o => o.IsComplete);
        }
    }
}
