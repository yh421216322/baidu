using System;
using UnityEngine;
using UnityEngine.UI; // Still needed for Slider, Button
using System.Collections.Generic;
using System.Linq;
using MyGameNamespace;
using QFramework;
using YourGameNamespace.Research;
using YourGameNamespace.Events;
using YourGameNamespace.Commands;
using YourGameNamespace.Resources;
using TMPro; // Added TMPro

namespace YourGameNamespace.UI
{
    public class ResearchDisplay : MonoBehaviour, IController
    {
        [Header("Resource Texts")]
        public TextMeshProUGUI foodText;
        public TextMeshProUGUI powerText;
        public TextMeshProUGUI ammoText;
        public TextMeshProUGUI medicineText;
        public TextMeshProUGUI researchPointsText;
        public TextMeshProUGUI electronicPartsText;

        [Header("Technology Lists")]
        public Transform availableTechUIParent;
        public Transform inProgressTechUIParent;
        public Transform completedTechUIParent;
        private readonly string techItemPrefabName = "Prefabs/UI/Items/TechItem_PF";
        public string researchConfirmationPanelPrefabPath = "Prefabs/UI/ResearchConfirmationPanel_PF";


        [Header("Current Research Info")]
        public TextMeshProUGUI currentResearchNameText;
        public TextMeshProUGUI currentResearchDescriptionText;
        public Slider researchProgressSlider;
        public TextMeshProUGUI researchProgressPercentageText;

        private ResearchModel mResearchModel;
        private ResearchSystem mResearchSystem;
        private ResourceModel mResourceModel;
        private ObjectPoolSystem _objectPoolSystem;
        private GameObject mCurrentConfirmationPanel;


        public IArchitecture GetArchitecture() => RegisterManager.Interface;

        private void Awake()
        {
            // Assuming resource texts are directly assigned in Inspector. If not, use transform.Find.
            // e.g. researchPointsText = researchPointsText ?? transform.Find("SomePanel/ResearchPointsText_TMP")?.GetComponent<TextMeshProUGUI>();

            currentResearchNameText = currentResearchNameText ?? transform.Find("CurrentResearchNameText_Element")?.GetComponent<TextMeshProUGUI>();
            currentResearchDescriptionText = currentResearchDescriptionText ?? transform.Find("CurrentResearchDescriptionText_Element")?.GetComponent<TextMeshProUGUI>();
            researchProgressSlider = researchProgressSlider ?? transform.Find("ResearchProgressSlider_Element")?.GetComponent<Slider>();
            researchProgressPercentageText = researchProgressPercentageText ?? transform.Find("ResearchProgressPercentageText_Element")?.GetComponent<TextMeshProUGUI>();

            if (researchPointsText == null) Debug.LogError("ResearchDisplay: researchPointsText (TextMeshProUGUI) not found or linked.");
            if (currentResearchNameText == null) Debug.LogError("ResearchDisplay: currentResearchNameText (TextMeshProUGUI) not found or linked.");
            if (researchProgressSlider == null) Debug.LogError("ResearchDisplay: researchProgressSlider (Slider) not found or linked.");
            if (researchProgressPercentageText == null) Debug.LogError("ResearchDisplay: researchProgressPercentageText (TextMeshProUGUI) not found or linked.");
        }

        void Start()
        {
            if (RegisterManager.Interface == null)
            {
                Debug.LogError("ResearchDisplay: GameArchitecture.Interface is null. Ensure GameInitializer runs first.");
                enabled = false;
                return;
            }

            mResearchModel = this.GetModel<ResearchModel>();
            mResearchSystem = this.GetSystem<ResearchSystem>();
            mResourceModel = this.GetModel<ResourceModel>();
            _objectPoolSystem = this.GetSystem<ObjectPoolSystem>();

            if (mResearchModel == null) Debug.LogError("ResearchDisplay: ResearchModel not found.");
            if (mResearchSystem == null) Debug.LogError("ResearchDisplay: ResearchSystem not found.");
            if (mResourceModel == null) Debug.LogError("ResearchDisplay: ResourceModel not found.");
            if (_objectPoolSystem == null) Debug.LogError("ResearchDisplay: ObjectPoolSystem not found.");
            if (string.IsNullOrEmpty(researchConfirmationPanelPrefabPath)) Debug.LogError("ResearchDisplay: researchConfirmationPanelPrefabPath is not set.");


            this.RegisterEvent<Model_TechnologyStatusUpdatedEvent>(e => RefreshTechnologyList()).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            this.RegisterEvent<TechnologyCompletedEvent>(e => RefreshTechnologyList()).UnRegisterWhenGameObjectDestroyed(gameObject); // Added from original GDD
            this.RegisterEvent<ResourceChangedEvent>(e => {
                 if(e.Type == GameResourceType.ResearchPoints) RefreshTechnologyList(); // Changed e.ResourceType to e.Type
            }).UnRegisterWhenGameObjectDestroyed(gameObject);


            if (mResearchSystem != null)
            {
                mResearchSystem.CurrentlyResearching.RegisterWithInitValue(UpdateCurrentResearchInfo).UnRegisterWhenGameObjectDestroyed(this.gameObject);
                mResearchSystem.CurrentResearchProgressNormalized.RegisterWithInitValue(UpdateResearchProgressUI).UnRegisterWhenGameObjectDestroyed(this.gameObject);
            }

            RefreshResourceTexts();
            RefreshTechnologyList();
        }


