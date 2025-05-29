using UnityEngine;
using UnityEngine.UI; // 用于 Text UI 组件
using QFramework;     // 用于访问 GameArchitecture 和模型
using YourGameNamespace; // 用于 GameResourceType 和 ResourceModel

namespace YourGameNamespace.UI
{
    // UI组件，负责在界面上显示各种资源的数量
    public class ResourceDisplay : MonoBehaviour
    {
        // 在Unity检视面板中分配的UI Text组件
        public Text foodText;           // 用于显示食物数量
        public Text powerText;          // 用于显示电力数量
        public Text ammoText;           // 用于显示弹药数量
        public Text medicineText;       // 用于显示药品数量
        public Text researchPointsText; // 用于显示研究点数 (已添加)
        // 注意：脚本中还有一个未在此处声明但在Awake中查找的 electronicPartsText，为保持一致性，此处不添加，依赖Awake中的查找。

        private ResourceModel resourceModel; // 对资源数据模型的引用

        void Start() // Unity生命周期方法，在第一次Update前执行
        {
            // 确保在访问模型之前 GameArchitecture 已经初始化。
            // 通常，这应该由场景中存在的 GameInitializer 脚本来保证。
            if (GameArchitecture.Interface == null)
            {
                // 这是一种后备措施，理想情况下 GameInitializer 应确保架构在任何UI脚本的Start方法之前已准备就绪。
                var go = new GameObject("GameArchitectureInitializer_Fallback"); // 创建一个临时的GameObject
                go.AddComponent<GameInitializer>(); // 动态添加GameInitializer组件以触发初始化
                Debug.LogWarning("资源显示 (ResourceDisplay)：GameArchitecture 尚未初始化。正在尝试使用后备 GameInitializer 进行初始化。这可能表明场景设置或脚本执行顺序存在问题。");
            }
            
            // 从 GameArchitecture 获取资源数据模型的实例
            resourceModel = GameArchitecture.Interface.GetModel<ResourceModel>();

            if (resourceModel == null) // 如果未能获取到模型
            {
                Debug.LogError("资源显示 (ResourceDisplay)：未能找到资源数据模型 (ResourceModel)。请确保该模型已在 GameArchitecture 中正确注册。此UI组件将无法正常工作。");
                this.enabled = false; // 禁用此脚本组件以防止后续错误
                return;
            }

            // 组件启动时执行一次初始UI更新
            UpdateUI();
        }

        void Update() // Unity生命周期方法，每帧调用一次
        {
            // 持续更新UI以反映资源数量的变化。
            // 注意：为了优化性能，对于频繁变化的数值，更推荐使用事件驱动的方式来更新UI，
            // 而不是在Update中每帧轮询。例如，ResourceModel可以在资源数量变化时发送一个事件，
            // ResourceDisplay监听此事件并仅在接收到事件时更新UI。
            if (resourceModel != null)
            {
                UpdateUI();
            }
        }

        // 更新所有资源相关的UI Text组件的显示内容
        void UpdateUI()
        {
            // 为每种资源类型更新对应的Text组件
            // ?. 安全操作符用于避免在Text组件未分配时产生空引用异常
            foodText?.SetText($"食物: {resourceModel.GetAmount(GameResourceType.Food)}");
            powerText?.SetText($"电力: {resourceModel.GetAmount(GameResourceType.Power)}");
            ammoText?.SetText($"弹药: {resourceModel.GetAmount(GameResourceType.Ammo)}");
            medicineText?.SetText($"药品: {resourceModel.GetAmount(GameResourceType.Medicine)}");
            researchPointsText?.SetText($"研究点: {resourceModel.GetAmount(GameResourceType.ResearchPoints)}"); 
            // 假设 electronicPartsText 也会在这里更新，如果它是在Awake中被正确查找和赋值的话。
            // 例如: electronicPartsText?.SetText($"电子零件: {resourceModel.GetAmount(GameResourceType.ElectronicParts)}");
        }
    }
}
