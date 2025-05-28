using System;
using UnityEngine;
using UnityEngine.UI;
using YourGameNamespace.Research;

namespace YourGameNamespace.UI
{
    public class TechDisplayItem : MonoBehaviour
    {
        public Text nameText;
        public Text descriptionText;
        public Text statusText; // 显示可用项目的成本，进行中项目的进度，“已完成”表示已完成
        public Button researchButton;
        public Slider progressBar; // 可选，用于进行中项目

        private Technology mTechnology;
        private ResearchSystem mResearchSystem;
        private ResourceModel mResourceModel; // 用于检查RP（研究点）以启用按钮
 
        private void Awake()
        {
            nameText = transform.Find("NameText").GetComponent<Text>();
            descriptionText = transform.Find("DescriptionText").GetComponent<Text>();
            statusText = transform.Find("StatusText").GetComponent<Text>();
            researchButton = transform.Find("ResearchButton").GetComponent<Button>();
            progressBar = transform.Find("ProgressBar").GetComponent<Slider>();
            
            researchButton.onClick.AddListener((() =>
            {
                Debug.Log("开始研究");
            }));
            
        

        }

        public void Setup(Technology tech, ResearchSystem researchSystem, ResourceModel resourceModel)
        {
            mTechId = tech.Id; // 缓存 ID
            mTechnology = tech;
            mResearchSystem = researchSystem;
            mResourceModel = resourceModel;

            nameText.text = mTechnology.Name;
            descriptionText.text = mTechnology.Description;

            // 删除这行，默认不再隐藏进度条
            // if (progressBar != null) progressBar.gameObject.SetActive(false);

            if (mTechnology.Status == ResearchStatus.Available)
            {
                if (statusText != null) statusText.text = $"Cost: {mTechnology.ResearchPointCost} RP";
                if (researchButton != null)
                {
                    researchButton.gameObject.SetActive(true);
                    researchButton.onClick.RemoveAllListeners();
                    researchButton.onClick.AddListener(StartResearch_OnClick);
                    researchButton.interactable = true;
                }
            }
            else if (mTechnology.Status == ResearchStatus.InProgress)
            {
                if (statusText != null) statusText.text = "In Progress...";
                if (researchButton != null) researchButton.gameObject.SetActive(false);
                if (progressBar != null)
                {
                    progressBar.gameObject.SetActive(true);
                    progressBar.value = mResearchSystem.GetCurrentResearchProgressNormalized();
                }
            }
            else if (mTechnology.Status == ResearchStatus.Completed)
            {
                if (statusText != null) statusText.text = "Status: Completed";
                if (researchButton != null) researchButton.gameObject.SetActive(false);
            }
            else
            {
                if (statusText != null)
                    statusText.text = $"Status: Locked (Requires: {string.Join(", ", mTechnology.PrerequisiteTechIds)})";
                if (researchButton != null) researchButton.gameObject.SetActive(false);
            }
        }

        void StartResearch_OnClick()
        {
            Debug.Log("研究中");
            if (mResearchSystem != null && mTechnology != null)
            {
                if (mResourceModel.GetAmount(GameResourceType.ResearchPoints) >= mTechnology.ResearchPointCost)
                {
                    if (!mResearchSystem.IsResearching()) // 再次检查是否有其他研究已开始
                    {
                        mResearchSystem.StartResearch(mTechnology.Id);
                        // 可选：父UI (ResearchDisplay) 应刷新以移动此项目
                    }
                    else
                    {
                         Debug.LogWarning("另一项研究已在进行中。");
                    }
                }
                else
                {
                    Debug.LogWarning("研究点不足。");
                }
            }
        }
        
        private string mTechId; // 缓存技术 ID，用于匹配

public string GetTechnologyId()
{
    return mTechId;
}

public bool Matches(Technology tech)
{
    return mTechId == tech.Id;
}


    }
}