        private void RefreshResourceTexts()
        {
            if (mResourceModel == null) return;
            if (foodText != null) foodText.text = $"食物: {mResourceModel.GetAmount(GameResourceType.Food)}";
            if (powerText != null) powerText.text = $"电力: {mResourceModel.GetAmount(GameResourceType.Power)}";
            if (ammoText != null) ammoText.text = $"弹药: {mResourceModel.GetAmount(GameResourceType.Ammo)}";
            if (medicineText != null) medicineText.text = $"药品: {mResourceModel.GetAmount(GameResourceType.Medicine)}";
            if (researchPointsText != null) researchPointsText.text = $"科研点数: {mResourceModel.GetAmount(GameResourceType.ResearchPoints)}";
            if (electronicPartsText != null) electronicPartsText.text = $"电子零件: {mResourceModel.GetAmount(GameResourceType.ElectronicParts)}";
        }

        public void RefreshTechnologyList()
        {
            if (mResearchModel == null || _objectPoolSystem == null)
            {
                Debug.LogError("ResearchDisplay: Cannot refresh technology list due to missing dependencies (Model or ObjectPoolSystem).");
                return;
            }

            PopulateTechList(availableTechUIParent, mResearchModel.GetAllTechnologies().Where(t => t.Status.Value == ResearchStatus.Available || t.Status.Value == ResearchStatus.Locked).ToList());
            PopulateTechList(completedTechUIParent, mResearchModel.GetAllTechnologies().Where(t => t.Status.Value == ResearchStatus.Completed).ToList());

            // Clear inProgressTechUIParent as current research is displayed separately
            if (inProgressTechUIParent != null)
            {
                foreach (Transform child in inProgressTechUIParent)
                {
                    _objectPoolSystem.Unspawn(child.gameObject);
                }
            }
        }

        void PopulateTechList(Transform parent, List<Technology> techs)
        {
            if (parent == null) return;

            foreach (Transform child in parent)
            {
                _objectPoolSystem.Unspawn(child.gameObject);
            }

            bool isCurrentlyResearching = mResearchSystem != null && mResearchSystem.IsResearching();

            foreach (Technology tech in techs)
            {
                GameObject techItemGO = _objectPoolSystem.Spawn(techItemPrefabName);
                if (techItemGO != null)
                {
                    techItemGO.SetActive(true);
                    techItemGO.transform.SetParent(parent, false);
                    TechDisplayItem displayItem = techItemGO.GetComponent<TechDisplayItem>();

                    if (displayItem != null)
                    {
                        displayItem.Setup(tech, HandleResearchRequest, isCurrentlyResearching, mResearchSystem, mResourceModel);
                    }
                    else
                    {
                        Debug.LogError("ResearchDisplay: TechUIPrefab instance is missing TechDisplayItem script.");
                        _objectPoolSystem.Unspawn(techItemGO);
                    }
                }
                else
                {
                    Debug.LogError($"ResearchDisplay: Failed to spawn '{techItemPrefabName}' from object pool.");
                }
            }
        }

        private void UpdateCurrentResearchInfo(Technology currentTech)
        {
            if (currentResearchNameText == null || currentResearchDescriptionText == null) return;

            if (currentTech != null)
            {
                currentResearchNameText.text = $"当前研究：{currentTech.Name}"; // Removed .Value
                currentResearchDescriptionText.text = currentTech.Description; // Removed .Value
                if (researchProgressSlider != null) researchProgressSlider.gameObject.SetActive(true);
                if (researchProgressPercentageText != null) researchProgressPercentageText.gameObject.SetActive(true);
            }
            else
            {
                currentResearchNameText.text = "当前没有研究项目";
                currentResearchDescriptionText.text = "点击一项可研究的科技以查看详情或开始研究。";
                if(researchProgressSlider != null) {
                    researchProgressSlider.gameObject.SetActive(false);
                    researchProgressSlider.value = 0;
                }
                if(researchProgressPercentageText != null) {
                     researchProgressPercentageText.gameObject.SetActive(false);
                    researchProgressPercentageText.text = "0%";
                }
            }
        }

