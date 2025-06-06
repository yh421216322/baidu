using QFramework;
// RandomEvent 类型定义在 YourGameNamespace.Events 命名空间下，此 using 已满足
// using YourGameNamespace.Events;

namespace YourGameNamespace.Events
{
    // 事件模型，用于存储当前活动事件
    public class EventModel : AbstractModel
    {
        // 当前活动的随机事件，使用 BindableProperty 以便UI或其他模块可以监听其变化
        // set 访问器设为 private，外部应通过专门的方法修改（如果需要）或由特定系统（如GEventSystem）直接修改其 .Value
        public BindableProperty<RandomEvent> CurrentEvent { get; private set; }

        // 模型初始化时调用
        protected override void OnInit()
        {
            // 初始化 BindableProperty，初始时没有激活的随机事件
            CurrentEvent = new BindableProperty<RandomEvent>(null);

            // 注意给其他开发者/未来的自己：
            // 由于 CurrentEvent 已更改为 BindableProperty，
            // 之前直接通过 mEventModel.CurrentEvent = newEvent; 或 mEventModel.CurrentEvent = null;
            // 进行赋值的代码（主要在 GEventSystem.cs 中）需要被更新为：
            // mEventModel.CurrentEvent.Value = newEvent;
            // 或
            // mEventModel.CurrentEvent.Value = null;
            // 这个修改将在 GEventSystem.cs 的重构阶段进行。
        }

        // 可选：如果希望提供一个封装的公共方法来设置 CurrentEvent，可以添加如下方法：
        // public void SetCurrentEvent(RandomEvent newEvent)
        // {
        //     CurrentEvent.Value = newEvent;
        // }
        //
        // public void ClearCurrentEvent()
        // {
        //     CurrentEvent.Value = null;
        // }
        // 但根据QFramework的实践，System可以直接修改其管理的Model的BindableProperty的.Value。
    }
}
