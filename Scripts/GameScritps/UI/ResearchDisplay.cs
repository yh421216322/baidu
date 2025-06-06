using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // Added for potential Linq operations
using MyGameNamespace; // For IObjectPoolSystem
using QFramework;
using YourGameNamespace.Research;
using YourGameNamespace.Events; // For Model_TechnologyStatusUpdatedEvent
using YourGameNamespace.Commands; // For StartResearchCommand
using YourGameNamespace.Resources; // Required for ResourceModel if it's in this namespace

namespace YourGameNamespace.UI
{
    public class ResearchDisplay : MonoBehaviour, IController
    {
        [Header("Resource Texts")]
        public Text foodText;
        public Text powerText;
        public Text ammoText;
        public Text medicineText;
        public Text researchPointsText;
        public Text electronicPartsText;

        [Header("Technology Lists")]
        public Transform availableTechUIParent;
        public Transform inProgressTechUIParent; // This might be replaced by current research UI
        public Transform completedTechUIParent;
        // public GameObject techUIPrefab; // Removed
        private readonly string techItemPrefabName = "Prefabs/UI/Items/TechItem_PF";

        [Header("Current Research Info")]
        public Text currentResearchNameText;
        public Text currentResearchDescriptionText;
        public Slider researchProgressSlider;
        public Text researchProgressPercentageText;

        private ResearchModel mResearchModel;
        private IResearchSystem mResearchSystem; // Changed to interface if available, otherwise concrete
        private ResourceModel mResourceModel;
        private IObjectPoolSystem _objectPoolSystem;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        private void Awake()
        {
            // Resource Texts
            foodText = foodText ?? transform.Find("ResourcePanel/FoodText")?.GetComponent<Text>();
            powerText = powerText ?? transform.Find("ResourcePanel/PowerText")?.GetComponent<Text>();
            ammoText = ammoText ?? transform.Find("ResourcePanel/AmmoText")?.GetComponent<Text>();
            medicineText = medicineText ?? transform.Find("ResourcePanel/MedicineText")?.GetComponent<Text>();
            researchPointsText = researchPointsText ?? transform.Find("ResourcePanel/ResearchPointsText")?.GetComponent<Text>();
            electronicPartsText = electronicPartsText ?? transform.Find("ResourcePanel/ElectronicPartsText")?.GetComponent<Text>();

            // Current Research Info Texts & Slider
            // Assuming these are direct children or part of a "CurrentResearchInfoPanel" type object.
            // Using direct names as per typical setup. Adjust paths if they are nested deeper.
            currentResearchNameText = currentResearchNameText ?? transform.Find("CurrentResearchNameText_Element")?.GetComponent<Text>();
            currentResearchDescriptionText = currentResearchDescriptionText ?? transform.Find("CurrentResearchDescriptionText_Element")?.GetComponent<Text>();
            researchProgressSlider = researchProgressSlider ?? transform.Find("ResearchProgressSlider_Element")?.GetComponent<Slider>();
            researchProgressPercentageText = researchProgressPercentageText ?? transform.Find("ResearchProgressPercentageText_Element")?.GetComponent<Text>();

            // Null checks for all fields
            if (foodText == null) Debug.LogWarning("ResearchDisplay: foodText not found or linked (optional).");
            if (powerText == null) Debug.LogWarning("ResearchDisplay: powerText not found or linked (optional).");
            if (ammoText == null) Debug.LogWarning("ResearchDisplay: ammoText not found or linked (optional).");
            if (medicineText == null) Debug.LogWarning("ResearchDisplay: medicineText not found or linked (optional).");
            if (researchPointsText == null) Debug.LogError("ResearchDisplay: researchPointsText not found or linked.");
            if (electronicPartsText == null) Debug.LogWarning("ResearchDisplay: electronicPartsText not found or linked (optional).");

            if (currentResearchNameText == null) Debug.LogError("ResearchDisplay: currentResearchNameText not found or linked.");
            if (currentResearchDescriptionText == null) Debug.LogWarning("ResearchDisplay: currentResearchDescriptionText not found or linked (optional).");
            if (researchProgressSlider == null) Debug.LogError("ResearchDisplay: researchProgressSlider not found or linked.");
            if (researchProgressPercentageText == null) Debug.LogError("ResearchDisplay: researchProgressPercentageText not found or linked.");
        }

