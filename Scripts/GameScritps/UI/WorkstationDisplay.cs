using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using System; // For Guid
using QFramework;
using YourGameNamespace.Workstations;
using YourGameNamespace.Events; // For Model_WorkstationRegisteredEvent
using System.Linq; // For potential Linq operations

namespace YourGameNamespace.UI
{
    public class WorkstationDisplay : MonoBehaviour, IController
    {
        public GameObject workstationItemPrefab;    // Prefab for individual workstation item UI
        public Transform workstationListContainer; // Container to hold workstation item instances

        private WorkstationModel mWorkstationModel; // Use IWorkstationModel if interface exists and is registered
        // private SurvivorModel mSurvivorModel; // Removed, child items will handle survivor details if needed

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void Start()
        {
            if (GameArchitecture.Interface == null)
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
            if (workstationItemPrefab == null) Debug.LogError("WorkstationDisplay: workstationItemPrefab 未在检视面板中分配！");

            // Register for events
            this.RegisterEvent<Model_WorkstationRegisteredEvent>(e => RefreshWorkstationList()).UnRegisterWhenGameObjectDestroyed(this);
            // TODO: Listen for Model_WorkstationRemovedEvent if workstations can be removed

            RefreshWorkstationList(); // Initial refresh
        }

        void RefreshWorkstationList()
        {
            if (mWorkstationModel == null || workstationListContainer == null || workstationItemPrefab == null)
            {
                Debug.LogError("WorkstationDisplay: 无法刷新工作站列表，缺少依赖项（Model, Container, or Prefab）。");
                return;
            }

            // Clear old items
            foreach (Transform child in workstationListContainer)
            {
                // If using an object pool, Unspawn here. For now, Destroy.
                Destroy(child.gameObject);
            }

            List<Workstation> workstations = mWorkstationModel.GetAllWorkstations();

            if (workstations.Count == 0)
            {
                Debug.Log("WorkstationDisplay: 尚未建造任何工作站。");
                return;
            }

            foreach (Workstation station in workstations)
            {
                GameObject itemGO = Instantiate(workstationItemPrefab, workstationListContainer);
                WorkstationListItemUI itemUI = itemGO.GetComponent<WorkstationListItemUI>();
                if (itemUI != null)
                {
                    itemUI.Setup(station, this); // WorkstationListItemUI will handle its own BindableProperty subscriptions
                }
                else
                {
                    Debug.LogError($"WorkstationDisplay: workstationItemPrefab '{workstationItemPrefab.name}' 上缺少 WorkstationListItemUI 脚本组件。");
                    Destroy(itemGO); // Clean up instantiated item if script is missing
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
