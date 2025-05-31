using System;
using UnityEngine;
using UnityEngine.UI;
using YourGameNamespace.Research;
using YourGameNamespace.Resources; // For ResourceModel if passed for cost checks

namespace YourGameNamespace.UI
{
    public class TechDisplayItem : MonoBehaviour
    {
        public Text nameText;
        public Text descriptionText;
        public Text statusText;
        public Button researchButton;
        public Slider progressBar; // Progress bar for this specific item if it's the one being researched

        private string mTechId;
        private Technology mTechnologyData; // Store the full tech data if needed for display updates
        private Action<string> mResearchRequestCallback;
        private IResearchSystem mResearchSystem; // Keep for progress if this item is current
        private ResourceModel mResourceModel;   // Keep for cost display/checks

        private void Awake()
        {
            nameText = nameText ?? transform.Find("NameText")?.GetComponent<Text>();
            descriptionText = descriptionText ?? transform.Find("DescriptionText")?.GetComponent<Text>();
            statusText = statusText ?? transform.Find("StatusText")?.GetComponent<Text>();
            researchButton = researchButton ?? transform.Find("ResearchButton")?.GetComponent<Button>();
            progressBar = progressBar ?? transform.Find("ProgressBar")?.GetComponent<Slider>();

            researchButton?.onClick.AddListener(HandleResearchButtonClick);
        }

        public void Setup(Technology technology, Action<string> researchRequestCallback, bool isAnotherResearchActive, IResearchSystem researchSystem, ResourceModel resourceModel)
        {
            mTechnologyData = technology;
            mTechId = technology.Id;
            mResearchRequestCallback = researchRequestCallback;
            mResearchSystem = researchSystem; // Store to get progress if this tech is current
            mResourceModel = resourceModel;   // Store for cost checks

            if (nameText != null) nameText.text = technology.Name; // Assumed localized
            if (descriptionText != null) descriptionText.text = technology.Description; // Assumed localized

            bool canAfford = mResourceModel.GetAmount(GameResourceType.ResearchPoints) >= technology.ResearchPointCost;

            if (technology.Status.Value == ResearchStatus.Available)
            {
                if (statusText != null) statusText.text = $"成本：{technology.ResearchPointCost} 研究点";
                if (researchButton != null)
                {
                    researchButton.gameObject.SetActive(true);
                    researchButton.interactable = !isAnotherResearchActive && canAfford;
                    if (isAnotherResearchActive)
                    {
                        if (statusText != null) statusText.text += " (其他研究进行中)";
                    }
                    else if (!canAfford)
                    {
                         if (statusText != null) statusText.text += " (研究点不足)";
                    }
                }
                if (progressBar != null) progressBar.gameObject.SetActive(false);
            }
            else if (technology.Status.Value == ResearchStatus.InProgress)
            {
                // This item is listed as "InProgress". This state typically applies to the *single* tech
                // being globally researched by the ResearchSystem.
                // The main ResearchDisplay progress bar will show the system's current research progress.
                // This item's progress bar should only show if *this specific tech* is the one
                // currently being researched by the system.
                bool isThisTechCurrentlyResearchedBySystem = mResearchSystem.CurrentlyResearching.Value != null && mResearchSystem.CurrentlyResearching.Value.Id == mTechId;

                if (statusText != null) statusText.text = "状态：研究中...";
                if (researchButton != null) researchButton.gameObject.SetActive(false);
                if (progressBar != null)
                {
                    progressBar.gameObject.SetActive(isThisTechCurrentlyResearchedBySystem);
                    if(isThisTechCurrentlyResearchedBySystem)
                    {
                        progressBar.value = mResearchSystem.CurrentResearchProgressNormalized.Value;
                    }
                }
            }
            else if (technology.Status.Value == ResearchStatus.Completed)
            {
                if (statusText != null) statusText.text = "状态：已完成";
                if (researchButton != null) researchButton.gameObject.SetActive(false);
                if (progressBar != null) progressBar.gameObject.SetActive(false);
            }
            else // Locked
            {
                if (statusText != null)
                {
                    string prereqs = technology.PrerequisiteTechIds.Count > 0 ?
                                     $" (前置技术：{string.Join("、", technology.PrerequisiteTechIds)})" :
                                     "";
                    statusText.text = $"状态：已锁定{prereqs}";
                }
                if (researchButton != null) researchButton.gameObject.SetActive(false);
                if (progressBar != null) progressBar.gameObject.SetActive(false);
            }
        }

        private void HandleResearchButtonClick()
        {
            if (mResearchRequestCallback != null && mTechnologyData != null)
            {
                // Optional: Re-check conditions here if needed, though button interactability should handle most cases.
                bool canAfford = mResourceModel.GetAmount(GameResourceType.ResearchPoints) >= mTechnologyData.ResearchPointCost;
                bool isResearchSystemBusy = mResearchSystem.IsResearching();

                if (mTechnologyData.Status.Value == ResearchStatus.Available && canAfford && !isResearchSystemBusy)
                {
                    Debug.Log($"TechDisplayItem: Requesting research for {mTechId}");
                    mResearchRequestCallback(mTechId);
                }
                else
                {
                    string reason = "";
                    if (mTechnologyData.Status.Value != ResearchStatus.Available) reason = "技术不是可研究状态。";
                    else if (isResearchSystemBusy) reason = "其他研究正在进行中。";
                    else if (!canAfford) reason = "研究点不足。";
                    Debug.LogWarning($"TechDisplayItem: Cannot request research for {mTechId}. Reason: {reason}");
                }
            }
        }
        
        public string GetTechnologyId() => mTechId;

        public bool Matches(Technology tech) => mTechId == tech?.Id;
    }
}
