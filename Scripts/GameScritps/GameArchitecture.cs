using QFramework;
using UnityEngine; 
using YourGameNamespace.Survivors; 
using YourGameNamespace.Workstations; 
using YourGameNamespace.Enemies; 
using YourGameNamespace.Combat; 
using YourGameNamespace.Framework; // 用于 GameDataModel (游戏数据模型)
using YourGameNamespace.Time;     // 用于 DayNightSystem (昼夜系统)
using YourGameNamespace.Events;   // 用于 EventModel (事件模型) 和 EventSystem (事件系统)
using YourGameNamespace.Research; // 用于 ResearchModel (研究模型) 和 ResearchSystem (研究系统)
using YourGameNamespace.Exploration; // 用于 ExplorationModel (探索模型) 和 ExplorationSystem (探索系统)

namespace YourGameNamespace
{
    // 游戏整体架构，继承自QFramework的Architecture
    public class GameArchitecture : Architecture<GameArchitecture>
    {
        // 初始化架构时调用，用于注册所有模型和系统
        protected override void Init()
        {
            // 注册事件系统 (GEventSystem是具体实现类名)
            this.RegisterSystem(new GEventSystem());
            
            // 注册各种数据模型
            this.RegisterModel(new ResourceModel());      // 资源模型
            this.RegisterModel(new SurvivorModel());      // 幸存者模型
            this.RegisterModel(new WorkstationModel());   // 工作站模型
            this.RegisterModel(new EnemyModel());         // 敌人模型
            this.RegisterModel(new GameDataModel());      // 核心游戏数据模型
            this.RegisterModel(new EventModel());         // 事件数据模型
            this.RegisterModel(new ResearchModel());      // 研究数据模型
            this.RegisterModel(new ExplorationModel());   // 探索数据模型 (已添加)

            // 注册各种游戏逻辑系统
            this.RegisterSystem(new ResourceManagerSystem());   // 资源管理系统
            this.RegisterSystem(new SurvivorManagerSystem());   // 幸存者管理系统
            this.RegisterSystem(new WorkstationSystem());     // 工作站系统
            this.RegisterSystem(new CombatSystem());          // 战斗系统
            this.RegisterSystem(new DayNightSystem());        // 昼夜循环系统
            this.RegisterSystem(new ResearchSystem());        // 研究系统
            this.RegisterSystem(new ExplorationSystem());     // 探索系统 (已添加)
            // ... 未来可能添加其他系统
        }
    }

    // 示例初始化器 (可以是一个 MonoBehaviour 脚本，在场景启动时确保GameArchitecture被初始化)
    // 例如:
    // public class GameLauncher : MonoBehaviour {
    //     void Awake() {
    //         GameArchitecture.Interface.Init(); // 或者直接访问Interface属性就会触发初始化
    //     }
    // }
}
