using UnityEngine;
using UnityEngine.UI; // 用于 UI Text 和 Button 组件
using System.Collections.Generic; // 用于 List
using System.Linq; // 用于 Linq 方法，如 Select 和 Join
using System; // 用于 Guid (全局唯一标识符)
using QFramework; // QFramework 框架
using YourGameNamespace.Exploration; // 探索系统相关命名空间
using YourGameNamespace.Survivors; // 用于 SurvivorModel (获取幸存者名称以显示)

namespace YourGameNamespace.UI
{
    // UI组件，用于显示探索相关信息，并提供开始远征的交互界面
    public class ExplorationDisplay : MonoBehaviour
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
        private ExplorationModel mExplorationModel;
        private ExplorationSystem mExplorationSystem;
        private SurvivorModel mSurvivorModel;

        // 静态变量，用于存储上次完成远征的结果，以便在UI上显示
        // 注意：静态变量在场景切换或代码重载后可能不会被重置，需要谨慎管理其生命周期。
        private static ExpeditionOutcome s_lastOutcome = null; 

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
            mExplorationModel = GameArchitecture.Interface.GetModel<ExplorationModel>();
            mExplorationSystem = GameArchitecture.Interface.GetSystem<ExplorationSystem>();
            mSurvivorModel = GameArchitecture.Interface.GetModel<SurvivorModel>();

            if (startExpeditionButton != null) // 如果开始远征按钮已在检视面板中分配
            {
                // 为按钮的点击事件添加监听器，当按钮被点击时调用 TryStartExpeditionFromInput 方法
                startExpeditionButton.onClick.AddListener(TryStartExpeditionFromInput);
            }
            else Debug.LogError("探索显示 (ExplorationDisplay)：开始远征按钮 (startExpeditionButton) 未在Unity检视面板中分配！");

            // 关于事件驱动更新的说明：
            // 注册一个事件监听器，当远征完成时由 ExplorationSystem 发送事件来更新UI，
            // 这种方式通常比在Update中轮询数据更高效。
            // 当前为简化实现，s_lastOutcome 是由 ExplorationSystem 通过静态方法 DisplayOutcome 手动设置的。
            // 示例 (使用QFramework事件系统): 
            // QFramework.TypeEventSystem.Global.Register<ExpeditionCompletedEvent>(OnExpeditionCompleted);

