using System; // 提供基本 .NET 类型支持，例如 Action
using UnityEngine; // Unity 核心 API
using UnityEngine.UI; // UI 相关功能（如 Text 组件）
using System.Collections.Generic; // C# 集合类，例如 List
// using MyGameNamespace; // 假设这是项目自定义的命名空间，当前脚本中未直接使用，可酌情保留或移除
using QFramework; // QFramework 游戏框架支持
using YourGameNamespace.Research; // 引入研究模块的命名空间，用于访问 Technology, ResearchModel, ResearchSystem 等

namespace YourGameNamespace.UI
{
    /// <summary>
    /// 研究界面显示管理脚本 (ResearchDisplay)。
    /// 负责在UI上显示当前资源数量、可研究的技术列表、正在研究的技术以及已完成的技术。
    /// </summary>
    public class ResearchDisplay : MonoBehaviour, IController // 实现 IController 接口以使用QFramework的事件和模型获取功能
    {
        // 公共字段：在Unity检视面板中分配的UI Text组件，用于显示各类资源数量
        public Text foodText;            // 食物数量显示文本框
        public Text powerText;           // 电力数量显示文本框
        public Text ammoText;            // 弹药数量显示文本框
        public Text medicineText;        // 药品数量显示文本框
        public Text researchPointsText;  // 科研点数显示文本框
        public Text electronicPartsText; // 电子零件数量显示文本框

        // 公共字段：用于分类显示不同状态技术的UI容器（Transform组件）
        public Transform availableTechUIParent;   // “可研究技术”列表的父级UI容器
        public Transform inProgressTechUIParent;  // “正在研究技术”列表的父级UI容器
        public Transform completedTechUIParent;   // “已完成技术”列表的父级UI容器
        public GameObject techUIPrefab;           // 单个技术条目UI元素的预制体 (Prefab)

        // 私有字段：对游戏核心系统和数据模型的引用
        private ResearchModel mResearchModel;   // 研究数据模型
        private ResearchSystem mResearchSystem; // 研究逻辑系统
        private ResourceModel mResourceModel;   // 资源数据模型
        
        // 刷新冷却计时器 (当前未使用，但保留了变量定义)
        private float mCoolDown = 2f;
        private float mCoolDownN = 2F; // 注意：变量名大小写不一致，通常建议统一

        /// <summary>
        /// Awake 是 Unity 的生命周期方法，在脚本实例被创建和加载时调用（早于Start）。
        /// 此处用于通过 Transform.Find 动态查找并设置各个UI Text组件的引用。
        /// 注意：这种查找方式依赖于UI层级结构和命名，如果UI结构改变，可能导致查找失败。
        /// 更稳妥的方式是在Unity检视面板中直接拖拽赋值这些公共字段。
        /// </summary>
        private void Awake()
        {
            // 动态查找并绑定UI组件到对应的公共字段
            // 假设这些Text组件位于名为 "ResourcePanel" 的子对象下
            foodText = transform.Find("ResourcePanel/FoodText")?.GetComponent<Text>();
            powerText = transform.Find("ResourcePanel/PowerText")?.GetComponent<Text>();
            ammoText = transform.Find("ResourcePanel/AmmoText")?.GetComponent<Text>();
            medicineText = transform.Find("ResourcePanel/MedicineText")?.GetComponent<Text>();
            researchPointsText = transform.Find("ResourcePanel/ResearchPointsText")?.GetComponent<Text>();
            electronicPartsText = transform.Find("ResourcePanel/ElectronicPartsText")?.GetComponent<Text>();
            
            // mResearchSystem 的获取移至 Start()，以确保 GameArchitecture 已初始化完毕
        }

