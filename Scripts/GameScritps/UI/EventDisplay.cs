using UnityEngine;
using UnityEngine.UI; // 用于 Text UI 组件
using QFramework;     // QFramework 框架
using YourGameNamespace.Events; // 用于 EventModel 和 RandomEvent

namespace YourGameNamespace.UI
{
    // UI组件，用于显示当前发生的随机事件信息
    public class EventDisplay : MonoBehaviour
    {
        public Text eventText; // 在Unity检视面板中分配此Text组件，用于显示事件内容
        private EventModel mEventModel; // 对事件数据模型的引用
        private float mDisplayDuration = 10f; // 事件消息在UI上显示的持续时间（秒）
        private float mTimeEventDisplayed = 0f; // 当前事件消息已显示的时间

        void Start() // Unity生命周期方法，在第一次Update前执行
        {
            // 检查 GameArchitecture 是否已初始化
            if (GameArchitecture.Interface == null)
            {
                Debug.LogError("事件显示 (EventDisplay)：GameArchitecture 尚未初始化！此UI组件可能无法正常工作。请确保 GameInitializer 先运行。");
                this.enabled = false; // 禁用此组件以防止错误
                return;
            }
            mEventModel = GameArchitecture.Interface.GetModel<EventModel>(); // 获取事件数据模型实例
            if (mEventModel == null) Debug.LogError("事件显示 (EventDisplay)：未能获取事件数据模型 (EventModel)！");
            
            if (eventText == null) // 检查UI Text组件是否已分配
            {
                Debug.LogError("事件显示 (EventDisplay)：UI Text组件 (eventText) 未在Unity检视面板中分配！");
                this.enabled = false; // 禁用此组件
                return;
            }
            eventText.text = "当前无活动事件。"; // 设置初始显示的文本
        }

        void Update() // Unity生命周期方法，每帧调用一次
        {
            // 安全检查，如果模型或UI Text组件不存在，则不执行更新逻辑
            if (mEventModel == null || eventText == null) return;

            if (mEventModel.CurrentEvent != null) // 如果当前有活动事件
            {
                // 显示新事件或当前仍在活动的事件信息
                // 注意：事件的 Title 和 Description 属性应已在各自的事件类中被翻译为中文
                eventText.text = $"事件：{mEventModel.CurrentEvent.Title}\n{mEventModel.CurrentEvent.Description}";
                mTimeEventDisplayed = 0f; // 为新的或当前事件重置已显示时间计时器
                
                // 关于事件显示逻辑的说明：
                // 当前的 EventSystem 逻辑似乎是在每次尝试触发事件时，如果未触发新事件，则会将 mEventModel.CurrentEvent 设置为 null。
                // 这意味着事件消息可能只显示很短时间（直到下一次 EventSystem.TryTriggerRandomEvent 调用）。
                // 
                // 如果希望事件消息在UI上持续显示 mDisplayDuration 所设定的时长，即使 EventSystem 已将 mEventModel.CurrentEvent 清空，
                // 则需要修改此处的逻辑：例如，可以将事件信息暂存到此脚本的一个局部变量中，
                // 然后基于 mTimeEventDisplayed 和 mDisplayDuration 来控制该暂存消息的显示时长。
                // 
                // 当前代码的行为是：只要 mEventModel.CurrentEvent 不为null，就显示该事件。
                // 一旦 EventSystem 将 mEventModel.CurrentEvent 置为null（通常在下一次TryTriggerRandomEvent且无新事件时），此UI也会相应地清除事件显示。
            }
            else // 如果当前没有活动事件 (mEventModel.CurrentEvent 为 null)
            {
                // 此逻辑块处理当 mEventModel.CurrentEvent 为空时，如何清除或保持上一条事件消息的显示。
                // 如果我们希望消息在UI上停留 mDisplayDuration 时长：
                // 检查 eventText 当前是否仍在显示上一条事件消息（即文本不为空且不是“当前无活动事件。”）
                if (!string.IsNullOrEmpty(eventText.text) && !eventText.text.StartsWith("当前无活动事件。"))
                {
                    mTimeEventDisplayed += UnityEngine.Time.deltaTime; // 累加显示时间
                    if (mTimeEventDisplayed >= mDisplayDuration) // 如果已达到设定的显示时长
                    {
                        eventText.text = "当前无活动事件。"; // 清除事件消息，显示默认文本
                        mTimeEventDisplayed = 0f; // 重置计时器
                    }
                }
                // 如果 eventText 为空，或者 eventText 当前显示的是 "事件：" 开头的消息（意味着上一帧还在显示事件，但本帧 CurrentEvent 变为null）
                // 则立即清除，显示默认文本。这对应了 EventSystem 将 CurrentEvent 置空的场景。
                else if (string.IsNullOrEmpty(eventText.text) || eventText.text.StartsWith("事件：")) 
                {
                     eventText.text = "当前无活动事件。"; // 如果EventSystem清除了CurrentEvent，则立即更新UI
                }
            }
        }
    }
}
