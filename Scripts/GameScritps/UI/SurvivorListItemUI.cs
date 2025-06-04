using QFramework;
using TMPro; // 使用 TextMeshPro 需要引入此命名空间
using UnityEngine;
using UnityEngine.UI; // 用于 Slider 等UI组件
using YourGameNamespace.Survivors; // For Survivor, SurvivorStatus enums etc.
using System.Collections.Generic; // For List<IUnRegister>

namespace YourGameNamespace.UI
{
    public class SurvivorListItemUI : MonoBehaviour // 通常UI项是MonoBehaviour
    {
        // --- UI Element References (需要在Unity编辑器中链接) ---
        public TextMeshProUGUI nameText;    // 用于显示幸存者名字
        public TextMeshProUGUI statusText;  // 用于显示幸存者状态
        public Slider foodSlider;           // 用于显示食物水平的滑动条
        public TextMeshProUGUI foodValueText; // 用于显示具体食物数值
        public Slider restSlider;           // 用于显示休息水平的滑动条
        public TextMeshProUGUI restValueText; // 用于显示具体休息数值
        public TextMeshProUGUI professionText; // 用于显示职业
        public TextMeshProUGUI workstationText; // 用于显示分配的工作站 (若有)

        // --- Private fields ---
        private Survivor mSurvivor;
        private List<IUnRegister> mUnregisters = new List<IUnRegister>(); // 用于管理绑定，在销毁时解绑

        // Setup方法，由父级Display调用以传递数据和依赖
        public void Setup(Survivor survivor)
        {
            mSurvivor = survivor;

            // 清理旧的绑定（如果此UI项被复用）
            ClearBindings();

            if (mSurvivor == null)
            {
                // 如果幸存者数据为空，则清空UI或显示默认/错误信息
                nameText.text = "N/A";
                statusText.text = "";
                if (foodSlider) foodSlider.gameObject.SetActive(false);
                if (foodValueText) foodValueText.text = "";
                if (restSlider) restSlider.gameObject.SetActive(false);
                if (restValueText) restValueText.text = "";
                if (professionText) professionText.text = "";
                if (workstationText) workstationText.text = "";
                return;
            }

            // 激活Slider（如果之前被隐藏）
            if (foodSlider) foodSlider.gameObject.SetActive(true);
            if (restSlider) restSlider.gameObject.SetActive(true);

            // 注册绑定到Survivor的BindableProperties
            mSurvivor.Name.RegisterWithInit(UpdateNameUI).AddTo(mUnregisters);
            mSurvivor.Status.RegisterWithInit(UpdateStatusUI).AddTo(mUnregisters);
            mSurvivor.FoodLevel.RegisterWithInit(UpdateFoodUI).AddTo(mUnregisters);
            mSurvivor.RestLevel.RegisterWithInit(UpdateRestUI).AddTo(mUnregisters);
            mSurvivor.Profession.RegisterWithInit(UpdateProfessionUI).AddTo(mUnregisters);
            mSurvivor.WorkstationId.RegisterWithInit(UpdateWorkstationUI).AddTo(mUnregisters);
        }

        private void UpdateNameUI(string newName)
        {
            if (nameText != null) nameText.text = $"姓名: {newName}"; // 本地化
        }

        private void UpdateStatusUI(SurvivorStatus newStatus)
        {
            if (statusText != null) statusText.text = $"状态: {GetLocalizedStatus(newStatus)}"; // 本地化
        }

        private void UpdateFoodUI(float newFoodLevel)
        {
            if (foodSlider != null) foodSlider.value = newFoodLevel / mSurvivor.MaxFoodLevel;
            if (foodValueText != null) foodValueText.text = $"{newFoodLevel:F0}/{mSurvivor.MaxFoodLevel:F0}";
        }

        private void UpdateRestUI(float newRestLevel)
        {
            if (restSlider != null) restSlider.value = newRestLevel / mSurvivor.MaxRestLevel;
            if (restValueText != null) restValueText.text = $"{newRestLevel:F0}/{mSurvivor.MaxRestLevel:F0}";
        }

        private void UpdateProfessionUI(SurvivorProfession newProfession)
        {
            if (professionText != null) professionText.text = $"职业: {GetLocalizedProfession(newProfession)}"; // 本地化
        }

        private void UpdateWorkstationUI(System.Guid? newWorkstationId)
        {
            if (workstationText != null)
            {
                if (newWorkstationId.HasValue)
                {
                    // 为了获取工作站名称/类型，理想情况下这里应该通过Model查询
                    // 为简化，暂时只显示ID或一个通用文本
                    // IWorkstationModel workstationModel = (GameArchitecture.Interface as GameArchitecture).GetModel<IWorkstationModel>();
                    // Workstation ws = workstationModel.GetWorkstationById(newWorkstationId.Value);
                    // workstationText.text = ws != null ? $"岗位: {ws.Type}" : "岗位: 未知";
                    workstationText.text = $"岗位ID: {newWorkstationId.Value.ToString().Substring(0,4)}.."; // 简化显示
                }
                else
                {
                    workstationText.text = "岗位: 无"; // 本地化
                }
            }
        }

        // 将枚举状态本地化为中文字符串
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
             // 此处应有职业的本地化逻辑
            return profession.ToString(); // 暂时返回枚举名
        }

        // 在对象销毁时清除所有绑定，防止内存泄漏
        private void OnDestroy()
        {
            ClearBindings();
        }

        private void ClearBindings()
        {
            foreach (var unregister in mUnregisters)
            {
                unregister.UnRegister();
            }
            mUnregisters.Clear();
        }
    }
}
