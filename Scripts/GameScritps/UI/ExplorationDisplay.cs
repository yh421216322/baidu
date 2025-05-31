using UnityEngine;
using UnityEngine.UI; // 用于 UI Text 和 Button 组件
using System.Collections.Generic; // 用于 List
using System.Linq; // 用于 Linq 方法，如 Select 和 Join
using System; // 用于 Guid (全局唯一标识符)
using QFramework; // QFramework 框架
using YourGameNamespace.Exploration; // 探索系统相关命名空间
using YourGameNamespace.Survivors; // 用于 SurvivorModel (获取幸存者名称以显示)
using YourGameNamespace.Events; // For event definitions
using YourGameNamespace.Commands; // For StartExpeditionCommand

namespace YourGameNamespace.UI
{
    // UI组件，用于显示探索相关信息，并提供开始远征的交互界面
    public class ExplorationDisplay : MonoBehaviour, IController
    {
        [Header("显示文本区域")] // Unity检视面板中的分组标题
        public Text availablePOIsText;    // 用于显示可用兴趣点(POI)列表的Text组件
        public Text activeExpeditionsText; // 用于显示当前活动远征列表的Text组件
        public Text expeditionReportText;  // 用于显示一般消息和上次远征结果的Text组件

        [Header("输入字段区域")] // Unity检视面板中的分组标题
        public InputField poiIdInput;        // 用于输入目标POI ID的InputField组件
        public InputField survivorIdsInput;  // 用于输入参与远征的幸存者ID列表（逗号分隔的Guid）的InputField组件
        public Button startExpeditionButton; // 用于触发开始远征操作的Button组件

        // 对所需模型和系统的引用
        private ExplorationModel mExplorationModel; // Assuming direct model usage if IExplorationModel is not specified as pre-existing
        private SurvivorModel mSurvivorModel;         // Assuming direct model usage if ISurvivorModel is not specified as pre-existing
        private IExplorationSystem mExplorationSystem;
        // private static ExpeditionOutcome s_lastOutcome = null; // Removed

        public IArchitecture GetArchitecture() => GameArchitecture.Interface;

        void Start() // Unity生命周期方法，在第一次Update前执行
        {
            // 对 GameArchitecture 初始化进行防御性检查，确保核心架构已准备就绪
            if (GameArchitecture.Interface == null)
            {
                 Debug.LogError("探索显示 (ExplorationDisplay)：GameArchitecture.Interface 为空。请确保 GameInitializer 脚本先于此脚本运行。");
                 enabled = false; // 如果架构未准备好，则禁用此UI组件以防止错误
                 return;
            }

            // 从 GameArchitecture 获取所需模型和系统的实例
            mExplorationModel = this.GetModel<ExplorationModel>();
            mSurvivorModel = this.GetModel<SurvivorModel>();
            mExplorationSystem = this.GetSystem<IExplorationSystem>();


            if (startExpeditionButton != null) // 如果开始远征按钮已在检视面板中分配
            {
                // 为按钮的点击事件添加监听器，当按钮被点击时调用 TryStartExpeditionFromInput 方法
                startExpeditionButton.onClick.AddListener(TryStartExpeditionFromInput);
            }
            else Debug.LogError("探索显示 (ExplorationDisplay)：开始远征按钮 (startExpeditionButton) 未在Unity检视面板中分配！");

            // Register Event Listeners
            this.RegisterEvent<Model_POIStatusUpdatedEvent>(e => RefreshAvailablePOIs()).UnRegisterWhenGameObjectDestroyed(this);
            this.RegisterEvent<Model_ActiveExpeditionAddedEvent>(e => RefreshActiveExpeditions()).UnRegisterWhenGameObjectDestroyed(this);
            this.RegisterEvent<Model_ActiveExpeditionRemovedEvent>(e => RefreshActiveExpeditions()).UnRegisterWhenGameObjectDestroyed(this);
            this.RegisterEvent<Exploration_ExpeditionOutcomeResolvedEvent>(OnExpeditionOutcomeResolved).UnRegisterWhenGameObjectDestroyed(this);

            RefreshAvailablePOIs();
            RefreshActiveExpeditions();
            ClearExpeditionReport();
        }
        
        // Update() method removed

        private void RefreshAvailablePOIs()
        {
            if (mExplorationModel == null || availablePOIsText == null) return;

            System.Text.StringBuilder poiSb = new System.Text.StringBuilder("可探索的地点：\n");
            var availablePois = mExplorationModel.GetAvailablePOIs();
            if (availablePois.Count == 0) poiSb.AppendLine("无");
            foreach (var poi in availablePois)
            {
                poiSb.AppendLine($"ID: {poi.Id}, 名称: {poi.Name}, 难度: {poi.Difficulty}, 时间: {poi.BaseExplorationTime}秒, 槽位: {poi.MaxSurvivorSlots}, 状态: {poi.Status}");
                poiSb.AppendLine($"  潜在奖励: {string.Join("、", poi.PotentialRewards.Select(r => r.ResourceType.ToString()))}");
                poiSb.AppendLine();
            }
            availablePOIsText.text = poiSb.ToString();
        }

