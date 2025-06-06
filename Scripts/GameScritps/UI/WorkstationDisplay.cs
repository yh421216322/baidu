using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using System; // For Guid
using QFramework;
using YourGameNamespace.Workstations;
using YourGameNamespace.Events; // For Model_WorkstationRegisteredEvent
using System.Linq; // For potential Linq operations
using MyGameNamespace; // For IObjectPoolSystem, assuming it's in MyGameNamespace

namespace YourGameNamespace.UI
{
    public class WorkstationDisplay : MonoBehaviour, IController
    {
        // public GameObject workstationItemPrefab; // Removed
        private readonly string workstationItemPrefabName = "Prefabs/UI/Items/WorkstationItem_PF";
        public Transform workstationListContainer;

        private WorkstationModel mWorkstationModel;
        private ObjectPoolSystem mObjectPoolSystem;
        // private SurvivorModel mSurvivorModel; // Already removed

        public IArchitecture GetArchitecture() => RegisterManager.Interface;

        void Start()
        {
            if (RegisterManager.Interface == null)
            {
                Debug.LogError("WorkstationDisplay: GameArchitecture 尚未初始化。");
                enabled = false;
                return;
            }

            mWorkstationModel = this.GetModel<WorkstationModel>(); // Use IWorkstationModel if interface exists

            if (mWorkstationModel == null)
            {
                Debug.LogError("WorkstationDisplay: 未能获取到 WorkstationModel！");
                enabled = false;
                return;
            }

            if (workstationListContainer == null) Debug.LogError("WorkstationDisplay: workstationListContainer 未在检视面板中分配！");
            // if (workstationItemPrefab == null) Debug.LogError("WorkstationDisplay: workstationItemPrefab 未在检视面板中分配！"); // Removed

            mObjectPoolSystem = this.GetSystem<ObjectPoolSystem>();
            if (mObjectPoolSystem == null)
            {
                Debug.LogError("WorkstationDisplay: 未能获取到 IObjectPoolSystem！列表项将无法通过对象池创建。");
            }

            // Register for events
            this.RegisterEvent<Model_WorkstationRegisteredEvent>(e => RefreshWorkstationList()).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            // TODO: Listen for Model_WorkstationRemovedEvent if workstations can be removed

            RefreshWorkstationList(); // Initial refresh
        }

        void RefreshWorkstationList()
        {
            if (mWorkstationModel == null || workstationListContainer == null) // Removed workstationItemPrefab from check
            {
                Debug.LogError("WorkstationDisplay: 无法刷新工作站列表，缺少依赖项（Model or Container）。");
                return;
            }

            // Clear old items
            foreach (Transform child in workstationListContainer)
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

            List<Workstation> workstations = mWorkstationModel.GetAllWorkstations();

            if (workstations.Count == 0)
            {
                Debug.Log("WorkstationDisplay: 尚未建造任何工作站。");
                return;
            }

            foreach (Workstation station in workstations)
            {
                if (mObjectPoolSystem == null)
                {
                    Debug.LogError("WorkstationDisplay: IObjectPoolSystem is null. Cannot spawn items.");
                    break;
                }

                GameObject itemGO = mObjectPoolSystem.Spawn(workstationItemPrefabName);
                if (itemGO != null)
                {
                    itemGO.transform.SetParent(workstationListContainer, false);
                    itemGO.SetActive(true);
                    WorkstationListItemUI itemUI = itemGO.GetComponent<WorkstationListItemUI>();
                    if (itemUI != null)
                    {
                        itemUI.Setup(station, this);
                    }
                    else
                    {
                        Debug.LogError($"WorkstationDisplay: 预制件 {workstationItemPrefabName} 上缺少 WorkstationListItemUI 脚本。");
                        mObjectPoolSystem.Unspawn(itemGO); // Recycle invalid item
                    }
                }
                else
                {
                    Debug.LogError($"WorkstationDisplay: 从对象池生成 {workstationItemPrefabName} 失败。请检查Resources路径和预制件。");
                }
            }
        }

        public void RequestAssignSurvivorToWorkstation(Guid workstationId)
        {
            // This method would be called by WorkstationListItemUI instances
            // Further UI logic might be needed here to select a survivor
            Debug.Log($"WorkstationDisplay: 请求为工作站 {workstationId} 分配幸存者 (具体选择和命令发送逻辑待实现)。");
            // Example of sending a command:
            // Guid survivorToAssign = ...; // Logic to get survivor ID
            // this.SendCommand(new AssignSurvivorToWorkstationCommand(survivorToAssign, workstationId));
        }

        // Update() method is removed as list updates are now event-driven,
        // and individual item updates are handled by WorkstationListItemUI.
    }
}
