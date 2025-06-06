using QFramework;
using UnityEngine;
using YourGameNamespace.Framework; // 用于 GameDataModel (核心游戏数据模型)
using YourGameNamespace.Combat;   // 用于 CombatSystem (战斗系统)
using YourGameNamespace.Events;   // 用于 EventSystem (事件系统) 及 DayChangedEvent (天数变化事件)

namespace YourGameNamespace.Time
{
    public interface IDayNightSystem : QFramework.QFISystem
    {
        float SecondsPerDay { get; set; }
        float TimeOfDayNormalized { get; }
        void UpdateDayCycle(float deltaTime);
    }

    // 昼夜系统，负责管理游戏内时间的流逝、天数更迭以及每日触发的事件
    public class DayNightSystem : AbstractSystem, IDayNightSystem // Implements IDayNightSystem
    {
        private GameDataModel mGameDataModel;     // 核心游戏数据模型
        // Note: mCombatSystem and mEventSystem should ideally be interfaces too (ICombatSystem, IGEventSystem)
        private ICombatSystem mCombatSystem;       // 战斗系统，用于触发每日僵尸潮
        private IGEventSystem mEventSystem;        // 事件系统 (GEventSystem 是具体类名，假设有IGEventSystem接口)

        private float mCurrentTimeInDay = 0f;     // 当前这一天已经过去的时间（秒）

        // 每天的总秒数，可配置，默认为60秒
        public float SecondsPerDay { get; set; } = 60f; 
        // 当天时间进度的规范化值 (0到1之间，0表示一天的开始，1表示一天的结束)
        public float TimeOfDayNormalized => mCurrentTimeInDay / SecondsPerDay;

        // 系统初始化
        protected override void OnInit()
        {
            mGameDataModel = this.GetModel<GameDataModel>();
            mCombatSystem = this.GetSystem<ICombatSystem>(); // Use interface
            mEventSystem = this.GetSystem<IGEventSystem>(); // Use interface, assuming IGEventSystem exists

            // 检查依赖项是否成功获取
            if (mGameDataModel == null) Debug.LogError("昼夜系统：核心游戏数据模型 (GameDataModel) 未找到！");
            if (mCombatSystem == null) Debug.LogError("昼夜系统：战斗系统 (ICombatSystem) 未找到！");
            if (mEventSystem == null) Debug.LogError("昼夜系统：事件系统 (IGEventSystem) 未找到！");

            mCurrentTimeInDay = 0f; // 重置当天时间计数器
            // 注意：由于 GameDataModel.CurrentDay 现在是 BindableProperty，需要通过 .Value 访问其值
            Debug.Log($"昼夜系统已初始化。当前游戏从第 {mGameDataModel.CurrentDay.Value} 天开始。");
            TriggerNewDayEvents(mGameDataModel.CurrentDay.Value); // 为第一天触发每日事件
        }

        // 更新昼夜循环，由 GameLoop 每帧调用
        public void UpdateDayCycle(float deltaTime)
        {
            // 如果游戏数据模型不存在，或基地生命值已耗尽（游戏结束），或已达到胜利天数，则停止循环
            // 注意：GameDataModel.BaseHealth 现在是 BindableProperty，需要通过 .Value 访问其值
            if (mGameDataModel == null || mGameDataModel.BaseHealth.Value <= 0) return;
            if (mGameDataModel.CurrentDay.Value > 100) return; // 假设100天为胜利条件

            mCurrentTimeInDay += deltaTime; // 累加真实时间到当天时间计数器

            // 如果当天时间已达到或超过一天的总秒数，则表示新的一天开始
            if (mCurrentTimeInDay >= SecondsPerDay)
            {
                mCurrentTimeInDay -= SecondsPerDay; // 或者可以直接设为 mCurrentTimeInDay = 0;
                mGameDataModel.IncrementDay();        // 调用IncrementDay方法来增加天数
                Debug.Log($"第 {mGameDataModel.CurrentDay.Value} 天已经来临！");
                this.SendEvent(new DayChangedEvent(mGameDataModel.CurrentDay.Value)); // 发送天数变化事件

                if (mGameDataModel.CurrentDay.Value > 100) // 再次检查是否达到胜利条件
                {
                    Debug.LogWarning("游戏胜利！你已成功存活超过100天！");
                    // 可以在此处发送一个更具体的 GameWonEvent 事件，供其他系统响应
                    return; // 停止昼夜循环
                }
                TriggerNewDayEvents(mGameDataModel.CurrentDay.Value); // 为新的一天触发每日事件
            }
        }

        // 触发新的一天开始时发生的事件
        private void TriggerNewDayEvents(int day)
        {
            if (mCombatSystem != null)
            {
                mCombatSystem.SpawnZombieWaveForDay(day); // 命令战斗系统根据当前天数生成僵尸潮
            }
            else
            {
                Debug.LogError("昼夜系统：在 TriggerNewDayEvents 方法中，战斗系统 (ICombatSystem) 未找到。无法生成僵尸。");
            }

            if (mEventSystem != null)
            {
                mEventSystem.TryTriggerRandomEvent(); // 命令事件系统尝试触发一个随机事件
            }
            else
            {
                Debug.LogError("昼夜系统：在 TriggerNewDayEvents 方法中，事件系统 (IGEventSystem) 未找到。无法触发随机事件。");
            }
        }
    }
}