        private void RefreshActiveExpeditions()
        {
            if (mExplorationModel == null || mSurvivorModel == null || activeExpeditionsText == null) return;

            System.Text.StringBuilder expSb = new System.Text.StringBuilder("进行中的远征：\n");
            var activeExpeditions = mExplorationModel.GetActiveExpeditions();
            if (activeExpeditions.Count == 0) expSb.AppendLine("无");
            foreach (var exp in activeExpeditions)
            {
                // Using .Value for BindableProperty Name if Survivor.Name was refactored. Assuming it's still direct string for now based on previous context.
                string survivorNames = string.Join("、", exp.AssignedSurvivorIds.Select(id => {
                    var survivor = mSurvivorModel.GetSurvivorById(id);
                    // If Survivor.Name is a BindableProperty<string>, use survivor.Name.Value
                    // For this refactoring, assuming Survivor.Name is still directly accessible string or a property that returns string
                    return survivor != null ? survivor.Name.Value : "未知幸存者";
                }));
                expSb.AppendLine($"ID: {exp.ExpeditionId.ToString().Substring(0, 8)}, POI: {exp.TargetPoiId}, 状态: {exp.Status}, 阶段时间: {exp.TimeElapsedOnCurrentPhase:F1}秒 / {GetCurrentPhaseDuration(exp):F1}秒, 参与者: ({survivorNames})");
            }
            activeExpeditionsText.text = expSb.ToString();
        }

        private void OnExpeditionOutcomeResolved(Exploration_ExpeditionOutcomeResolvedEvent e)
        {
            if (expeditionReportText == null || e.Outcome == null) return;

            ExpeditionOutcome outcome = e.Outcome;
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine("远征报告：");
            sb.AppendLine($"是否成功：{(outcome.WasSuccessful ? "成功！" : "失败。")}");
            sb.AppendLine("叙事日志：");
            sb.AppendLine(outcome.NarrativeLog); // Assuming NarrativeLog is already localized or contains keys for localization

            if (outcome.ResourcesFound.Any())
            {
                sb.AppendLine("找到资源：");
                foreach (var res in outcome.ResourcesFound) sb.AppendLine($"- {res.Key}: {res.Value} 单位");
            }
            if (outcome.SurvivorStatusChanges.Any())
            {
                sb.AppendLine("幸存者状态更新：");
                foreach (var change in outcome.SurvivorStatusChanges) sb.AppendLine($"- {change}"); // Assuming changes are localized
            }
            expeditionReportText.text = sb.ToString();
        }

        private void ClearExpeditionReport()
        {
            if (expeditionReportText != null)
            {
                expeditionReportText.text = "尚无远征报告。";
            }
        }

        // 获取远征当前阶段的总时长
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

        // 尝试从输入字段的内容开始一次新的远征
        void TryStartExpeditionFromInput()
        {
            string poiId = poiIdInput.text;
            string[] survivorIdStrings = survivorIdsInput.text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<Guid> survivorGuids = new List<Guid>();

            if (string.IsNullOrWhiteSpace(poiId))
            {
                expeditionReportText.text = "错误：兴趣点 (POI) 的ID不能为空。";
                return;
            }

            if (survivorIdStrings.Length == 0)
            {
                expeditionReportText.text = "错误：必须至少选择一名幸存者参与远征。";
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
                    expeditionReportText.text = $"错误：输入的幸存者ID '{idStr.Trim()}' 格式无效。请输入有效的GUID。";
                    return;
                }
            }
            
            // Client-side check can be kept for immediate feedback, or rely on command failure events if implemented
            // For now, we will send the command directly.
            // string reasonForFailure;
            // if (!mExplorationSystem.CanStartExpeditionToPOI(poiId, survivorGuids, out reasonForFailure))
            // {
            //     expeditionReportText.text = $"无法开始远征：{reasonForFailure}";
            //     return;
            // }

            this.SendCommand(new StartExpeditionCommand(poiId, survivorGuids));
            expeditionReportText.text = "已发送远征指令..."; // Optional: immediate feedback

            poiIdInput.text = "";
            survivorIdsInput.text = "";
            // s_lastOutcome = null; // Removed, outcome display is handled by OnExpeditionOutcomeResolved
        }
        
        // Removed static method DisplayOutcome
    }
}
// Comments about event-driven UI updates are now implemented or outdated by this refactoring.
