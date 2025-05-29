using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace YourGameNamespace.Events
{
    public class GEventSystem : AbstractSystem
    {
        // 存储用于创建不同随机事件实例的工厂方法列表
        private List<System.Func<RandomEvent>> mEventFactories = new List<System.Func<RandomEvent>>();
        private EventModel mEventModel; // 事件模型，用于存储当前活动事件

        protected override void OnInit()
        {
        
            mEventModel = this.GetModel<EventModel>();
            if (mEventModel == null) Debug.LogError("事件系统：未找到EventModel！");

            // 注册所有可用的随机事件工厂
            mEventFactories.Add(() => new FoodSpoilageEvent()); // 食物腐败事件
            mEventFactories.Add(() => new ResourceDiscoveryEvent()); // 资源发现事件
            mEventFactories.Add(() => new SurvivorSicknessEvent()); // 幸存者生病事件
        }

        // 尝试触发一个随机事件
        public void TryTriggerRandomEvent()
        {
            if (mEventModel == null)
            {
                Debug.LogError("事件系统：EventModel为空，无法触发事件。");
                return;
            }

            // 假设有25%的几率触发一个事件
            if (UnityEngine.Random.value < 0.25f) 
            {
                if (mEventFactories.Count == 0)
                {
                    Debug.LogWarning("事件系统：没有注册任何事件工厂。");
                    mEventModel.CurrentEvent = null; // 清除先前可能存在的事件
                    return;
                }

                // 从列表中随机选择一个事件工厂
                var eventFactory = mEventFactories[UnityEngine.Random.Range(0, mEventFactories.Count)];
                RandomEvent newEvent = eventFactory(); // 创建新的事件实例
                
                // 注意: newEvent.Execute(this.GetArchitecture()); 应该在这里调用，
                // 或者在事件被设置到模型后由另一个系统处理。
                // 根据现有代码，事件的 Execute 方法似乎应该在事件被选中后立即调用。
                // 为了保持与原始代码相似的逻辑（日志先行），我们先记录尝试执行。
                // 实际的 newEvent.Execute() 会在 RandomEvent 基类或具体事件类中处理模型交互。
                Debug.Log($"事件系统：尝试执行事件：{newEvent.Title}");
               
                newEvent.Execute(this.GetArchitecture()); // 执行事件逻辑
                
                mEventModel.CurrentEvent = newEvent; // 将当前事件存储到模型中，以便UI等其他系统可以访问
                Debug.Log($"事件已触发：{newEvent.Title} - {newEvent.Description}");
            }
            else
            {
                // 如果没有新事件触发，则清除模型中可能存在的旧事件
                if (mEventModel.CurrentEvent != null)
                {
                    Debug.Log("事件系统：未触发新事件，清除先前显示的事件。");
                }
                mEventModel.CurrentEvent = null; 
            }
        }
    }
}
