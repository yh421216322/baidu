using QFramework;

namespace YourGameNamespace
{
    // 资源管理器系统
    // 注意：当前这个系统的实现非常基础，大部分资源管理的实际逻辑位于 ResourceModel 中。
    // 这个系统目前主要作为一个占位符或者未来扩展功能（例如，处理复杂的资源转换规则、全局资源事件等）的框架。
    public class ResourceManagerSystem : AbstractSystem
    {
        // 系统初始化时调用
        protected override void OnInit()
        {
            // 目前在此处不需要执行任何特定的初始化操作。
            // 任何需要初始化的逻辑，例如加载资源配置或设置初始资源，
            // 更多的是在 ResourceModel 的 OnInit 中处理，或者由 GameInitializer 来设定。
        }
    }
}
