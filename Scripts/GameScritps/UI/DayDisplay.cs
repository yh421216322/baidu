using UnityEngine;
using UnityEngine.UI;
using QFramework;
using YourGameNamespace.Framework; // 用于 GameDataModel
using YourGameNamespace.Time;     // 用于 DayNightSystem

namespace YourGameNamespace.UI
{
    public class DayDisplay : MonoBehaviour
    {
        public Text dayText; 
        public Text timeText; 
        public Text baseHealthText;

        private GameDataModel mGameDataModel;
        private DayNightSystem mDayNightSystem;

        void Start()
        {
            if (GameArchitecture.Interface == null)
            {
                Debug.LogError("日期显示：GameArchitecture 未初始化！");
                this.enabled = false;
                return;
            }
            mGameDataModel = GameArchitecture.Interface.GetModel<GameDataModel>();
            mDayNightSystem = GameArchitecture.Interface.GetSystem<DayNightSystem>();

            if (mGameDataModel == null) Debug.LogError("日期显示：未找到 GameDataModel！");
            if (mDayNightSystem == null) Debug.LogError("日期显示：未找到 DayNightSystem！");
            if (dayText == null || timeText == null || baseHealthText == null) 
                Debug.LogError("日期显示：一个或多个文本字段未在检视面板中分配！");
        }

        void Update()
        {
            if (dayText == null || timeText == null || baseHealthText == null) return; // 防止未分配字段的保护措施

            if (mGameDataModel != null)
            {
                if (mGameDataModel.BaseHealth <= 0)
                {
                    baseHealthText.text = "Base HP: 0";
                    timeText.text = "GAME OVER";
                    dayText.text = "Day: " + mGameDataModel.CurrentDay;
                }
                else if (mGameDataModel.CurrentDay > 100)
                {
                    baseHealthText.text = "Base HP: " + mGameDataModel.BaseHealth.ToString("F0");
                    timeText.text = "VICTORY!";
                    dayText.text = "Day: " + mGameDataModel.CurrentDay;
                }
                else
                {
                    dayText.text = "Day: " + mGameDataModel.CurrentDay;
                    baseHealthText.text = "Base HP: " + mGameDataModel.BaseHealth.ToString("F0");
                    if (mDayNightSystem != null)
                    {
                        timeText.text = $"Time: {mDayNightSystem.TimeOfDayNormalized * 100:F0}%";
                    }
                    else
                    {
                        timeText.text = "Time: N/A";
                    }
                }
            }
            else
            {
                dayText.text = "Day: N/A";
                baseHealthText.text = "Base HP: N/A";
                timeText.text = "Time: N/A";
            }
        }
    }
}
