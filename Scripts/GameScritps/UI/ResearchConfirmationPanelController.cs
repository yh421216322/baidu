// ResearchConfirmationPanelController.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using QFramework;
using YourGameNamespace.Research; // For Technology
using System;

using MyGameNamespace; // Assuming RegisterManager and ObjectPoolSystem might be here

namespace YourGameNamespace.UI
{
    public class ResearchConfirmationPanelController : MonoBehaviour, IController, IPoolable // Added IPoolable
    {
        public TextMeshProUGUI techNameText; // 名称文本
        public TextMeshProUGUI techDescriptionText; // 描述文本
        public TextMeshProUGUI techCostText; // 成本文本
        public Button confirmButton; // 确认按钮
        public Button cancelButton; // 取消按钮

        private Technology mCurrentTechnology; // 当前科技信息
        private Action<string> mOnConfirmAction; // 确认回调
        private Action mOnCancelAction; // 取消回调 (可选)
        private ObjectPoolSystem mObjectPoolSystem; // 对象池系统 (具体类)


        void Awake()
        {
            // Systems are fetched in InitAndShow or Setup, not Awake if pooled and setup externally
            // mObjectPoolSystem = this.GetSystem<ObjectPoolSystem>();

            if (confirmButton == null || cancelButton == null)
            {
                Debug.LogError("ResearchConfirmationPanelController: 确认或取消按钮未在Inspector中分配!");
                return;
            }
            confirmButton.onClick.AddListener(OnConfirmButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
        }

        /// <summary>
        /// 初始化确认面板并显示。
        /// </summary>
        public void InitAndShow(Technology technology, Action<string> confirmAction, Action cancelAction)
        {
            mCurrentTechnology = technology;
            mOnConfirmAction = confirmAction;
            mOnCancelAction = cancelAction;

            // Fetch systems here, as GetArchitecture() is now valid
            mObjectPoolSystem = this.GetSystem<ObjectPoolSystem>();
            if (mObjectPoolSystem == null) Debug.LogError("ResearchConfirmationPanelController: 对象池系统未找到!");


            if (mCurrentTechnology == null)
            {
                Debug.LogError("ResearchConfirmationPanelController: Setup时传入的Technology为空!");
                AttemptClosePanel(false); // Close without confirming
                return;
            }

            if (techNameText != null)
            {
                techNameText.text = $"科技名称: {mCurrentTechnology.Name.Value}"; // 假设 Name 是 BindableProperty<string>
            }
            else Debug.LogError("ResearchConfirmationPanelController: techNameText (科技名称文本) 未分配!");


            if (techDescriptionText != null)
            {
                techDescriptionText.text = $"描述: {mCurrentTechnology.Description.Value}"; // 假设 Description 是 BindableProperty<string>
            }
            else Debug.LogError("ResearchConfirmationPanelController: techDescriptionText (科技描述文本) 未分配!");

            if (techCostText != null)
            {
                techCostText.text = $"成本: {mCurrentTechnology.ResearchPointCost} 科研点"; // 文本已包含中文
            }
            else Debug.LogError("ResearchConfirmationPanelController: techCostText (科技成本文本) 未分配!");

            gameObject.SetActive(true);
            // TODO: 可以添加打开面板的动画
        }

        private void OnConfirmButtonClick()
        {
            if (mCurrentTechnology == null || mOnConfirmAction == null)
            {
                 Debug.LogError("ResearchConfirmationPanelController: OnConfirmButtonClick时 Technology 或 ConfirmAction 为空!");
                 AttemptClosePanel(false);
                 return;
            }
            mOnConfirmAction.Invoke(mCurrentTechnology.Id);
            AttemptClosePanel(true); // Close and indicate confirmation
        }

        private void OnCancelButtonClick()
        {
            AttemptClosePanel(false); // Close and indicate cancellation
        }

        private void AttemptClosePanel(bool confirmed)
        {
            if (!confirmed && mOnCancelAction != null)
            {
                mOnCancelAction.Invoke();
            }

            // TODO: 可以添加关闭面板的动画
            if (mObjectPoolSystem != null)
            {
                mObjectPoolSystem.Recycle(gameObject); // This will call OnRecycled
            }
            else
            {
                gameObject.SetActive(false); // Fallback
                OnRecycled(); // Manual call if no pool available for some reason
            }
        }

        public IArchitecture GetArchitecture()
        {
            return RegisterManager.Interface; // Changed to RegisterManager
        }

        // --- IPoolable Implementation ---
        public void OnRecycled()
        {
            // Debug.Log("ResearchConfirmationPanelController OnRecycled"); // 中文日志
            // 清理回调和监听器，重置状态
            mOnConfirmAction = null;
            mOnCancelAction = null;
            // 按钮监听器在Awake中添加，如果面板被复用且Awake不重复调用，则可能需要在这里RemoveAllListeners
            // 但通常对象池回收时，下次Spawn会重新Setup/Init，重新AddListener前应有RemoveAllListeners
            if (confirmButton != null) confirmButton.onClick.RemoveAllListeners(); // Best to clean up here
            if (cancelButton != null) cancelButton.onClick.RemoveAllListeners();   // Best to clean up here

            // 重置文本等（可选，如果Setup总会覆盖）
            // if(techNameText) techNameText.text = "";

            gameObject.SetActive(false);
        }
        public bool IsRecycled { get; set; }


        void OnDestroy() // Unity's OnDestroy, good for final cleanup if not pooled or error cases
        {
            if (IsRecycled) return; // If pooled and recycled, OnRecycled should have handled it.

            if (confirmButton != null) confirmButton.onClick.RemoveAllListeners();
            if (cancelButton != null) cancelButton.onClick.RemoveAllListeners();
            mOnConfirmAction = null;
            mOnCancelAction = null;
        }
    }
}
