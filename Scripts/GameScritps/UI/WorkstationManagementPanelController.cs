// WorkstationManagementPanelController.cs
using UnityEngine;
using UnityEngine.UI; // Ensure this is present for Text, Button
// using TMPro; // Removed
using QFramework;
using YourGameNamespace.Workstations;
using YourGameNamespace.Survivors;
using YourGameNamespace.Commands;
using System;
using System.Collections.Generic; // Required for List

using MyGameNamespace; // Assuming RegisterManager might be here, or it's global

namespace YourGameNamespace.UI
{
    public class WorkstationManagementPanelController : MonoBehaviour, IController // Removed IPoolable
    {
        // UI 引用
        public Text selectedWorkstationNameText; // Changed to Text
        public Transform assignedSurvivorsContainer;  // 已分配幸存者列表容器
        public Transform availableSurvivorsContainer; // 可分配幸存者列表容器
        public GameObject survivorItemPrefab;         // 幸存者列表项预制件 (WorkstationAssignSurvivorItem_PF)
        public Button closeButton;

        private Guid mCurrentWorkstationId;
        private WorkstationModel mWorkstationModel; // Should use concrete type as per recent changes
        private SurvivorModel mSurvivorModel;   // Should use concrete type
        private ObjectPoolSystem mObjectPoolSystem; // Changed to concrete type

        private List<GameObject> mAssignedSurvivorItems = new List<GameObject>();
        private List<GameObject> mAvailableSurvivorItems = new List<GameObject>();

        void Awake()
        {
            // GetSystem calls will use concrete types after GetArchitecture() is fixed
            // mObjectPoolSystem = this.GetSystem<ObjectPoolSystem>(); // Will be set in InitAndShow or similar

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(ClosePanel);
            }
            else
            {
                Debug.LogError("WorkstationManagementPanelController: 关闭按钮未分配!");
            }

            if (survivorItemPrefab == null)
            {
                Debug.LogError("WorkstationManagementPanelController: 幸存者列表项预制件未分配!");
            }
        }

        // Renamed from Setup to InitAndShow for consistency with other pooled panels
        public void InitAndShow(Guid workstationId, WorkstationModel workstationModel, SurvivorModel survivorModel)
        {
            mCurrentWorkstationId = workstationId;
            // Systems/Models should be fetched via GetArchitecture() if not passed directly
            // For this pattern, we assume they are passed in or fetched if this panel is managed by another controller that already has them.
            // If this panel is spawned independently and needs to fetch, it should do so after GetArchitecture() is valid.
            mObjectPoolSystem = this.GetSystem<ObjectPoolSystem>(); // Now uses concrete type

            // These models are passed in, which is fine.
            mWorkstationModel = workstationModel;
            mSurvivorModel = survivorModel;

            if (mWorkstationModel == null || mSurvivorModel == null || mObjectPoolSystem == null) {
                Debug.LogError("WorkstationManagementPanelController: Models or ObjectPoolSystem未能正确初始化!");
                AttemptClosePanel(); // Try to recycle if possible
                return;
            }

            RefreshPanelData();
            gameObject.SetActive(true);
            // TODO: 可以添加打开动画
        }

        private void RefreshPanelData()
        {
            if (mCurrentWorkstationId == Guid.Empty) return;

            Workstation workstation = mWorkstationModel.GetWorkstationById(mCurrentWorkstationId);
            if (workstation == null)
            {
                Debug.LogError($"WorkstationManagementPanelController: 未找到ID为 {mCurrentWorkstationId} 的工作站!");
                AttemptClosePanel();
                return;
            }

            if (selectedWorkstationNameText != null)
            {
                // TODO: 本地化工作站类型名称
                selectedWorkstationNameText.text = $"管理: {workstation.Type} (容量: {workstation.AssignedSurvivorCount.Value}/{workstation.MaxAssignedSurvivors})";
            }

            PopulateSurvivorLists(workstation);
        }

