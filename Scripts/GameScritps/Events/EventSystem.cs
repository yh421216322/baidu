using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace YourGameNamespace.Events
{
    public class GEventSystem : AbstractSystem
    {
        private List<System.Func<RandomEvent>> mEventFactories = new List<System.Func<RandomEvent>>();
        private EventModel mEventModel;

        protected override void OnInit()
        {
        
            mEventModel = this.GetModel<EventModel>();
            if (mEventModel == null) Debug.LogError("EventSystem: EventModel not found!");

            // 注册事件工厂
            mEventFactories.Add(() => new FoodSpoilageEvent());
            mEventFactories.Add(() => new ResourceDiscoveryEvent());
            mEventFactories.Add(() => new SurvivorSicknessEvent());
        }

        public void TryTriggerRandomEvent()
        {
            if (mEventModel == null)
            {
                Debug.LogError("EventSystem: EventModel is null, cannot trigger event.");
                return;
            }

            if (UnityEngine.Random.value < 0.25f) // 每次触发有25%的几率
            {
                if (mEventFactories.Count == 0)
                {
                    Debug.LogWarning("EventSystem: No event factories registered.");
                    mEventModel.CurrentEvent = null; // 清除先前的事件
                    return;
                }

                var eventFactory = mEventFactories[UnityEngine.Random.Range(0, mEventFactories.Count)];
                RandomEvent newEvent = eventFactory();
                
                Debug.Log($"EventSystem: Attempting to execute event: {newEvent.Title}");
               
                
                mEventModel.CurrentEvent = newEvent; // 存储以便UI显示
                Debug.Log($"Event Triggered: {newEvent.Title} - {newEvent.Description}");
            }
            else
            {
                if (mEventModel.CurrentEvent != null)
                {
                    Debug.Log("EventSystem: No new event triggered, clearing previous event from display.");
                }
                mEventModel.CurrentEvent = null; // 如果没有新事件触发，则清除先前的事件
            }
        }
    }
}