        private void UpdateResearchProgressUI(float normalizedProgress)
        {
            if (researchProgressSlider != null)
            {
                researchProgressSlider.value = normalizedProgress;
            }
            if (researchProgressPercentageText != null)
            {
                researchProgressPercentageText.text = $"{normalizedProgress * 100:F0}%";
            }
        }

        private void HandleResearchRequest(string techId)
        {
            if (string.IsNullOrEmpty(techId) || mResearchModel == null || mResearchSystem == null || mResourceModel == null || _objectPoolSystem == null)
            {
                 Debug.LogError("ResearchDisplay: HandleResearchRequest - Missing critical references.");
                 return;
            }

            Technology techToResearch = mResearchModel.GetTechnology(techId);
            if (techToResearch == null) {
                Debug.LogError($"ResearchDisplay: 未找到ID为 {techId} 的科技。");
                return;
            }

            // Perform checks before showing confirmation
            if (techToResearch.Status.Value != ResearchStatus.Available) {
                Debug.LogWarning($"ResearchDisplay: 科技 {techId} 当前不是可研究状态。状态: {techToResearch.Status.Value}");
                // Optionally show player feedback e.g. using a temporary message system
                // this.SendCommand(new ShowMessageCommand("此科技当前不可研究。"));
                RefreshTechnologyList();
                return;
            }
            if (mResearchSystem.IsResearching()) {
                 Debug.LogWarning($"ResearchDisplay: 其他研究正在进行中 ({mResearchSystem.GetCurrentResearch()?.Name})，无法开始 {techId}。"); // Removed .Value
                 // this.SendCommand(new ShowMessageCommand("已有其他研究正在进行中。"));
                 RefreshTechnologyList();
                 return;
            }
            if (!mResourceModel.HasEnough(GameResourceType.ResearchPoints, techToResearch.ResearchPointCost)) {
                Debug.LogWarning($"ResearchDisplay: 研究点不足，无法开始 {techId}。");
                // this.SendCommand(new ShowMessageCommand($"研究点不足，需要 {techToResearch.ResearchPointCost} 点。"));
                RefreshTechnologyList();
                return;
            }

            if (mCurrentConfirmationPanel != null && mCurrentConfirmationPanel.activeSelf)
            {
                Debug.LogWarning("ResearchDisplay: 确认面板已打开。");
                return;
            }

            mCurrentConfirmationPanel = _objectPoolSystem.Spawn(researchConfirmationPanelPrefabPath);
            if (mCurrentConfirmationPanel == null)
            {
                Debug.LogError($"ResearchDisplay: 无法生成确认面板: {researchConfirmationPanelPrefabPath}. 直接开始研究作为后备。");
                this.SendCommand(new StartResearchCommand(techId)); // Fallback
                return;
            }

            GameObject mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
            if (mainCanvas != null)
            {
                mCurrentConfirmationPanel.transform.SetParent(mainCanvas.transform, false);
            }
             mCurrentConfirmationPanel.transform.localPosition = Vector3.zero;
            RectTransform panelRect = mCurrentConfirmationPanel.GetComponent<RectTransform>();
            if (panelRect != null) {
                panelRect.anchorMin = new Vector2(0.5f, 0.5f);
                panelRect.anchorMax = new Vector2(0.5f, 0.5f);
                panelRect.pivot = new Vector2(0.5f, 0.5f);
                panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 400); // Example size
                panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 300); // Example size
            }


            var panelController = mCurrentConfirmationPanel.GetComponent<ResearchConfirmationPanelController>();
            if (panelController != null)
            {
                panelController.InitAndShow(techToResearch, // Changed Setup to InitAndShow
                confirmedTechId => {
                    this.SendCommand(new StartResearchCommand(confirmedTechId));
                },
                () => {
                    Debug.Log($"ResearchDisplay: 研究 {techToResearch.Id} 已被用户取消。");
                });
            }
            else
            {
                Debug.LogError($"ResearchDisplay: 确认面板预制件 {researchConfirmationPanelPrefabPath} 上缺少 ResearchConfirmationPanelController 脚本。直接开始研究作为后备。");
                _objectPoolSystem.Unspawn(mCurrentConfirmationPanel); // Changed from Recycle
                mCurrentConfirmationPanel = null;
                this.SendCommand(new StartResearchCommand(techId)); // Fallback
            }
        }

        void OnDestroy()
        {
            // Unregistration is handled by UnRegisterWhenGameObjectDestroyed
            // Recycle items if they were managed in a list by this script (current code doesn't maintain such list here)
            if (mCurrentConfirmationPanel != null && _objectPoolSystem != null)
            {
                _objectPoolSystem.Unspawn(mCurrentConfirmationPanel); // Changed from Recycle
                mCurrentConfirmationPanel = null;
            }
        }
    }
}
