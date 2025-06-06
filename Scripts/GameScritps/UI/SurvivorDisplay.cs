using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using QFramework;
using YourGameNamespace.Survivors;
using YourGameNamespace.Events; // Added for Model_SurvivorAddedEvent
using System.Linq; // Added for potential Linq operations, though not strictly needed for current GetAllSurvivors

namespace YourGameNamespace.UI
{
using MyGameNamespace; // For IObjectPoolSystem, assuming it's in MyGameNamespace based on ObjectPoolSystem.cs

namespace YourGameNamespace.UI
{
    public class SurvivorDisplay : MonoBehaviour, IController
    {
        // public GameObject survivorItemPrefab; // Removed
        private readonly string survivorItemPrefabName = "Prefabs/UI/Items/SurvivorItem_PF";
        public Transform survivorListContainer;

        private SurvivorModel mSurvivorModel;
        private IObjectPoolSystem mObjectPoolSystem;

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
            // if (survivorItemPrefab == null) Debug.LogError("SurvivorDisplay: survivorItemPrefab 未在检视面板中分配！"); // Removed check for public field

            mObjectPoolSystem = this.GetSystem<IObjectPoolSystem>();
            if (mObjectPoolSystem == null)
            {
                Debug.LogError("SurvivorDisplay: 未能获取到 IObjectPoolSystem！列表项将无法通过对象池创建。");
            }

            // Register for events
            this.RegisterEvent<Model_SurvivorAddedEvent>(e => RefreshSurvivorList()).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            // Potentially listen to Model_SurvivorRemovedEvent, Model_SurvivorDataUpdatedEvent in the future

            RefreshSurvivorList(); // Initial refresh
        }

        void RefreshSurvivorList()
        {
            if (mSurvivorModel == null || survivorListContainer == null ) // Removed survivorItemPrefab from check
            {
                Debug.LogError("SurvivorDisplay:无法刷新幸存者列表，缺少依赖项（Model or Container）。");
                return;
            }

            // Clear old items
            foreach (Transform child in survivorListContainer)
            {
                if (mObjectPoolSystem != null)
                {
                    mObjectPoolSystem.Unspawn(child.gameObject);
                }
                else
                {
                    Destroy(child.gameObject); // Fallback if pool is missing
                }
            }

            List<Survivor> survivors = mSurvivorModel.GetAllSurvivors();

            if (survivors.Count == 0)
            {
                Debug.Log("SurvivorDisplay: 目前还没有幸存者。");
                return;
            }

            foreach (Survivor survivor in survivors)
            {
                if (mObjectPoolSystem == null)
                {
                    Debug.LogError("SurvivorDisplay: IObjectPoolSystem is null. Cannot spawn items.");
                    break;
                }

                GameObject itemGO = mObjectPoolSystem.Spawn(survivorItemPrefabName);
                if (itemGO != null)
                {
                    itemGO.transform.SetParent(survivorListContainer, false);
                    itemGO.SetActive(true);
                    SurvivorListItemUI itemUI = itemGO.GetComponent<SurvivorListItemUI>();
                    if (itemUI != null)
                    {
                        itemUI.Setup(survivor);
                    }
                    else
                    {
                        Debug.LogError($"SurvivorDisplay: 预制件 {survivorItemPrefabName} 上缺少 SurvivorListItemUI 脚本。");
                        mObjectPoolSystem.Unspawn(itemGO); // Recycle invalid item
                    }
                }
                else
                {
                    Debug.LogError($"SurvivorDisplay: 从对象池生成 {survivorItemPrefabName} 失败。请检查Resources路径和预制件。");
                }
            }
        }

        // Update() method is removed as list updates are now event-driven,
        // and individual item updates are handled by SurvivorListItemUI.
    }
}
