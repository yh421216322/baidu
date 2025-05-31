using UnityEngine;
using UnityEngine.UI; // 用于 Text UI 组件
using QFramework;     // 用于访问 GameArchitecture 和模型
using YourGameNamespace; // 用于 GameResourceType 和 GameArchitecture
using YourGameNamespace.Events; // 用于 ResourceChangedEvent
using YourGameNamespace.Resources; // Required for ResourceModel if it's in this namespace

namespace YourGameNamespace.UI
{
    // UI组件，负责在界面上显示各种资源的数量
    public class ResourceDisplay : MonoBehaviour, IController
    {
        // 在Unity检视面板中分配的UI Text组件
        public Text foodText;           // 用于显示食物数量
        public Text powerText;          // 用于显示电力数量
        public Text ammoText;           // 用于显示弹药数量
        public Text medicineText;       // 用于显示药品数量
        public Text researchPointsText; // 用于显示研究点数
        // 根据实际需要添加或移除其他资源类型的显示

        private ResourceModel mResourceModel; // 对资源数据模型的引用

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void Start() // Unity生命周期方法，在第一次Update前执行
        {
            // 确保在访问模型之前 GameArchitecture 已经初始化。
            if (GameArchitecture.Interface == null)
            {
                Debug.LogError("ResourceDisplay: GameArchitecture 尚未初始化！此UI组件可能无法正常工作。请确保 GameInitializer 先运行。");
                this.enabled = false;
                return;
            }
            
            // 从 GameArchitecture 获取资源数据模型的实例
            mResourceModel = this.GetModel<ResourceModel>();

            if (mResourceModel == null) // 如果未能获取到模型
            {
                Debug.LogError("ResourceDisplay: 未能获取到 ResourceModel！UI可能不会更新。");
                this.enabled = false; // 禁用此脚本组件以防止后续错误
                return;
            }

            // 注册对 ResourceChangedEvent 的监听
            this.RegisterEvent<ResourceChangedEvent>(OnResourceChanged).UnRegisterWhenGameObjectDestroyed(this);
            RefreshAllResourceTexts(); // 初始刷新
        }

        private void OnResourceChanged(ResourceChangedEvent e)
        {
            // 为确保所有数据一致且简单处理，直接刷新所有文本
            // 如果未来资源种类非常多，可以优化为只更新 e.Type 对应的文本
            RefreshAllResourceTexts();
        }

        private void RefreshAllResourceTexts()
        {
            if (mResourceModel == null) return;

            if (foodText != null) foodText.text = "食物: " + mResourceModel.GetAmount(GameResourceType.Food);
            if (powerText != null) powerText.text = "电力: " + mResourceModel.GetAmount(GameResourceType.Power);
            if (ammoText != null) ammoText.text = "弹药: " + mResourceModel.GetAmount(GameResourceType.Ammo);
            if (medicineText != null) medicineText.text = "药品: " + mResourceModel.GetAmount(GameResourceType.Medicine);
            if (researchPointsText != null) researchPointsText.text = "研究点: " + mResourceModel.GetAmount(GameResourceType.ResearchPoints);
            // 根据实际需要添加或移除其他资源类型的显示
        }

        // Update() 方法已移除，UI更新由事件驱动
    }
}
