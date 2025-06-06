// WorkstationAssignSurvivorItemUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using QFramework; // For IArchitecture
using YourGameNamespace.Survivors; // For Survivor data

namespace YourGameNamespace.UI
{
    public class WorkstationAssignSurvivorItemUI : MonoBehaviour
    {
        public TextMeshProUGUI survivorNameText;
        public TextMeshProUGUI survivorProfessionText;
        public Button actionButton; // "分配" 或 "解除分配"
        public TextMeshProUGUI actionButtonText;

        private Survivor mSurvivor;
        private Guid mWorkstationId;
        private bool mIsAssigned; // 标记此项代表的是已分配还是可分配列表中的幸存者
        private Action<Guid, Guid> mOnClickCallback; // 回调格式: (survivorId, workstationId)
        private IArchitecture mArchitecture; // 用于发送命令

        public void Setup(Survivor survivor, Guid workstationId, bool isAssigned, Action<Guid, Guid> onClickCallback, IArchitecture architecture)
        {
            mSurvivor = survivor;
            mWorkstationId = workstationId;
            mIsAssigned = isAssigned;
            mOnClickCallback = onClickCallback;
            mArchitecture = architecture;

            if (survivorNameText != null)
            {
                survivorNameText.text = survivor.Name.Value; // 假设Survivor.Name是BindableProperty<string>
            }
            else
            {
                Debug.LogError("WorkstationAssignSurvivorItemUI: 幸存者名称文本未分配!");
            }

            if (survivorProfessionText != null)
            {
                survivorProfessionText.text = $"职业: {survivor.Profession.Value.ToString()}"; // 假设Profession是BindableProperty<SurvivorProfession>
            }
            else
            {
                Debug.LogError("WorkstationAssignSurvivorItemUI: 幸存者职业文本未分配!");
            }

            if (actionButtonText != null)
            {
                 actionButtonText.text = mIsAssigned ? "解除分配" : "分配";
            }
            else
            {
                Debug.LogError("WorkstationAssignSurvivorItemUI: 按钮文本未分配!");
            }

            if (actionButton != null)
            {
                actionButton.onClick.RemoveAllListeners(); // 确保清除旧监听器
                actionButton.onClick.AddListener(OnActionButtonClick);
            }
            else
            {
                Debug.LogError("WorkstationAssignSurvivorItemUI: 操作按钮未分配!");
            }
        }

        private void OnActionButtonClick()
        {
            if (mSurvivor == null || mWorkstationId == Guid.Empty)
            {
                Debug.LogError("WorkstationAssignSurvivorItemUI: Survivor 或 WorkstationId 未正确设置!");
                return;
            }

            Debug.Log($"WorkstationAssignSurvivorItemUI: 按钮点击 - Survivor: {mSurvivor.Name.Value}, Workstation: {mWorkstationId}, IsAssigned: {mIsAssigned}");
            mOnClickCallback?.Invoke(mSurvivor.Id, mWorkstationId);
        }

        void OnDestroy()
        {
            if (actionButton != null)
            {
                actionButton.onClick.RemoveAllListeners();
            }
        }
    }
}
