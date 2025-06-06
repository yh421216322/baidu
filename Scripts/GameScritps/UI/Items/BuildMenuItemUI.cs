// 文件路径: Scripts/GameScritps/UI/Items/BuildMenuItemUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using YourGameNamespace.Buildings;
using YourGameNamespace.Resources;
using QFramework; // Added for IPoolable

namespace YourGameNamespace.UI
{
    public class BuildMenuItemUI : MonoBehaviour, IPoolable // Implements IPoolable
    {
        [Header("UI 引用")] // UI References (in Chinese)
        public TextMeshProUGUI buildingNameText; // 建筑名称文本
        public TextMeshProUGUI buildingDescriptionText; // 建筑描述文本
        public TextMeshProUGUI costText; // 成本文本
        public Image buildingIcon; // 建筑图标 (可选)
        public Button buildButton; // 建造按钮

        public BuildingType mBuildingType { get; private set; } // 公开以便外部访问 (例如被PanelController读取以获取成本)
        private Action<BuildingType> mOnBuildRequestCallback;

        public void Setup(
            BuildingType type,
            string name,
            string description,
            List<(GameResourceType resource, int amount)> costs,
            Sprite icon, // 可为null
            Action<BuildingType> onBuildRequestCallback)
        {
            mBuildingType = type;
            mOnBuildRequestCallback = onBuildRequestCallback;

            buildingNameText.text = name;
            buildingDescriptionText.text = description;

            string costStr = "成本: "; // 成本:
            if (costs != null && costs.Count > 0)
            {
                List<string> costEntries = new List<string>();
                foreach (var cost in costs)
                {
                    costEntries.Add($"{GetLocalizedResourceName(cost.resource)} x{cost.amount}");
                }
                costStr += string.Join("，", costEntries); // 使用中文逗号
            }
            else
            {
                costStr += "无"; // 无
            }
            costText.text = costStr;

            if (buildingIcon != null)
            {
                buildingIcon.sprite = icon;
                buildingIcon.gameObject.SetActive(icon != null);
            } else if (buildingIcon != null) { // This case is redundant if buildingIcon is null initially
                 buildingIcon.gameObject.SetActive(false);
            }

            buildButton.onClick.RemoveAllListeners();
            buildButton.onClick.AddListener(OnBuildButtonClicked);
        }

        private void OnBuildButtonClicked()
        {
            mOnBuildRequestCallback?.Invoke(mBuildingType);
        }

        private string GetLocalizedResourceName(GameResourceType resourceType)
        {
            // (保持之前的本地化逻辑)
            switch (resourceType)
            {
                case GameResourceType.Food: return "食物";
                case GameResourceType.Power: return "电力";
                case GameResourceType.Ammo: return "弹药";
                case GameResourceType.Medicine: return "药品";
                case GameResourceType.ResearchPoints: return "科研点";
                case GameResourceType.ElectronicParts: return "电子零件";
                case GameResourceType.Wood: return "木材";
                case GameResourceType.Scrap: return "废料";
                default: return resourceType.ToString();
            }
        }

        // --- IPoolable Implementation ---
        public void OnRecycled()
        {
            mOnBuildRequestCallback = null;
            if (buildButton != null) buildButton.onClick.RemoveAllListeners();
            // Reset image, texts if necessary
            if(buildingIcon != null) buildingIcon.sprite = null;
            if(buildingNameText != null) buildingNameText.text = ""; // Check for null before accessing
            if(buildingDescriptionText != null) buildingDescriptionText.text = ""; // Check for null
            if(costText != null) costText.text = ""; // Check for null
            gameObject.SetActive(false);
        }
        public bool IsRecycled { get; set; }
    }
}
