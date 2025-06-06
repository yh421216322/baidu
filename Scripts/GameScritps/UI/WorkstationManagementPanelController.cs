// WorkstationManagementPanelController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using QFramework;
using YourGameNamespace.Workstations;
using YourGameNamespace.Survivors;
using YourGameNamespace.Commands;
using System;
using System.Collections.Generic; // Required for List

namespace YourGameNamespace.UI
{
    public class WorkstationManagementPanelController : MonoBehaviour, IController
    {
        // UI 引用
        public TextMeshProUGUI selectedWorkstationNameText;
        public Transform assignedSurvivorsContainer;  // 已分配幸存者列表容器
        public Transform availableSurvivorsContainer; // 可分配幸存者列表容器
        public GameObject survivorItemPrefab;         // 幸存者列表项预制件 (WorkstationAssignSurvivorItem_PF)
        public Button closeButton;

        private Guid mCurrentWorkstationId;
        private WorkstationModel mWorkstationModel;
        private SurvivorModel mSurvivorModel;
        private IObjectPoolSystem mObjectPoolSystem;

        private List<GameObject> mAssignedSurvivorItems = new List<GameObject>();
        private List<GameObject> mAvailableSurvivorItems = new List<GameObject>();

        void Awake()
        {
            // QFramework 获取系统
            mObjectPoolSystem = this.GetSystem<IObjectPoolSystem>();

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

        public void Setup(Guid workstationId, WorkstationModel workstationModel, SurvivorModel survivorModel)
        {
            mCurrentWorkstationId = workstationId;
            mWorkstationModel = workstationModel; // 从外部传入，确保获取的是最新的
            mSurvivorModel = survivorModel;     // 从外部传入

            if (mWorkstationModel == null || mSurvivorModel == null) {
                Debug.LogError("WorkstationManagementPanelController: WorkstationModel 或 SurvivorModel 未能正确初始化!");
                ClosePanel();
                return;
            }

            RefreshPanelData();
            gameObject.SetActive(true); // 确保面板可见
            // TODO: 可以添加打开动画
        }

        private void RefreshPanelData()
        {
            if (mCurrentWorkstationId == Guid.Empty) return;

            Workstation workstation = mWorkstationModel.GetWorkstationById(mCurrentWorkstationId);
            if (workstation == null)
            {
                Debug.LogError($"WorkstationManagementPanelController: 未找到ID为 {mCurrentWorkstationId} 的工作站!");
                ClosePanel();
                return;
            }

            if (selectedWorkstationNameText != null)
            {
                selectedWorkstationNameText.text = $"管理工作站: {workstation.Type.ToString()} (ID: {workstation.Id.ToString().Substring(0, 4)})";
            }

            PopulateSurvivorLists(workstation);
        }

        private void PopulateSurvivorLists(Workstation workstation)
        {
            // 清理旧列表项
            ClearSurvivorItems(mAssignedSurvivorItems, assignedSurvivorsContainer);
            ClearSurvivorItems(mAvailableSurvivorItems, availableSurvivorsContainer);

            mAssignedSurvivorItems.Clear();
            mAvailableSurvivorItems.Clear();

            List<Survivor> allSurvivors = mSurvivorModel.GetAllSurvivors();

            foreach (Survivor survivor in allSurvivors)
            {
                bool isAssignedToCurrentWorkstation = workstation.AssignedSurvivorIds.Contains(survivor.Id);

                if (isAssignedToCurrentWorkstation)
                {
                    // 实例化并设置已分配的幸存者项
                    GameObject itemGO = mObjectPoolSystem.Instantiate(survivorItemPrefab, assignedSurvivorsContainer);
                    if (itemGO == null) continue;

                    var itemUI = itemGO.GetComponent<WorkstationAssignSurvivorItemUI>();
                    if (itemUI != null)
                    {
                        itemUI.Setup(survivor, mCurrentWorkstationId, true, HandleUnassignSurvivor, this.GetArchitecture());
                    }
                    mAssignedSurvivorItems.Add(itemGO);
                }
                else if (survivor.Status.Value == SurvivorStatus.Idle) // 仅显示空闲的幸存者作为可分配
                {
                    // 实例化并设置可分配的幸存者项
                    GameObject itemGO = mObjectPoolSystem.Instantiate(survivorItemPrefab, availableSurvivorsContainer);
                    if (itemGO == null) continue;

                    var itemUI = itemGO.GetComponent<WorkstationAssignSurvivorItemUI>();
                    if (itemUI != null)
                    {
                        itemUI.Setup(survivor, mCurrentWorkstationId, false, HandleAssignSurvivor, this.GetArchitecture());
                    }
                    mAvailableSurvivorItems.Add(itemGO);
                }
            }
        }

        private void ClearSurvivorItems(List<GameObject> items, Transform container)
        {
            foreach (GameObject item in items)
            {
                mObjectPoolSystem.Recycle(item);
            }
            items.Clear();
            // 如果对象池系统不负责移除子对象，则需要手动移除
            // foreach (Transform child in container)
            // {
            //     Destroy(child.gameObject); // 或者使用对象池回收
            // }
        }


        private void HandleAssignSurvivor(Guid survivorId, Guid workstationId)
        {
            Debug.Log($"[UI操作] 请求分配幸存者 {survivorId} 到工作站 {workstationId}");
            this.SendCommand(new AssignSurvivorToWorkstationCommand(survivorId, workstationId));
            // 理想情况下，命令执行后会有事件通知，或者我们在这里延迟刷新
            // 为简单起见，直接刷新或依赖事件
            // TODO: 考虑监听 AssignSurvivorToWorkstationCommandCompletedEvent (如果存在)
            RefreshPanelData();
        }

        private void HandleUnassignSurvivor(Guid survivorId, Guid workstationId)
        {
            Debug.Log($"[UI操作] 请求从工作站 {workstationId} 解除分配幸存者 {survivorId}");
            this.SendCommand(new UnassignSurvivorFromWorkstationCommand(survivorId, workstationId));
            // TODO: 考虑监听 UnassignSurvivorFromWorkstationCommandCompletedEvent (如果存在)
            RefreshPanelData();
        }

        private void ClosePanel()
        {
            // TODO: 可以添加关闭动画
            gameObject.SetActive(false); // 先隐藏
            mObjectPoolSystem.Recycle(gameObject); // 然后回收
            // 或者通知 WorkstationDisplay 来处理回收
            // this.SendEvent<WorkstationManagementPanelClosedEvent>(new WorkstationManagementPanelClosedEvent());
        }

        public IArchitecture GetArchitecture()
        {
            return GlobalGameArchitecture.Interface; // 假设有一个全局访问点
        }

        void OnDestroy() {
            if (closeButton != null) {
                closeButton.onClick.RemoveListener(ClosePanel);
            }
            // 清理列表项，以防对象池未完全处理
            ClearSurvivorItems(mAssignedSurvivorItems, assignedSurvivorsContainer);
            ClearSurvivorItems(mAvailableSurvivorItems, availableSurvivorsContainer);
        }
    }

    // 假设的全局架构访问点，需要项目中实际存在
    public static class GlobalGameArchitecture
    {
        public static GameArchitecture Interface {
            get {
                // 这个实现取决于你的项目如何设置 GameArchitecture 的单例或服务定位器
                // 例如，如果 GameInitializer 创建并持有一个静态实例:
                // return GameInitializer.Instance?.GameArch;
                // 或者，如果 GameArchitecture 自身是单例:
                // return GameArchitecture.Instance;
                // 此处仅为示例，需要替换为项目中实际的访问方式
                if (_interface == null) _interface = new GameArchitecture(); // 极简示例
                return _interface;
            }
            set => _interface = value; // 允许外部设置，例如在 GameInitializer 中
        }
        private static GameArchitecture _interface;
    }
}
