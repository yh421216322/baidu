using QFramework;

namespace YourGameNamespace.Events
{
    // 随机事件的抽象基类
    public abstract class RandomEvent
    {
        // 事件的标题 (通常用于UI显示)
        public string Title { get; protected set; }
        // 事件的描述 (通常用于UI显示，在Execute方法中设置)
        public string Description { get; protected set; }

        // 执行事件效果的抽象方法
        // 派生类将实现此方法以应用特定的游戏逻辑更改
        public abstract void Execute(IArchitecture architecture);
    }
}
