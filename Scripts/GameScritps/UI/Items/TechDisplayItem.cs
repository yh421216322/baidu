using System;
using UnityEngine;
using UnityEngine.UI; // Still needed for Button, Slider
using YourGameNamespace.Research;
using YourGameNamespace.Resources;
using QFramework;
using System.Collections.Generic;
using System.Linq;
using TMPro; // Added TMPro

namespace YourGameNamespace.UI
{
    public class TechDisplayItem : MonoBehaviour
    {
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI statusText;
        public Button researchButton;
        public Slider progressBar;
        public TextMeshProUGUI feedbackText;

        private string mTechId;
        private Technology mTechnology;
        private Action<string> mResearchRequestCallback;
        private IResearchSystem mResearchSystemRef;
        private ResourceModel mResourceModelRef;
        private bool mIsAnotherResearchActiveRef;

        private List<IUnRegister> mTechItemUnregisters = new List<IUnRegister>();

        private void Awake()
        {
            nameText = nameText ?? transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            descriptionText = descriptionText ?? transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            statusText = statusText ?? transform.Find("StatusText")?.GetComponent<TextMeshProUGUI>();
            researchButton = researchButton ?? transform.Find("ResearchButton")?.GetComponent<Button>();
            progressBar = progressBar ?? transform.Find("ProgressBar")?.GetComponent<Slider>();
            feedbackText = feedbackText ?? transform.Find("FeedbackText")?.GetComponent<TextMeshProUGUI>();

            if (nameText == null) Debug.LogError("TechDisplayItem: NameText not found or linked.");
            if (descriptionText == null) Debug.LogError("TechDisplayItem: DescriptionText not found or linked.");
            if (statusText == null) Debug.LogError("TechDisplayItem: StatusText not found or linked.");
            if (researchButton == null) Debug.LogError("TechDisplayItem: ResearchButton not found or linked.");
            // ProgressBar and FeedbackText can be optional, so warnings are fine.
            if (progressBar == null) Debug.LogWarning("TechDisplayItem: ProgressBar (Slider) not found or linked.");
            if (feedbackText == null) Debug.LogWarning("TechDisplayItem: FeedbackText (TextMeshProUGUI) not found or linked.");
        }

        public void Setup(Technology technology, Action<string> researchRequestCallback, bool isAnotherResearchActive, IResearchSystem researchSystem, ResourceModel resourceModel)
        {
            ClearPreviousBindings();

            mTechnology = technology;
            mTechId = technology.Id;
            mResearchRequestCallback = researchRequestCallback;
            mResearchSystemRef = researchSystem;
            mResourceModelRef = resourceModel;
            mIsAnotherResearchActiveRef = isAnotherResearchActive;

            if (nameText != null) nameText.text = mTechnology.Name; // Assuming Name is BindableProperty<string>
            if (descriptionText != null) descriptionText.text = mTechnology.Description; // Assuming Description is BindableProperty<string>


            mTechnology.Status.RegisterWithInitValue(UpdateUIBasedOnStatus)
                .UnRegisterWhenGameObjectDestroyed(this.gameObject);
            // Add listener for the research button
            if (researchButton != null)
            {
                researchButton.onClick.RemoveAllListeners(); // Crucial for items that might be reused by object pools
                researchButton.onClick.AddListener(HandleResearchButtonClick);
            }
         

            if (mResearchSystemRef != null && mResearchSystemRef.CurrentlyResearching.Value == mTechnology)
            {
                mResearchSystemRef.CurrentResearchProgressNormalized.RegisterWithInitValue(UpdateProgressBar).UnRegisterWhenGameObjectDestroyed(this.gameObject);
              
            }
            else if (progressBar != null)
            {
                 progressBar.gameObject.SetActive(false);
                 progressBar.value = 0;
            }
        }

