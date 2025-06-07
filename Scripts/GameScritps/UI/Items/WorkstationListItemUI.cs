using QFramework;
using TMPro; // Ensure this is used
using UnityEngine;
using UnityEngine.UI;
using YourGameNamespace.Workstations;
using System;
using System.Collections.Generic; // For List<IUnRegister>

namespace YourGameNamespace.UI
{
    public class WorkstationListItemUI : MonoBehaviour // Removed IPoolable
    {
        public TextMeshProUGUI stationTypeText; // Changed to TextMeshProUGUI
        public TextMeshProUGUI assignedSurvivorsText; // Changed to TextMeshProUGUI
        public Slider productionProgressBar;
        public TextMeshProUGUI productionProgressText; // Changed to TextMeshProUGUI
        public Button manageButton;

        private Workstation mWorkstation;
        private WorkstationDisplay mParentDisplay;
        private List<IUnRegister> mUnregisters = new List<IUnRegister>();

        private void Awake()
        {
            stationTypeText = stationTypeText ?? transform.Find("StationTypeText")?.GetComponent<TextMeshProUGUI>();
            assignedSurvivorsText = assignedSurvivorsText ?? transform.Find("AssignedSurvivorsText")?.GetComponent<TextMeshProUGUI>();
            productionProgressBar = productionProgressBar ?? transform.Find("ProductionProgressBar")?.GetComponent<Slider>();
            productionProgressText = productionProgressText ?? transform.Find("ProductionProgressText")?.GetComponent<TextMeshProUGUI>();
            manageButton = manageButton ?? transform.Find("ManageButton")?.GetComponent<Button>();

            if (stationTypeText == null) Debug.LogError("WorkstationListItemUI: stationTypeText 未找到或未链接!");
            if (assignedSurvivorsText == null) Debug.LogError("WorkstationListItemUI: assignedSurvivorsText 未找到或未链接!");
        }

        public void Setup(Workstation workstation, WorkstationDisplay parentDisplay)
        {
            mWorkstation = workstation;
            mParentDisplay = parentDisplay;

            ClearBindings();

            if (mWorkstation == null)
            {
                if (stationTypeText) stationTypeText.text = "无效工作站";
                if (assignedSurvivorsText) assignedSurvivorsText.text = "人数: N/A";
                if (productionProgressBar) productionProgressBar.gameObject.SetActive(false);
                if (productionProgressText) productionProgressText.text = "";
                if (manageButton) manageButton.onClick.RemoveAllListeners();
                return;
            }

            if (productionProgressBar) productionProgressBar.gameObject.SetActive(true);

            if (stationTypeText) stationTypeText.text = $"类型: {GetLocalizedWorkstationType(mWorkstation.Type)}";

            mWorkstation.AssignedSurvivorCount.RegisterWithInitValue(UpdateAssignedSurvivorsUI)
                .UnRegisterWhenGameObjectDestroyed(this.gameObject)
                .AddTo(mUnregisters);

            mWorkstation.ProductionProgress.RegisterWithInitValue(UpdateProductionProgressUI)
                .UnRegisterWhenGameObjectDestroyed(this.gameObject)
                .AddTo(mUnregisters);

            if (manageButton != null)
            {
                manageButton.onClick.RemoveAllListeners();
                manageButton.onClick.AddListener(OnManageButtonClicked);
            }
        }

        private void UpdateAssignedSurvivorsUI(int currentCount)
        {
            if (assignedSurvivorsText != null && mWorkstation != null)
            {
                assignedSurvivorsText.text = $"人数: {currentCount}/{mWorkstation.MaxAssignedSurvivors}";
            }
        }

        private void UpdateProductionProgressUI(float progress)
        {
            if (mWorkstation == null) return;
            float normalizedProgress = 0.0f; // Ensured valid float literal
            if (mWorkstation.ProductionCycleTime > 0)
            {
                normalizedProgress = progress / mWorkstation.ProductionCycleTime;
            }
            else if (progress > 0)
            {
                normalizedProgress = 1.0f; // Ensured valid float literal
            }

            if (productionProgressBar != null) productionProgressBar.value = normalizedProgress;
            if (productionProgressText != null) productionProgressText.text = $"{normalizedProgress * 100:F0}%";
        }

        private void OnManageButtonClicked()
        {
            if (mWorkstation != null && mParentDisplay != null)
            {
                mParentDisplay.RequestAssignSurvivorToWorkstation(mWorkstation.Id);
                UnityEngine.Debug.Log($"请求管理工作站: {GetLocalizedWorkstationType(mWorkstation.Type)} (ID: {mWorkstation.Id})");
            }
        }

        private string GetLocalizedWorkstationType(WorkstationType type)
        {
            switch (type)
            {
                case WorkstationType.Farm: return "农场";
                case WorkstationType.PowerPlant: return "发电厂";
                case WorkstationType.Workshop: return "工坊";
                case WorkstationType.Clinic: return "诊所";
                case WorkstationType.ResearchLab: return "科研实验室";
                default: return type.ToString();
            }
        }

        private void ClearBindings()
        {
            foreach (var unregister in mUnregisters)
            {
                unregister.UnRegister();
            }
            mUnregisters.Clear();
        }

        public void OnRecycled()
        {
            ClearBindings();
            gameObject.SetActive(false);
        }
        public bool IsRecycled { get; set; }

        void OnDestroy()
        {
            ClearBindings();
        }
    }
}
