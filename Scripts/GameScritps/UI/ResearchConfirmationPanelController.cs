// ResearchConfirmationPanelController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using QFramework;
using YourGameNamespace.Research; // For Technology
using System;

namespace YourGameNamespace.UI
{
    public class ResearchConfirmationPanelController : MonoBehaviour, IController
    {
        public TextMeshProUGUI techNameText;
        public TextMeshProUGUI techDescriptionText;
        public TextMeshProUGUI techCostText;
        public Button confirmButton;
        public Button cancelButton;

        private Technology mCurrentTechnology;
        private Action<string> mOnConfirmAction;
        private Action mOnCancelAction;
        private IObjectPoolSystem mObjectPoolSystem;


        void Awake()
        {
            mObjectPoolSystem = this.GetSystem<IObjectPoolSystem>();

            if (confirmButton == null || cancelButton == null)
            {
                Debug.LogError("ResearchConfirmationPanelController: 确认或取消按钮未在Inspector中分配!");
                return;
            }
            confirmButton.onClick.AddListener(OnConfirmButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
        }

        public void Setup(Technology technology, Action<string> confirmAction, Action cancelAction)
        {
            mCurrentTechnology = technology;
            mOnConfirmAction = confirmAction;
            mOnCancelAction = cancelAction;

            if (mCurrentTechnology == null)
            {
                Debug.LogError("ResearchConfirmationPanelController: Setup时传入的Technology为空!");
                ClosePanel(false); // Close without confirming
                return;
            }

            if (techNameText != null)
            {
                // Assuming Technology.Name is BindableProperty<string> or just string
                techNameText.text = $"科技名称: {mCurrentTechnology.Name.Value}";
            }
            else Debug.LogError("ResearchConfirmationPanelController: techNameText 未分配!");


            if (techDescriptionText != null)
            {
                techDescriptionText.text = $"描述: {mCurrentTechnology.Description.Value}";
            }
            else Debug.LogError("ResearchConfirmationPanelController: techDescriptionText 未分配!");

            if (techCostText != null)
            {
                // Assuming GameResourceType.ResearchPoints is how research points are identified
                techCostText.text = $"成本: {mCurrentTechnology.ResearchPointCost} 研究点";
            }
            else Debug.LogError("ResearchConfirmationPanelController: techCostText 未分配!");

            gameObject.SetActive(true);
            // TODO: Add panel open animation if desired
        }

        private void OnConfirmButtonClick()
        {
            if (mCurrentTechnology == null || mOnConfirmAction == null)
            {
                 Debug.LogError("ResearchConfirmationPanelController: OnConfirmButtonClick时 Technology 或 ConfirmAction 为空!");
                 ClosePanel(false);
                 return;
            }
            mOnConfirmAction.Invoke(mCurrentTechnology.Id);
            ClosePanel(true);
        }

        private void OnCancelButtonClick()
        {
            ClosePanel(false);
        }

        private void ClosePanel(bool confirmed)
        {
            if (!confirmed && mOnCancelAction != null)
            {
                mOnCancelAction.Invoke();
            }

            // TODO: Add panel close animation if desired
            gameObject.SetActive(false); // Hide first
            if (mObjectPoolSystem != null)
            {
                mObjectPoolSystem.Recycle(gameObject);
            }
            else
            {
                Destroy(gameObject); // Fallback
            }
        }

        public IArchitecture GetArchitecture()
        {
            return GlobalGameArchitecture.Interface; // Using the existing global access point
        }

        void OnDestroy()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveAllListeners();
            if (cancelButton != null) cancelButton.onClick.RemoveAllListeners();
            mOnConfirmAction = null; // Clear delegates
            mOnCancelAction = null;
        }
    }
}
