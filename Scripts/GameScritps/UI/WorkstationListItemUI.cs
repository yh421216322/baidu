using QFramework;
using TMPro; // 使用 TextMeshPro
using UnityEngine;
using UnityEngine.UI; // 用于 Button, Slider 等
using YourGameNamespace.Workstations; // For Workstation, WorkstationType
using System; // For Guid
using System.Collections.Generic; // For List<IUnRegister>

namespace YourGameNamespace.UI
{
    public class WorkstationListItemUI : MonoBehaviour
    {
        // --- UI Element References (需要在Unity编辑器中链接) ---
        public TextMeshProUGUI stationTypeText;         // 显示工作站类型
        public TextMeshProUGUI assignedSurvivorsText;   // 显示已分配幸存者数量/上限
        public Slider productionProgressSlider;     // 显示生产进度
        public TextMeshProUGUI productionProgressText;  // 显示具体生产进度百分比或时间
        public Button manageButton;                 // （示例）管理/分配幸存者按钮

        // --- Private fields ---
        private Workstation mWorkstation;
        private WorkstationDisplay mParentDisplay; // 父级Display的引用，用于回调
        private List<IUnRegister> mUnregisters = new List<IUnRegister>();

        public void Setup(Workstation workstation, WorkstationDisplay parentDisplay)
        {
            mWorkstation = workstation;
            mParentDisplay = parentDisplay;

            ClearBindings();

            if (mWorkstation == null)
            {
                // 处理空数据情况
                stationTypeText.text = "N/A";
                assignedSurvivorsText.text = "人数: -/-";
                if (productionProgressSlider) productionProgressSlider.gameObject.SetActive(false);
                if (productionProgressText) productionProgressText.text = "";
                if (manageButton) manageButton.gameObject.SetActive(false);
                return;
            }

            if (productionProgressSlider) productionProgressSlider.gameObject.SetActive(true);
            if (manageButton) manageButton.gameObject.SetActive(true);

            // 绑定到Workstation的BindableProperties
            // 直接显示类型，不需要绑定，因为类型通常不变
            if (stationTypeText != null) stationTypeText.text = $"类型: {GetLocalizedWorkstationType(mWorkstation.Type)}"; // 本地化

            mWorkstation.AssignedSurvivorCount.RegisterWithInit(UpdateAssignedSurvivorsUI).AddTo(mUnregisters);
            mWorkstation.ProductionProgress.RegisterWithInit(UpdateProductionProgressUI).AddTo(mUnregisters);

            // 设置按钮回调
            if (manageButton != null)
            {
                manageButton.onClick.RemoveAllListeners(); // 清除旧监听器
                manageButton.onClick.AddListener(OnManageButtonClicked);
            }
        }

        private void UpdateAssignedSurvivorsUI(int count)
        {
            if (assignedSurvivorsText != null)
            {
                // 假设工作站有最大容量的概念，这里暂时硬编码一个示例值，实际应从mWorkstation.MaxCapacity等获取
                int maxCapacity = GetMaxCapacityForWorkstation(mWorkstation.Type); // 需要一个辅助方法
                assignedSurvivorsText.text = $"人数: {count}/{maxCapacity}"; // 本地化
            }
        }

        private void UpdateProductionProgressUI(float progress)
        {
            if (productionProgressSlider != null)
            {
                if (mWorkstation.ProductionCycleTime > 0)
                {
                    productionProgressSlider.value = progress / mWorkstation.ProductionCycleTime;
                }
                else
                {
                    productionProgressSlider.value = 0;
                }
            }
            if (productionProgressText != null)
            {
                 if (mWorkstation.ProductionCycleTime > 0)
                {
                    productionProgressText.text = $"{(progress / mWorkstation.ProductionCycleTime) * 100:F0}%";
                }
                else
                {
                    productionProgressText.text = "N/A";
                }
            }
        }

        private void OnManageButtonClicked()
        {
            if (mWorkstation != null && mParentDisplay != null)
            {
                // 通知父级Display，用户想要管理这个工作站（例如分配幸存者）
                // 父级Display会处理具体的逻辑，比如打开一个幸存者选择面板，然后发送Command
                mParentDisplay.RequestAssignSurvivorToWorkstation(mWorkstation.Id);
                UnityEngine.Debug.Log($"请求管理工作站: {mWorkstation.Type} (ID: {mWorkstation.Id})");
            }
        }

        // 辅助方法：获取不同类型工作站的最大容量 (示例)
        private int GetMaxCapacityForWorkstation(WorkstationType type)
        {
            // 这个数据应该从配置或者Workstation自身属性获取
            switch (type)
            {
                case WorkstationType.Farm: return 3;
                case WorkstationType.PowerPlant: return 2;
                case WorkstationType.Workshop: return 1;
                case WorkstationType.Clinic: return 1;
                case WorkstationType.ResearchLab: return 1;
                default: return 1;
            }
        }

        private string GetLocalizedWorkstationType(WorkstationType type)
        {
            // 此处应有工作站类型的本地化逻辑
            return type.ToString(); // 暂时返回枚举名
        }

        private void ClearBindings()
        {
            foreach (var unregister in mUnregisters)
            {
                unregister.UnRegister();
            }
            mUnregisters.Clear();
        }

        private void OnDestroy()
        {
            ClearBindings();
        }
    }
}
