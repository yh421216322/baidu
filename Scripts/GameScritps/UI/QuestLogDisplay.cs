using UnityEngine;
using UnityEngine.UI; // 用于 UI Text 组件
using System.Collections.Generic; // 用于 List
using System.Linq;
using MyGameNamespace; // 用于 Linq 方法，如 .Any() 和 .Where()
using QFramework; // 用于事件注册和获取模型 (IController)
using YourGameNamespace.Quests; // 用于 QuestModel, Quest, QuestStatus, etc.
using YourGameNamespace.Events; // 用于具体的任务事件类

namespace YourGameNamespace.UI
{
    // UI组件，用于显示任务日志，包括活动任务、已完成任务以及任务相关的通知
    public class QuestLogDisplay : MonoBehaviour, IController // 实现 IController 接口以方便使用QFramework的 this.RegisterEvent 等扩展方法
    {
        public Text activeQuestsText;      // 在Unity检视面板中分配，用于显示活动任务列表的Text组件
        public Text completedQuestsText;   // 可选，在Unity检视面板中分配，用于显示已完成任务列表的Text组件
        public Text questNotificationText; // 在Unity检视面板中分配，用于显示简短的任务通知消息（如“新任务已接取”）

        private QuestModel mQuestModel; // 对任务数据模型的引用
        private float mNotificationTimeLeft = 0f; // 当前通知消息剩余显示时间
        private const float NOTIFICATION_DURATION = 3.0f; // 通知消息默认显示时长（秒）

        // IController 接口要求实现此方法，返回当前游戏架构的实例
        public IArchitecture GetArchitecture() => RegisterManager.Interface;

        private void Awake()
        {
            activeQuestsText = activeQuestsText ?? transform.Find("ActiveQuestsText")?.GetComponent<Text>();
            completedQuestsText = completedQuestsText ?? transform.Find("CompletedQuestsText")?.GetComponent<Text>();
            questNotificationText = questNotificationText ?? transform.Find("QuestNotificationText")?.GetComponent<Text>();

            if (activeQuestsText == null) Debug.LogError("QuestLogDisplay: UI元素 'ActiveQuestsText' 未能成功获取或链接。");
            if (completedQuestsText == null) Debug.LogWarning("QuestLogDisplay: UI元素 'CompletedQuestsText' 未链接 (可选)。"); // Optional field
            if (questNotificationText == null) Debug.LogError("QuestLogDisplay: UI元素 'QuestNotificationText' 未能成功获取或链接。");
        }

        void Start() // Unity生命周期方法，在第一次Update前执行
        {
            // 检查 GameArchitecture 是否已初始化
            if (RegisterManager.Interface == null) {
                Debug.LogError("任务日志显示 (QuestLogDisplay)：GameArchitecture 尚未初始化。请确保 GameInitializer 脚本先于此脚本运行。");
                enabled = false; // 禁用此组件以防止错误
                if (activeQuestsText != null) activeQuestsText.text = "错误：任务系统未初始化";
                return;
            }
            mQuestModel = this.GetModel<QuestModel>(); // 使用QFramework的扩展方法获取任务数据模型实例

            if (mQuestModel != null)
            {
                // 注册对任务相关事件的监听 (用于通知)
                this.RegisterEvent<QuestActivatedEvent>(OnQuestActivated).UnRegisterWhenGameObjectDestroyed(this.gameObject);
                this.RegisterEvent<QuestSucceededEvent>(OnQuestSucceeded).UnRegisterWhenGameObjectDestroyed(this.gameObject);
                this.RegisterEvent<QuestObjectiveCompletedEvent>(OnQuestObjectiveCompleted).UnRegisterWhenGameObjectDestroyed(this.gameObject);

                // 当任何任务状态更新时，刷新整个列表
                this.RegisterEvent<Model_QuestStatusUpdatedEvent>(e => RefreshQuestList()).UnRegisterWhenGameObjectDestroyed(this.gameObject);

                RefreshQuestList(); // 初始刷新
            }
            else
            {
                Debug.LogError("QuestLogDisplay: 未能获取到 QuestModel！");
                if (activeQuestsText != null) activeQuestsText.text = "错误：任务系统未初始化";
            }

            if (questNotificationText != null) questNotificationText.text = ""; // 初始化通知文本为空
        }

        void Update() // Unity生命周期方法，每帧调用一次
        {
            // 处理通知消息的显示计时
            if (mNotificationTimeLeft > 0)
            {
                mNotificationTimeLeft -= UnityEngine.Time.deltaTime; // 减少剩余显示时间
                if (mNotificationTimeLeft <= 0 && questNotificationText != null)
                {
                    questNotificationText.text = ""; // 时间到了则清空通知文本
                }
            }
        }
        
        // 事件处理：当任务被激活时调用
        private void OnQuestActivated(QuestActivatedEvent e)
        {
            ShowNotification($"新任务已接取：{e.ActivatedQuest.Title}"); // 显示通知
            // RefreshQuestList(); // Model_QuestStatusUpdatedEvent will handle this
        }

        // 事件处理：当任务成功完成时调用
        private void OnQuestSucceeded(QuestSucceededEvent e)
        {
            ShowNotification($"任务完成：{e.SucceededQuest.Title}！"); // 显示通知
            // RefreshQuestList(); // Model_QuestStatusUpdatedEvent will handle this
        }
        