        private void PopulateSurvivorLists(Workstation workstation)
        {
            ClearInstantiatedSurvivorItems(mAssignedSurvivorItems, assignedSurvivorsContainer);
            ClearInstantiatedSurvivorItems(mAvailableSurvivorItems, availableSurvivorsContainer);

            List<Survivor> allSurvivors = mSurvivorModel.GetAllSurvivors();
            bool canAssignMore = workstation.AssignedSurvivorIds.Count < workstation.MaxAssignedSurvivors;

            foreach (Survivor survivor in allSurvivors)
            {
                bool isAssignedToCurrentWorkstation = workstation.AssignedSurvivorIds.Contains(survivor.Id);

                if (isAssignedToCurrentWorkstation)
                {
                    GameObject itemGO = mObjectPoolSystem.Spawn(survivorItemPrefab.name); // Corrected: Spawn with name only
                    if (itemGO == null) { Debug.LogError("Failed to spawn survivorItemPrefab for assigned list"); continue; }
                    itemGO.transform.SetParent(assignedSurvivorsContainer, false); // Set parent after spawn

                    var itemUI = itemGO.GetComponent<WorkstationAssignSurvivorItemUI>();
                    if (itemUI != null)
                    {
                        itemUI.Setup(survivor, mCurrentWorkstationId, true, HandleUnassignSurvivor, this.GetArchitecture());
                    }
                    mAssignedSurvivorItems.Add(itemGO);
                }
                // Only show in available list if survivor is Idle AND there's space
                else if (survivor.Status.Value == SurvivorStatus.Idle && canAssignMore)
                {
                    GameObject itemGO = mObjectPoolSystem.Spawn(survivorItemPrefab.name); // Corrected: Spawn with name only
                     if (itemGO == null) { Debug.LogError("Failed to spawn survivorItemPrefab for available list"); continue; }
                    itemGO.transform.SetParent(availableSurvivorsContainer, false); // Set parent after spawn

                    var itemUI = itemGO.GetComponent<WorkstationAssignSurvivorItemUI>();
                    if (itemUI != null)
                    {
                        itemUI.Setup(survivor, mCurrentWorkstationId, false, HandleAssignSurvivor, this.GetArchitecture());
                    }
                    mAvailableSurvivorItems.Add(itemGO);
                }
            }
        }

        private void ClearInstantiatedSurvivorItems(List<GameObject> itemsList, Transform container) // Renamed for clarity
        {
            if (mObjectPoolSystem == null) return;
            foreach (GameObject item in itemsList)
            {
                if(item != null) mObjectPoolSystem.Unspawn(item); // Changed from Recycle
            }
            itemsList.Clear();
        }


        private void HandleAssignSurvivor(Guid survivorId, Guid workstationId)
        {
            Debug.Log($"[UI操作] 请求分配幸存者 {survivorId} 到工作站 {workstationId}");
            this.SendCommand(new AssignSurvivorToWorkstationCommand(survivorId, workstationId));
            // TODO: Listen to AssignSurvivorToWorkstationResultEvent for more robust UI update
            RefreshPanelData();
        }

        private void HandleUnassignSurvivor(Guid survivorId, Guid workstationId)
        {
            Debug.Log($"[UI操作] 请求从工作站 {workstationId} 解除分配幸存者 {survivorId}");
            this.SendCommand(new UnassignSurvivorFromWorkstationCommand(survivorId, workstationId));
            // TODO: Listen to command result event
            RefreshPanelData();
        }

        private void ClosePanel()
        {
            // TODO: 可以添加关闭动画
            if (mObjectPoolSystem != null) {
                 mObjectPoolSystem.Unspawn(gameObject); // Changed from Recycle
            } else {
                gameObject.SetActive(false); // Fallback
                OnRecycled(); // Manual call if no pool
            }
        }

        private void AttemptClosePanel() // Used when setup fails
        {
            if (mObjectPoolSystem != null && IsRecycled == false) { // Check IsRecycled if available
                 mObjectPoolSystem.Unspawn(gameObject); // Changed from Recycle
            } else if (gameObject.activeSelf) {
                gameObject.SetActive(false);
            }
        }


        public IArchitecture GetArchitecture()
        {
            return RegisterManager.Interface; // Changed to RegisterManager
        }

        // --- IPoolable Implementation ---
        public void OnRecycled()
        {
            // Debug.Log("WorkstationManagementPanelController OnRecycled"); // Chinese Log
            ClearInstantiatedSurvivorItems(mAssignedSurvivorItems, assignedSurvivorsContainer);
            ClearInstantiatedSurvivorItems(mAvailableSurvivorItems, availableSurvivorsContainer);
            // Event listeners should be managed with AddTo(mEventUnregisters) or similar if this panel itself registers global events
            // For button listeners, OnDestroy is one place, or clear them here if necessary.
            // If this panel registers to QFramework global events, they should be unregistered here or via UnRegisterWhenGameObjectDestroyed.
            gameObject.SetActive(false);
        }
        public bool IsRecycled { get; set; }


        void OnDestroy() { // Unity's OnDestroy
            if (closeButton != null) {
                closeButton.onClick.RemoveAllListeners();
            }
            // Ensure items are cleared if panel is destroyed instead of recycled
            // This might double-clear if ClosePanel->Recycle was called, but Recycle should handle it.
            ClearInstantiatedSurvivorItems(mAssignedSurvivorItems, assignedSurvivorsContainer);
            ClearInstantiatedSurvivorItems(mAvailableSurvivorItems, availableSurvivorsContainer);
        }
    }

    // Removed GlobalGameArchitecture static class
}
