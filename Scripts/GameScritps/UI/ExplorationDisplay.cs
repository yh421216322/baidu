using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // 用于 Select 和 Join
using System; // 用于 Guid
using QFramework;
using YourGameNamespace.Exploration;
using YourGameNamespace.Survivors; // 用于 SurvivorModel (获取幸存者名称以显示)

namespace YourGameNamespace.UI
{
    public class ExplorationDisplay : MonoBehaviour
    {
        [Header("Display Texts")]
        public Text availablePOIsText;
        public Text activeExpeditionsText;
        public Text expeditionReportText; // 用于一般消息和上次结果

        [Header("Input Fields")]
        public InputField poiIdInput;
        public InputField survivorIdsInput; // 逗号分隔的 Guid
        public Button startExpeditionButton;

        private ExplorationModel mExplorationModel;
        private ExplorationSystem mExplorationSystem;
        private SurvivorModel mSurvivorModel;

        // 用于存储上次完成远征的结果以供显示
        private static ExpeditionOutcome s_lastOutcome = null; 

        void Start()
        {
            // 对 GameArchitecture 初始化进行防御性检查
            if (GameArchitecture.Interface == null)
            {
                 Debug.LogError("探索显示：GameArchitecture.Interface 为空。请确保 GameInitializer 先运行。");
                 enabled = false; // 如果架构未准备好，则禁用此组件
                 return;
            }

            mExplorationModel = GameArchitecture.Interface.GetModel<ExplorationModel>();
            mExplorationSystem = GameArchitecture.Interface.GetSystem<ExplorationSystem>();
            mSurvivorModel = GameArchitecture.Interface.GetModel<SurvivorModel>();

            if (startExpeditionButton != null)
            {
                startExpeditionButton.onClick.AddListener(TryStartExpeditionFromInput);
            }
            else Debug.LogError("探索显示：未在检视面板中分配 StartExpeditionButton。");

            // 注册一个事件，当远征完成时 ExplorationSystem 可以发送该事件
            // 这比轮询更好。目前，为简单起见，我们将从 ResolveExpeditionOutcome 手动设置 s_lastOutcome。
            // 示例: QFramework.TypeEventSystem.Global.Register<ExpeditionCompletedEvent>(OnExpeditionCompleted);

            RefreshDisplay();
        }
        
        // 如果使用QF事件，事件处理程序示例
        // public void OnExpeditionCompleted(ExpeditionCompletedEvent e) {
        //    s_lastOutcome = e.Expedition.Outcome;
        //    expeditionReportText.text = "Expedition Completed!\n" + FormatOutcome(s_lastOutcome);
        //    RefreshDisplay();
        // }
        // public void OnDestroy() {
        //    QFramework.TypeEventSystem.Global.UnRegister<ExpeditionCompletedEvent>(OnExpeditionCompleted);
        // }


        void Update()
        {
            // 定期刷新显示。可以通过事件进行优化。
            if (UnityEngine.Time.frameCount % 30 == 0) // 大约每秒刷新两次
            {
                RefreshDisplay();
            }
        }

        void RefreshDisplay()
        {
            if (mExplorationModel == null || mSurvivorModel == null) return;

            // 1. 可用POI
            if (availablePOIsText != null)
            {
                System.Text.StringBuilder poiSb = new System.Text.StringBuilder("Available Points of Interest:\n");
                var availablePois = mExplorationModel.GetAvailablePOIs();
                if (availablePois.Count == 0) poiSb.AppendLine("None");
                foreach (var poi in availablePois)
                {
                    poiSb.AppendLine($"ID: {poi.Id}, Name: {poi.Name}, Diff: {poi.Difficulty}, Time: {poi.BaseExplorationTime}s, Slots: {poi.MaxSurvivorSlots}, Status: {poi.Status}");
                    poiSb.AppendLine($"  Rewards: {string.Join(", ", poi.PotentialRewards.Select(r => r.ResourceType.ToString()))}");
                    poiSb.AppendLine();
                }
                availablePOIsText.text = poiSb.ToString();
            }

            // 2. 活动中的远征
            if (activeExpeditionsText != null)
            {
                System.Text.StringBuilder expSb = new System.Text.StringBuilder("Active Expeditions:\n");
                var activeExpeditions = mExplorationModel.GetActiveExpeditions();
                if (activeExpeditions.Count == 0) expSb.AppendLine("None");
                foreach (var exp in activeExpeditions)
                {
                    string survivorNames = string.Join(", ", exp.AssignedSurvivorIds.Select(id => mSurvivorModel.GetSurvivorById(id)?.Name ?? "Unknown"));
                    expSb.AppendLine($"ID: {exp.ExpeditionId.ToString().Substring(0, 8)}, POI: {exp.TargetPoiId}, Status: {exp.Status}, Phase Time: {exp.TimeElapsedOnCurrentPhase:F1}s / {GetCurrentPhaseDuration(exp):F1}s, Survivors: ({survivorNames})");
                }
                activeExpeditionsText.text = expSb.ToString();
            }

            // 3. 上次远征报告 (如果有)
            // 一旦 ExplorationSystem 中的事件更新 s_lastOutcome，此部分将更加健壮
            if (expeditionReportText != null && s_lastOutcome != null)
            {
                 expeditionReportText.text = "Last Expedition Report:\n" + FormatOutcome(s_lastOutcome);
                 // 显示一次后可能会清除 s_lastOutcome，或者有一个专门的“清除报告”按钮
            }
        }
        
