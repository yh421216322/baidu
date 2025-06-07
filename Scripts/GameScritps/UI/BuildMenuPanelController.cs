// 文件路径: Scripts/GameScritps/UI/BuildMenuPanelController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using QFramework;
using YourGameNamespace.Buildings;
using YourGameNamespace.Commands;
using YourGameNamespace.Events;
using YourGameNamespace.Resources;
using MyGameNamespace; // For IObjectPoolSystem
// using DG.Tweening; // Uncomment if DOTween is actually used

namespace YourGameNamespace.UI
{
    public class BuildMenuPanelController : MonoBehaviour, IController, IPoolable
    {
        [Header("UI 引用")] // UI References (in Chinese)
        public Transform buildMenuItemsContainer; // 建造菜单项容器
        public Button closeButton; // 关闭按钮
        public string buildMenuItemPrefabPath = "Prefabs/UI/Items/BuildMenuItem_PF";

        private ObjectPoolSystem mObjectPoolSystem; // Changed from IObjectPoolSystem
        private BuildingSystem mBuildingSystem; // Changed from IBuildingSystem
        private ResourceModel mResourceModel;   // Changed from IResourceModel

        private List<GameObject> mInstantiatedItems = new List<GameObject>();
        private List<IUnRegister> mEventUnregisters = new List<IUnRegister>();


        public IArchitecture GetArchitecture() => RegisterManager.Interface;

        // InitAndShow is preferred over Awake for dependencies if panel is pooled/managed by another system
        /// <summary>
        /// 初始化系统引用并显示面板。应在对象池生成并激活此面板后由外部调用。
        /// </summary>
        public void InitAndShow()
        {
            // 获取系统和模型引用
            mObjectPoolSystem = this.GetSystem<ObjectPoolSystem>(); // Use concrete class
            mBuildingSystem = this.GetSystem<BuildingSystem>(); // Use concrete class
            mResourceModel = this.GetModel<ResourceModel>(); // Use concrete class

            // 进行必要的空检查
            if (buildMenuItemsContainer == null) Debug.LogError("BuildMenuPanelController: 建造菜单项容器 (buildMenuItemsContainer) 未在检视面板中分配!");
            if (closeButton == null) Debug.LogError("BuildMenuPanelController: 关闭按钮 (closeButton) 未在检视面板中分配!");
            if (string.IsNullOrEmpty(buildMenuItemPrefabPath)) Debug.LogError("BuildMenuPanelController: 建造菜单项预制件路径 (buildMenuItemPrefabPath) 未设置!");

            if (mObjectPoolSystem == null) Debug.LogError("建造菜单：对象池系统 (ObjectPoolSystem) 未找到！");
            if (mBuildingSystem == null) Debug.LogError("建造菜单：建筑系统 (BuildingSystem) 未找到！");
            if (mResourceModel == null) Debug.LogError("建造菜单：资源模型 (ResourceModel) 未找到！");

            // 设置关闭按钮监听
            if (closeButton != null) // 再次检查以防万一
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(ClosePanel);
            }

            // 清理旧的事件监听器并重新注册
            foreach(var unReg in mEventUnregisters) unReg.UnRegister();
            mEventUnregisters.Clear();

            this.RegisterEvent<BuildBuildingResultEvent>(OnBuildResult)
                .UnRegisterWhenGameObjectDestroyed(gameObject).AddTo(mEventUnregisters); // AddTo 用于手动管理反注册列表
            this.RegisterEvent<ResourceChangedEvent>(OnResourcesChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject).AddTo(mEventUnregisters);

            PopulateBuildMenu(); // 填充菜单项

            // DOTween animation for panel appearing (conceptual)
            // transform.localScale = Vector3.one * 0.7f; // Example start scale for animation
            // transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
            gameObject.SetActive(true); // 确保面板在初始化后可见
        }

        private void PopulateBuildMenu()
        {
            if (mObjectPoolSystem == null || mBuildingSystem == null || buildMenuItemsContainer == null) return;
            ClearInstantiatedItems();

            foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
            {
                GameObject itemGO = mObjectPoolSystem.Spawn(buildMenuItemPrefabPath);
                if (itemGO == null) {
                     Debug.LogError($"BuildMenuPanelController: 无法从对象池生成预制件: {buildMenuItemPrefabPath} (用于建筑类型: {type})");
                    continue;
                }

                itemGO.transform.SetParent(buildMenuItemsContainer, false);
                itemGO.SetActive(true);
                mInstantiatedItems.Add(itemGO);

                var itemUI = itemGO.GetComponent<BuildMenuItemUI>();
                if (itemUI != null)
                {
                    string name = GetLocalizedBuildingName(type);
                    string description = GetBuildingDescription(type);
                    List<(GameResourceType resource, int amount)> costs = mBuildingSystem.GetBuildingConstructionCosts(type);
                    Sprite icon = null; // TODO: 实现图标加载逻辑 (例如从 Addressables 或 Resources)

                    itemUI.Setup(type, name, description, costs, icon, RequestBuild);
                    UpdateButtonInteractability(itemUI, costs);
                } else {
                    Debug.LogError($"BuildMenuPanelController: 预制件 {buildMenuItemPrefabPath} 上缺少 BuildMenuItemUI 脚本。");
                    mObjectPoolSystem.Recycle(itemGO);
                    mInstantiatedItems.Remove(itemGO);
                }
            }
        }

