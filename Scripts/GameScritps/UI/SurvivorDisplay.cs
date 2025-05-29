using UnityEngine;
using UnityEngine.UI; // 用于 Text UI 组件
using System.Text;    // 用于 StringBuilder 高效构建字符串
using System.Collections.Generic; // 用于 List
using QFramework;     // QFramework 框架
using YourGameNamespace.Survivors; // 引入幸存者模块的命名空间，如果需要，请根据实际项目调整

namespace YourGameNamespace.UI
{
    // UI组件，用于显示幸存者列表及其详细信息
    public class SurvivorDisplay : MonoBehaviour
    {
        public Text survivorListText; // 在Unity检视面板中分配此Text组件，用于显示幸存者列表
        private SurvivorModel mSurvivorModel; // 对幸存者数据模型的引用

        void Start() // Unity生命周期方法，在第一次Update前执行
        {
            // 确保 GameArchitecture 已经初始化。
            // 这是获取模型和系统的先决条件。
            if (GameArchitecture.Interface == null)
            {
                // 这是一种后备措施。理想情况下，应由场景中唯一的 GameInitializer 脚本来确保架构的及时初始化。
                // 如果执行到这里，说明 GameInitializer 可能缺失或执行顺序有问题。
                var go = new GameObject("GameArchitectureInitializer_Fallback_SurvivorDisplay"); // 创建一个临时的GameObject
                go.AddComponent<GameInitializer>(); // 动态添加GameInitializer组件（假设GameInitializer脚本可访问且能正确初始化）
                Debug.LogWarning("幸存者显示 (SurvivorDisplay)：GameArchitecture 尚未初始化。正在尝试使用后备 GameInitializer 进行初始化。请检查场景设置或脚本执行顺序。");
            }

            // 从 GameArchitecture 获取幸存者数据模型的实例
            mSurvivorModel = GameArchitecture.Interface.GetModel<SurvivorModel>();
            if (mSurvivorModel == null) // 如果未能获取到模型
            {
                Debug.LogError("幸存者显示 (SurvivorDisplay)：未能找到幸存者数据模型 (SurvivorModel)。请确保该模型已在 GameArchitecture 中正确注册。此UI组件将无法正常工作。");
                if (survivorListText != null)
                {
                    survivorListText.text = "错误：未找到幸存者数据模型 (SurvivorModel)。"; // 在UI上显示错误信息
                }
                this.enabled = false; // 禁用此脚本组件以防止后续错误
            }
        }

        void Update() // Unity生命周期方法，每帧调用一次
        {
            // 安全检查，如果模型或UI Text组件不存在，则不执行更新逻辑
            if (mSurvivorModel == null || survivorListText == null)
            {
                return;
            }

            StringBuilder sb = new StringBuilder(); // 使用 StringBuilder 来高效构建最终的显示字符串
            List<Survivor> survivors = mSurvivorModel.GetAllSurvivors(); // 获取所有幸存者的列表

            sb.AppendLine("=== 幸存者列表 ==="); // 列表标题
            if (survivors.Count == 0) // 如果当前没有幸存者
            {
                sb.AppendLine("目前还没有幸存者。"); // 显示提示信息
            }
            else
            {
                // 遍历所有幸存者，格式化并添加其信息到StringBuilder
                foreach (Survivor survivor in survivors)
                {
                    // 注意：survivor.Name, survivor.Profession, survivor.Status 等属性的显示值如果需要翻译，
                    // 应当在其各自的枚举或数据类中处理，或者通过一个专门的本地化服务获取。
                    // 此处假设它们能直接提供或转换为适合显示的（可能是英文或已本地化的）字符串。
                    // F0 表示将浮点数格式化为无小数的整数。
                    sb.AppendLine($"- {survivor.Name} ({survivor.Profession}) | 食物:{survivor.FoodLevel:F0} 休息:{survivor.RestLevel:F0} | 力量:{survivor.Attributes.Strength} 敏捷:{survivor.Attributes.Dexterity} 智力:{survivor.Attributes.Intelligence} | 状态: {survivor.Status}");
                }
            }
            survivorListText.text = sb.ToString(); // 更新UI Text组件的文本内容
        }
    }
}
