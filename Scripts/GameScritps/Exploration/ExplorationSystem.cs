using QFramework;
using UnityEngine; // 用于 Debug.Log, Time.deltaTime
using System.Collections.Generic; // 用于 List
using System; // 用于 Guid
using YourGameNamespace.Survivors; // 用于 SurvivorModel

using YourGameNamespace.Events;   // 用于 POIExploredEvent (兴趣点探索完毕事件)

namespace YourGameNamespace.Exploration
{
    // 探索系统，管理远征和兴趣点(POI)的逻辑
    public class ExplorationSystem : AbstractSystem
    {
        private ExplorationModel mExplorationModel; // 探索数据模型
        private SurvivorModel mSurvivorModel;       // 幸存者数据模型
        private ResourceModel mResourceModel;       // 资源数据模型
        // private EventSystem mEventSystem; // 事件系统，可用于从探索结果中触发更复杂的事件 (当前未直接使用)

        protected override void OnInit()
        {
         
            mExplorationModel = this.GetModel<ExplorationModel>();
            mSurvivorModel = this.GetModel<SurvivorModel>();
            mResourceModel = this.GetModel<ResourceModel>();
            // mEventSystem = this.GetSystem<EventSystem>(); // 如果需要，在这里获取事件系统实例
        }

        // 检查是否可以开始前往指定POI的远征
        public bool CanStartExpeditionToPOI(string poiId, List<Guid> survivorIds, out string reason)
        {
            reason = string.Empty; // 初始化失败原因
            var poi = mExplorationModel.GetPOI(poiId);
            if (poi == null) { reason = "兴趣点未找到。"; return false; }
            if (poi.Status == POIStatus.BeingExplored) { reason = $"{poi.Name} 已在探索中。"; return false; }
            if (poi.Status == POIStatus.Depleted) { reason = $"{poi.Name} 已耗尽。"; return false; }

            if (survivorIds == null || survivorIds.Count == 0) { reason = "未选择任何幸存者参与远征。"; return false; }
            if (survivorIds.Count > poi.MaxSurvivorSlots) { reason = $"参与 {poi.Name} 的幸存者过多。最大槽位：{poi.MaxSurvivorSlots}。"; return false; }

            foreach (var survivorId in survivorIds)
            {
                var survivor = mSurvivorModel.GetSurvivorById(survivorId);
                if (survivor == null) { reason = $"选择的幸存者 (ID: {survivorId}) 未找到。"; return false; }
                if (survivor.Status != SurvivorStatus.Idle) { reason = $"幸存者 {survivor.Name} 当前状态不为空闲 (状态：{survivor.Status})。"; return false; }
            }
            return true; // 所有检查通过
        }
        
        // 开始一次远征
        public bool StartExpedition(string poiId, List<Guid> survivorIds)
        {
            if (!CanStartExpeditionToPOI(poiId, survivorIds, out string reason))
            {
                Debug.LogWarning($"无法开始前往 {poiId} 的远征：{reason}");
                return false;
            }

            var poi = mExplorationModel.GetPOI(poiId);
            // 实际在POI的探索时间（将来可以根据幸存者技能等因素变得更复杂）
            float actualExplorationTimeAtPoi = poi.BaseExplorationTime; 

            Expedition newExpedition = new Expedition(poiId, survivorIds, actualExplorationTimeAtPoi);
            newExpedition.Status = ExpeditionStatus.Departing; // 远征从“出发中”阶段开始
            
            mExplorationModel.AddExpedition(newExpedition); // 将新远征添加到活动列表
            mExplorationModel.UpdatePOIStatus(poiId, POIStatus.BeingExplored); // 更新POI状态为“探索中”

            foreach (var survivorId in survivorIds)
            {
                var survivor = mSurvivorModel.GetSurvivorById(survivorId);
                survivor.Status = SurvivorStatus.OnExpedition; // 将幸存者状态设置为“远征中”（此状态需要已在SurvivorStatus枚举中定义）
            }
            
            Debug.Log($"远征 {newExpedition.ExpeditionId} 已开始，目的地：{poi.Name}，参与者：{survivorIds.Count} 名幸存者。前往时间：{newExpedition.TravelTimeToPoi}秒，探索时间：{actualExplorationTimeAtPoi}秒，返回时间：{newExpedition.TravelTimeBackToBase}秒。");
            return true;
        }

