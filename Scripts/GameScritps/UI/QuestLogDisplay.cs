using UnityEngine;
using UnityEngine.UI; // 用于 UI Text 组件
using System.Collections.Generic; // 用于 List
using System.Linq; // 用于 Linq 方法，如 .Any() 和 .Where()
using QFramework; // 用于事件注册和获取模型 (IController)
using YourGameNamespace.Quests; // 用于 QuestModel, QuestActivatedEvent 等任务相关类
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
        public IArchitecture GetArchitecture() => GameArchitecture.Interface; 

        void Start() // Unity生命周期方法，在第一次Update前执行
        {
            // 检查 GameArchitecture 是否已初始化
            if (GameArchitecture.Interface == null) {
                Debug.LogError("任务日志显示 (QuestLogDisplay)：GameArchitecture 尚未初始化。请确保 GameInitializer 脚本先于此脚本运行。");
                enabled = false; // 禁用此组件以防止错误
                return;
            }
            mQuestModel = this.GetModel<QuestModel>(); // 使用QFramework的扩展方法获取任务数据模型实例

            // 注册对任务相关事件的监听
            this.RegisterEvent<QuestActivatedEvent>(OnQuestActivated);         // 监听任务激活事件
            this.RegisterEvent<QuestSucceededEvent>(OnQuestSucceeded);         // 监听任务成功事件
            this.RegisterEvent<QuestObjectiveCompletedEvent>(OnQuestObjectiveCompleted); // 监听任务目标完成事件

            if (questNotificationText != null) questNotificationText.text = ""; // 初始化通知文本为空
            RefreshQuestDisplay(); // 初始刷新一次任务日志显示
        }

        void Update() // Unity生命周期方法，每帧调用一次
        {
            // 更新任务目标进度显示。
            // 注意：特定目标的更新也可以完全通过事件驱动来实现，以获得更好的性能。
            // 为简化起见，如果当前有任何活动任务，则定期刷新活动任务列表的显示，
            // 以便能反映出 CurrentAmount (当前数量) 的变化。
            if (mQuestModel != null && mQuestModel.ActiveQuests.Any() && UnityEngine.Time.frameCount % 30 == 0) // 大约每秒刷新两次
            {
                RefreshActiveQuestsDisplay();
            }

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
            RefreshQuestDisplay(); // 刷新任务日志
        }

        // 事件处理：当任务成功完成时调用
        private void OnQuestSucceeded(QuestSucceededEvent e)
        {
            ShowNotification($"任务完成：{e.SucceededQuest.Title}！"); // 显示通知
            RefreshQuestDisplay(); // 刷新任务日志
        }
        
        // 事件处理：当任务的某个目标完成时调用
        private void OnQuestObjectiveCompleted(QuestObjectiveCompletedEvent e)
        {
            ShowNotification($"目标完成：{e.Objective.Description} (任务：{e.ParentQuest.Title})"); // 显示通知
            RefreshQuestDisplay(); // 刷新任务日志以更新目标状态
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
        void RefreshQuestDisplay()
        {
            if (mQuestModel == null) return; // 安全检查
            RefreshActiveQuestsDisplay();
            RefreshCompletedQuestsDisplay(); // 如果 completedQuestsText 已分配，则刷新已完成任务列表
        }

        // 刷新活动任务列表的显示
        void RefreshActiveQuestsDisplay()
        {
            if (activeQuestsText == null || mQuestModel == null) return; // 安全检查

            System.Text.StringBuilder sb = new System.Text.StringBuilder("--- 当前任务 ---\n");
            // 直接使用 QuestModel 中维护的 ActiveQuests 列表
            var activeQuests = mQuestModel.ActiveQuests; 

            if (!activeQuests.Any()) // 如果没有活动任务
            {
                sb.AppendLine("无"); // 显示“无”
            }
            else
            {
                foreach (var quest in activeQuests) // 遍历所有活动任务
                {
                    // 注意：quest.Title 和 quest.Description 应已在 QuestModel 中被翻译为中文
                    sb.AppendLine($"[{quest.Title}] - {quest.Description}");
                    foreach (var obj in quest.Objectives) // 遍历任务的每个目标
                    {
                        string statusMark = obj.IsComplete ? "[√]" : "[ ]"; // 根据目标是否完成显示标记
                        // 注意：obj.Description 应已在 QuestModel 中被翻译为中文
                        sb.AppendLine($"  {statusMark} {obj.Description} ({obj.CurrentAmount}/{obj.RequiredAmount})");
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
            var completed = mQuestModel.AllQuests.Values.Where(q => q.Status == QuestStatus.Success).ToList();
            
            if (!completed.Any()) // 如果没有已完成的任务
            {
                sb.AppendLine("无"); // 显示“无”
            }
            else
            {
                foreach (var quest in completed) // 遍历所有已完成的任务
                {
                    // 注意：quest.Title 应已在 QuestModel 中被翻译为中文
                    sb.AppendLine($"- {quest.Title}");
                }
            }
            completedQuestsText.text = sb.ToString(); // 更新UI Text组件的文本
        }
        
        // Unity生命周期方法：当对象被销毁时调用
        void OnDestroy() 
        {
            // 注销之前注册的事件是非常重要的，以防止在对象销毁后事件系统仍然尝试调用其方法，导致错误或内存泄漏。
            if (GameArchitecture.Interface != null) { // 检查 GameArchitecture 是否仍然存在 (例如，在应用退出时可能已被销毁)
                this.UnRegisterEvent<QuestActivatedEvent>(OnQuestActivated);
                this.UnRegisterEvent<QuestSucceededEvent>(OnQuestSucceeded);
                this.UnRegisterEvent<QuestObjectiveCompletedEvent>(OnQuestObjectiveCompleted);
            }
        }
    }
}
