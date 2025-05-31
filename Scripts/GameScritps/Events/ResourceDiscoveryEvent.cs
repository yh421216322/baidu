using QFramework; 
using UnityEngine; 
using System;

namespace YourGameNamespace.Events
{
    // 资源发现随机事件
    public class ResourceDiscoveryEvent : RandomEvent
    {
        public ResourceDiscoveryEvent() 
        { 
            Title = "发现资源贮藏点！"; // 事件标题
        }

        public override void Execute(IArchitecture architecture)
        {
            var resourceModel = architecture.GetModel<ResourceModel>();
            if (resourceModel == null)
            {
                Description = "资源发现事件执行失败：未找到ResourceModel。"; // 事件描述：失败情况
                Debug.LogError(Description);
                return;
            }

            // 从所有可用的GameResourceType中随机选择一种类型
            GameResourceType foundType = (GameResourceType)Enum.GetValues(typeof(GameResourceType)).GetValue(UnityEngine.Random.Range(0, Enum.GetValues(typeof(GameResourceType)).Length));
            // 随机发现15到50个单位的资源
            int amountFound = UnityEngine.Random.Range(15, 51);
            resourceModel.AddResource(foundType, amountFound); // 将发现的资源添加到模型中
            Description = $"发现了一个隐藏的贮藏点！找到了 {amountFound} 单位的 {foundType}。"; // 事件描述：成功发现资源
            Debug.Log(Description);
        }
    }
}
