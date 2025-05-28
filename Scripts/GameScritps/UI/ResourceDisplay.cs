using UnityEngine;
using UnityEngine.UI; // Text 所需
using QFramework; // 访问 GameArchitecture 和模型所需
using YourGameNamespace; // GameResourceType 和 ResourceModel 所需

namespace YourGameNamespace.UI
{
    public class ResourceDisplay : MonoBehaviour
    {
        public Text foodText;
        public Text powerText;
        public Text ammoText;
        public Text medicineText;
        public Text researchPointsText; // 已添加 researchPointsText

        private ResourceModel resourceModel;

        void Start()
        {
            // 确保在访问模型前 GameArchitecture 已初始化
            // 这可能已由 GameInitializer 脚本完成
            if (GameArchitecture.Interface == null)
            {
                // 这是后备方案，理想情况下 GameInitializer 应首先运行
                var go = new GameObject("GameArchitectureInitializer_Fallback");
                go.AddComponent<GameInitializer>(); 
                Debug.LogWarning("GameArchitecture 未初始化。使用后备 GameInitializer 进行初始化。");
            }
            
            resourceModel = GameArchitecture.Interface.GetModel<ResourceModel>();

            if (resourceModel == null)
            {
                Debug.LogError("未找到资源模型 (ResourceModel)。请确保它已在 GameArchitecture 中注册。");
                this.enabled = false; // 如果模型丢失，则禁用此脚本
                return;
            }

            // 初始UI更新
            UpdateUI();
        }

        void Update()
        {
            // 持续更新UI（以后可以用事件进行优化）
            if (resourceModel != null)
            {
                UpdateUI();
            }
        }

        void UpdateUI()
        {
            if (foodText != null) foodText.text = "Food: " + resourceModel.GetAmount(GameResourceType.Food);
            if (powerText != null) powerText.text = "Power: " + resourceModel.GetAmount(GameResourceType.Power);
            if (ammoText != null) ammoText.text = "Ammo: " + resourceModel.GetAmount(GameResourceType.Ammo);
            if (medicineText != null) medicineText.text = "Medicine: " + resourceModel.GetAmount(GameResourceType.Medicine);
            if (researchPointsText != null) researchPointsText.text = "RP: " + resourceModel.GetAmount(GameResourceType.ResearchPoints); // 已添加
        }
    }
}