            RefreshDisplay(); // 初始刷新一次UI显示内容
        }
        
        // 如果使用QFramework事件系统，可以定义如下的事件处理程序示例：
        // public void OnExpeditionCompleted(ExpeditionCompletedEvent e) {
        //    s_lastOutcome = e.Expedition.Outcome; // 更新上次远征结果
        //    expeditionReportText.text = "远征完成！\n" + FormatOutcome(s_lastOutcome); // 在UI上显示完成信息和结果详情
        //    RefreshDisplay(); // 刷新整个探索显示界面
        // }
        // // 别忘了在组件销毁时取消注册事件，以避免内存泄漏
        // public void OnDestroy() {
        //    QFramework.TypeEventSystem.Global.UnRegister<ExpeditionCompletedEvent>(OnExpeditionCompleted);
        // }


        void Update() // Unity生命周期方法，每帧调用一次
        {
            // 定期刷新显示内容。可以通过事件驱动的方式进行优化，以避免不必要的每帧更新。
            if (UnityEngine.Time.frameCount % 30 == 0) // 大约每秒刷新两次 (基于帧率假设)
            {
                RefreshDisplay();
            }
        }

        // 刷新UI显示内容
        void RefreshDisplay()
        {
            if (mExplorationModel == null || mSurvivorModel == null) return; // 安全检查，确保所需模型存在

            // 1. 显示可用兴趣点(POI)列表
            if (availablePOIsText != null)
            {
                System.Text.StringBuilder poiSb = new System.Text.StringBuilder("可探索的地点：\n");
                var availablePois = mExplorationModel.GetAvailablePOIs();
                if (availablePois.Count == 0) poiSb.AppendLine("无"); // 如果没有可用的POI，则显示“无”
                foreach (var poi in availablePois)
                {
                    // 注意：poi.Name 应已被翻译为中文
                    poiSb.AppendLine($"ID: {poi.Id}, 名称: {poi.Name}, 难度: {poi.Difficulty}, 时间: {poi.BaseExplorationTime}秒, 槽位: {poi.MaxSurvivorSlots}, 状态: {poi.Status}");
                    poiSb.AppendLine($"  潜在奖励: {string.Join("、", poi.PotentialRewards.Select(r => r.ResourceType.ToString()))}"); // 将资源类型列表连接成字符串
                    poiSb.AppendLine();
                }
                availablePOIsText.text = poiSb.ToString();
            }

            // 2. 显示当前活动中的远征列表
            if (activeExpeditionsText != null)
            {
                System.Text.StringBuilder expSb = new System.Text.StringBuilder("进行中的远征：\n");
                var activeExpeditions = mExplorationModel.GetActiveExpeditions();
                if (activeExpeditions.Count == 0) expSb.AppendLine("无"); // 如果没有活动远征，则显示“无”
                foreach (var exp in activeExpeditions)
                {
                    // 获取参与远征的幸存者名称列表，如果找不到幸存者则显示“未知”
                    string survivorNames = string.Join("、", exp.AssignedSurvivorIds.Select(id => mSurvivorModel.GetSurvivorById(id)?.Name ?? "未知幸存者"));
                    expSb.AppendLine($"ID: {exp.ExpeditionId.ToString().Substring(0, 8)}, POI: {exp.TargetPoiId}, 状态: {exp.Status}, 阶段时间: {exp.TimeElapsedOnCurrentPhase:F1}秒 / {GetCurrentPhaseDuration(exp):F1}秒, 幸存者: ({survivorNames})");
                }
                activeExpeditionsText.text = expSb.ToString();
            }

            // 3. 显示上次远征的报告 (如果存在)
            // 当 ExplorationSystem 中的事件更新 s_lastOutcome 时，此部分的显示会更加健壮和及时。
            if (expeditionReportText != null && s_lastOutcome != null)
            {
                 expeditionReportText.text = "上次远征报告：\n" + FormatOutcome(s_lastOutcome);
                 // 考虑在显示一次后清除 s_lastOutcome，或者提供一个专门的“清除报告”按钮来由玩家控制。
            }
        }
        
        // 格式化远征结果对象为可读的字符串
        private string FormatOutcome(ExpeditionOutcome outcome)
        {
            if (outcome == null) return "无远征结果数据。";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            sb.AppendLine($"是否成功：{(outcome.WasSuccessful ? "是" : "否")}"); // 根据布尔值显示“是”或“否”
            sb.AppendLine($"日志记录：\n{outcome.NarrativeLog}"); // 显示叙事日志 (应已包含中文)
             if (outcome.ResourcesFound.Any()) { // 如果找到了任何资源
                sb.AppendLine("找到的资源：");
                foreach(var res in outcome.ResourcesFound) sb.AppendLine($"- {res.Key}: {res.Value} 单位");
            }
            if (outcome.SurvivorStatusChanges.Any()) { // 如果有幸存者状态发生变化
                sb.AppendLine("幸存者状态更新：");
                foreach(var change in outcome.SurvivorStatusChanges) sb.AppendLine($"- {change}"); // 状态变化描述应已为中文
            }
            return sb.ToString();
        }

        // 获取远征当前阶段的总时长
        float GetCurrentPhaseDuration(Expedition exp)
        {
            switch (exp.Status)
            {
                case ExpeditionStatus.Departing: return exp.TravelTimeToPoi;    // 出发阶段时长
                case ExpeditionStatus.Exploring: return exp.ExplorationTimeAtPoi; // 探索阶段时长
                case ExpeditionStatus.Returning: return exp.TravelTimeBackToBase; // 返回阶段时长
                default: return 0f; // 其他状态（如准备中、已完成）无特定阶段时长
            }
        }

        // 尝试从输入字段的内容开始一次新的远征
        void TryStartExpeditionFromInput()
        {
            if (mExplorationSystem == null) // 检查探索系统是否可用
            {
                expeditionReportText.text = "错误：探索系统 (ExplorationSystem) 当前不可用。";
                return;
            }

            string poiId = poiIdInput.text; // 获取输入的POI ID
            // 将逗号分隔的幸存者ID字符串分割成数组，并移除空条目
            string[] survivorIdStrings = survivorIdsInput.text.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            List<Guid> survivorGuids = new List<Guid>(); // 用于存储转换后的幸存者GUID列表

            if (string.IsNullOrWhiteSpace(poiId)) // 检查POI ID是否为空
            {
                expeditionReportText.text = "错误：兴趣点 (POI) 的ID不能为空。";
                return;
            }

            if (survivorIdStrings.Length == 0) // 检查是否选择了幸存者
            {
                expeditionReportText.text = "错误：必须至少选择一名幸存者参与远征。";
                return;
            }

            // 解析输入的幸存者ID字符串
            foreach (string idStr in survivorIdStrings)
            {
                if (Guid.TryParse(idStr.Trim(), out Guid guid)) // 尝试将字符串转换为GUID
                {
                    survivorGuids.Add(guid); // 添加到GUID列表
                }
                else // 如果转换失败
                {
                    expeditionReportText.text = $"错误：输入的幸存者ID '{idStr.Trim()}' 格式无效。请输入有效的GUID。";
                    return;
                }
            }
            
            // 使用 ExplorationSystem 的 CanStartExpeditionToPOI 方法检查是否可以开始远征，并获取详细原因
            if (!mExplorationSystem.CanStartExpeditionToPOI(poiId, survivorGuids, out string reason))
            {
                expeditionReportText.text = $"无法开始远征：{reason}"; // 显示无法开始的原因 (reason应已为中文)
                return;
            }

            // 如果所有检查通过，则开始远征
            if (mExplorationSystem.StartExpedition(poiId, survivorGuids))
            {
                expeditionReportText.text = $"已成功派遣远征队前往兴趣点 (POI)：{poiId}，参与者共 {survivorGuids.Count} 名幸存者。";
                poiIdInput.text = ""; // 清空POI ID输入字段
                survivorIdsInput.text = ""; // 清空幸存者ID输入字段
                s_lastOutcome = null; // 清除上次远征结果的显示，为新的报告做准备
            }
            else
            {
                // 此处的失败原因理论上应已被 CanStartExpeditionToPOI 捕获并显示，
                // 但作为后备错误处理，提供一个通用错误消息。
                expeditionReportText.text = "错误：未能成功开始远征。请检查控制台日志以获取更详细的信息。";
            }
            RefreshDisplay(); // 立即刷新UI显示，以反映远征状态的变化
        }
        
        // 静态方法，允许 ExplorationSystem 在远征完成后调用此方法来更新UI显示的上次远征结果
        public static void DisplayOutcome(ExpeditionOutcome outcome)
        {
            s_lastOutcome = outcome; // 更新静态变量，RefreshDisplay方法会在下次调用时使用此新结果
        }
    }
}

