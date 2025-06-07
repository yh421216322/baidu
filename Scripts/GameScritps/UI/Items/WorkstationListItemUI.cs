using QFramework;
// using TMPro; // Removed
using UnityEngine;
using UnityEngine.UI; // Ensure this is present for Text, Slider, Button
using YourGameNamespace.Workstations;
using System;
using System.Collections.Generic; // For List<IUnRegister>

namespace YourGameNamespace.UI
{
    public class WorkstationListItemUI : MonoBehaviour // Removed IPoolable
    {
        public Text stationTypeText; // Changed to Text
        public Text assignedSurvivorsText; // Changed to Text
        public Slider productionProgressBar;
        public Text productionProgressText; // Changed to Text
        public Button manageButton;

        private Workstation mWorkstation;
        private WorkstationDisplay mParentDisplay;
        // private List<IUnRegister> mUnregisters = new List<IUnRegister>(); // Removed

        private void Awake()
        {
            stationTypeText = stationTypeText ?? transform.Find("StationTypeText")?.GetComponent<Text>();
            assignedSurvivorsText = assignedSurvivorsText ?? transform.Find("AssignedSurvivorsText")?.GetComponent<Text>();
            productionProgressBar = productionProgressBar ?? transform.Find("ProductionProgressBar")?.GetComponent<Slider>();
            productionProgressText = productionProgressText ?? transform.Find("ProductionProgressText")?.GetComponent<Text>();
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
                .UnRegisterWhenGameObjectDestroyed(this.gameObject);

            mWorkstation.ProductionProgress.RegisterWithInitValue(UpdateProductionProgressUI)
                .UnRegisterWhenGameObjectDestroyed(this.gameObject);

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
            // foreach (var unregister in mUnregisters) // Removed
            // { // Removed
            //     unregister.UnRegister(); // Removed
            // } // Removed
            // mUnregisters.Clear(); // Removed
            // Existing listeners on mWorkstation properties are handled by UnRegisterWhenGameObjectDestroyed.
            // Button listeners are handled by direct RemoveAllListeners if needed (e.g. in OnRecycled or before re-adding).
        }

        public void OnRecycled()
        {
            // ClearBindings(); // Call to ClearBindings might be redundant if it only handled mUnregisters.
            // Specific cleanup for pooled objects:
            if (manageButton != null) manageButton.onClick.RemoveAllListeners();
            // Reset texts or other UI states if necessary
            if (stationTypeText) stationTypeText.text = "";
            if (assignedSurvivorsText) assignedSurvivorsText.text = "";
            if (productionProgressText) productionProgressText.text = "";
            if (productionProgressBar) productionProgressBar.value = 0;

            gameObject.SetActive(false);
        }
        public bool IsRecycled { get; set; }

        void OnDestroy()
        {
            // ClearBindings(); // UnRegisterWhenGameObjectDestroyed handles QF event/BindableProperty unregistrations.
            // Manual Unity UI event cleanup if not handled by OnRecycled (e.g., if not pooled but destroyed)
             if (manageButton != null && !IsRecycled) // Avoid double removal if OnRecycled was called by pool
            {
                 manageButton.onClick.RemoveAllListeners();
            }
        }
    }
}
