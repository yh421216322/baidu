using System.Collections.Generic;
using QFramework; // 添加 using 语句

namespace YourGameNamespace.Research
{
    // 技术研究的状态枚举
    public enum ResearchStatus
    {
        Locked,     // 锁定状态：前置条件未满足，或技术尚未被发现/解锁
        Available,  // 可用状态：所有前置条件均已满足，可以开始研究
        InProgress, // 进行中：当前正在积极研究此技术
        Completed   // 已完成：技术研究已成功完成，其效果已应用于游戏
    }

    // 代表一项可研究的技术的数据类
    public class Technology
    {
        public string Id { get; private set; } // 技术的唯一标识符
        public string Name { get; private set; } // 技术的名称 (例如，“高级耕作”)
        public string Description { get; private set; } // 技术的详细描述
        public int ResearchPointCost { get; private set; } // 完成此项研究所需的研究点数（也可能代表时间或努力）
        public List<string> PrerequisiteTechIds { get; private set; } // 前置技术ID列表，表示必须先完成这些技术才能研究此技术
        public List<TechnologyEffectData> Effects { get; private set; } // 此技术完成后将应用的效果列表
        // 将 Status 修改为 BindableProperty，并设其 set 访问器为 private
        public BindableProperty<ResearchStatus> Status { get; private set; }

        // 构造函数
        public Technology(string id, string name, string description, int cost, List<string> prerequisites, List<TechnologyEffectData> effects)
        {
            Id = id;
            Name = name;                 // 技术名称应为中文
            Description = description;   // 技术描述应为中文
            ResearchPointCost = cost;
            PrerequisiteTechIds = prerequisites ?? new List<string>(); // 如果传入null则初始化为空列表
            Effects = effects ?? new List<TechnologyEffectData>();     // 如果传入null则初始化为空列表
            // 在构造函数中初始化 Status BindableProperty
            this.Status = new BindableProperty<ResearchStatus>(ResearchStatus.Locked); // 新创建的技术默认为锁定状态
        }

        // 添加公共方法 UpdateStatus 来修改 Status 的值
        public void UpdateStatus(ResearchStatus newStatus)
        {
            if (this.Status.Value != newStatus) // 只有当新状态与当前状态不同时才更新，以避免不必要的事件触发
            {
                this.Status.Value = newStatus;
            }
        }
    }
}
