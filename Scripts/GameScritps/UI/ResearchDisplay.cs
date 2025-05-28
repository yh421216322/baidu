using System; // 提供基本 .NET 类型支持
using UnityEngine; // Unity 核心 API
using UnityEngine.UI; // UI 相关功能（如 Text）
using System.Collections.Generic;
using MyGameNamespace; // 集合类
using QFramework; // 游戏框架支持
using YourGameNamespace.Research; // 引入研究模块

namespace YourGameNamespace.UI
{
    /// <summary>
    /// 用于管理研究界面显示的脚本。
    /// 显示资源数量、可用技术、进行中的技术和已完成的技术。
    /// </summary>
    public class ResearchDisplay : MonoBehaviour,IController
    {

        
        // 公共字段：在 Inspector 中分配的 UI 文本组件
        public Text foodText;            // 食物数量显示
        public Text powerText;           // 电力数量显示
        public Text ammoText;            // 弹药数量显示
        public Text medicineText;        // 药品数量显示
        public Text researchPointsText;  // 科研点数显示
        public Text electronicPartsText; // 电子零件数量显示

        // 公共字段：技术状态分类的 UI 容器
        public Transform availableTechUIParent;   // 可用技术的父级容器
        public Transform inProgressTechUIParent;  // 进行中的技术的父级容器
        public Transform completedTechUIParent;   // 已完成的技术的父级容器
        public GameObject techUIPrefab;           // 技术 UI 的预制体

        // 私有字段：引用游戏系统和模型
        private ResearchModel mResearchModel;
        private ResearchSystem mResearchSystem;
        private ResourceModel mResourceModel;
        
        // 刷新冷却时间
        private float mCoolDown = 2f;
        private float mCoolDownN = 2F;

        /// <summary>
        /// Awake 是 Unity 生命周期方法，在对象初始化时调用。
        /// 用于查找并设置各个 UI 文本组件的引用。
        /// </summary>
        private void Awake()
        {
            // 查找并绑定 UI 组件到对应的公共字段
            foodText = transform.Find("ResourcePanel/FoodText").GetComponent<Text>();
            powerText = transform.Find("ResourcePanel/PowerText").GetComponent<Text>();
            ammoText = transform.Find("ResourcePanel/AmmoText").GetComponent<Text>();
            medicineText = transform.Find("ResourcePanel/MedicineText").GetComponent<Text>();
            researchPointsText = transform.Find("ResourcePanel/ResearchPointsText").GetComponent<Text>();
            electronicPartsText = transform.Find("ResourcePanel/ElectronicPartsText").GetComponent<Text>();
            mResearchSystem = this.GetSystem<ResearchSystem>();
            //1
        }

        /// <summary>
        /// Start 是 Unity 生命周期方法，在组件第一次启用时调用。
        /// 用于获取 GameArchitecture 接口中的模型和系统，并初始化 UI。
        /// </summary>
        void Start()
        {
            // 确保 GameArchitecture.Interface 已经初始化
            if (GameArchitecture.Interface == null)
            {
                Debug.LogError("研究显示：GameArchitecture.Interface 为空。请确保 GameInitializer 先运行。");

                // 尝试手动查找并添加 GameInitializer
                var gameInitializer = FindObjectOfType<GameInitializer>();
                if (gameInitializer == null && Application.isPlaying)
                {
                    Debug.LogWarning("研究显示：未找到 GameInitializer，尝试添加一个。");
                    gameObject.AddComponent<GameInitializer>(); // 添加 GameInitializer 组件
                }
                else if (gameInitializer != null)
                {
                    // 如果存在 GameInitializer，但 Interface 仍为 null，可能是执行顺序问题
                    // 可以考虑使用事件或延迟加载来解决
                }
            }

            // 获取模型和系统实例
            mResearchModel = GameArchitecture.Interface?.GetModel<ResearchModel>();
            mResearchSystem = GameArchitecture.Interface?.GetSystem<ResearchSystem>();
            mResourceModel = GameArchitecture.Interface?.GetModel<ResourceModel>();

            // 检查必要组件是否已正确获取
            if (mResearchModel == null) Debug.LogError("研究显示：未找到 ResearchModel！请确保它已注册且 GameArchitecture 已初始化。");
            if (mResearchSystem == null) Debug.LogError("研究显示：未找到 ResearchSystem！请确保它已注册。");
            if (mResourceModel == null) Debug.LogError("研究显示：未找到 ResourceModel！请确保它已注册。");

            RefreshUI(); // 初始化时刷新 UI
        }

        /// <summary>
        /// Update 是 Unity 生命周期方法，每帧调用一次。
        /// 用于持续刷新 UI，以便及时反映数据变化。
        /// </summary>
        void Update()
        {
            //RefreshUI(); // 刷新 UI
        }

