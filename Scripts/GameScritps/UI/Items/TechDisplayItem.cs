using System;
using UnityEngine;
using UnityEngine.UI; // Should use Text, not TextMeshProUGUI
using YourGameNamespace.Research;
using YourGameNamespace.Resources;
using QFramework;
using System.Collections.Generic;
using System.Linq;

namespace YourGameNamespace.UI
{
    public class TechDisplayItem : MonoBehaviour
    {
        public Text nameText; // Changed from TextMeshProUGUI
        public Text descriptionText; // Changed from TextMeshProUGUI
        public Text statusText; // Changed from TextMeshProUGUI
        public Button researchButton;
        public Slider progressBar;
        public Text feedbackText; // Changed from TextMeshProUGUI

        private string mTechId;
        private Technology mTechnology;
        private Action<string> mResearchRequestCallback;
        private IResearchSystem mResearchSystemRef;
        private ResourceModel mResourceModelRef;
        private bool mIsAnotherResearchActiveRef;

        private List<IUnRegister> mTechItemUnregisters = new List<IUnRegister>();

        private void Awake()
        {
            nameText = nameText ?? transform.Find("NameText")?.GetComponent<Text>();
            descriptionText = descriptionText ?? transform.Find("DescriptionText")?.GetComponent<Text>();
            statusText = statusText ?? transform.Find("StatusText")?.GetComponent<Text>();
            researchButton = researchButton ?? transform.Find("ResearchButton")?.GetComponent<Button>();
            progressBar = progressBar ?? transform.Find("ProgressBar")?.GetComponent<Slider>();
            feedbackText = feedbackText ?? transform.Find("FeedbackText")?.GetComponent<Text>();

            if (nameText == null) Debug.LogError("TechDisplayItem: NameText not found or linked.");
            if (descriptionText == null) Debug.LogError("TechDisplayItem: DescriptionText not found or linked.");
            if (statusText == null) Debug.LogError("TechDisplayItem: StatusText not found or linked.");
            if (researchButton == null) Debug.LogError("TechDisplayItem: ResearchButton not found or linked.");
            if (progressBar == null) Debug.LogWarning("TechDisplayItem: ProgressBar not found or linked (optional).");
            if (feedbackText == null) Debug.LogWarning("TechDisplayItem: FeedbackText not found or linked (optional, might use StatusText as fallback).");
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

            if (nameText != null) nameText.text = mTechnology.Name.Value; // Assuming Name is BindableProperty<string>
            if (descriptionText != null) descriptionText.text = mTechnology.Description.Value; // Assuming Description is BindableProperty<string>

            mTechnology.Status.RegisterWithInit(UpdateUIBasedOnStatus).AddTo(mTechItemUnregisters);

            if (mResearchSystemRef != null && mResearchSystemRef.CurrentlyResearching.Value == mTechnology)
            {
                mResearchSystemRef.CurrentResearchProgressNormalized.RegisterWithInit(UpdateProgressBar).AddTo(mTechItemUnregisters);
            }
            else if (progressBar != null)
            {
                 progressBar.gameObject.SetActive(false);
                 progressBar.value = 0;
            }
        }

        private void UpdateUIBasedOnStatus(ResearchStatus newStatus)
        {
            if (mTechnology == null || statusText == null) return; // researchButton could be null if not applicable for all states

            if (feedbackText != null) feedbackText.text = "";

            bool canAfford = mResourceModelRef.HasEnough(GameResourceType.ResearchPoints, mTechnology.ResearchPointCost);

            if(researchButton) researchButton.gameObject.SetActive(false);
            if (progressBar != null) progressBar.gameObject.SetActive(false);

            switch (newStatus)
            {
                case ResearchStatus.Available:
                    statusText.text = $"状态：可研究 (成本：{mTechnology.ResearchPointCost}点)";
                    if(researchButton) researchButton.gameObject.SetActive(true);
                    if(researchButton) researchButton.interactable = !mIsAnotherResearchActiveRef && canAfford;
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
                    if (mResearchSystemRef != null && mResearchSystemRef.CurrentlyResearching.Value == mTechnology)
                    {
                        if (progressBar != null) progressBar.gameObject.SetActive(true);
                    }
                    break;
                case ResearchStatus.Completed:
                    statusText.text = "状态：已完成";
                    break;
                case ResearchStatus.Locked:
                    statusText.text = "状态：已锁定";
                    if (mTechnology.PrerequisiteTechIds.Count > 0 && mResearchSystemRef != null)
                    {
                        string prereqs = string.Join("、 ", mTechnology.PrerequisiteTechIds
                            .Select(id => mResearchSystemRef.GetTechnologyNameById(id) ?? $"未知ID:{id.Substring(0,4)}"));
                        AppendFeedbackText($"前置条件未满足: {prereqs}");
                    } else if (mTechnology.PrerequisiteTechIds.Count > 0) {
                        AppendFeedbackText($"前置条件未满足");
                    }
                    break;
                default:
                    statusText.text = $"状态：{newStatus}";
                    break;
            }
        }

        private void UpdateProgressBar(float normalizedProgress)
        {
            if (progressBar != null && mTechnology == mResearchSystemRef?.CurrentlyResearching.Value)
            {
                progressBar.value = normalizedProgress;
                progressBar.gameObject.SetActive(mTechnology.Status.Value == ResearchStatus.InProgress);
            }
            else if (progressBar != null)
            {
                progressBar.gameObject.SetActive(false);
            }
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
