using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using System; // For Guid
using QFramework;
using YourGameNamespace.Workstations;
using YourGameNamespace.Survivors; // 需要 SurvivorModel
using YourGameNamespace.Events; // For Model_WorkstationRegisteredEvent
using System.Linq; // For potential Linq operations
using MyGameNamespace; // For IObjectPoolSystem, assuming it's in MyGameNamespace

namespace YourGameNamespace.UI
{
    public class WorkstationDisplay : MonoBehaviour, IController
    {
        private readonly string workstationItemPrefabName = "Prefabs/UI/Items/WorkstationItem_PF";
        public Transform workstationListContainer;
        // 为管理面板添加预制件路径字段，确保在Inspector中赋值或此处提供有效默认值
        public string workstationManagementPanelPrefabPath = "Prefabs/UI/WorkstationManagementPanel_PF";


        private WorkstationModel mWorkstationModel;
        private SurvivorModel mSurvivorModel; // 添加 SurvivorModel 引用
        private ObjectPoolSystem mObjectPoolSystem;
        private GameObject mCurrentManagementPanel; // 当前打开的管理面板实例

        public IArchitecture GetArchitecture() => RegisterManager.Interface;

        void Start()
        {
            if (RegisterManager.Interface == null)
            {
                Debug.LogError("WorkstationDisplay: GameArchitecture 尚未初始化。");
                enabled = false;
                return;
            }

            mWorkstationModel = this.GetModel<WorkstationModel>();
            mSurvivorModel = this.GetModel<SurvivorModel>(); // 获取 SurvivorModel

            if (mWorkstationModel == null)
            {
                Debug.LogError("WorkstationDisplay: 未能获取到 WorkstationModel！");
                enabled = false;
                return;
            }
            if (mSurvivorModel == null)
            {
                Debug.LogError("WorkstationDisplay: 未能获取到 SurvivorModel！管理面板可能无法正确显示幸存者。");
                // 根据游戏设计，这可能是也可能不是一个使整个脚本失效的错误
            }


            if (workstationListContainer == null) Debug.LogError("WorkstationDisplay: workstationListContainer 未在检视面板中分配！");

            mObjectPoolSystem = this.GetSystem<ObjectPoolSystem>();
            if (mObjectPoolSystem == null)
            {
                Debug.LogError("WorkstationDisplay: 未能获取到 IObjectPoolSystem！列表项和管理面板将无法通过对象池创建。");
            }
             if (string.IsNullOrEmpty(workstationManagementPanelPrefabPath))
            {
                Debug.LogError("WorkstationDisplay: 工作站管理面板预制件路径未设置!");
            }


            // Register for events
            this.RegisterEvent<Model_WorkstationRegisteredEvent>(e => RefreshWorkstationList()).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            // 监听工作站分配变化事件，该事件应在 AssignSurvivorToWorkstationCommand 和 UnassignSurvivorFromWorkstationCommand 成功后发送
            // 如果没有特定的 WorkstationAssignmentsChangedEvent，可以考虑监听一个更通用的数据更新事件，
            // 或者在管理面板关闭时强制刷新。为简化，这里假设存在一个这样的事件。
            // 你可能需要创建 YourGameNamespace.Commands.WorkstationAssignmentsChangedEvent
            // this.RegisterEvent<WorkstationAssignmentsChangedEvent>(e => RefreshWorkstationListForSpecific(e.WorkstationId)).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            // 简单的刷新方式：
            this.RegisterEvent<YourGameNamespace.Commands.AssignSurvivorToWorkstationCommand.CompletedEvent>(e => RefreshWorkstationList());
            this.RegisterEvent<YourGameNamespace.Commands.UnassignSurvivorFromWorkstationCommand.CompletedEvent>(e => RefreshWorkstationList());


            RefreshWorkstationList(); // Initial refresh
        }

        // 可选：如果只想刷新特定工作站的显示而不是整个列表
        // void RefreshWorkstationListForSpecific(Guid workstationId)
        // {
        //     foreach(Transform child in workstationListContainer)
        //     {
        //         var itemUI = child.GetComponent<WorkstationListItemUI>();
        //         if (itemUI != null && itemUI.WorkstationId == workstationId)
        //         {
        //             Workstation station = mWorkstationModel.GetWorkstationById(workstationId);
        //             if (station != null) itemUI.Setup(station, this); // 假设Setup可以被这样调用来刷新
        //             break;
        //         }
        //     }
        // }


        void RefreshWorkstationList()
        {
            if (mWorkstationModel == null || workstationListContainer == null)
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
            if (mObjectPoolSystem == null) {
                Debug.LogError("WorkstationDisplay: ObjectPoolSystem 未初始化，无法打开管理面板。");
                return;
            }
            if (mSurvivorModel == null) {
                 Debug.LogError("WorkstationDisplay: SurvivorModel 未初始化，无法打开管理面板。");
                return;
            }


            if (mCurrentManagementPanel != null && mCurrentManagementPanel.activeSelf)
            {
                Debug.LogWarning("WorkstationDisplay: 管理面板已打开。请先关闭当前面板。");
                // 可选：将现有面板带到最前或重新 Setup
                // var existingController = mCurrentManagementPanel.GetComponent<WorkstationManagementPanelController>();
                // if (existingController != null) existingController.Setup(workstationId, mWorkstationModel, mSurvivorModel);
                return;
            }

            if (string.IsNullOrEmpty(workstationManagementPanelPrefabPath))
            {
                Debug.LogError("WorkstationDisplay: 工作站管理面板预制件路径为空，无法打开面板。");
                return;
            }

            mCurrentManagementPanel = mObjectPoolSystem.Spawn(workstationManagementPanelPrefabPath);

            if (mCurrentManagementPanel == null)
            {
                Debug.LogError($"WorkstationDisplay: 无法从对象池实例化或生成管理面板: {workstationManagementPanelPrefabPath}");
                return;
            }

            GameObject mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
            if (mainCanvas != null)
            {
                mCurrentManagementPanel.transform.SetParent(mainCanvas.transform, false);
            }
            else
            {
                Debug.LogWarning("WorkstationDisplay: 未找到具有 'MainCanvas' 标签的Canvas。管理面板可能层级不正确。");
            }

            mCurrentManagementPanel.transform.localPosition = Vector3.zero;
            mCurrentManagementPanel.transform.localScale = Vector3.one;
            RectTransform panelRect = mCurrentManagementPanel.GetComponent<RectTransform>();
            if (panelRect != null) {
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
            }

            var panelController = mCurrentManagementPanel.GetComponent<WorkstationManagementPanelController>();
            if (panelController != null)
            {
                panelController.Setup(workstationId, mWorkstationModel, mSurvivorModel);
            }
            else
            {
                Debug.LogError($"WorkstationDisplay: 管理面板预制件 {workstationManagementPanelPrefabPath} 上缺少 WorkstationManagementPanelController 脚本。");
                mObjectPoolSystem.Unspawn(mCurrentManagementPanel);
                mCurrentManagementPanel = null;
            }
        }

        void OnDestroy()
        {
            // 确保回收当前打开的面板，如果存在的话
            if (mCurrentManagementPanel != null && mObjectPoolSystem != null) {
                mObjectPoolSystem.Unspawn(mCurrentManagementPanel);
                mCurrentManagementPanel = null;
            }
            // QFramework的UnRegisterWhenGameObjectDestroyed会自动处理事件解注册
        }
    }
}
