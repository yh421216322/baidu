using QFramework;
// using TMPro; // Removed
using UnityEngine;
using UnityEngine.UI;
using YourGameNamespace.Workstations;
using System;
using System.Collections.Generic;

namespace YourGameNamespace.UI
{
    public class WorkstationListItemUI : MonoBehaviour
    {
        // --- UI Element References (to be linked in Unity Editor or found in Awake) ---
        public Text stationTypeText;
        public Text assignedSurvivorsText;
        public Slider productionProgressBar;
        public Text productionProgressText;
        public Button manageButton;

        // --- Private fields ---
        private Workstation mWorkstation;
        private WorkstationDisplay mParentDisplay;
        private List<IUnRegister> mUnregisters = new List<IUnRegister>();

        private void Awake()
        {
            stationTypeText = stationTypeText ?? transform.Find("StationTypeText")?.GetComponent<Text>();
            assignedSurvivorsText = assignedSurvivorsText ?? transform.Find("AssignedSurvivorsText")?.GetComponent<Text>();
            productionProgressBar = productionProgressBar ?? transform.Find("ProductionProgressBar")?.GetComponent<Slider>();
            productionProgressText = productionProgressText ?? transform.Find("ProductionProgressText")?.GetComponent<Text>();
            manageButton = manageButton ?? transform.Find("ManageButton")?.GetComponent<Button>();

            if (stationTypeText == null) Debug.LogError("WorkstationListItemUI: StationTypeText not found or linked.");
            if (assignedSurvivorsText == null) Debug.LogError("WorkstationListItemUI: AssignedSurvivorsText not found or linked.");
            if (productionProgressBar == null) Debug.LogError("WorkstationListItemUI: ProductionProgressBar not found or linked.");
            if (productionProgressText == null) Debug.LogError("WorkstationListItemUI: ProductionProgressText not found or linked.");
            if (manageButton == null) Debug.LogError("WorkstationListItemUI: ManageButton not found or linked.");
        }

        public void Setup(Workstation workstation, WorkstationDisplay parentDisplay)
        {
            mWorkstation = workstation;
            mParentDisplay = parentDisplay;

            ClearBindings();

            if (mWorkstation == null)
            {
                if (stationTypeText) stationTypeText.text = "N/A";
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

        private void UpdateAssignedSurvivorsUI(int count)
        {
            if (assignedSurvivorsText != null) assignedSurvivorsText.text = $"人数: {count}";
        }

        private void UpdateProductionProgressUI(float progress)
        {
            if (mWorkstation == null) return;
            float normalizedProgress = 0f;
            if (mWorkstation.ProductionCycleTime > 0)
            {
                normalizedProgress = progress / mWorkstation.ProductionCycleTime;
            }
            else if (progress > 0)
            {
                normalizedProgress = 1f;
            }

            if (productionProgressBar != null) productionProgressBar.value = normalizedProgress;
            if (productionProgressText != null) productionProgressText.text = $"{normalizedProgress * 100:F0}%";
        }

        private void OnManageButtonClicked()
        {
            if (mWorkstation != null && mParentDisplay != null)
            {
                mParentDisplay.RequestAssignSurvivorToWorkstation(mWorkstation.Id);
                UnityEngine.Debug.Log($"请求管理工作站: {mWorkstation.Type} (ID: {mWorkstation.Id})");
            }
        }

        private string GetLocalizedWorkstationType(WorkstationType type)
        {
            return type.ToString();
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
