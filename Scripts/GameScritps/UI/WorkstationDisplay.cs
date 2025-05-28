using UnityEngine;
using UnityEngine.UI;
using System.Text;
using System.Collections.Generic;
using QFramework;
using YourGameNamespace.Workstations; // 用于 WorkstationModel, Workstation
using YourGameNamespace.Survivors;   // 用于 SurvivorModel, Survivor

namespace YourGameNamespace.UI
{
    public class WorkstationDisplay : MonoBehaviour
    {
        public Text workstationListText; // 在Unity检视面板中分配
        private WorkstationModel mWorkstationModel;
        private SurvivorModel mSurvivorModel;

        void Start()
        {
            // 确保GameArchitecture已初始化
            if (GameArchitecture.Interface == null)
            {
                var go = new GameObject("GameArchitectureInitializer_Fallback_WorkstationDisplay");
                go.AddComponent<GameInitializer>(); 
                Debug.LogWarning("WorkstationDisplay：GameArchitecture 未初始化。使用后备 GameInitializer 进行初始化。");
            }

            mWorkstationModel = GameArchitecture.Interface.GetModel<WorkstationModel>();
            mSurvivorModel = GameArchitecture.Interface.GetModel<SurvivorModel>();

            if (mWorkstationModel == null)
            {
                Debug.LogError("未找到工作站模型 (WorkstationModel)。请确保它已在 GameArchitecture 中注册。");
                if (workstationListText != null) workstationListText.text = "Error: WorkstationModel not found.";
                // 可选：禁用脚本或其部分功能
            }
            if (mSurvivorModel == null)
            {
                Debug.LogError("未找到幸存者模型 (SurvivorModel)。请确保它已在 GameArchitecture 中注册。");
                if (workstationListText != null) workstationListText.text += "\nError: SurvivorModel not found.";
                // 可选：禁用脚本或其部分功能
            }
        }

        void Update()
        {
            if (mWorkstationModel == null || mSurvivorModel == null || workstationListText == null)
            {
                return;
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("== Workstations ==");
            List<Workstation> workstations = mWorkstationModel.GetAllWorkstations();

            if (workstations.Count == 0)
            {
                sb.AppendLine("No workstations built yet.");
            }
            else
            {
                foreach (var station in workstations)
                {
                    sb.AppendLine($"- {station.Type} (ID: {station.Id.ToString().Substring(0,4)}) Progress: {station.ProductionProgress:F1}/{station.ProductionCycleTime:F1}s");
                    sb.Append("  Assigned: ");
                    if (station.AssignedSurvivorIds.Count == 0)
                    {
                        sb.Append("None");
                    }
                    else
                    {
                        for (int i = 0; i < station.AssignedSurvivorIds.Count; i++)
                        {
                            var survivorId = station.AssignedSurvivorIds[i];
                            var survivor = mSurvivorModel.GetSurvivorById(survivorId);
                            sb.Append(survivor != null ? survivor.Name : $"Unknown (ID:{survivorId.ToString().Substring(0,4)})");
                            if (i < station.AssignedSurvivorIds.Count - 1) sb.Append(", ");
                        }
                    }
                    sb.AppendLine();
                }
            }
            workstationListText.text = sb.ToString();
        }
    }
}