// 关于事件驱动UI更新的建议（非当前实现）：
// 为了获得更佳的性能和代码结构，推荐使用QFramework的TypeEventSystem来实现UI更新。
// 1. 定义一个事件类，例如：
//    public class ExpeditionCompletedEvent {
//        public Expedition Expedition; // 包含完成的远征对象
//        public ExpeditionCompletedEvent(Expedition exp) { Expedition = exp; }
//    }
// 2. 在 ExplorationSystem 的 ResolveExpeditionOutcome 方法中，当远征结果处理完毕后：
//    this.SendEvent(new ExpeditionCompletedEvent(expedition)); // 发送远征完成事件
// 3. 然后，ExplorationDisplay 脚本可以注册监听此事件：
//    - 在 Start() 或 Awake() 中: QFramework.TypeEventSystem.Global.Register<ExpeditionCompletedEvent>(OnExpeditionCompleted);
//    - 实现 OnExpeditionCompleted(ExpeditionCompletedEvent e) 方法来处理事件并更新UI。
//    - 在 OnDestroy() 中: QFramework.TypeEventSystem.Global.UnRegister<ExpeditionCompletedEvent>(OnExpeditionCompleted);
// 对于当前子任务的要求，使用静态的 DisplayOutcome 方法是一个更简单的临时解决方案。

