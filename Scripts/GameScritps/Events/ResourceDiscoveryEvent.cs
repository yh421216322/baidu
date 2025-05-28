using QFramework; 
using UnityEngine; 
using System;

namespace YourGameNamespace.Events
{
    public class ResourceDiscoveryEvent : RandomEvent
    {
        public ResourceDiscoveryEvent() 
        { 
            Title = "Resource Cache Found!"; 
        }

        public override void Execute(IArchitecture architecture)
        {
            var resourceModel = architecture.GetModel<ResourceModel>();
            if (resourceModel == null)
            {
                Description = "资源发现事件失败：未找到ResourceModel。";
                Debug.LogError(Description);
                return;
            }

            GameResourceType foundType = (GameResourceType)Enum.GetValues(typeof(GameResourceType)).GetValue(UnityEngine.Random.Range(0, Enum.GetValues(typeof(GameResourceType)).Length));
            int amountFound = UnityEngine.Random.Range(15, 51);
            resourceModel.AddResource(foundType, amountFound);
            Description = $"发现了一个隐藏的贮藏点！找到 {amountFound} {foundType}。";
            Debug.Log(Description);
        }
    }
}
