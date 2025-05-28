using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using QFramework;
using YourGameNamespace.Survivors; // 如果需要，调整命名空间

namespace YourGameNamespace.UI
{
    public class SurvivorDisplay : MonoBehaviour
    {
        public Text survivorListText; // 在Unity检视面板中分配
        private SurvivorModel mSurvivorModel;

        void Start()
        {
            // 确保GameArchitecture已初始化
            if (GameArchitecture.Interface == null)
            {
                // 这是后备方案，理想情况下GameInitializer应首先运行
                var go = new GameObject("GameArchitectureInitializer_Fallback_SurvivorDisplay");
                go.AddComponent<GameInitializer>(); // 假设GameInitializer可访问
                Debug.LogWarning("SurvivorDisplay：GameArchitecture 未初始化。使用后备 GameInitializer 进行初始化。");
            }

            mSurvivorModel = GameArchitecture.Interface.GetModel<SurvivorModel>();
            if (mSurvivorModel == null)
            {
                Debug.LogError("未找到幸存者模型 (SurvivorModel)。请确保它已在 GameArchitecture 中注册。");
                if (survivorListText != null)
                {
                    survivorListText.text = "Error: SurvivorModel not found.";
                }
                this.enabled = false; // 如果模型丢失，则禁用此脚本
            }
        }

        void Update()
        {
            if (mSurvivorModel == null || survivorListText == null)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            List<Survivor> survivors = mSurvivorModel.GetAllSurvivors();

            sb.AppendLine("== Survivors ==");
            if (survivors.Count == 0)
            {
                sb.AppendLine("No survivors yet.");
            }
            else
            {
                foreach (Survivor survivor in survivors)
                {
                    sb.AppendLine($"- {survivor.Name} ({survivor.Profession}) | F:{survivor.FoodLevel:F0} R:{survivor.RestLevel:F0} | Str:{survivor.Attributes.Strength} Dex:{survivor.Attributes.Dexterity} Int:{survivor.Attributes.Intelligence} | Status: {survivor.Status}");
                }
            }
            survivorListText.text = sb.ToString();
        }
    }
}
