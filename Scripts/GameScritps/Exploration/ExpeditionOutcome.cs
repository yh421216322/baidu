using System.Collections.Generic;

namespace YourGameNamespace.Exploration
{
    // 代表远征结果的数据类
    public class ExpeditionOutcome
    {
        // 远征是否总体成功
        public bool WasSuccessful { get; set; } = true; 
        // 本次远征中找到的资源及其数量
        public Dictionary<GameResourceType, int> ResourcesFound { get; private set; } = new Dictionary<GameResourceType, int>();
        // 幸存者状态变化的描述列表 (例如："幸存者A 受伤了", "幸存者B 安然无恙")
        public List<string> SurvivorStatusChanges { get; private set; } = new List<string>(); 
        // 远征的叙事性日志或摘要，用于向玩家展示发生了什么
        public string NarrativeLog { get; set; } = ""; 

        // 向结果中添加找到的资源
        public void AddResource(GameResourceType type, int amount)
        {
            if (ResourcesFound.ContainsKey(type)) // 如果已记录过该类型资源
            {
                ResourcesFound[type] += amount; // 则累加数量
            }
            else
            {
                ResourcesFound.Add(type, amount); // 否则添加新的资源条目
            }
        }
    }
}