        private string FormatOutcome(ExpeditionOutcome outcome)
        {
            if (outcome == null) return "No outcome data.";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"Success: {outcome.WasSuccessful}");
            sb.AppendLine($"Log: {outcome.NarrativeLog}");
             if (outcome.ResourcesFound.Any()) {
                sb.AppendLine("Resources Found:");
                foreach(var res in outcome.ResourcesFound) sb.AppendLine($"- {res.Key}: {res.Value}");
            }
            if (outcome.SurvivorStatusChanges.Any()) {
                sb.AppendLine("Survivor Updates:");
                foreach(var change in outcome.SurvivorStatusChanges) sb.AppendLine($"- {change}");
            }
            return sb.ToString();
        }


        float GetCurrentPhaseDuration(Expedition exp)
        {
            switch (exp.Status)
            {
                case ExpeditionStatus.Departing: return exp.TravelTimeToPoi;
                case ExpeditionStatus.Exploring: return exp.ExplorationTimeAtPoi;
                case ExpeditionStatus.Returning: return exp.TravelTimeBackToBase;
                default: return 0f;
            }
        }

        void TryStartExpeditionFromInput()
        {
            if (mExplorationSystem == null)
            {
                expeditionReportText.text = "错误：探索系统 (ExplorationSystem) 不可用。";
                return;
            }

            string poiId = poiIdInput.text;
            string[] survivorIdStrings = survivorIdsInput.text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<Guid> survivorGuids = new List<Guid>();

            if (string.IsNullOrWhiteSpace(poiId))
            {
                expeditionReportText.text = "错误：POI ID 不能为空。";
                return;
            }

            if (survivorIdStrings.Length == 0)
            {
                expeditionReportText.text = "错误：必须至少提供一个幸存者ID。";
                return;
            }

            foreach (string idStr in survivorIdStrings)
            {
                if (Guid.TryParse(idStr.Trim(), out Guid guid))
                {
                    survivorGuids.Add(guid);
                }
                else
                {
                    expeditionReportText.text = $"错误：无效的幸存者GUID格式：'{idStr}'。";
                    return;
                }
            }
            
            // 使用 CanStartExpeditionToPOI 获取详细原因
            if (!mExplorationSystem.CanStartExpeditionToPOI(poiId, survivorGuids, out string reason))
            {
                expeditionReportText.text = $"无法开始远征：{reason}";
                return;
            }

            if (mExplorationSystem.StartExpedition(poiId, survivorGuids))
            {
                expeditionReportText.text = $"已开始前往POI：{poiId} 的远征，参与者：{survivorGuids.Count} 名幸存者。";
                poiIdInput.text = ""; // 清空输入字段
                survivorIdsInput.text = "";
                s_lastOutcome = null; // 清除上次结果显示
            }
            else
            {
                // 原因应该已由 CanStartExpeditionToPOI 捕获，但作为后备：
                expeditionReportText.text = "错误：未能开始远征。请检查控制台以获取详细信息。";
            }
            RefreshDisplay(); // 立即更新UI
        }
        
        // 静态方法，允许 ExplorationSystem 发布结果
        public static void DisplayOutcome(ExpeditionOutcome outcome)
        {
            s_lastOutcome = outcome;
        }
    }
}

// 最好为此使用QFramework的TypeEventSystem。
// 定义一个事件：
// public class ExpeditionCompletedEvent {
//    public Expedition Expedition;
//    public ExpeditionCompletedEvent(Expedition exp) { Expedition = exp; }
// }
// 在 ExplorationSystem.ResolveExpeditionOutcome 中，设置结果后：
//    this.SendEvent(new ExpeditionCompletedEvent(expedition));
// 然后 ExplorationDisplay 可以注册/取消注册此事件。
// 对于此子任务，静态 DisplayOutcome 是一个更简单的变通方法。

