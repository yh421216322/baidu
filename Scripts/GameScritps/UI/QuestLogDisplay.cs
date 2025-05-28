using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using QFramework; // 用于事件注册
using YourGameNamespace.Quests;
using YourGameNamespace.Events; // 用于任务事件

namespace YourGameNamespace.UI
{
    public class QuestLogDisplay : MonoBehaviour, IController // 实现 IController 以方便使用 this.RegisterEvent
    {
        public Text activeQuestsText;
        public Text completedQuestsText; // 可选
        public Text questNotificationText; // 用于简短消息

        private QuestModel mQuestModel;
        private float mNotificationTimeLeft = 0f;
        private const float NOTIFICATION_DURATION = 3.0f;

        public IArchitecture GetArchitecture() => GameArchitecture.Interface; // IController 所需


        void Start()
        {
            if (GameArchitecture.Interface == null) {
                Debug.LogError("任务日志显示：GameArchitecture 未就绪。请确保 GameInitializer 先运行。");
                enabled = false; return;
            }
            mQuestModel = this.GetModel<QuestModel>(); // IController 的 QF 扩展

            // 注册事件
            this.RegisterEvent<QuestActivatedEvent>(OnQuestActivated);
            this.RegisterEvent<QuestSucceededEvent>(OnQuestSucceeded);
            this.RegisterEvent<QuestObjectiveCompletedEvent>(OnQuestObjectiveCompleted);


            if (questNotificationText != null) questNotificationText.text = "";
            RefreshQuestDisplay();
        }

        void Update()
        {
            // 更新目标进度显示（特定目标更新也可以是事件驱动的）
            // 为简单起见，如果任何任务正在进行中，则刷新活动任务以显示CurrentAmount的变化。
            if (mQuestModel != null && mQuestModel.ActiveQuests.Any() && UnityEngine.Time.frameCount % 30 == 0) // 定期刷新活动任务
            {
                RefreshActiveQuestsDisplay();
            }

            if (mNotificationTimeLeft > 0)
            {
                mNotificationTimeLeft -= UnityEngine.Time.deltaTime;
                if (mNotificationTimeLeft <= 0 && questNotificationText != null)
                {
                    questNotificationText.text = "";
                }
            }
        }
        
        private void OnQuestActivated(QuestActivatedEvent e)
        {
            ShowNotification($"新任务：{e.ActivatedQuest.Title}");
            RefreshQuestDisplay();
        }

        private void OnQuestSucceeded(QuestSucceededEvent e)
        {
            ShowNotification($"任务完成：{e.SucceededQuest.Title}！");
            RefreshQuestDisplay();
        }
        
        private void OnQuestObjectiveCompleted(QuestObjectiveCompletedEvent e)
        {
            ShowNotification($"目标完成：{e.Objective.Description} (任务：{e.ParentQuest.Title})");
            RefreshQuestDisplay(); // 刷新以更新目标状态
        }


        void ShowNotification(string message)
        {
            if (questNotificationText != null)
            {
                Debug.Log($"任务日志通知：{message}");
                questNotificationText.text = message;
                mNotificationTimeLeft = NOTIFICATION_DURATION;
            }
        }

        void RefreshQuestDisplay()
        {
            if (mQuestModel == null) return;
            RefreshActiveQuestsDisplay();
            RefreshCompletedQuestsDisplay(); // 可选
        }

        void RefreshActiveQuestsDisplay()
        {
            if (activeQuestsText == null || mQuestModel == null) return;

            System.Text.StringBuilder sb = new System.Text.StringBuilder("--- Active Quests ---\n");
            // 直接使用 mQuestModel.ActiveQuests，因为它由 QuestModel.UpdateQuestStatus 维护
            var activeQuests = mQuestModel.ActiveQuests; 

            if (!activeQuests.Any())
            {
                sb.AppendLine("None");
            }
            else
            {
                foreach (var quest in activeQuests)
                {
                    sb.AppendLine($"[{quest.Title}] - {quest.Description}");
                    foreach (var obj in quest.Objectives)
                    {
                        string statusMark = obj.IsComplete ? "[X]" : "[ ]";
                        sb.AppendLine($"  {statusMark} {obj.Description} ({obj.CurrentAmount}/{obj.RequiredAmount})");
                    }
                    sb.AppendLine();
                }
            }
            activeQuestsText.text = sb.ToString();
        }

        void RefreshCompletedQuestsDisplay()
        {
            if (completedQuestsText == null || mQuestModel == null) return; // 可选字段

            System.Text.StringBuilder sb = new System.Text.StringBuilder("--- Completed Quests ---\n");
            var completed = mQuestModel.AllQuests.Values.Where(q => q.Status == QuestStatus.Success).ToList();
            
            if (!completed.Any())
            {
                sb.AppendLine("None");
            }
            else
            {
                foreach (var quest in completed)
                {
                    sb.AppendLine($"- {quest.Title}");
                }
            }
            completedQuestsText.text = sb.ToString();
        }
        
        void OnDestroy() // 注销事件很重要
        {
            if (GameArchitecture.Interface != null) { // 检查是否为空（例如在应用退出时）
                this.UnRegisterEvent<QuestActivatedEvent>(OnQuestActivated);
                this.UnRegisterEvent<QuestSucceededEvent>(OnQuestSucceeded);
                this.UnRegisterEvent<QuestObjectiveCompletedEvent>(OnQuestObjectiveCompleted);
            }
        }
    }
}