        // 更新所有活动远征的状态
        public void UpdateActiveExpeditions(float deltaTime)
        {
            List<Expedition> completedExpeditions = new List<Expedition>(); // 用于存储本帧完成的远征
            List<Expedition> expeditions = mExplorationModel.GetActiveExpeditions(); // 获取当前所有活动远征的副本进行处理

            if (expeditions.Count > 0)
            {
                // 基础日志，如果过于频繁，可以考虑移除或设置为更详细的日志级别
                // Debug.Log($"正在更新 {expeditions.Count} 个活动远征..."); 
            }

            foreach (var expedition in expeditions)
            {
                expedition.TimeElapsedOnCurrentPhase += deltaTime; // 更新当前阶段已用时间

                switch (expedition.Status)
                {
                    case ExpeditionStatus.Departing: // 如果正在前往POI
                        if (expedition.TimeElapsedOnCurrentPhase >= expedition.TravelTimeToPoi)
                        {
                            expedition.Status = ExpeditionStatus.Exploring; // 到达POI，开始探索
                            expedition.TimeElapsedOnCurrentPhase = 0f;    // 重置阶段计时器
                            Debug.Log($"远征 {expedition.ExpeditionId} 已抵达 {expedition.TargetPoiId}。开始探索阶段。");
                        }
                        break;
                    case ExpeditionStatus.Exploring: // 如果正在探索POI
                        if (expedition.TimeElapsedOnCurrentPhase >= expedition.ExplorationTimeAtPoi)
                        {
                            expedition.Status = ExpeditionStatus.Returning; // 探索完毕，开始返回
                            expedition.TimeElapsedOnCurrentPhase = 0f;    // 重置阶段计时器
                            Debug.Log($"远征 {expedition.ExpeditionId} 完成对 {expedition.TargetPoiId} 的探索。开始返回阶段。");
                        }
                        break;
                    case ExpeditionStatus.Returning: // 如果正在返回基地
                        if (expedition.TimeElapsedOnCurrentPhase >= expedition.TravelTimeBackToBase)
                        {
                            expedition.Status = ExpeditionStatus.Completed; // 返回基地，远征完成
                            Debug.Log($"远征 {expedition.ExpeditionId} 已从 {expedition.TargetPoiId} 返回。正在处理远征结果...");
                            ResolveExpeditionOutcome(expedition); // 处理远征结果
                            completedExpeditions.Add(expedition); // 添加到待移除列表
                        }
                        break;
                }
            }

            // 移除所有已完成的远征
            foreach (var completed in completedExpeditions)
            {
                mExplorationModel.RemoveExpedition(completed);
            }
        }

