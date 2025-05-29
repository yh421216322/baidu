using UnityEngine;
using UnityEngine.UI; // 用于 Text UI 组件
using System.Text;    // 用于 StringBuilder 高效构建字符串
using System.Collections.Generic; // 用于 List
using QFramework;     // QFramework 框架
using YourGameNamespace.Workstations; // 用于 WorkstationModel, Workstation 等工作站相关类
using YourGameNamespace.Survivors;   // 用于 SurvivorModel, Survivor 等幸存者相关类

namespace YourGameNamespace.UI
{
    // UI组件，用于显示当前所有工作站及其状态的列表
    public class WorkstationDisplay : MonoBehaviour
    {
        public Text workstationListText; // 在Unity检视面板中分配此Text组件，用于显示工作站列表信息
        private WorkstationModel mWorkstationModel; // 对工作站数据模型的引用
        private SurvivorModel mSurvivorModel;     // 对幸存者数据模型的引用 (用于获取已分配幸存者的名称)

        void Start() // Unity生命周期方法，在第一次Update前执行
        {
            // 确保 GameArchitecture 已经初始化。
            // 这是获取模型和系统的先决条件。
            if (GameArchitecture.Interface == null)
            {
                // 后备措施：如果 GameArchitecture 未初始化，尝试动态添加 GameInitializer。
                // 理想情况下，场景中应有唯一的 GameInitializer 确保架构及时初始化。
                var go = new GameObject("GameArchitectureInitializer_Fallback_WorkstationDisplay");
                go.AddComponent<GameInitializer>(); 
                Debug.LogWarning("工作站显示 (WorkstationDisplay)：GameArchitecture 尚未初始化。正在尝试使用后备 GameInitializer 进行初始化。请检查场景设置或脚本执行顺序。");
            }

            // 从 GameArchitecture 获取所需数据模型的实例
            mWorkstationModel = GameArchitecture.Interface.GetModel<WorkstationModel>();
            mSurvivorModel = GameArchitecture.Interface.GetModel<SurvivorModel>();

            // 检查模型是否成功获取
            if (mWorkstationModel == null)
            {
                Debug.LogError("工作站显示 (WorkstationDisplay)：未能找到工作站数据模型 (WorkstationModel)。请确保该模型已在 GameArchitecture 中正确注册。此UI组件将无法正常工作。");
                if (workstationListText != null) workstationListText.text = "错误：未找到工作站数据模型 (WorkstationModel)。";
                // 可选：如果关键模型丢失，可以禁用此脚本组件或其部分功能以防止后续错误。
                // this.enabled = false; 
            }
            if (mSurvivorModel == null)
            {
                Debug.LogError("工作站显示 (WorkstationDisplay)：未能找到幸存者数据模型 (SurvivorModel)。请确保该模型已在 GameArchitecture 中正确注册。");
                if (workstationListText != null) workstationListText.text += "\n错误：未找到幸存者数据模型 (SurvivorModel)。";
                // 可选：禁用脚本或其部分功能
            }
        }

        void Update() // Unity生命周期方法，每帧调用一次
        {
            // 安全检查，如果模型或UI Text组件不存在，则不执行更新逻辑
            if (mWorkstationModel == null || mSurvivorModel == null || workstationListText == null)
            {
                return;
            }

            StringBuilder sb = new StringBuilder(); // 使用 StringBuilder 高效构建显示字符串
            sb.AppendLine("=== 工作站列表 ==="); // 列表标题
            List<Workstation> workstations = mWorkstationModel.GetAllWorkstations(); // 获取所有工作站

            if (workstations.Count == 0) // 如果当前没有已建造的工作站
            {
                sb.AppendLine("尚未建造任何工作站。"); // 显示提示信息
            }
            else
            {
                // 遍历所有工作站，格式化并添加其信息到StringBuilder
                foreach (var station in workstations)
                {
                    // 注意：station.Type.ToString() 会返回枚举成员的名称 (例如 "Farm")。
                    // 如果需要显示本地化的工作站类型名称 (例如 "农场")，
                    // 则需要一个转换机制或在 WorkstationType 枚举的扩展方法中处理。
                    sb.AppendLine($"- {station.Type} (ID: {station.Id.ToString().Substring(0,4)}) 生产进度: {station.ProductionProgress:F1}/{station.ProductionCycleTime:F1}秒");
                    sb.Append("  已分配幸存者: ");
                    if (station.AssignedSurvivorIds.Count == 0) // 如果没有幸存者被分配到此工作站
                    {
                        sb.Append("无");
                    }
                    else
                    {
                        // 列出所有已分配的幸存者名称
                        for (int i = 0; i < station.AssignedSurvivorIds.Count; i++)
                        {
                            var survivorId = station.AssignedSurvivorIds[i];
                            var survivor = mSurvivorModel.GetSurvivorById(survivorId);
                            // 如果能找到幸存者，则显示其名称；否则显示未知及ID（截断部分）
                            sb.Append(survivor != null ? survivor.Name : $"未知 (ID:{survivorId.ToString().Substring(0,4)})");
                            if (i < station.AssignedSurvivorIds.Count - 1) sb.Append("、"); // 如果不是最后一个，则添加逗号分隔符
                        }
                    }
                    sb.AppendLine(); // 添加换行符，准备下一条工作站信息
                }
            }
            workstationListText.text = sb.ToString(); // 更新UI Text组件的文本内容
        }
    }
}
