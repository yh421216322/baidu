using MyGameNamespace;
using UnityEngine;
using TMPro; // 使用 TextMeshPro
using QFramework;
using YourGameNamespace.Framework;
using YourGameNamespace.Survivors; // For ISurvivorModel
using YourGameNamespace.Events;   // For Model_SurvivorAddedEvent and potentially Model_SurvivorRemovedEvent

namespace YourGameNamespace.UI
{
    public class DayDisplay : MonoBehaviour, IController
    {
        public TextMeshProUGUI dayText;
        public TextMeshProUGUI timeText;
        public TextMeshProUGUI baseHealthText;
        public TextMeshProUGUI housingCapacityText; // 新增：住房容量文本

        private IGameDataModel mGameDataModel; // 使用接口
        private ISurvivorModel mSurvivorModel; // 使用接口

        private void Awake()
        {
            // 更改为 GetComponent<TextMeshProUGUI>()
            dayText = dayText ?? transform.Find("DayText")?.GetComponent<TextMeshProUGUI>();
            timeText = timeText ?? transform.Find("TimeText")?.GetComponent<TextMeshProUGUI>();
            baseHealthText = baseHealthText ?? transform.Find("BaseHealthText")?.GetComponent<TextMeshProUGUI>();
            housingCapacityText = housingCapacityText ?? transform.Find("HousingCapacityText")?.GetComponent<TextMeshProUGUI>();


            if (dayText == null) Debug.LogError("DayDisplay: UI元素 'DayText' (TextMeshPro) 未能成功获取或链接。");
            if (timeText == null) Debug.LogError("DayDisplay: UI元素 'TimeText' (TextMeshPro) 未能成功获取或链接。");
            if (baseHealthText == null) Debug.LogError("DayDisplay: UI元素 'BaseHealthText' (TextMeshPro) 未能成功获取或链接。");
            if (housingCapacityText == null) Debug.LogError("DayDisplay: UI元素 'HousingCapacityText' (TextMeshPro) 未能成功获取或链接!");
        }

        void Start()
        {
            if (RegisterManager.Interface == null)
            {
                Debug.LogError("日期显示 (DayDisplay)：GameArchitecture 尚未初始化！");
                return;
            }

            mGameDataModel = this.GetModel<IGameDataModel>();
            mSurvivorModel = this.GetModel<ISurvivorModel>(); // 获取幸存者模型

            if (mGameDataModel == null) Debug.LogError("日期显示 (DayDisplay)：未能获取核心游戏数据模型 (IGameDataModel)！");
            if (mSurvivorModel == null) Debug.LogError("日期显示 (DayDisplay)：未能获取幸存者数据模型 (ISurvivorModel)！");

            if (dayText == null || timeText == null || baseHealthText == null || housingCapacityText == null)
                Debug.LogError("日期显示 (DayDisplay)：一个或多个必要的UI TextMeshPro组件未在Unity检视面板中分配！");

            if (mGameDataModel != null)
            {
                mGameDataModel.CurrentDay.RegisterWithInitValue(day => {
                    UpdateDayText(day);
                    CheckGameEndConditions(); // 游戏结束条件检查也可能依赖天数
                }).UnRegisterWhenGameObjectDestroyed(this.gameObject);

                mGameDataModel.BaseHealth.RegisterWithInitValue(health => {
                    UpdateBaseHealthText(health);
                    CheckGameEndConditions();
                }).UnRegisterWhenGameObjectDestroyed(this.gameObject);

                // 注册监听住房容量变化
                mGameDataModel.MaxHousingCapacity.RegisterWithInitValue(UpdateHousingDisplayFromCapacityChange)
                   .UnRegisterWhenGameObjectDestroyed(gameObject);
            }

            // 为了动态更新当前幸存者数量，监听幸存者增加/移除的事件
            // 假设 SurvivorModel 发送这些事件
            this.RegisterEvent<Model_SurvivorAddedEvent>(e => UpdateHousingDisplay())
               .UnRegisterWhenGameObjectDestroyed(gameObject);
            // 假设存在 Model_SurvivorRemovedEvent
            // this.RegisterEvent<Model_SurvivorRemovedEvent>(e => UpdateHousingDisplay())
            //    .UnRegisterWhenGameObjectDestroyed(gameObject);

            // 初始刷新一次住房显示 (如果 MaxHousingCapacity 的 RegisterWithInitValue 不足以覆盖初始情况)
            // UpdateHousingDisplay(); // RegisterWithInitValue for MaxHousingCapacity handles initial call with capacity
        }

        // Parameterized version for MaxHousingCapacity changes
        private void UpdateHousingDisplayFromCapacityChange(int newMaxCapacity)
        {
            UpdateHousingDisplay(); // Just trigger the general update
        }

        private void UpdateHousingDisplay()
        {
            if (housingCapacityText != null && mGameDataModel != null && mSurvivorModel != null)
            {
                int currentSurvivors = mSurvivorModel.GetAllSurvivors().Count;
                // MaxHousingCapacity.Value 会从 mGameDataModel 获取最新的值
                housingCapacityText.text = $"住房: {currentSurvivors}/{mGameDataModel.MaxHousingCapacity.Value}";
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

            string timeStr = "";

            if (currentHealth <= 0)
            {
                timeStr = "游戏结束"; // 本地化: "游戏结束"
                if (dayText != null) dayText.text = "天数: " + currentDay;
                if (baseHealthText != null) baseHealthText.text = "基地生命: 0";
            }
            else if (currentDay > 100) // 假设100天为胜利条件
            {
                timeStr = "游戏胜利!"; // 本地化: "游戏胜利!"
            }
            // 如果游戏正常进行，timeText可以考虑显示其他信息，或保持为空
            // 例如，如果 DayNightSystem 存在并提供时间百分比:
            // else {
            //      IDayNightSystem dns = this.GetSystem<IDayNightSystem>();
            //      if (dns != null) timeStr = $"时间: {dns.TimeOfDayNormalized.Value * 100:F0}%";
            // }
            timeText.text = timeStr;
        }
        
        public IArchitecture GetArchitecture()
        {
            return RegisterManager.Interface;
        }
    }
}
