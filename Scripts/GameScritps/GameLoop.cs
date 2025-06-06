using MyGameNamespace;
using UnityEngine;
using QFramework;
using YourGameNamespace.Workstations; 
using YourGameNamespace.Combat; 
using YourGameNamespace.Time; 
using YourGameNamespace.Survivors; 
using YourGameNamespace.Research; 
using YourGameNamespace.Exploration; // 需要 ExplorationSystem 命名空间

namespace YourGameNamespace
{
    // 游戏主循环脚本，负责驱动游戏中各个系统的更新
    public class GameLoop : MonoBehaviour
    {
        // 引用各个游戏系统
        private WorkstationSystem mWorkstationSystem;     // 工作站系统
        private CombatSystem mCombatSystem;               // 战斗系统
        private DayNightSystem mDayNightSystem;           // 昼夜系统
        private SurvivorManagerSystem mSurvivorManagerSystem; // 幸存者管理系统
        private ResearchSystem mResearchSystem;           // 研究系统
        private ExplorationSystem mExplorationSystem;     // 探索系统 (已添加)

        void Start() // Unity生命周期方法，在第一次Update调用前执行
        {
            // 检查 GameArchitecture 是否已初始化，这是获取系统和模型的前提
            if (RegisterManager.Interface == null)
            {
                Debug.LogError("游戏主循环 (GameLoop) 的 Start 方法调用时，GameArchitecture 尚未初始化。请确保 GameInitializer 脚本先于此脚本运行。");
                this.enabled = false; // 禁用此脚本以防止后续错误
                return;
            }
            
            // 从 GameArchitecture 获取各个系统的实例
            mWorkstationSystem = RegisterManager.Interface.GetSystem<WorkstationSystem>();
            if (mWorkstationSystem == null) Debug.LogWarning("游戏主循环 (GameLoop) 中未找到工作站系统 (WorkstationSystem)。");

            mCombatSystem = RegisterManager.Interface.GetSystem<CombatSystem>();
            if (mCombatSystem == null) Debug.LogWarning("游戏主循环 (GameLoop) 中未找到战斗系统 (CombatSystem)。");

            mDayNightSystem = RegisterManager.Interface.GetSystem<DayNightSystem>();
            if (mDayNightSystem == null) Debug.LogWarning("游戏主循环 (GameLoop) 中未找到昼夜系统 (DayNightSystem)。");

            mSurvivorManagerSystem = RegisterManager.Interface.GetSystem<SurvivorManagerSystem>();
            if (mSurvivorManagerSystem == null) Debug.LogWarning("游戏主循环 (GameLoop) 中未找到幸存者管理系统 (SurvivorManagerSystem)。");

            mResearchSystem = RegisterManager.Interface.GetSystem<ResearchSystem>(); 
            if (mResearchSystem == null) Debug.LogWarning("游戏主循环 (GameLoop) 中未找到研究系统 (ResearchSystem)。");

            mExplorationSystem = RegisterManager.Interface.GetSystem<ExplorationSystem>(); // 初始化探索系统
            if (mExplorationSystem == null) Debug.LogError("游戏主循环 (GameLoop) 的 Start() 方法中未找到探索系统 (ExplorationSystem)。请确保它已在 GameArchitecture 中正确注册。");
        }

        void Update() // Unity生命周期方法，每帧调用一次
        {
            float deltaTime = UnityEngine.Time.deltaTime; // 获取自上一帧以来的时间差

            // 依次更新各个游戏系统
            if (mWorkstationSystem != null)
            {
                mWorkstationSystem.UpdateAllWorkstations(deltaTime); // 更新所有工作站状态
            }

            if (mCombatSystem != null)
            {
                mCombatSystem.UpdateCombat(deltaTime); // 更新战斗逻辑
            }

            if (mDayNightSystem != null)
            {
                mDayNightSystem.UpdateDayCycle(deltaTime); // 更新昼夜循环
            }

            if (mSurvivorManagerSystem != null) 
            {
                mSurvivorManagerSystem.UpdateSurvivorNeeds(deltaTime); // 更新幸存者需求
            }

            if (mResearchSystem != null) 
            {
                mResearchSystem.UpdateResearchProcess(deltaTime); // 更新研究进度
            }

            if (mExplorationSystem != null) // 调用探索系统的更新方法
            {
                mExplorationSystem.UpdateActiveExpeditions(deltaTime); // 更新活动远征的状态
            }
        }
    }
}