        /// <summary>
        /// 刷新整个研究界面的 UI。
        /// 包括资源显示、技术列表等。
        /// </summary>
        public void RefreshUI()
        {
            // 检查模型、系统和预制件是否有效
            if (mResearchModel == null || mResearchSystem == null || mResourceModel == null || techUIPrefab == null)
            {
                // 如果在 Start 期间某些组件为 null，则尝试重新获取
                if (GameArchitecture.Interface != null)
                {
                    mResearchModel = mResearchModel ?? GameArchitecture.Interface.GetModel<ResearchModel>();
                    mResearchSystem = mResearchSystem ?? GameArchitecture.Interface.GetSystem<ResearchSystem>();
                    mResourceModel = mResourceModel ?? GameArchitecture.Interface.GetModel<ResourceModel>();
                }

                // 如果仍然为 null，则返回
                if (mResearchModel == null || mResearchSystem == null || mResourceModel == null || techUIPrefab == null)
                {
                    return;
                }
            }

            // 更新资源文本显示
            if (foodText != null)
                foodText.text = $"食物: {mResourceModel.GetAmount(GameResourceType.Food)}";
            
            if (powerText != null)
                powerText.text = $"电力: {mResourceModel.GetAmount(GameResourceType.Power)}";
            
            if (ammoText != null)
                ammoText.text = $"弹药: {mResourceModel.GetAmount(GameResourceType.Ammo)}";
            
            if (medicineText != null)
                medicineText.text = $"药品: {mResourceModel.GetAmount(GameResourceType.Medicine)}";
            
            if (researchPointsText != null)
                researchPointsText.text = $"科研点: {mResourceModel.GetAmount(GameResourceType.ResearchPoints)}";
            
            if (electronicPartsText != null)
                electronicPartsText.text = $"电子零件: {mResourceModel.GetAmount(GameResourceType.ElectronicParts)}";

            // 更新所有技术的状态（如解锁、进度等）
            mResearchModel.UpdateAllTechnologyStatuses();
            mCoolDownN+=UnityEngine.Time.deltaTime;
          
                 RefreshTechnologyList(availableTechUIParent, mResearchModel.GetAvailableTechnologies());
                 RefreshTechnologyList(inProgressTechUIParent, mResearchModel.GetInProgressTechnologies());
                 RefreshTechnologyList(completedTechUIParent, mResearchModel.GetCompletedTechnologies());
                //ForceRefreshAllLists();

                // 刷新不同状态的技术列表

        }

        private void ForceRefreshAllLists()
        {
            RefreshTechnologyList(availableTechUIParent, mResearchModel.GetAvailableTechnologies());
            RefreshTechnologyList(inProgressTechUIParent, mResearchModel.GetInProgressTechnologies());
            RefreshTechnologyList(completedTechUIParent, mResearchModel.GetCompletedTechnologies());
        }
        /// <summary>
        /// 刷新指定类型的技术列表。
        /// </summary>
        /// <param name="parent">该类技术的 UI 容器</param>
        /// <param name="techs">要显示的技术列表</param>
        void RefreshTechnologyList(Transform parent, List<Technology> techs)
        {
            if (parent == null) return;

            // 清除现有 UI 条目
            foreach (Transform child in parent)
            {
                Destroy(child.gameObject);
            }

            // 创建新的 UI 条目
            foreach (Technology tech in techs)
            {
                GameObject techItemGO = Instantiate(techUIPrefab, parent);
                TechDisplayItem displayItem = techItemGO.GetComponent<TechDisplayItem>();

                // 设置 TechDisplayItem 数据
                if (displayItem != null)
                {
                    displayItem.Setup(tech, mResearchSystem, mResourceModel);
                }
                else
                {
                    Debug.LogError($"研究显示：TechUIPrefab 缺少 TechDisplayItem 脚本组件！");
                }
            }
        }
        
        private void OnEnable()
        {
            mResearchSystem.OnTechnologyStatusChanged += HandleTechnologyStatusChange;
        }

        private void OnDisable()
        {
            mResearchSystem.OnTechnologyStatusChanged -= HandleTechnologyStatusChange;
        }

        /// <summary>
        /// 处理单个技术状态变化，只刷新对应 UI 条目
        /// </summary>
        private void HandleTechnologyStatusChange(Technology tech)
        {
            // 尝试在三个容器中查找对应 TechDisplayItem 并刷新
            UpdateTechInContainer(availableTechUIParent, tech);
            UpdateTechInContainer(inProgressTechUIParent, tech);
            UpdateTechInContainer(completedTechUIParent, tech);
        }

        private void UpdateTechInContainer(Transform parent, Technology tech)
        {
            foreach (Transform child in parent)
            {
                var displayItem = child.GetComponent<TechDisplayItem>();
                if (displayItem != null && displayItem.Matches(tech))
                {
                    displayItem.Setup(tech, mResearchSystem, mResourceModel);
                    return;
                }
            }
        }


        public IArchitecture GetArchitecture()
        {
            return GameArchitecture.Interface;
        }
    }
}
