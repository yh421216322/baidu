using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using QFramework;
using YourGameNamespace.Survivors;
using YourGameNamespace.Events; // Added for Model_SurvivorAddedEvent
using System.Linq; // Added for potential Linq operations, though not strictly needed for current GetAllSurvivors

namespace YourGameNamespace.UI
{
    public class SurvivorDisplay : MonoBehaviour, IController
    {
        // public Text survivorListText; // Removed
        public GameObject survivorItemPrefab;    // Prefab for individual survivor item UI
        public Transform survivorListContainer; // Container to hold survivor item instances

        private SurvivorModel mSurvivorModel;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void Start()
        {
            if (GameArchitecture.Interface == null)
            {
                Debug.LogError("SurvivorDisplay: GameArchitecture 尚未初始化。");
                enabled = false;
                return;
            }

            mSurvivorModel = this.GetModel<SurvivorModel>();
            if (mSurvivorModel == null)
            {
                Debug.LogError("SurvivorDisplay: 未能获取到 SurvivorModel！");
                enabled = false;
                return;
            }

            if (survivorListContainer == null) Debug.LogError("SurvivorDisplay: survivorListContainer 未在检视面板中分配！");
            if (survivorItemPrefab == null) Debug.LogError("SurvivorDisplay: survivorItemPrefab 未在检视面板中分配！");

            // Register for events
            this.RegisterEvent<Model_SurvivorAddedEvent>(e => RefreshSurvivorList()).UnRegisterWhenGameObjectDestroyed(this);
            // Potentially listen to Model_SurvivorRemovedEvent, Model_SurvivorDataUpdatedEvent in the future

            RefreshSurvivorList(); // Initial refresh
        }

        void RefreshSurvivorList()
        {
            if (mSurvivorModel == null || survivorListContainer == null || survivorItemPrefab == null)
            {
                Debug.LogError("SurvivorDisplay:无法刷新幸存者列表，缺少依赖项（Model, Container, or Prefab）。");
                return;
            }

            // Clear old items
            foreach (Transform child in survivorListContainer)
            {
                // If using an object pool, Unspawn here. For now, Destroy.
                Destroy(child.gameObject);
            }

            List<Survivor> survivors = mSurvivorModel.GetAllSurvivors();

            if (survivors.Count == 0)
            {
                // Optionally, display a "No survivors" message, perhaps by enabling/disabling a dedicated Text object.
                // For now, an empty container means no survivors.
                Debug.Log("SurvivorDisplay: 目前还没有幸存者。");
                return;
            }

            foreach (Survivor survivor in survivors)
            {
                GameObject itemGO = Instantiate(survivorItemPrefab, survivorListContainer);
                SurvivorListItemUI itemUI = itemGO.GetComponent<SurvivorListItemUI>();
                if (itemUI != null)
                {
                    itemUI.Setup(survivor); // SurvivorListItemUI will handle its own BindableProperty subscriptions
                }
                else
                {
                    Debug.LogError($"SurvivorDisplay: survivorItemPrefab '{survivorItemPrefab.name}' 上缺少 SurvivorListItemUI 脚本组件。");
                    Destroy(itemGO); // Clean up instantiated item if script is missing
                }
            }
        }

        // Update() method is removed as list updates are now event-driven,
        // and individual item updates are handled by SurvivorListItemUI.
    }
}
