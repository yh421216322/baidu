using QFramework;
using UnityEngine; 
using YourGameNamespace.Survivors; 
using YourGameNamespace.Workstations; 
using YourGameNamespace.Enemies; 
using YourGameNamespace.Combat; 
using YourGameNamespace.Framework; // 用于 GameDataModel
using YourGameNamespace.Time;     // 用于 DayNightSystem
using YourGameNamespace.Events;   // 用于 EventModel 和 EventSystem
using YourGameNamespace.Research; // 用于 ResearchModel 和 ResearchSystem
using YourGameNamespace.Exploration; // 用于 ExplorationModel 和 ExplorationSystem

namespace YourGameNamespace
{
    public class GameArchitecture : Architecture<GameArchitecture>
    {
        protected override void Init()
        {
            this.RegisterSystem(new GEventSystem());
            // 注册模型
            this.RegisterModel(new ResourceModel());
            this.RegisterModel(new SurvivorModel()); 
            this.RegisterModel(new WorkstationModel());
            this.RegisterModel(new EnemyModel());
            this.RegisterModel(new GameDataModel());
            this.RegisterModel(new EventModel());
            this.RegisterModel(new ResearchModel());
            this.RegisterModel(new ExplorationModel()); // 已添加 ExplorationModel

            // 注册系统
            this.RegisterSystem(new ResourceManagerSystem());
            this.RegisterSystem(new SurvivorManagerSystem()); 
            this.RegisterSystem(new WorkstationSystem());
            this.RegisterSystem(new CombatSystem());
            this.RegisterSystem(new DayNightSystem());
            
            this.RegisterSystem(new ResearchSystem());
            this.RegisterSystem(new ExplorationSystem()); // 已添加 ExplorationSystem
            // ... 其他系统
        }
    }

    // 示例初始化器 (可以是一个 MonoBehaviour)
    
}
