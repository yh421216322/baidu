using QFramework;
using UnityEngine; // 用于 Mathf.Clamp

namespace YourGameNamespace.Framework
{
    // 核心游戏数据模型，存储游戏全局状态信息
    public class GameDataModel : AbstractModel
    {
        // 当前游戏进行到的天数，使用 BindableProperty 以便UI或其他模块可以监听其变化
        public BindableProperty<int> CurrentDay { get; private set; }
        // 玩家基地的当前健康值，使用 BindableProperty 以便UI或其他模块可以监听其变化
        public BindableProperty<float> BaseHealth { get; private set; }

        private const float MAX_BASE_HEALTH = 100f; // 定义基地最大生命值常量

        // 模型初始化时调用
        protected override void OnInit()
        {
            // 初始化 BindableProperty
            CurrentDay = new BindableProperty<int>(1);
            BaseHealth = new BindableProperty<float>(MAX_BASE_HEALTH);
        }

        // 增加天数
        public void IncrementDay()
        {
            CurrentDay.Value++;
            // BindableProperty 会自动通知已注册的监听者。
            // 如果确实需要全局QFramework事件，可以在此添加:
            // this.SendEvent(new Model_CurrentDayChangedEvent(CurrentDay.Value));
        }

        // 对基地造成伤害
        public void ApplyDamageToBase(float amount)
        {
            if (amount <= 0) return; // 伤害值应为正数

            // float oldValue = BaseHealth.Value; // 如果发送事件需要旧值
            BaseHealth.Value -= amount;
            if (BaseHealth.Value < 0)
            {
                BaseHealth.Value = 0;
            }
            // BindableProperty 会自动通知。如果需要全局事件:
            // this.SendEvent(new Model_BaseHealthChangedEvent(BaseHealth.Value, oldValue, MAX_BASE_HEALTH));
        }

        // 设置基地健康值 (例如，用于修复或直接设定)
        public void SetBaseHealth(float value)
        {
            // float oldValue = BaseHealth.Value; // 如果发送事件需要旧值
            // 使用 Mathf.Clamp 确保健康值在有效范围内 (0 到 MAX_BASE_HEALTH)
            BaseHealth.Value = Mathf.Clamp(value, 0, MAX_BASE_HEALTH);
            // BindableProperty 会自动通知。如果需要全局事件:
            // this.SendEvent(new Model_BaseHealthChangedEvent(BaseHealth.Value, oldValue, MAX_BASE_HEALTH));
        }

        // 重置数据到初始状态 (例如，用于新游戏或测试)
        public void ResetData()
        {
            CurrentDay.Value = 1;
            BaseHealth.Value = MAX_BASE_HEALTH;
            // 如果需要一个明确的游戏数据重置事件，可以在此发送:
            // this.SendEvent(new Model_GameDataResetEvent());
        }
    }
}
