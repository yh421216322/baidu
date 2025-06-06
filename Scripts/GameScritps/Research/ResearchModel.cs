using QFramework;
using System.Collections.Generic;
using System.Linq; // 用于 Linq 操作，例如 .Where() 和 .All()
using YourGameNamespace.Workstations; // 用于 WorkstationType 枚举
using YourGameNamespace.Events;       // 用于 Model_TechnologyStatusUpdatedEvent

namespace YourGameNamespace.Research
{
    // 研究数据模型，存储所有可研究的技术及其状态
    public class ResearchModel : AbstractModel // 移除 ICanGetSystem，因为不再需要GetSystem<ResearchSystem>()
    {
        // 存储所有已定义技术的字典，键为技术ID
        private Dictionary<string, Technology> mAllTechnologies = new Dictionary<string, Technology>();
        // private ResearchSystem mResearchSystem; // 对研究系统的引用，用于触发状态变更事件 -- 已移除

        // 模型初始化时调用
        protected override void OnInit()
        {
            // mResearchSystem = this.GetSystem<ResearchSystem>(); // 获取研究系统实例 -- 已移除
            PopulateInitialTechnologies(); // 填充初始技术数据
            UpdateAllTechnologyStatuses(); // 基于已完成的前置条件，在游戏开始时更新一次所有技术的状态
        }

        // 填充初始技术数据的方法
        private void PopulateInitialTechnologies()
        {
            // 示例技术1：改进型农业 I
            mAllTechnologies.Add("TECH_FARM_1", new Technology("TECH_FARM_1", "改进型农业 I", "农场食物产量提高10%。", 50, null,
                new List<TechnologyEffectData> { new TechnologyEffectData(TechnologyEffectType.IncreaseProductionMultiplier, 0.1f, WorkstationType.Farm, GameResourceType.Food) }));

            // 示例技术2：基础弹药制作
            mAllTechnologies.Add("TECH_AMMO_1", new Technology("TECH_AMMO_1", "基础弹药制作", "工坊每次生产循环的弹药产量提高1单位。", 75, null,
                new List<TechnologyEffectData> { new TechnologyEffectData(TechnologyEffectType.IncreaseProductionOutput, 1f, WorkstationType.Workshop, GameResourceType.Ammo) }));
            
            // 概念性技术：解锁研究实验室。假设 WorkstationType.ResearchLab (研究实验室) 已存在。
            mAllTechnologies.Add("TECH_UNLOCK_LAB", new Technology("TECH_UNLOCK_LAB", "基础研究方法", "解锁研究实验室。", 25, null,
                new List<TechnologyEffectData> { new TechnologyEffectData(TechnologyEffectType.UnlockWorkstation, (float)WorkstationType.ResearchLab, WorkstationType.ResearchLab ) }));

            // 添加一个依赖于其他技术的示例技术，用于测试前置条件逻辑
             mAllTechnologies.Add("TECH_FARM_2", new Technology("TECH_FARM_2", "高级农业", "农场食物产量进一步提高15%。", 100,
                new List<string> { "TECH_FARM_1" }, // 前置技术ID：TECH_FARM_1
                new List<TechnologyEffectData> { new TechnologyEffectData(TechnologyEffectType.IncreaseProductionMultiplier, 0.15f, WorkstationType.Farm, GameResourceType.Food) }));
            
            // 添加基础弹道学技术示例
            mAllTechnologies.Add("TECH_BALLISTICS_1", new Technology("TECH_BALLISTICS_1", "基础弹道学", "幸存者攻击力提高5%。", 100, null,
                new List<TechnologyEffectData> { new TechnologyEffectData(TechnologyEffectType.ModifySurvivorStat, 0.05f) }));

            // 已添加的基础无线电通讯技术 (TECH_RADIO_BASIC)
            mAllTechnologies.Add("TECH_RADIO_BASIC", new Technology(
                "TECH_RADIO_BASIC", 
                "基础无线电通讯",
                "允许进行初步的远程信号传输。对于寻求外界联系至关重要。",
                30, // 研究点成本
                null, // 无前置技术条件
                new List<TechnologyEffectData>() // 此技术本身无直接游戏效果，主要用作任务系统中的标记或前置条件
            ));
        }

