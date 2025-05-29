using System; // 用于 Action 等基本 .NET 类型
using UnityEngine;
using UnityEngine.UI; // 用于 UI 组件，如 Text, Button, Slider
using YourGameNamespace.Research; // 引入研究模块的命名空间，用于 Technology, ResearchSystem 等

namespace YourGameNamespace.UI
{
    // UI项，用于显示单个技术的信息和交互（例如开始研究按钮）
    public class TechDisplayItem : MonoBehaviour
    {
        // 公共字段，在Unity检视面板中分配对应的UI组件
        public Text nameText;          // 显示技术名称的Text组件
        public Text descriptionText;   // 显示技术描述的Text组件
        public Text statusText;        // 显示技术状态、成本或进度的Text组件
        public Button researchButton;    // “开始研究”按钮
        public Slider progressBar;     // 可选，用于显示正在研究项目的进度条

        // 私有字段，存储当前UI项关联的技术信息和所需系统引用
        private Technology mTechnology;       // 当前UI项代表的技术对象
        private ResearchSystem mResearchSystem; // 研究系统的引用
        private ResourceModel mResourceModel;   // 资源模型的引用，用于检查研究点数是否足够

        // Awake 方法在脚本实例被创建时调用
        private void Awake()
        {
            // 通过名称查找并获取子GameObject上的UI组件引用
            // 注意：这种查找方式依赖于UI预制件的层级结构和命名，如果更改了预制件结构，可能需要更新这些查找路径。
            nameText = transform.Find("NameText")?.GetComponent<Text>();
            descriptionText = transform.Find("DescriptionText")?.GetComponent<Text>();
            statusText = transform.Find("StatusText")?.GetComponent<Text>();
            researchButton = transform.Find("ResearchButton")?.GetComponent<Button>();
            progressBar = transform.Find("ProgressBar")?.GetComponent<Slider>();
            
            // 为研究按钮添加点击事件监听器（初始的匿名方法，将在Setup中被替换或清除）
            researchButton?.onClick.AddListener(() =>
            {
                Debug.Log("“开始研究”按钮被点击（初始监听器）。"); // 此日志通常在Setup方法正确执行后不会出现
            });
        }

        // 设置并初始化此技术UI项的显示内容和交互逻辑
        public void Setup(Technology tech, ResearchSystem researchSystem, ResourceModel resourceModel)
        {
            mTechId = tech.Id; // 缓存技术ID，用于后续可能的匹配操作
            mTechnology = tech;
            mResearchSystem = researchSystem;
            mResourceModel = resourceModel;

            // 设置技术名称和描述文本 (假设 Technology 对象的 Name 和 Description 属性已包含本地化文本)
            nameText.text = mTechnology.Name; 
            descriptionText.text = mTechnology.Description;

            // 根据技术状态更新UI显示
            if (mTechnology.Status == ResearchStatus.Available) // 如果技术当前可研究
            {
                if (statusText != null) statusText.text = $"成本：{mTechnology.ResearchPointCost} 研究点"; // 显示研究成本
                if (researchButton != null)
                {
                    researchButton.gameObject.SetActive(true); // 显示研究按钮
                    researchButton.onClick.RemoveAllListeners(); // 移除所有旧的点击监听器
                    researchButton.onClick.AddListener(StartResearch_OnClick); // 添加新的点击监听器
                    researchButton.interactable = true; // 确保按钮可交互
                }
                progressBar?.gameObject.SetActive(false); // 可用状态下通常不显示进度条
            }
            else if (mTechnology.Status == ResearchStatus.InProgress) // 如果技术正在研究中
            {
                if (statusText != null) statusText.text = "研究中..."; // 显示“研究中...”
                if (researchButton != null) researchButton.gameObject.SetActive(false); // 隐藏研究按钮
                if (progressBar != null) // 如果有进度条UI组件
                {
                    progressBar.gameObject.SetActive(true); // 显示进度条
                    // 更新进度条的值 (假设 ResearchSystem 提供了获取当前研究进度的标准化方法)
                    progressBar.value = mResearchSystem.GetCurrentResearchProgressNormalized(); 
                }
            }
            else if (mTechnology.Status == ResearchStatus.Completed) // 如果技术已完成研究
            {
                if (statusText != null) statusText.text = "状态：已完成"; // 显示“已完成”
                if (researchButton != null) researchButton.gameObject.SetActive(false); // 隐藏研究按钮
                progressBar?.gameObject.SetActive(false); // 完成状态下通常不显示进度条
            }
            else // 如果技术处于其他状态（例如 Locked - 锁定）
            {
                if (statusText != null)
                    // 显示锁定状态及所需前置技术 (PrerequisiteTechIds中的ID应转换为可读的技术名称，此处简化为直接显示ID)
                    statusText.text = $"状态：已锁定 (前置技术：{string.Join("、", mTechnology.PrerequisiteTechIds)})";
                if (researchButton != null) researchButton.gameObject.SetActive(false); // 隐藏研究按钮
                progressBar?.gameObject.SetActive(false); // 锁定状态下通常不显示进度条
            }
        }

        // 当“开始研究”按钮被点击时调用的方法
        void StartResearch_OnClick()
        {
            Debug.Log($"尝试开始研究技术：{mTechnology?.Name} (ID: {mTechnology?.Id})");
            if (mResearchSystem != null && mTechnology != null)
            {
                // 检查是否有足够的研究点数
                if (mResourceModel.GetAmount(GameResourceType.ResearchPoints) >= mTechnology.ResearchPointCost)
                {
                    // 再次检查当前是否没有其他研究正在进行（防止并发研究，如果系统不支持的话）
                    if (!mResearchSystem.IsResearching()) 
                    {
                        mResearchSystem.StartResearch(mTechnology.Id); // 调用研究系统开始研究
                        // 可选：通知父级UI (ResearchDisplay) 刷新整个列表或此项的状态，
                        // 以便将此技术项移动到“进行中”列表，并更新其UI。
                        // (当前实现中，ResearchDisplay的HandleTechnologyStatusChange会通过RefreshUI处理)
                    }
                    else
                    {
                         Debug.LogWarning("无法开始新的研究，因为当前已有另一项研究正在进行中。");
                    }
                }
                else
                {
                    Debug.LogWarning($"研究点不足，无法开始研究 {mTechnology.Name}。需要研究点：{mTechnology.ResearchPointCost}，当前拥有：{mResourceModel.GetAmount(GameResourceType.ResearchPoints)}。");
                }
            }
        }
        
        private string mTechId; // 缓存技术ID，用于在父级UI刷新时进行匹配和更新

        // 获取此UI项关联的技术ID
        public string GetTechnologyId()
        {
            return mTechId;
        }

        // 检查此UI项是否代表传入的Technology对象
        public bool Matches(Technology tech)
        {
            return mTechId == tech?.Id; // 使用 ?. 安全操作符避免 tech 为 null 时出错
        }
    }
}
