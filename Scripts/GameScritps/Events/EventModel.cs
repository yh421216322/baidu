using QFramework;

namespace YourGameNamespace.Events
{
    // 事件模型，用于存储当前活动事件
    public class EventModel : AbstractModel
    {
        // 当前活动的随机事件
        public RandomEvent CurrentEvent { get; set; }


        protected override void OnInit()
        {
            // 初始化时不需要执行特殊操作
            // CurrentEvent 将由 EventSystem 设置
        }
    }
}
