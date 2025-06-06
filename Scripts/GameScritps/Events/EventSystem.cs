using QFramework;
using System.Collections.Generic;
using UnityEngine;
// YourGameNamespace.Events 已经通过 namespace YourGameNamespace.Events 声明，不需要额外 using

namespace YourGameNamespace.Events
{
    public interface IGEventSystem : QFramework.QFISystem
    {
        void TryTriggerRandomEvent();
    }

    public class GEventSystem : AbstractSystem, IGEventSystem, IController // Implements IGEventSystem
    {
        // 存储用于创建不同随机事件实例的工厂方法列表
        private List<System.Func<RandomEvent>> mEventFactories = new List<System.Func<RandomEvent>>();
        private EventModel mEventModel; // 事件模型，用于存储当前活动事件

        // IController 接口要求
        public IArchitecture GetArchitecture() => YourGameNamespace.GameArchitecture.Interface; // 假设GameArchitecture是你的架构类

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
                    mEventModel.CurrentEvent.Value = null; // 使用 .Value 赋值
                    return;
                }

                // 从列表中随机选择一个事件工厂
                var eventFactory = mEventFactories[UnityEngine.Random.Range(0, mEventFactories.Count)];
                RandomEvent newEvent = eventFactory(); // 创建新的事件实例
                
                Debug.Log($"事件系统：尝试执行事件：{newEvent.Title}");
               
                newEvent.Execute(this.GetArchitecture()); // 执行事件逻辑，传递架构引用

                mEventModel.CurrentEvent.Value = newEvent; // 使用 .Value 赋值
                Debug.Log($"事件已触发：{newEvent.Title} - {newEvent.Description}");

                // 发送 System_RandomEventExecutedEvent 事件
                this.SendEvent(new System_RandomEventExecutedEvent() { EventData = newEvent });
            }
            else
            {
                // 如果没有新事件触发，则清除模型中可能存在的旧事件
                if (mEventModel.CurrentEvent.Value != null) // 使用 .Value 读取
                {
                    Debug.Log("事件系统：未触发新事件，清除先前显示的事件。");
                }
                mEventModel.CurrentEvent.Value = null; // 使用 .Value 赋值
            }
        }
    }
}