        /// <summary>
        /// Start 是 Unity 的生命周期方法，在组件的第一次Update调用前执行。
        /// 用于获取对 GameArchitecture 中注册的模型和系统的引用，并进行初始UI刷新。
        /// </summary>
        void Start()
        {
            // 确保 GameArchitecture.Interface (QFramework的架构单例) 已经初始化
            if (GameArchitecture.Interface == null)
            {
                Debug.LogError("研究显示 (ResearchDisplay)：GameArchitecture.Interface 为空。请确保 GameInitializer 脚本先于此脚本运行。");

                // 尝试进行后备初始化：手动查找场景中的 GameInitializer 并确保其运行
                var gameInitializer = FindObjectOfType<GameInitializer>();
                if (gameInitializer == null && Application.isPlaying) // 如果场景中不存在 GameInitializer 且游戏正在运行
                {
                    Debug.LogWarning("研究显示 (ResearchDisplay)：未在场景中找到 GameInitializer 实例，正在尝试动态添加一个。");
                    gameObject.AddComponent<GameInitializer>(); // 动态添加 GameInitializer 组件到当前GameObject
                }
                // 如果 GameInitializer 存在但 Interface 仍为 null，可能是由于脚本执行顺序问题。
                // 此时，可以考虑使用事件机制或延迟加载的方式来确保依赖项在访问前已准备就绪。
            }

            // 从 GameArchitecture 获取模型和系统的实例
            // 使用 ?. 安全操作符，避免在 GameArchitecture.Interface 为 null 时产生异常
            mResearchModel = GameArchitecture.Interface?.GetModel<ResearchModel>();
            mResearchSystem = GameArchitecture.Interface?.GetSystem<ResearchSystem>(); // this.GetSystem<T>() 是IController提供的扩展
            mResourceModel = GameArchitecture.Interface?.GetModel<ResourceModel>();

            // 检查必要的模型和系统是否已成功获取
            if (mResearchModel == null) Debug.LogError("研究显示 (ResearchDisplay)：未能获取研究数据模型 (ResearchModel)！请确保其已在 GameArchitecture 中注册且架构已正确初始化。");
            if (mResearchSystem == null) Debug.LogError("研究显示 (ResearchDisplay)：未能获取研究系统 (ResearchSystem)！请确保其已在 GameArchitecture 中注册。");
            if (mResourceModel == null) Debug.LogError("研究显示 (ResearchDisplay)：未能获取资源数据模型 (ResourceModel)！请确保其已在 GameArchitecture 中注册。");

            RefreshUI(); // 组件启动时执行一次UI刷新
        }

        /// <summary>
        /// Update 是 Unity 的生命周期方法，每帧都会调用。
        /// 当前此方法为空，UI的刷新主要由 RefreshUI() 方法（可能被外部调用或事件触发）和 HandleTechnologyStatusChange 事件处理器负责。
        /// 如果需要每帧轮询更新，可以在此添加 RefreshUI() 调用，但通常不推荐以避免性能开销。
        /// </summary>
        void Update()
        {
            // RefreshUI(); // 例如，如果需要每帧强制刷新UI（不推荐）
        }

