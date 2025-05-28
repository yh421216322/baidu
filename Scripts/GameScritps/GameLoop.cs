using UnityEngine;
using QFramework;
using YourGameNamespace.Workstations; 
using YourGameNamespace.Combat; 
using YourGameNamespace.Time; 
using YourGameNamespace.Survivors; 
using YourGameNamespace.Research; 
using YourGameNamespace.Exploration; // ExplorationSystem 所需

namespace YourGameNamespace
{
    public class GameLoop : MonoBehaviour
    {
        private WorkstationSystem mWorkstationSystem;
        private CombatSystem mCombatSystem;
        private DayNightSystem mDayNightSystem; 
        private SurvivorManagerSystem mSurvivorManagerSystem; 
        private ResearchSystem mResearchSystem; 
        private ExplorationSystem mExplorationSystem; // 已添加 ExplorationSystem

        void Start()
        {
            if (GameArchitecture.Interface == null)
            {
                Debug.LogError("GameLoop 的 Start 调用时 GameArchitecture 尚未初始化。请确保 GameInitializer 先运行。");
                this.enabled = false; 
                return;
            }
            
            mWorkstationSystem = GameArchitecture.Interface.GetSystem<WorkstationSystem>();
            if (mWorkstationSystem == null) Debug.LogWarning("GameLoop 中未找到 WorkstationSystem。");

            mCombatSystem = GameArchitecture.Interface.GetSystem<CombatSystem>();
            if (mCombatSystem == null) Debug.LogWarning("GameLoop 中未找到 CombatSystem。");

            mDayNightSystem = GameArchitecture.Interface.GetSystem<DayNightSystem>();
            if (mDayNightSystem == null) Debug.LogWarning("GameLoop 中未找到 DayNightSystem。");

            mSurvivorManagerSystem = GameArchitecture.Interface.GetSystem<SurvivorManagerSystem>();
            if (mSurvivorManagerSystem == null) Debug.LogWarning("GameLoop 中未找到 SurvivorManagerSystem。");

            mResearchSystem = GameArchitecture.Interface.GetSystem<ResearchSystem>(); 
            if (mResearchSystem == null) Debug.LogWarning("GameLoop 中未找到 ResearchSystem。");

            mExplorationSystem = GameArchitecture.Interface.GetSystem<ExplorationSystem>(); // 初始化 ExplorationSystem
            if (mExplorationSystem == null) Debug.LogError("GameLoop.Start() 中未找到 ExplorationSystem。请确保它已在 GameArchitecture 中注册。");
        }

        void Update()
        {
            float deltaTime = UnityEngine.Time.deltaTime;

            if (mWorkstationSystem != null)
            {
                mWorkstationSystem.UpdateAllWorkstations(deltaTime);
            }

            if (mCombatSystem != null)
            {
                mCombatSystem.UpdateCombat(deltaTime);
            }

            if (mDayNightSystem != null)
            {
                mDayNightSystem.UpdateDayCycle(deltaTime);
            }

            if (mSurvivorManagerSystem != null) 
            {
                mSurvivorManagerSystem.UpdateSurvivorNeeds(deltaTime);
            }

            if (mResearchSystem != null) 
            {
                mResearchSystem.UpdateResearchProcess(deltaTime);
            }

            if (mExplorationSystem != null) // 调用 UpdateActiveExpeditions
            {
                mExplorationSystem.UpdateActiveExpeditions(deltaTime);
            }
        }
    }
}
