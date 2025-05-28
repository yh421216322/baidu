using QFramework;
using UnityEngine;
using YourGameNamespace.Framework; // 用于 GameDataModel
using YourGameNamespace.Combat;   // 用于 CombatSystem
using YourGameNamespace.Events;   // 用于 EventSystem

namespace YourGameNamespace.Time
{
    public class DayNightSystem : AbstractSystem
    {
        private GameDataModel mGameDataModel;
        private CombatSystem mCombatSystem;
        private GEventSystem mEventSystem; // 已添加 EventSystem
        private float mCurrentTimeInDay = 0f;

        public float SecondsPerDay { get; set; } = 60f; 
        public float TimeOfDayNormalized => mCurrentTimeInDay / SecondsPerDay;

        protected override void OnInit()
        {
         
            mGameDataModel = this.GetModel<GameDataModel>();
            mCombatSystem = this.GetSystem<CombatSystem>();
            mEventSystem = this.GetSystem<GEventSystem>(); // 初始化 EventSystem

            if (mGameDataModel == null) Debug.LogError("昼夜系统：游戏数据模型 (GameDataModel) 为空！");
            if (mCombatSystem == null) Debug.LogError("昼夜系统：战斗系统 (CombatSystem) 为空！");
            if (mEventSystem == null) Debug.LogError("昼夜系统：事件系统 (EventSystem) 为空！");

            mCurrentTimeInDay = 0f; 
            Debug.Log($"昼夜系统已初始化。第 {mGameDataModel.CurrentDay} 天开始。");
            TriggerNewDayEvents(mGameDataModel.CurrentDay); 
        }

        public void UpdateDayCycle(float deltaTime)
        {
            if (mGameDataModel == null || mGameDataModel.BaseHealth <= 0) return; // 如果游戏结束或模型丢失，则停止循环
            if (mGameDataModel.CurrentDay > 100) return; // 如果游戏胜利，则停止

            mCurrentTimeInDay += deltaTime;

            if (mCurrentTimeInDay >= SecondsPerDay)
            {
                mCurrentTimeInDay -= SecondsPerDay; // 或者 mCurrentTimeInDay = 0;
                mGameDataModel.CurrentDay++;
                Debug.Log($"第 {mGameDataModel.CurrentDay} 天已经开始！");
                this.SendEvent(new DayChangedEvent(mGameDataModel.CurrentDay)); // 发送事件

                if (mGameDataModel.CurrentDay > 100)
                {
                    Debug.LogWarning("游戏胜利！成功存活100天！");
                    // 可以在此处发送 GameWon 事件
                    return;
                }
                TriggerNewDayEvents(mGameDataModel.CurrentDay);
            }
        }

        private void TriggerNewDayEvents(int day)
        {
            if (mCombatSystem != null)
            {
                mCombatSystem.SpawnZombieWaveForDay(day); 
            }
            else
            {
                Debug.LogError("昼夜系统：在 TriggerNewDayEvents 中战斗系统 (CombatSystem) 为空。无法生成僵尸。");
            }

            if (mEventSystem != null)
            {
                mEventSystem.TryTriggerRandomEvent(); // 触发随机事件
            }
            else
            {
                Debug.LogError("昼夜系统：在 TriggerNewDayEvents 中事件系统 (EventSystem) 为空。无法触发事件。");
            }
        }
    }
}
