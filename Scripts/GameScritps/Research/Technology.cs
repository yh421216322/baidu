using System.Collections.Generic;

namespace YourGameNamespace.Research
{
    public enum ResearchStatus
    {
        Locked,     // 前置条件未满足或尚未发现
        Available,  // 可以研究
        InProgress, // 当前正在研究
        Completed   // 研究完成，效果已应用
    }

    public class Technology
    {
        public string Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public int ResearchPointCost { get; private set; }
        public List<string> PrerequisiteTechIds { get; private set; }
        public List<TechnologyEffectData> Effects { get; private set; }
        public ResearchStatus Status { get; set; } // 状态将由 ResearchModel/System 管理

        public Technology(string id, string name, string description, int cost, List<string> prerequisites, List<TechnologyEffectData> effects)
        {
            Id = id;
            Name = name;
            Description = description;
            ResearchPointCost = cost;
            PrerequisiteTechIds = prerequisites ?? new List<string>();
            Effects = effects ?? new List<TechnologyEffectData>();
            Status = ResearchStatus.Locked; // 初始状态
        }
    }
}