        /// <summary>
        /// 刷新整个研究界面的用户界面元素。
        /// 包括各类资源的数量显示，以及不同状态（可研究、研究中、已完成）的技术列表。
        /// </summary>
        public void RefreshUI()
        {
            // 安全检查：确保所有必要的模型、系统和UI预制件都已正确设置
            if (mResearchModel == null || mResearchSystem == null || mResourceModel == null || techUIPrefab == null)
            {
                // 如果在 Start 方法执行期间某些依赖项仍为 null (例如由于执行顺序问题)，则尝试在此处重新获取它们
                if (GameArchitecture.Interface != null)
                {
                    mResearchModel = mResearchModel ?? GameArchitecture.Interface.GetModel<ResearchModel>();
                    mResearchSystem = mResearchSystem ?? GameArchitecture.Interface.GetSystem<ResearchSystem>();
                    mResourceModel = mResourceModel ?? GameArchitecture.Interface.GetModel<ResourceModel>();
                }

                // 如果重新获取后依赖项仍然为 null，则记录错误并返回，以避免后续操作产生空引用异常
                if (mResearchModel == null || mResearchSystem == null || mResourceModel == null || techUIPrefab == null)
                {
                     Debug.LogError("研究显示 (ResearchDisplay)：刷新UI失败，因为一个或多个必要的模型/系统或UI预制件 (techUIPrefab) 未能正确初始化。");
                    return;
                }
            }

            // 更新资源数量的文本显示
            if (foodText != null)
                foodText.text = $"食物: {mResourceModel.GetAmount(GameResourceType.Food)}";
            if (powerText != null)
                powerText.text = $"电力: {mResourceModel.GetAmount(GameResourceType.Power)}";
            if (ammoText != null)
                ammoText.text = $"弹药: {mResourceModel.GetAmount(GameResourceType.Ammo)}";
            if (medicineText != null)
                medicineText.text = $"药品: {mResourceModel.GetAmount(GameResourceType.Medicine)}";
            if (researchPointsText != null)
                researchPointsText.text = $"科研点数: {mResourceModel.GetAmount(GameResourceType.ResearchPoints)}";
            if (electronicPartsText != null)
                electronicPartsText.text = $"电子零件: {mResourceModel.GetAmount(GameResourceType.ElectronicParts)}";

            // 更新所有技术的状态（例如，检查是否有技术因前置条件满足而从“锁定”变为“可研究”）
            mResearchModel.UpdateAllTechnologyStatuses();
            
            // mCoolDownN += UnityEngine.Time.deltaTime; // 此冷却逻辑当前未被使用来限制刷新频率，可以考虑移除或完善
          
            // 刷新不同状态的技术列表的UI显示
            RefreshTechnologyList(availableTechUIParent, mResearchModel.GetAvailableTechnologies());
            RefreshTechnologyList(inProgressTechUIParent, mResearchModel.GetInProgressTechnologies());
            RefreshTechnologyList(completedTechUIParent, mResearchModel.GetCompletedTechnologies());
            // ForceRefreshAllLists(); // 如果需要强制刷新所有列表，可以调用此方法（当前被注释掉）
        }

        // 强制刷新所有技术列表的私有辅助方法 (当前未使用)
        private void ForceRefreshAllLists()
        {
            RefreshTechnologyList(availableTechUIParent, mResearchModel.GetAvailableTechnologies());
            RefreshTechnologyList(inProgressTechUIParent, mResearchModel.GetInProgressTechnologies());
            RefreshTechnologyList(completedTechUIParent, mResearchModel.GetCompletedTechnologies());
        }

        /// <summary>
        /// 刷新指定父容器下的技术列表UI显示。
        /// 此方法会先清除父容器下所有旧的技术条目UI，然后为传入的技术列表中的每项技术创建一个新的UI条目。
        /// </summary>
        /// <param name="parent">用于容纳技术条目UI的父级Transform组件。</param>
        /// <param name="techs">要在此容器中显示的技术对象列表。</param>
        void RefreshTechnologyList(Transform parent, List<Technology> techs)
        {
            if (parent == null) return; // 如果父容器未分配，则不执行任何操作

            // 清除当前父容器下的所有现有UI条目 (通常是旧的技术条目)
            foreach (Transform child in parent)
            {
                Destroy(child.gameObject); // 销毁子GameObject
            }

            // 为列表中的每项技术创建并设置新的UI条目
            foreach (Technology tech in techs)
            {
                GameObject techItemGO = Instantiate(techUIPrefab, parent); // 实例化技术UI预制件，并将其置于指定父容器下
                TechDisplayItem displayItem = techItemGO.GetComponent<TechDisplayItem>(); // 获取预制件上的 TechDisplayItem 脚本组件

                if (displayItem != null) // 如果成功获取到脚本组件
                {
                    // 调用 TechDisplayItem 的 Setup 方法，传递技术数据、研究系统和资源模型引用，以初始化其显示内容和交互逻辑
                    displayItem.Setup(tech, mResearchSystem, mResourceModel);
                }
                else
                {
                    Debug.LogError($"研究显示 (ResearchDisplay)：实例化的技术UI预制件 (techUIPrefab) 上缺少 TechDisplayItem 脚本组件！请检查预制件配置。");
                }
            }
        }
        
