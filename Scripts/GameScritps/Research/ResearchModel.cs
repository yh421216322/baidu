using QFramework;
using System.Collections.Generic;
using System.Linq; // 用于 Linq 操作，如 Where 和 All
using YourGameNamespace.Workstations; // 用于 WorkstationType


namespace YourGameNamespace.Research
{
    public class ResearchModel : AbstractModel,ICanGetSystem
    {
        private Dictionary<string, Technology> mAllTechnologies = new Dictionary<string, Technology>();
        private ResearchSystem mResearchSystem;
        protected override void OnInit()
        {
            mResearchSystem = this.GetSystem<ResearchSystem>();
            PopulateInitialTechnologies();
            UpdateAllTechnologyStatuses(); // 基于已完成的前置条件进行初始状态更新
          
        }

        private void PopulateInitialTechnologies()
        {
            // 上一步的示例技术
            mAllTechnologies.Add("TECH_FARM_1", new Technology("TECH_FARM_1", "Improved Farming I", "Increases food output from Farms by 10%.", 50, null,
                new List<TechnologyEffectData> { new TechnologyEffectData(TechnologyEffectType.IncreaseProductionMultiplier, 0.1f, WorkstationType.Farm, GameResourceType.Food) }));

            mAllTechnologies.Add("TECH_AMMO_1", new Technology("TECH_AMMO_1", "Basic Ammunition Crafting", "Improves ammo production at Workshops by 1 unit per cycle.", 75, null,
                new List<TechnologyEffectData> { new TechnologyEffectData(TechnologyEffectType.IncreaseProductionOutput, 1f, WorkstationType.Workshop, GameResourceType.Ammo) }));
            
            // 概念性：解锁研究实验室。假设 WorkstationType.ResearchLab 将存在。
            mAllTechnologies.Add("TECH_UNLOCK_LAB", new Technology("TECH_UNLOCK_LAB", "Basic Research Methods", "Unlocks the Research Lab.", 25, null,
                new List<TechnologyEffectData> { new TechnologyEffectData(TechnologyEffectType.UnlockWorkstation, (float)WorkstationType.ResearchLab, WorkstationType.ResearchLab ) }));

            // 添加一个依赖于其他技术的用于测试前置条件的技术
             mAllTechnologies.Add("TECH_FARM_2", new Technology("TECH_FARM_2", "Advanced Farming", "Further increases food output by 15%.", 100, 
                new List<string> { "TECH_FARM_1" },
                new List<TechnologyEffectData> { new TechnologyEffectData(TechnologyEffectType.IncreaseProductionMultiplier, 0.15f, WorkstationType.Farm, GameResourceType.Food) }));
            
            // 添加基础弹道学技术
            mAllTechnologies.Add("TECH_BALLISTICS_1", new Technology("TECH_BALLISTICS_1", "Basic Ballistics", "Increases survivor attack power by 5%.", 100, null,
                new List<TechnologyEffectData> { new TechnologyEffectData(TechnologyEffectType.ModifySurvivorStat, 0.05f) }));

            // 已添加 TECH_RADIO_BASIC
            mAllTechnologies.Add("TECH_RADIO_BASIC", new Technology(
                "TECH_RADIO_BASIC", 
                "Basic Radio Communications", 
                "Allows for rudimentary long-range signaling. Essential for reaching out.", 
                30, // 研究点成本
                null, // 前置条件
                new List<TechnologyEffectData>() // 无直接游戏效果，用作任务标记
            ));
        }

        public Technology GetTechnology(string techId)
        {
            mAllTechnologies.TryGetValue(techId, out var tech);
            return tech;
        }

        public List<Technology> GetAllTechnologies()
        {
            return new List<Technology>(mAllTechnologies.Values);
        }

        public List<Technology> GetAvailableTechnologies()
        {
            return mAllTechnologies.Values.Where(t => t.Status == ResearchStatus.Available).ToList();
        }

        public List<Technology> GetCompletedTechnologies()
        {
            return mAllTechnologies.Values.Where(t => t.Status == ResearchStatus.Completed).ToList();
        }
        
        public List<Technology> GetInProgressTechnologies()
        {
            return mAllTechnologies.Values.Where(t => t.Status == ResearchStatus.InProgress).ToList();
        }

        public void UpdateTechnologyStatus(string techId, ResearchStatus newStatus)
        {
            if (mAllTechnologies.TryGetValue(techId, out var tech))
            {
                tech.Status = newStatus;
            }
            
            
        }

        // 调用此方法以根据已完成的前置条件更新状态
        public void UpdateAllTechnologyStatuses()
        {
            foreach (var tech in mAllTechnologies.Values)
            {
                var oldStatus = tech.Status;
                if (tech.Status == ResearchStatus.Locked) // 只尝试解锁锁定的技术
                {
                    bool prerequisitesMet = true;
                    if (tech.PrerequisiteTechIds != null && tech.PrerequisiteTechIds.Count > 0)
                    {
                        foreach (string prereqId in tech.PrerequisiteTechIds)
                        {
                            if (!mAllTechnologies.TryGetValue(prereqId, out var prereqTech) || prereqTech.Status != ResearchStatus.Completed)
                            {
                                prerequisitesMet = false;
                                break;
                            }
                        }
                    }
                    if (prerequisitesMet)
                    {
                        tech.Status = ResearchStatus.Available;
                    }
                }
                if (oldStatus != tech.Status)
                {
                    // 通过 ResearchSystem 触发事件
                    mResearchSystem.NotifyTechnologyStatusChanged(tech);
                }
              
            }
        }
    }
}