        // 事件处理：当任务的某个目标完成时调用
        private void OnQuestObjectiveCompleted(QuestObjectiveCompletedEvent e)
        {
            ShowNotification($"目标完成：{e.Objective.Description} (任务：{e.ParentQuest.Title})"); // 显示通知
            // RefreshQuestList(); // Model_QuestStatusUpdatedEvent will handle this
        }

        // 在UI上显示一条通知消息
        void ShowNotification(string message)
        {
            if (questNotificationText != null) // 确保通知文本组件已分配
            {
                Debug.Log($"任务日志通知：{message}"); // 同时在控制台记录日志，方便调试
                questNotificationText.text = message; // 设置UI文本
                mNotificationTimeLeft = NOTIFICATION_DURATION; // 重置通知显示计时
            }
        }

        // 刷新整个任务日志的显示（包括活动任务和已完成任务）
        // Renamed from RefreshQuestDisplay to RefreshQuestList
        void RefreshQuestList()
        {
            if (mQuestModel == null)
            {
                if(activeQuestsText != null) activeQuestsText.text = "错误：任务数据模型不可用。";
                if(completedQuestsText != null) completedQuestsText.text = "";
                return;
            }
            RefreshActiveQuestsDisplay();
            RefreshCompletedQuestsDisplay(); // 如果 completedQuestsText 已分配，则刷新已完成任务列表
        }

        // 刷新活动任务列表的显示
        void RefreshActiveQuestsDisplay()
        {
            if (activeQuestsText == null || mQuestModel == null) return; // 安全检查

            System.Text.StringBuilder sb = new System.Text.StringBuilder("--- 当前任务 ---\n");
            // Get active quests from the model
            var activeQuests = mQuestModel.AllQuests.Values.Where(q => q.Status.Value == QuestStatus.Active).ToList();

            if (!activeQuests.Any()) // 如果没有活动任务
            {
                sb.AppendLine("无"); // 显示“无”
            }
            else
            {
                foreach (var quest in activeQuests) // 遍历所有活动任务
                {
                    // Quest.Title and Quest.Description are assumed to be localized
                    sb.AppendLine($"[{quest.Title}] - {quest.Description}");
                    sb.AppendLine($"状态：{GetLocalizedQuestStatus(quest.Status.Value)}"); // Localized status
                    foreach (var obj in quest.Objectives) // 遍历任务的每个目标
                    {
                        // obj.IsComplete directly uses CurrentAmount.Value internally
                        string statusMark = obj.IsComplete ? "[√]" : "[ ]";
                        // obj.Description is assumed to be localized
                        // obj.CurrentAmount.Value for progress
                        sb.AppendLine($"  {statusMark} 目标：{obj.Description} (进度：{obj.CurrentAmount.Value}/{obj.RequiredAmount})");
                    }
                    sb.AppendLine(); // 添加空行以分隔不同任务
                }
            }
            activeQuestsText.text = sb.ToString(); // 更新UI Text组件的文本
        }

        // 刷新已完成任务列表的显示 (可选功能)
        void RefreshCompletedQuestsDisplay()
        {
            if (completedQuestsText == null || mQuestModel == null) return; // 如果未分配此UI组件，则不执行

            System.Text.StringBuilder sb = new System.Text.StringBuilder("--- 已完成的任务 ---\n");
            // 从所有任务中筛选出状态为“成功”的任务
            var completed = mQuestModel.AllQuests.Values.Where(q => q.Status.Value == QuestStatus.Success).ToList();
            
            if (!completed.Any()) // 如果没有已完成的任务
            {
                sb.AppendLine("无"); // 显示“无”
            }
            else
            {
                foreach (var quest in completed) // 遍历所有已完成的任务
                {
                    // quest.Title is assumed to be localized
                    sb.AppendLine($"- {quest.Title} (状态：{GetLocalizedQuestStatus(quest.Status.Value)})");
                }
            }
            completedQuestsText.text = sb.ToString(); // 更新UI Text组件的文本
        }

        private string GetLocalizedQuestStatus(QuestStatus status)
        {
            switch (status)
            {
                case QuestStatus.NotStarted: return "未开始";
                case QuestStatus.Active: return "进行中";
                case QuestStatus.Success: return "已成功";
                case QuestStatus.Failure: return "已失败";
                default: return status.ToString();
            }
        }

        // OnDestroy() is no longer needed for manual unregistration if all events use UnRegisterWhenGameObjectDestroyed
        // If any events were registered without it, OnDestroy would be needed.
        // For this refactor, assuming all relevant events will use UnRegisterWhenGameObjectDestroyed.
        void OnDestroy()
        {
            // Manual unregistration is not strictly necessary if all
            // QFramework event registrations use .UnRegisterWhenGameObjectDestroyed(this)
            // However, if any were missed or registered differently, they'd go here.
            // For example, if QuestActivatedEvent etc. were not auto-unregistered:
            // if (GameArchitecture.Interface != null && mQuestModel != null) {
            //     this.UnRegisterEvent<QuestActivatedEvent>(OnQuestActivated);
            //     this.UnRegisterEvent<QuestSucceededEvent>(OnQuestSucceeded);
            //     this.UnRegisterEvent<QuestObjectiveCompletedEvent>(OnQuestObjectiveCompleted);
            // }
        }
    }
}
