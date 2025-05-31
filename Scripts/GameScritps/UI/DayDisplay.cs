using UnityEngine;
using UnityEngine.UI; // 用于 Text UI 组件
using QFramework;     // QFramework 框架
using YourGameNamespace.Framework; // 用于 GameDataModel (核心游戏数据模型)
// using YourGameNamespace.Time; // DayNightSystem is no longer directly used here

namespace YourGameNamespace.UI
{
    // UI组件，用于显示游戏内的天数、时间进度和基地健康状况
    public class DayDisplay : MonoBehaviour, IController
    {
        // 在Unity检视面板中分配的UI Text组件
        public Text dayText;        // 用于显示当前天数
        public Text timeText;       // 用于显示当天时间进度或游戏状态（如游戏结束/胜利）
        public Text baseHealthText; // 用于显示基地健康值

        // 对所需模型和系统的引用
        private GameDataModel mGameDataModel;
        // private DayNightSystem mDayNightSystem; // Removed

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void Start() // Unity生命周期方法，在第一次Update前执行
        {
            // 检查 GameArchitecture 是否已初始化
            if (GameArchitecture.Interface == null)
            {
                Debug.LogError("日期显示 (DayDisplay)：GameArchitecture 尚未初始化！此UI组件可能无法正常工作。请确保 GameInitializer 先运行。");
                this.enabled = false; // 禁用此组件以防止错误
                return;
            }
            // 获取所需模型和系统的实例
            mGameDataModel = this.GetModel<GameDataModel>();

            // 检查依赖项是否成功获取
            if (mGameDataModel == null) Debug.LogError("日期显示 (DayDisplay)：未能获取核心游戏数据模型 (GameDataModel)！");
            // if (mDayNightSystem == null) Debug.LogError("日期显示 (DayDisplay)：未能获取昼夜系统 (DayNightSystem)！"); // Removed
            if (dayText == null || timeText == null || baseHealthText == null) 
                Debug.LogError("日期显示 (DayDisplay)：一个或多个必要的UI Text组件（dayText, timeText, baseHealthText）未在Unity检视面板中分配！");

            if (mGameDataModel != null)
            {
                mGameDataModel.CurrentDay.RegisterWithInit(day => {
                    UpdateDayText(day);
                    CheckGameEndConditions();
                }).UnRegisterWhenGameObjectDestroyed(this);

                mGameDataModel.BaseHealth.RegisterWithInit(health => {
                    UpdateBaseHealthText(health);
                    CheckGameEndConditions();
                }).UnRegisterWhenGameObjectDestroyed(this);
            }
        }

        private void UpdateDayText(int day)
        {
            if (dayText != null) dayText.text = "天数: " + day;
        }

        private void UpdateBaseHealthText(float health)
        {
            if (baseHealthText != null) baseHealthText.text = "基地生命: " + health.ToString("F0");
        }

        private void CheckGameEndConditions()
        {
            if (mGameDataModel == null || timeText == null) return;

            int currentDay = mGameDataModel.CurrentDay.Value;
            float currentHealth = mGameDataModel.BaseHealth.Value;

            string timeStr = ""; // Default to empty or a placeholder if DayNightSystem interaction is removed

            if (currentHealth <= 0)
            {
                timeStr = "游戏结束";
                // Ensure final state text, though Register callbacks might have updated them
                if (dayText != null) dayText.text = "天数: " + currentDay;
                if (baseHealthText != null) baseHealthText.text = "基地生命: 0";
            }
            else if (currentDay > 100)
            {
                timeStr = "游戏胜利!";
            }
            // else // Normal game time display logic (if needed from DayNightSystem)
            // {
            //     // IDayNightSystem dayNightSystem = this.GetSystem<IDayNightSystem>(); // Example if DayNightSystem is needed
            //     // if (dayNightSystem != null)
            //     // {
            //     //     // Placeholder for time display logic, e.g., from a BindableProperty in DayNightSystem
            //     //     // timeStr = $"时间: {dayNightSystem.GetTimeOfDayNormalized() * 100:F0}%";
            //     // }
            // }
            timeText.text = timeStr;
        }

        // Update() method is removed as UI updates are driven by BindableProperty callbacks
    }
}