        private void UpdateButtonInteractability(BuildMenuItemUI itemUI, List<(GameResourceType resource, int amount)> costs)
        {
            if (itemUI == null || itemUI.buildButton == null || mResourceModel == null) return;
            bool canAfford = true;
            if (costs != null && costs.Count > 0) { // 检查 costs 是否为 null 且有内容
                foreach (var cost in costs) {
                    if (!mResourceModel.HasEnough(cost.resource, cost.amount)) {
                        canAfford = false; break;
                    }
                }
            } else {
                // 如果成本列表为null或空 (例如，建筑未定义成本或免费)，根据游戏逻辑决定是否可建造
                // 对于免费建筑，canAfford应为true；对于未定义成本的，可能应为false
                // 假设 GetBuildingConstructionCosts 在未定义时返回null或空列表，这里将其视为不可建造
                 canAfford = (costs != null && costs.Count == 0); // Only true if costs list is empty (free), false if null (undefined)
            }
            itemUI.buildButton.interactable = canAfford;
        }

        private void OnResourcesChanged(ResourceChangedEvent e)
        {
            // 当资源变化时，更新所有建造按钮的可交互状态
            if(mBuildingSystem == null) return; // Guard against system not being ready
            foreach(var itemGO in mInstantiatedItems) {
                var itemUI = itemGO.GetComponent<BuildMenuItemUI>();
                if(itemUI != null) {
                    // 使用 BuildMenuItemUI 实例中存储的 BuildingType
                    List<(GameResourceType resource, int amount)> costs = mBuildingSystem.GetBuildingConstructionCosts(itemUI.mBuildingType);
                    UpdateButtonInteractability(itemUI, costs);
                }
            }
        }

        private void RequestBuild(BuildingType type)
        {
            // Debug.Log($"[UI] 请求建造: {type}"); // Chinese Log
            this.SendCommand(new BuildBuildingCommand(type, true)); // isPlayerAction is true
        }

        private void OnBuildResult(BuildBuildingResultEvent e)
        {
            // if (!e.Success) {
            //    Debug.LogWarning($"[UI] 提示：建造 {e.BuildingTypeAttempted} 失败。原因: {e.FailureReasonKey}"); // Chinese Log
            //    // TODO: Display user-friendly message to player
            // }
            // 资源变化会由 OnResourcesChanged 处理，进而更新按钮状态。
            // 建筑成功后，菜单通常保持打开，除非有特定设计要求关闭。
        }

        // --- Placeholder Data Fetching & Localization ---
        private string GetLocalizedBuildingName(BuildingType type) {
            switch(type) {
                case BuildingType.Tent: return "帐篷";
                case BuildingType.MedicalPost: return "医疗站";
                case BuildingType.Cookhouse: return "食堂";
                case BuildingType.Farm: return "农场";
                case BuildingType.PowerPlant: return "发电厂";
                case BuildingType.Workshop: return "工坊";
                case BuildingType.Clinic: return "诊所";
                case BuildingType.ResearchLab: return "科研实验室";
                default: return type.ToString();
            }
        }
        private string GetBuildingDescription(BuildingType type) {
            switch(type) {
                case BuildingType.Tent: return "提供5个基础床位。";
                case BuildingType.MedicalPost: return "可容纳2名病人。";
                case BuildingType.Cookhouse: return "加工食材，提高效率。";
                case BuildingType.Farm: return "生产食物。";
                case BuildingType.PowerPlant: return "为基地提供电力。";
                case BuildingType.Workshop: return "生产工具与弹药。";
                case BuildingType.Clinic: return "生产药品并治疗伤病。";
                case BuildingType.ResearchLab: return "进行科技研究。";
                default: return $"这是 {GetLocalizedBuildingName(type)} 的描述。";
            }
        }

        private void ClearInstantiatedItems() {
            if(mObjectPoolSystem == null) return;
            foreach (var item in mInstantiatedItems) { if(item != null) mObjectPoolSystem.Recycle(item); } // Check item null
            mInstantiatedItems.Clear();
        }

        /// <summary>
        /// 关闭面板并回收自身。
        /// </summary>
        public void ClosePanel() {
            // DOTween animation for panel disappearing (conceptual)
            // transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() => {
                if (mObjectPoolSystem != null) { mObjectPoolSystem.Recycle(this.gameObject); } // OnRecycled will be called
                else { gameObject.SetActive(false); OnRecycled(); } // Manual call if no pool
            // });
        }

        // --- IPoolable Implementation ---
        public void OnRecycled() {
            // Debug.Log("BuildMenuPanelController OnRecycled called."); // Chinese Log
            ClearInstantiatedItems();
            foreach(var unReg in mEventUnregisters) unReg.UnRegister();
            mEventUnregisters.Clear();
            gameObject.SetActive(false);
        }
        public bool IsRecycled { get; set; }
    }
}
