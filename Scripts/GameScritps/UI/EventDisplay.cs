using MyGameNamespace;
using UnityEngine;
using UnityEngine.UI; // 用于 Text UI 组件
using QFramework;     // QFramework 框架
using YourGameNamespace.Events; // 用于 EventModel 和 RandomEvent

namespace YourGameNamespace.UI
{
    // UI组件，用于显示当前发生的随机事件信息
    public class EventDisplay : MonoBehaviour, IController
    {
        public Text eventText; // 在Unity检视面板中分配此Text组件，用于显示事件内容
        private EventModel mEventModel; // 对事件数据模型的引用
        // private float mDisplayTimer; // Removed
        // public float mDisplayDuration = 5f; // Removed

        public IArchitecture GetArchitecture() => RegisterManager.Interface;

        private void Awake()
        {
            eventText = eventText ?? transform.Find("EventText")?.GetComponent<Text>();
            if (eventText == null) Debug.LogError("EventDisplay: UI元素 'EventText' 未能成功获取或链接。请检查Hierarchy中的命名和组件。");
        }

        void Start() // Unity生命周期方法，在第一次Update前执行
        {
            // 检查 GameArchitecture 是否已初始化
            if (RegisterManager.Interface == null)
            {
                Debug.LogError("事件显示 (EventDisplay)：GameArchitecture 尚未初始化！此UI组件可能无法正常工作。请确保 GameInitializer 先运行。");
                this.enabled = false; // 禁用此组件以防止错误
                if (eventText != null) eventText.text = "错误：事件系统未初始化";
                return;
            }
            mEventModel = this.GetModel<EventModel>(); // 获取事件数据模型实例

            if (eventText == null) // 检查UI Text组件是否已分配
            {
                Debug.LogError("事件显示 (EventDisplay)：UI Text组件 (eventText) 未在Unity检视面板中分配！");
                this.enabled = false; // 禁用此组件
                return;
            }

            if (mEventModel != null)
            {
                mEventModel.CurrentEvent.RegisterWithInitValue(OnCurrentEventChanged).UnRegisterWhenGameObjectDestroyed(this.gameObject); // 使用 this.gameObject
            }
            else
            {
                Debug.LogError("EventDisplay: 未能获取到 EventModel！UI可能不会更新。");
                if (eventText != null) eventText.text = "错误：事件系统未初始化";
            }
        }

        private void OnCurrentEventChanged(RandomEvent currentEvent)
        {
            if (eventText == null) return;

            if (currentEvent != null)
            {
                // 假设 RandomEvent 有 Title 和 Description 属性，并且它们已经是本地化后的中文
                eventText.text = $"事件：{currentEvent.Title}\n{currentEvent.Description}";
            }
            else
            {
                eventText.text = "当前无事件"; // 本地化
            }
        }

        // Update() 方法已移除，UI更新由 BindableProperty 回调驱动
    }
}
