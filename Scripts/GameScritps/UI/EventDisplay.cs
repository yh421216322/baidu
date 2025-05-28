using UnityEngine;
using UnityEngine.UI;
using QFramework;
using YourGameNamespace.Events;

namespace YourGameNamespace.UI
{
    public class EventDisplay : MonoBehaviour
    {
        public Text eventText; // 在Unity检视面板中分配
        private EventModel mEventModel;
        private float mDisplayDuration = 10f; // 事件消息显示时长
        private float mTimeEventDisplayed = 0f;

        void Start()
        {
            if (GameArchitecture.Interface == null)
            {
                Debug.LogError("事件显示：GameArchitecture 未初始化！");
                this.enabled = false;
                return;
            }
            mEventModel = GameArchitecture.Interface.GetModel<EventModel>();
            if (mEventModel == null) Debug.LogError("事件显示：未找到 EventModel！");
            if (eventText == null)
            {
                Debug.LogError("事件显示：eventText 未在检视面板中分配！");
                this.enabled = false;
                return;
            }
            eventText.text = "No active events."; // 初始文本
        }

        void Update()
        {
            if (mEventModel == null || eventText == null) return;

            if (mEventModel.CurrentEvent != null)
            {
                // 新事件或事件仍处于活动状态
                eventText.text = $"Event: {mEventModel.CurrentEvent.Title}\n{mEventModel.CurrentEvent.Description}";
                mTimeEventDisplayed = 0f; // 为新/当前事件重置计时器
                
                // 显示一次后，在模型中将CurrentEvent设置为空，
                // 这样除非设置了新事件，否则不会重新显示。
                // EventSystem的逻辑意味着一个事件在一个“触发”周期内是活动的。
                // 对于随时间持续显示，此逻辑可能需要根据CurrentEvent的管理方式进行调整。
                // 提示中的EventSystem会在没有新事件触发时清除CurrentEvent。
                // 这意味着如果没有新事件，事件消息将显示，然后在下一次EventSystem.TryTriggerRandomEvent()时更改为“No active events”。
                // 要使其持续mDisplayDuration时长：
                // 如果我们希望在此处进行定时显示，需要确保mEventModel.CurrentEvent不会立即被EventSystem置空。
                // 目前，只要它在mEventModel.CurrentEvent中，就会显示该事件。
                // 如果EventSystem清除了它，此UI也将清除。
            }
            else
            {
                // CurrentEvent为空，表示在EventSystem的上次检查中没有触发新事件
                // 或者显示的事件已根据EventSystem的逻辑“过期”。
                // 如果我们希望消息停留mDisplayDuration时长：
                if (!string.IsNullOrEmpty(eventText.text) && !eventText.text.StartsWith("No active events."))
                {
                    mTimeEventDisplayed += UnityEngine.Time.deltaTime;
                    if (mTimeEventDisplayed >= mDisplayDuration)
                    {
                        eventText.text = "No active events.";
                        mTimeEventDisplayed = 0f; // 重置计时器
                    }
                }
                else if (string.IsNullOrEmpty(eventText.text) || eventText.text.StartsWith("Event:")) // 如果之前显示事件且现在CurrentEvent为空
                {
                     eventText.text = "No active events."; // 如果EventSystem将其置空，则立即清除
                }
            }
        }
    }
}