        // 根据技术ID获取技术对象
        public Technology GetTechnology(string techId)
        {
            mAllTechnologies.TryGetValue(techId, out var tech);
            return tech;
        }

        // 获取所有已定义技术的列表
        public List<Technology> GetAllTechnologies()
        {
            return new List<Technology>(mAllTechnologies.Values); // 返回字典中所有值的列表副本
        }

        // 获取所有当前可供研究的技术列表 (状态为 Available)
        public List<Technology> GetAvailableTechnologies()
        {
            return mAllTechnologies.Values.Where(t => t.Status == ResearchStatus.Available).ToList();
        }

        // 获取所有已完成研究的技术列表 (状态为 Completed)
        public List<Technology> GetCompletedTechnologies()
        {
            return mAllTechnologies.Values.Where(t => t.Status == ResearchStatus.Completed).ToList();
        }
        
        // 获取所有正在进行中研究的技术列表 (状态为 InProgress)
        public List<Technology> GetInProgressTechnologies()
        {
            return mAllTechnologies.Values.Where(t => t.Status == ResearchStatus.InProgress).ToList();
        }

        // 更新指定ID的技术的状态
        public void UpdateTechnologyStatus(string techId, ResearchStatus newStatus)
        {
            if (mAllTechnologies.TryGetValue(techId, out var techToUpdate)) // 如果技术存在
            {
                var oldStatus = techToUpdate.Status; // 获取旧状态
                if (oldStatus != newStatus) // 仅当状态实际改变时才更新并发送事件
                {
                    techToUpdate.Status.Value = newStatus; // 更新其状态
                    this.SendEvent(new Model_TechnologyStatusUpdatedEvent()
                    {
                        TechId = techId,
                        NewStatus = newStatus,
                        OldStatus = oldStatus
                    });
                }
            }
        }

        // 遍历所有技术，并根据其已完成的前置条件更新它们的状态
        // 这个方法应该在可能影响技术可用性的事件发生后（例如，某项技术研究完成）被调用
        public void UpdateAllTechnologyStatuses()
        {
            foreach (var tech in mAllTechnologies.Values) // 遍历所有技术
            {
                if (tech.Status == ResearchStatus.Locked) // 只尝试解锁当前处于“锁定”状态的技术
                {
                    bool prerequisitesMet = true; // 假设所有前置条件都已满足
                    if (tech.PrerequisiteTechIds != null && tech.PrerequisiteTechIds.Count > 0) // 如果该技术有前置技术要求
                    {
                        foreach (string prereqId in tech.PrerequisiteTechIds) // 遍历所有前置技术ID
                        {
                            // 检查每个前置技术是否存在且已完成研究
                            if (!mAllTechnologies.TryGetValue(prereqId, out var prereqTech) || prereqTech.Status != ResearchStatus.Completed)
                            {
                                prerequisitesMet = false; // 如果任何一个前置条件未满足，则标记为false
                                break; // 并跳出内部循环
                            }
                        }
                    }
                    if (prerequisitesMet) // 如果所有前置条件都已满足
                    {
                        var oldStatus = tech.Status; // 此时 oldStatus 必定是 Locked
                        tech.Status.Value = ResearchStatus.Available; // 将技术状态更新为“可用”
                        // 由于是从 Locked 变为 Available，状态必然改变，所以发送事件
                        this.SendEvent(new Model_TechnologyStatusUpdatedEvent()
                        {
                            TechId = tech.Id,
                            NewStatus = tech.Status,
                            OldStatus = oldStatus
                        });
                    }
                }
                // 对于其他状态转换（例如 InProgress -> Completed），这些通常由 ResearchSystem 在完成研究时通过调用 UpdateTechnologyStatus 来处理，
                // UpdateTechnologyStatus 内部会发送事件。
                // 如果此方法也需要处理其他类型的自动状态更新（例如，因某些条件技术变为不可用），则也应在此处添加相应的事件发送逻辑。
            }
        }
    }
}
