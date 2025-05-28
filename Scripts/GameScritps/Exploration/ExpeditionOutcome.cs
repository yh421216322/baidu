using System.Collections.Generic;


namespace YourGameNamespace.Exploration
{
    public class ExpeditionOutcome
    {
        public bool WasSuccessful { get; set; } = true; // 总体成功状态
        public Dictionary<GameResourceType, int> ResourcesFound { get; private set; } = new Dictionary<GameResourceType, int>();
        public List<string> SurvivorStatusChanges { get; private set; } = new List<string>(); // 例如："幸存者名称 受伤了"
        public string NarrativeLog { get; set; } = ""; // 事件摘要

        public void AddResource(GameResourceType type, int amount)
        {
            if (ResourcesFound.ContainsKey(type))
            {
                ResourcesFound[type] += amount;
            }
            else
            {
                ResourcesFound.Add(type, amount);
            }
        }
    }
}