        // Unity生命周期方法：当组件被启用时调用
        private void OnEnable()
        {
            // 确保 ResearchSystem 已初始化后再订阅事件
            if (mResearchSystem != null) {
                 mResearchSystem.OnTechnologyStatusChanged += HandleTechnologyStatusChange; // 订阅技术状态变更事件
            } else {
                // 尝试在启用时再次获取，以防 Start() 中获取失败或执行顺序问题
                 if (GameArchitecture.Interface != null) {
                    mResearchSystem = GameArchitecture.Interface.GetSystem<ResearchSystem>();
                    if (mResearchSystem != null) {
                        mResearchSystem.OnTechnologyStatusChanged += HandleTechnologyStatusChange;
                    } else {
                         Debug.LogError("研究显示 (ResearchDisplay) OnEnable：未能获取 ResearchSystem，无法订阅技术状态变更事件。");
                    }
                 }
            }
        }

        // Unity生命周期方法：当组件被禁用时调用
        private void OnDisable()
        {
            if (mResearchSystem != null)
            {
                mResearchSystem.OnTechnologyStatusChanged -= HandleTechnologyStatusChange; // 取消订阅技术状态变更事件，防止内存泄漏
            }
        }

        /// <summary>
        /// 处理单个技术状态发生变化的事件。
        /// 当接收到技术状态变更通知时，此方法会尝试在相应的UI容器中查找并更新该技术的显示。
        /// </summary>
        /// <param name="tech">状态已发生变化的技术对象。</param>
        private void HandleTechnologyStatusChange(Technology tech)
        {
            // 尝试在三个主要的UI容器（可研究、研究中、已完成）中查找并更新对应技术的UI条目
            // 这种方式比完全刷新所有列表更高效，因为它只针对发生变化的技术进行局部更新。
            // 但前提是该技术条目UI已存在于某个列表中。如果技术状态变化导致其从一个列表移动到另一个（例如从“研究中”到“已完成”），
            // 则可能需要更复杂的逻辑或依赖于 RefreshUI 来完全重建列表。
            // 当前的 RefreshUI 包含了 UpdateAllTechnologyStatuses，它会重新分类技术，所以这里可能只需要触发一次完整的 RefreshUI。
            // 为了更精细的控制和潜在的性能优化，保留单独更新的逻辑，但实际效果取决于列表管理策略。
            // UpdateTechInContainer(availableTechUIParent, tech);
            // UpdateTechInContainer(inProgressTechUIParent, tech);
            // UpdateTechInContainer(completedTechUIParent, tech);
            // 考虑到状态变化可能导致技术在不同列表间移动，直接调用RefreshUI可能是最简单且能保证一致性的做法。
             RefreshUI(); 
        }

        // 在指定的父容器中查找并更新特定技术的UI条目 (当前被HandleTechnologyStatusChange中的RefreshUI替代)
        private void UpdateTechInContainer(Transform parent, Technology tech)
        {
            if (parent == null) return;
            foreach (Transform child in parent)
            {
                var displayItem = child.GetComponent<TechDisplayItem>();
                // 如果找到了与技术ID匹配的UI条目
                if (displayItem != null && displayItem.Matches(tech)) 
                {
                    displayItem.Setup(tech, mResearchSystem, mResourceModel); // 重新设置其显示数据
                    return; // 找到并更新后即可返回
                }
            }
        }

        // IController 接口实现，返回游戏架构实例
        public IArchitecture GetArchitecture()
        {
            return GameArchitecture.Interface;
        }
    }
}
