using UnityEngine;
using UnityEngine.UI; // 用于 Text UI 组件
using QFramework;     // QFramework 框架
using YourGameNamespace.Framework; // 用于 GameDataModel (核心游戏数据模型)
using YourGameNamespace.Time;     // 用于 DayNightSystem (昼夜系统)

namespace YourGameNamespace.UI
{
    // UI组件，用于显示游戏内的天数、时间进度和基地健康状况
    public class DayDisplay : MonoBehaviour
    {
        // 在Unity检视面板中分配的UI Text组件
        public Text dayText;        // 用于显示当前天数
        public Text timeText;       // 用于显示当天时间进度或游戏状态（如游戏结束/胜利）
        public Text baseHealthText; // 用于显示基地健康值

        // 对所需模型和系统的引用
        private GameDataModel mGameDataModel;
        private DayNightSystem mDayNightSystem;

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
            mGameDataModel = GameArchitecture.Interface.GetModel<GameDataModel>();
            mDayNightSystem = GameArchitecture.Interface.GetSystem<DayNightSystem>();

            // 检查依赖项是否成功获取
            if (mGameDataModel == null) Debug.LogError("日期显示 (DayDisplay)：未能获取核心游戏数据模型 (GameDataModel)！");
            if (mDayNightSystem == null) Debug.LogError("日期显示 (DayDisplay)：未能获取昼夜系统 (DayNightSystem)！");
            if (dayText == null || timeText == null || baseHealthText == null) 
                Debug.LogError("日期显示 (DayDisplay)：一个或多个必要的UI Text组件（dayText, timeText, baseHealthText）未在Unity检视面板中分配！");
        }

        void Update() // Unity生命周期方法，每帧调用一次
        {
            // 安全检查，如果必要的UI Text组件未分配，则不执行更新逻辑
            if (dayText == null || timeText == null || baseHealthText == null) return; 

            if (mGameDataModel != null) // 如果核心游戏数据模型存在
            {
                if (mGameDataModel.BaseHealth <= 0) // 如果基地生命值耗尽（游戏结束）
                {
                    baseHealthText.text = "基地生命: 0"; // 显示基地生命为0
                    timeText.text = "游戏结束";        // 显示游戏结束信息
                    dayText.text = "天数: " + mGameDataModel.CurrentDay; // 显示结束时的天数
                }
                else if (mGameDataModel.CurrentDay > 100) // 如果达到胜利条件（例如，存活超过100天）
                {
                    baseHealthText.text = "基地生命: " + mGameDataModel.BaseHealth.ToString("F0"); // 显示基地当前生命值
                    timeText.text = "游戏胜利！";       // 显示胜利信息
                    dayText.text = "天数: " + mGameDataModel.CurrentDay; // 显示胜利时的天数
                }
                else // 正常游戏进行中
                {
                    dayText.text = "天数: " + mGameDataModel.CurrentDay; // 显示当前天数
                    baseHealthText.text = "基地生命: " + mGameDataModel.BaseHealth.ToString("F0"); // 显示当前基地生命值 (F0格式化为无小数整数)
                    
                    if (mDayNightSystem != null) // 如果昼夜系统存在
                    {
                        // 显示当天时间进度百分比 (F0格式化为无小数整数)
                        timeText.text = $"时间进度: {mDayNightSystem.TimeOfDayNormalized * 100:F0}%"; 
                    }
                    else
                    {
                        timeText.text = "时间进度: N/A"; // 如果昼夜系统不存在，则显示不可用
                    }
                }
            }
            else // 如果核心游戏数据模型不存在
            {
                // 显示所有信息为不可用 (N/A)
                dayText.text = "天数: N/A";
                baseHealthText.text = "基地生命: N/A";
                timeText.text = "时间进度: N/A";
            }
        }
    }
}