        private void UpdateUIBasedOnStatus(ResearchStatus newStatus)
        {
            if (mTechnology == null || statusText == null) return;

            if (feedbackText != null) feedbackText.text = ""; // Clear previous feedback

            bool canAfford = mResourceModelRef.HasEnough(GameResourceType.ResearchPoints, mTechnology.ResearchPointCost);

            // Default button state
            TextMeshProUGUI buttonText = researchButton?.GetComponentInChildren<TextMeshProUGUI>();
            if(researchButton) researchButton.gameObject.SetActive(true); // Default to visible, then hide if needed
            if(researchButton) researchButton.interactable = false; // Default to not interactable
            if(buttonText) buttonText.text = "研究"; // Default text

            if (progressBar != null) progressBar.gameObject.SetActive(false); // Default to hidden

            switch (newStatus)
            {
                case ResearchStatus.Available:
                    statusText.text = $"状态：可研究 (成本：{mTechnology.ResearchPointCost}点)";
                    if(researchButton) researchButton.interactable = !mIsAnotherResearchActiveRef && canAfford;
                    if(buttonText) buttonText.text = "研究";
                    if (!canAfford)
                    {
                        AppendFeedbackText("研究点不足");
                    }
                    else if (mIsAnotherResearchActiveRef)
                    {
                        AppendFeedbackText("其他研究进行中");
                    }
                    break;
                case ResearchStatus.InProgress:
                    statusText.text = "状态：研究中...";
                    if(buttonText) buttonText.text = "研究中";
                    if (mResearchSystemRef != null && mResearchSystemRef.CurrentlyResearching.Value == mTechnology)
                    {
                        if (progressBar != null) progressBar.gameObject.SetActive(true);
                    }
                    break;
                case ResearchStatus.Completed:
                    statusText.text = "状态：已完成";
                     if(buttonText) buttonText.text = "已完成";
                    if (progressBar != null) // Show completed progress bar at 100%
                    {
                        progressBar.gameObject.SetActive(true);
                        progressBar.value = 1f;
                    }
                    break;
                case ResearchStatus.Locked:
                    statusText.text = "状态：已锁定";
                    if(buttonText) buttonText.text = "已锁定";
                    if (mTechnology.PrerequisiteTechIds.Count > 0 && mResearchSystemRef != null)
                    {
                        string prereqs = string.Join("、 ", mTechnology.PrerequisiteTechIds
                            .Select(id => mResearchSystemRef.GetTechnologyNameById(id) ?? $"未知ID:{id.Substring(0,4)}"));
                        AppendFeedbackText($"前置条件未满足: {prereqs}");
                    } else if (mTechnology.PrerequisiteTechIds.Count > 0) {
                        AppendFeedbackText($"前置条件未满足");
                    }
                     if(researchButton) researchButton.gameObject.SetActive(false); // Hide button for locked
                    break;
                default:
                    statusText.text = $"状态：{newStatus}";
                    if(researchButton) researchButton.gameObject.SetActive(false);
                    break;
            }
        }

        private void UpdateProgressBar(float normalizedProgress)
        {
            if (progressBar != null && mTechnology != null && mResearchSystemRef != null && mTechnology == mResearchSystemRef.CurrentlyResearching.Value)
            {
                progressBar.value = normalizedProgress;
                // Visibility is handled by UpdateUIBasedOnStatus
            }
            // else if (progressBar != null) // This case is mostly handled by UpdateUIBasedOnStatus hiding progress bar
            // {
            //    progressBar.gameObject.SetActive(false);
            // }
        }

        private void AppendFeedbackText(string message)
        {
            if (feedbackText != null)
            {
                if (string.IsNullOrEmpty(feedbackText.text)) feedbackText.text = message;
                else feedbackText.text += $"; {message}";
            }
            else if (statusText != null)
            {
                 statusText.text += $" ({message})";
            }
        }

        private void HandleResearchButtonClick()
        {
            if (mResearchRequestCallback != null && mTechnology != null)
            {
                bool canAfford = mResourceModelRef.HasEnough(GameResourceType.ResearchPoints, mTechnology.ResearchPointCost);
                bool isSystemCurrentlyResearching = mResearchSystemRef.IsResearching();

                if (mTechnology.Status.Value == ResearchStatus.Available && canAfford && !isSystemCurrentlyResearching)
                {
                    Debug.Log($"TechDisplayItem: Requesting research for {mTechId}");
                    mResearchRequestCallback(mTechId);
                }
                else
                {
                    string reason = "";
                    if (mTechnology.Status.Value != ResearchStatus.Available) reason = "技术不是可研究状态。";
                    else if (isSystemCurrentlyResearching) reason = "其他研究正在进行中。";
                    else if (!canAfford) reason = "研究点不足。";
                    Debug.LogWarning($"TechDisplayItem: Cannot request research for {mTechId}. Reason: {reason}");
                }
            }
        }

        private void ClearPreviousBindings()
        {
            foreach (var unregister in mTechItemUnregisters)
            {
                unregister.UnRegister();
            }
            mTechItemUnregisters.Clear();
        }

        void OnDestroy()
        {
            ClearPreviousBindings();
        }

        public string GetTechnologyId() => mTechId;
        public bool Matches(Technology tech) => mTechId == tech?.Id;
    }
}