        // 处理远征的结果
        private void ResolveExpeditionOutcome(Expedition expedition)
        {
            var poi = mExplorationModel.GetPOI(expedition.TargetPoiId);
            if (poi == null) // 安全检查，确保POI仍然存在
            {
                Debug.LogError($"处理远征 {expedition.ExpeditionId} 的结果时未找到兴趣点 {expedition.TargetPoiId}。");
                expedition.Outcome.WasSuccessful = false;
                expedition.Outcome.NarrativeLog = "严重错误：处理远征结果时兴趣点数据丢失。";
                // 即使在这种错误情况下，也要确保将幸存者状态设置回“空闲”
                foreach (var survivorId in expedition.AssignedSurvivorIds)
                {
                    var surv = mSurvivorModel.GetSurvivorById(survivorId);
                    if (surv != null && surv.Status == SurvivorStatus.OnExpedition) surv.Status = SurvivorStatus.Idle;
                }
                return;
            }

            expedition.Outcome.NarrativeLog = $"远征队对 {poi.Name} 的探索报告：\n";

            // 1. 处理资源奖励
            bool anyResourceFound = false; // 是否找到任何资源
            foreach (var rewardDef in poi.PotentialRewards) // 遍历POI的潜在奖励定义
            {
                if (UnityEngine.Random.value <= rewardDef.Probability) // 根据概率判定是否获得该奖励
                {
                    int amountGranted = UnityEngine.Random.Range(rewardDef.MinQuantity, rewardDef.MaxQuantity + 1); // 在最小和最大数量之间随机一个值
                    if (amountGranted > 0)
                    {
                        expedition.Outcome.AddResource(rewardDef.ResourceType, amountGranted); // 将找到的资源添加到远征结果中
                        mResourceModel.AddResource(rewardDef.ResourceType, amountGranted); // 将资源添加到全局资源模型
                        string foundMsg = $"找到 {amountGranted} 单位 {rewardDef.ResourceType}。";
                        expedition.Outcome.NarrativeLog += $"- {foundMsg}\n";
                        anyResourceFound = true;
                    }
                }
            }
            if (!anyResourceFound)
            {
                expedition.Outcome.NarrativeLog += "- 未找到任何有价值的资源。\n";
            }

            // 2. 处理幸存者风险（例如受伤）
            float baseInjuryChance = 0.05f; // 5% 的基础受伤几率
            foreach (var survivorId in expedition.AssignedSurvivorIds)
            {
                var survivor = mSurvivorModel.GetSurvivorById(survivorId);
                // 如果幸存者不存在或状态异常（理论上不应发生，如果逻辑正确），则跳过
                if (survivor == null || survivor.Status != SurvivorStatus.OnExpedition) continue; 

                float injuryRisk = baseInjuryChance * poi.Difficulty; // 受伤风险受POI难度影响
                if (UnityEngine.Random.value <= injuryRisk) // 根据风险判定是否受伤
                {
                    survivor.Status = SurvivorStatus.Injured; // 设置幸存者状态为受伤
                    string injuryMsg = $"{survivor.Name} 在远征中受伤了。";
                    expedition.Outcome.SurvivorStatusChanges.Add(injuryMsg); // 记录状态变化
                    expedition.Outcome.NarrativeLog += $"- {injuryMsg}\n";
                }
                else
                {
                    survivor.Status = SurvivorStatus.Idle; // 未受伤则设置回空闲状态
                    // 对于安然无恙返回的情况，没有特定的单独日志条目，通常包含在总体远征成功信息中。
                }
            }
            
            // 远征的总体成功状态（将来可以定义得更复杂，例如，如果所有幸存者都受伤了，可能不视为完全成功）
            expedition.Outcome.WasSuccessful = true; // 目前，只要远征完成，就视为成功。

            // 3. 更新POI状态
            // 目前总是将POI状态设置为“已探索”。如果POI的奖励是有限的，则将来可能需要将其设置为“已耗尽”。
            mExplorationModel.UpdatePOIStatus(poi.Id, POIStatus.Explored);
            expedition.Outcome.NarrativeLog += $"- {poi.Name} 区域已被探索完毕。\n";


            // 4. 记录最终的远征结果日志
            Debug.LogWarning($"--- 远征已结束：{expedition.ExpeditionId} 前往 {poi.Name} ---");
            Debug.Log($"是否成功：{expedition.Outcome.WasSuccessful}");
            Debug.Log("叙事日志：\n" + expedition.Outcome.NarrativeLog);

            if (expedition.Outcome.ResourcesFound.Count > 0)
            {
                Debug.Log("找到的总资源：");
                foreach (var item in expedition.Outcome.ResourcesFound)
                {
                    Debug.Log($"- {item.Key}：{item.Value} 单位");
                }
            }
            else { Debug.Log("本次远征未能带回任何资源。");}


            if (expedition.Outcome.SurvivorStatusChanges.Count > 0)
            {
                Debug.Log("幸存者状态更新：");
                foreach (var change in expedition.Outcome.SurvivorStatusChanges)
                {
                    Debug.Log($"- {change}");
                }
            }
            else { Debug.Log("所有参与远征的幸存者均已安全返回，状态无特殊变化（已恢复至空闲状态）。");}
            Debug.LogWarning($"---------------------------------------------------");

        // 更新UI显示远征结果
        YourGameNamespace.UI.ExplorationDisplay.DisplayOutcome(expedition.Outcome);

        // 为任务系统等其他系统发送POI探索完成事件
        if (expedition.Outcome.WasSuccessful) // 或者可以根据更具体的“成功探索”条件来判断
        {
            this.SendEvent(new POIExploredEvent(poi.Id));
        }
        }
    }
}
