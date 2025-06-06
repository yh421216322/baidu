using QFramework;
// using TMPro; // Removed
using UnityEngine;
using UnityEngine.UI;
using YourGameNamespace.Survivors;
using System.Collections.Generic;

namespace YourGameNamespace.UI
{
    public class SurvivorListItemUI : MonoBehaviour
    {
        // --- UI Element References (to be linked in Unity Editor or found in Awake) ---
        public Text nameText;
        public Text statusText;
        public Slider foodSlider;
        public Text foodValueText;
        public Slider restSlider;
        public Text restValueText;
        public Text professionText;
        public Text workstationText;

        // --- Private fields ---
        private Survivor mSurvivor;
        private List<IUnRegister> mUnregisters = new List<IUnRegister>();

        private void Awake()
        {
            nameText = nameText ?? transform.Find("NameText")?.GetComponent<Text>();
            statusText = statusText ?? transform.Find("StatusText")?.GetComponent<Text>();
            foodSlider = foodSlider ?? transform.Find("FoodSlider")?.GetComponent<Slider>();
            foodValueText = foodValueText ?? transform.Find("FoodValueText")?.GetComponent<Text>();
            restSlider = restSlider ?? transform.Find("RestSlider")?.GetComponent<Slider>();
            restValueText = restValueText ?? transform.Find("RestValueText")?.GetComponent<Text>();
            professionText = professionText ?? transform.Find("ProfessionText")?.GetComponent<Text>();
            workstationText = workstationText ?? transform.Find("WorkstationText")?.GetComponent<Text>();

            if (nameText == null) Debug.LogError("SurvivorListItemUI: NameText not found or linked.");
            if (statusText == null) Debug.LogError("SurvivorListItemUI: StatusText not found or linked.");
            if (foodSlider == null) Debug.LogError("SurvivorListItemUI: FoodSlider not found or linked.");
            if (foodValueText == null) Debug.LogError("SurvivorListItemUI: FoodValueText not found or linked.");
            if (restSlider == null) Debug.LogError("SurvivorListItemUI: RestSlider not found or linked.");
            if (restValueText == null) Debug.LogError("SurvivorListItemUI: RestValueText not found or linked.");
            if (professionText == null) Debug.LogError("SurvivorListItemUI: ProfessionText not found or linked.");
            if (workstationText == null) Debug.LogError("SurvivorListItemUI: WorkstationText not found or linked.");
        }

        public void Setup(Survivor survivor)
        {
            mSurvivor = survivor;
            ClearBindings();

            if (mSurvivor == null)
            {
                if (nameText) nameText.text = "N/A";
                if (statusText) statusText.text = "";
                if (foodSlider) foodSlider.gameObject.SetActive(false);
                if (foodValueText) foodValueText.text = "";
                if (restSlider) restSlider.gameObject.SetActive(false);
                if (restValueText) restValueText.text = "";
                if (professionText) professionText.text = "";
                if (workstationText) workstationText.text = "";
                return;
            }

            if (foodSlider) foodSlider.gameObject.SetActive(true);
            if (restSlider) restSlider.gameObject.SetActive(true);

            mSurvivor.Name.RegisterWithInit(UpdateNameUI).AddTo(mUnregisters);
            mSurvivor.Status.RegisterWithInit(UpdateStatusUI).AddTo(mUnregisters);
            mSurvivor.FoodLevel.RegisterWithInit(UpdateFoodUI).AddTo(mUnregisters);
            mSurvivor.RestLevel.RegisterWithInit(UpdateRestUI).AddTo(mUnregisters);
            mSurvivor.Profession.RegisterWithInit(UpdateProfessionUI).AddTo(mUnregisters);
            mSurvivor.WorkstationId.RegisterWithInit(UpdateWorkstationUI).AddTo(mUnregisters);
        }

        private void UpdateNameUI(string newName)
        {
            if (nameText != null) nameText.text = $"姓名: {newName}";
        }

        private void UpdateStatusUI(SurvivorStatus newStatus)
        {
            if (statusText != null) statusText.text = $"状态: {GetLocalizedStatus(newStatus)}";
        }

        private void UpdateFoodUI(float newFoodLevel)
        {
            if (foodSlider != null && mSurvivor != null && mSurvivor.MaxFoodLevel > 0) foodSlider.value = newFoodLevel / mSurvivor.MaxFoodLevel;
            if (foodValueText != null && mSurvivor != null) foodValueText.text = $"{newFoodLevel:F0}/{mSurvivor.MaxFoodLevel:F0}";
        }

        private void UpdateRestUI(float newRestLevel)
        {
            if (restSlider != null && mSurvivor != null && mSurvivor.MaxRestLevel > 0) restSlider.value = newRestLevel / mSurvivor.MaxRestLevel;
            if (restValueText != null && mSurvivor != null) restValueText.text = $"{newRestLevel:F0}/{mSurvivor.MaxRestLevel:F0}";
        }

        private void UpdateProfessionUI(SurvivorProfession newProfession)
        {
            if (professionText != null) professionText.text = $"职业: {GetLocalizedProfession(newProfession)}";
        }

        private void UpdateWorkstationUI(System.Guid? newWorkstationId)
        {
            if (workstationText != null)
            {
                if (newWorkstationId.HasValue)
                {
                    workstationText.text = $"岗位ID: {newWorkstationId.Value.ToString().Substring(0,4)}..";
                }
                else
                {
                    workstationText.text = "岗位: 无";
                }
            }
        }

        private string GetLocalizedStatus(SurvivorStatus status)
        {
            switch (status)
            {
                case SurvivorStatus.Idle: return "空闲";
                case SurvivorStatus.Working: return "工作中";
                case SurvivorStatus.Resting: return "休息中";
                case SurvivorStatus.Injured: return "受伤";
                case SurvivorStatus.NeedsAttention: return "需要关注!";
                case SurvivorStatus.OnExpedition: return "远征中";
                default: return status.ToString();
            }
        }

        private string GetLocalizedProfession(SurvivorProfession profession)
        {
            return profession.ToString();
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