        void Start()
        {
            if (GameArchitecture.Interface == null)
            {
                Debug.LogError("ResearchDisplay: GameArchitecture.Interface is null. Ensure GameInitializer runs first.");
                enabled = false;
                return;
            }

            mResearchModel = this.GetModel<ResearchModel>();
            mResearchSystem = this.GetSystem<IResearchSystem>(); // Assuming IResearchSystem exists and is registered
            mResourceModel = this.GetModel<ResourceModel>();
            _objectPoolSystem = this.GetSystem<IObjectPoolSystem>();

            if (mResearchModel == null) Debug.LogError("ResearchDisplay: ResearchModel not found.");
            if (mResearchSystem == null) Debug.LogError("ResearchDisplay: IResearchSystem not found.");
            if (mResourceModel == null) Debug.LogError("ResearchDisplay: ResourceModel not found.");
            if (_objectPoolSystem == null) Debug.LogError("ResearchDisplay: IObjectPoolSystem not found.");
            // if (techUIPrefab == null) Debug.LogError("ResearchDisplay: techUIPrefab not assigned."); // Removed check for public field


            // Register for events and BindableProperty changes
            this.RegisterEvent<Model_TechnologyStatusUpdatedEvent>(e => RefreshTechnologyList()).UnRegisterWhenGameObjectDestroyed(this.gameObject); // Already correct

            if (mResearchSystem != null)
            {
                mResearchSystem.CurrentlyResearching.RegisterWithInit(UpdateCurrentResearchInfo).UnRegisterWhenGameObjectDestroyed(this.gameObject); // Already correct
                mResearchSystem.CurrentResearchProgressNormalized.RegisterWithInit(UpdateResearchProgressUI).UnRegisterWhenGameObjectDestroyed(this.gameObject); // Already correct
            }

            RefreshResourceTexts(); // Initial resource text update
            RefreshTechnologyList(); // Initial list refresh
            // UpdateCurrentResearchInfo and UpdateResearchProgressUI will be called by RegisterWithInit
        }

        // Update() method can be removed if all updates are event-driven

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
            if (mResearchModel == null || _objectPoolSystem == null) // Removed techUIPrefab from check
            {
                Debug.LogError("ResearchDisplay: Cannot refresh technology list due to missing dependencies (Model or ObjectPoolSystem).");
                return;
            }

            // Optional: This call might belong elsewhere (e.g., system or command)
            // For now, keeping it to ensure statuses are up-to-date before display.
            // Consider if Model_TechnologyStatusUpdatedEvent already implies statuses are correct.
            // mResearchModel.UpdateAllTechnologyStatuses();

            // Determine which lists to show. For now, we can filter by status.
            // The old inProgressTechUIParent might not be needed if current research is shown separately.
            PopulateTechList(availableTechUIParent, mResearchModel.GetAllTechnologies().Where(t => t.Status.Value == ResearchStatus.Available).ToList());
            PopulateTechList(completedTechUIParent, mResearchModel.GetAllTechnologies().Where(t => t.Status.Value == ResearchStatus.Completed).ToList());

            // Clear inProgressTechUIParent if it's no longer used for a list
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
                        // Pass tech data, the research request handler, and whether another research is active
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
                currentResearchNameText.text = $"当前研究：{currentTech.Name}"; // Name should be localized
                currentResearchDescriptionText.text = currentTech.Description; // Description should be localized
            }
            else
            {
                currentResearchNameText.text = "当前没有研究项目";
                currentResearchDescriptionText.text = "";
                if(researchProgressSlider != null) researchProgressSlider.value = 0;
                if(researchProgressPercentageText != null) researchProgressPercentageText.text = "0%";
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
            if (string.IsNullOrEmpty(techId)) return;
            Debug.Log($"ResearchDisplay: Received research request for Tech ID: {techId}. Sending command...");
            this.SendCommand(new StartResearchCommand(techId));
        }

        // OnEnable, OnDisable, HandleTechnologyStatusChange, UpdateTechInContainer, ForceRefreshAllLists are removed
        // as their functionality is replaced by QFramework event system or new design.
    }
}
