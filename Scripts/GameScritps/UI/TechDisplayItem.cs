using System;
using UnityEngine;
using UnityEngine.UI;
using YourGameNamespace.Research;
using YourGameNamespace.Resources;
using QFramework; // Added
using System.Collections.Generic; // Added
using System.Linq; // Added for Linq in prerequisite display

namespace YourGameNamespace.UI
{
    public class TechDisplayItem : MonoBehaviour
    {
        public Text nameText;
        public Text descriptionText;
        public Text statusText;
        public Button researchButton;
        public Slider progressBar;
        public Text feedbackText; // New UI element for detailed feedback

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
            feedbackText = feedbackText ?? transform.Find("FeedbackText")?.GetComponent<Text>(); // Assuming path

            if (feedbackText == null && statusText != null) {
                // Fallback: if no dedicated feedbackText, use statusText and append.
                // This is a simple fallback; a dedicated feedbackText is cleaner.
                 Debug.LogWarning("TechDisplayItem: FeedbackText not assigned, will append to StatusText if necessary.");
            }


            researchButton?.onClick.AddListener(HandleResearchButtonClick);
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

            if (nameText != null) nameText.text = mTechnology.Name;
            if (descriptionText != null) descriptionText.text = mTechnology.Description;

            mTechnology.Status.RegisterWithInit(UpdateUIBasedOnStatus).AddTo(mTechItemUnregisters);

            if (mResearchSystemRef != null && mResearchSystemRef.CurrentlyResearching.Value == mTechnology)
            {
                mResearchSystemRef.CurrentResearchProgressNormalized.RegisterWithInit(UpdateProgressBar).AddTo(mTechItemUnregisters);
            }
            else if (progressBar != null) // Ensure progress bar is hidden if not the current research
            {
                 progressBar.gameObject.SetActive(false);
                 progressBar.value = 0;
            }
        }

        private void UpdateUIBasedOnStatus(ResearchStatus newStatus)
        {
            if (mTechnology == null || researchButton == null || statusText == null) return;

            // Default feedback text to empty
            if (feedbackText != null) feedbackText.text = "";

            bool canAfford = mResourceModelRef.HasEnough(GameResourceType.ResearchPoints, mTechnology.ResearchPointCost);

            researchButton.gameObject.SetActive(false); // Default to hidden
            if (progressBar != null) progressBar.gameObject.SetActive(false); // Default to hidden

            switch (newStatus)
            {
                case ResearchStatus.Available:
                    statusText.text = $"状态：可研究 (成本：{mTechnology.ResearchPointCost}点)"; // Localized
                    researchButton.gameObject.SetActive(true);
                    researchButton.interactable = !mIsAnotherResearchActiveRef && canAfford;
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
                    statusText.text = "状态：研究中..."; // Localized
                    if (mResearchSystemRef != null && mResearchSystemRef.CurrentlyResearching.Value == mTechnology)
                    {
                        if (progressBar != null) progressBar.gameObject.SetActive(true);
                        // Progress bar value will be updated by its own binding if this is the current research
                    }
                    break;
                case ResearchStatus.Completed:
                    statusText.text = "状态：已完成"; // Localized
                    break;
                case ResearchStatus.Locked:
                    statusText.text = "状态：已锁定"; // Localized
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
            else if (statusText != null) // Fallback to statusText
            {
                 statusText.text += $" ({message})";
            }
        }

        private void HandleResearchButtonClick()
        {
            if (mResearchRequestCallback != null && mTechnology != null)
            {
                bool canAfford = mResourceModelRef.HasEnough(GameResourceType.ResearchPoints, mTechnology.ResearchPointCost);

                // mIsAnotherResearchActiveRef might be stale here if it's not updated when ResearchSystem.CurrentlyResearching changes.
                // It's safer to re-query the ResearchSystem directly if possible, or ensure ResearchDisplay re-setups items on such changes.
                // For now, assume mIsAnotherResearchActiveRef passed in Setup is the primary gate for initial display.
                // The command itself will ultimately fail if another research is truly active.
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
                    // Optionally update feedbackText here too, or rely on UpdateUIBasedOnStatus re-triggering.
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

        // Keep these if ResearchDisplay still uses them for quick access, though not strictly necessary
        // if all updates are driven by status changes.
        public string GetTechnologyId() => mTechId;
        public bool Matches(Technology tech) => mTechId == tech?.Id;
    }
}
