using QFramework;
using UnityEngine; // 用于 Debug.Log, Time.deltaTime
using System.Collections.Generic; // 用于 List
using System; // 用于 Guid
using YourGameNamespace.Survivors; // 用于 SurvivorModel

using YourGameNamespace.Events;   // 用于 POIExploredEvent

namespace YourGameNamespace.Exploration
{
    public class ExplorationSystem : AbstractSystem
    {
        private ExplorationModel mExplorationModel;
        private SurvivorModel mSurvivorModel;
        private ResourceModel mResourceModel;
        // private EventSystem mEventSystem; // 用于从探索结果中触发事件

        protected override void OnInit()
        {
         
            mExplorationModel = this.GetModel<ExplorationModel>();
            mSurvivorModel = this.GetModel<SurvivorModel>();
            mResourceModel = this.GetModel<ResourceModel>();
            // mEventSystem = this.GetSystem<EventSystem>(); // 用于从探索结果中触发事件
        }

        public bool CanStartExpeditionToPOI(string poiId, List<Guid> survivorIds, out string reason)
        {
            reason = string.Empty;
            var poi = mExplorationModel.GetPOI(poiId);
            if (poi == null) { reason = "Point of Interest not found."; return false; }
            if (poi.Status == POIStatus.BeingExplored) { reason = $"{poi.Name} is already being explored."; return false; }
            if (poi.Status == POIStatus.Depleted) { reason = $"{poi.Name} is depleted."; return false; }

            if (survivorIds == null || survivorIds.Count == 0) { reason = "No survivors selected for the expedition."; return false; }
            if (survivorIds.Count > poi.MaxSurvivorSlots) { reason = $"Too many survivors for {poi.Name}. Max slots: {poi.MaxSurvivorSlots}."; return false; }

            foreach (var survivorId in survivorIds)
            {
                var survivor = mSurvivorModel.GetSurvivorById(survivorId);
                if (survivor == null) { reason = $"Selected survivor (ID: {survivorId}) not found."; return false; }
                if (survivor.Status != SurvivorStatus.Idle) { reason = $"Survivor {survivor.Name} is not Idle (Status: {survivor.Status})."; return false; }
            }
            return true;
        }
        
        public bool StartExpedition(string poiId, List<Guid> survivorIds)
        {
            if (!CanStartExpeditionToPOI(poiId, survivorIds, out string reason))
            {
                Debug.LogWarning($"无法开始远征至 {poiId}：{reason}");
                return false;
            }

            var poi = mExplorationModel.GetPOI(poiId);
            // 实际探索时间（以后可以更复杂，例如基于幸存者技能）
            float actualExplorationTimeAtPoi = poi.BaseExplorationTime; 

            Expedition newExpedition = new Expedition(poiId, survivorIds, actualExplorationTimeAtPoi);
            newExpedition.Status = ExpeditionStatus.Departing; // 从出发阶段开始
            
            mExplorationModel.AddExpedition(newExpedition);
            mExplorationModel.UpdatePOIStatus(poiId, POIStatus.BeingExplored);

            foreach (var survivorId in survivorIds)
            {
                var survivor = mSurvivorModel.GetSurvivorById(survivorId);
                survivor.Status = SurvivorStatus.OnExpedition; // 此状态需要添加到 SurvivorStatus 枚举中
            }
            
            Debug.Log($"远征 {newExpedition.ExpeditionId} 已开始，前往 {poi.Name}，参与者：{survivorIds.Count} 名幸存者。行程时间：{newExpedition.TravelTimeToPoi}秒，探索时间：{actualExplorationTimeAtPoi}秒，返回时间：{newExpedition.TravelTimeBackToBase}秒。");
            return true;
        }

        public void UpdateActiveExpeditions(float deltaTime)
        {
            List<Expedition> completedExpeditions = new List<Expedition>();
            List<Expedition> expeditions = mExplorationModel.GetActiveExpeditions(); // 获取副本

            if (expeditions.Count > 0)
            {
                // 基础日志，如果过于频繁可以移除
                // Debug.Log($"正在更新 {expeditions.Count} 个活动远征..."); 
            }

            foreach (var expedition in expeditions)
            {
                expedition.TimeElapsedOnCurrentPhase += deltaTime;

                switch (expedition.Status)
                {
                    case ExpeditionStatus.Departing:
                        if (expedition.TimeElapsedOnCurrentPhase >= expedition.TravelTimeToPoi)
                        {
                            expedition.Status = ExpeditionStatus.Exploring;
                            expedition.TimeElapsedOnCurrentPhase = 0f;
                            Debug.Log($"远征 {expedition.ExpeditionId} 已抵达 {expedition.TargetPoiId}。开始探索阶段。");
                        }
                        break;
                    case ExpeditionStatus.Exploring:
                        if (expedition.TimeElapsedOnCurrentPhase >= expedition.ExplorationTimeAtPoi)
                        {
                            expedition.Status = ExpeditionStatus.Returning;
                            expedition.TimeElapsedOnCurrentPhase = 0f;
                            Debug.Log($"远征 {expedition.ExpeditionId} 完成探索 {expedition.TargetPoiId}。开始返回阶段。");
                        }
                        break;
                    case ExpeditionStatus.Returning:
                        if (expedition.TimeElapsedOnCurrentPhase >= expedition.TravelTimeBackToBase)
                        {
                            expedition.Status = ExpeditionStatus.Completed;
                            Debug.Log($"远征 {expedition.ExpeditionId} 已从 {expedition.TargetPoiId} 返回。正在处理结果...");
                            ResolveExpeditionOutcome(expedition); 
                            completedExpeditions.Add(expedition);
                        }
                        break;
                }
            }

            foreach (var completed in completedExpeditions)
            {
                mExplorationModel.RemoveExpedition(completed);
            }
        }

        private void ResolveExpeditionOutcome(Expedition expedition)
        {
            var poi = mExplorationModel.GetPOI(expedition.TargetPoiId);
            if (poi == null)
            {
                Debug.LogError($"处理远征 {expedition.ExpeditionId} 结果时未找到POI {expedition.TargetPoiId}。");
                expedition.Outcome.WasSuccessful = false;
                expedition.Outcome.NarrativeLog = "严重错误：处理结果时POI数据丢失。";
                // 即使在这种错误情况下，也要确保幸存者状态设置回Idle
                foreach (var survivorId in expedition.AssignedSurvivorIds)
                {
                    var surv = mSurvivorModel.GetSurvivorById(survivorId);
                    if (surv != null && surv.Status == SurvivorStatus.OnExpedition) surv.Status = SurvivorStatus.Idle;
                }
                return;
            }

            expedition.Outcome.NarrativeLog = $"远征 {poi.Name} 报告：\n";

            // 1. 资源奖励
            bool anyResourceFound = false;
            foreach (var rewardDef in poi.PotentialRewards)
            {
                if (UnityEngine.Random.value <= rewardDef.Probability)
                {
                    int amountGranted = UnityEngine.Random.Range(rewardDef.MinQuantity, rewardDef.MaxQuantity + 1);
                    if (amountGranted > 0)
                    {
                        expedition.Outcome.AddResource(rewardDef.ResourceType, amountGranted);
                        mResourceModel.AddResource(rewardDef.ResourceType, amountGranted); // 添加到全局资源
                        string foundMsg = $"找到 {amountGranted} {rewardDef.ResourceType}。";
                        expedition.Outcome.NarrativeLog += $"- {foundMsg}\n";
                        anyResourceFound = true;
                    }
                }
            }
            if (!anyResourceFound)
            {
                expedition.Outcome.NarrativeLog += "- 未找到重要资源。\n";
            }

            // 2. 幸存者风险（受伤）
            float baseInjuryChance = 0.05f; // 5% 基础受伤几率
            foreach (var survivorId in expedition.AssignedSurvivorIds)
            {
                var survivor = mSurvivorModel.GetSurvivorById(survivorId);
                if (survivor == null || survivor.Status != SurvivorStatus.OnExpedition) continue; // 如果逻辑正确，不应发生

                float injuryRisk = baseInjuryChance * poi.Difficulty;
                if (UnityEngine.Random.value <= injuryRisk)
                {
                    survivor.Status = SurvivorStatus.Injured;
                    string injuryMsg = $"{survivor.Name} 在远征中受伤。";
                    expedition.Outcome.SurvivorStatusChanges.Add(injuryMsg);
                    expedition.Outcome.NarrativeLog += $"- {injuryMsg}\n";
                }
                else
                {
                    survivor.Status = SurvivorStatus.Idle;
                    // 对于安然无恙返回的情况，没有特定的日志，已包含在总体远征成功信息中。
                }
            }
            
            // 总体成功状态以后可以定义得更复杂（例如，如果所有幸存者都受伤了，则不完全成功）
            expedition.Outcome.WasSuccessful = true; // 目前，如果完成，则视为成功。

            // 3. POI 状态更新
            // 目前总是设置为已探索。如果奖励有限，则可能设置为已枯竭。
            mExplorationModel.UpdatePOIStatus(poi.Id, POIStatus.Explored);
            expedition.Outcome.NarrativeLog += $"- {poi.Name} 已被探索。\n";


            // 4. 最终结果日志
            Debug.LogWarning($"--- 远征已解决：{expedition.ExpeditionId} 至 {poi.Name} ---");
            Debug.Log($"成功：{expedition.Outcome.WasSuccessful}");
            Debug.Log("叙事日志：\n" + expedition.Outcome.NarrativeLog);

            if (expedition.Outcome.ResourcesFound.Count > 0)
            {
                Debug.Log("找到的总资源：");
                foreach (var item in expedition.Outcome.ResourcesFound)
                {
                    Debug.Log($"- {item.Key}：{item.Value}");
                }
            }
            else { Debug.Log("此次远征未回收任何资源。");}


            if (expedition.Outcome.SurvivorStatusChanges.Count > 0)
            {
                Debug.Log("幸存者状态更新：");
                foreach (var change in expedition.Outcome.SurvivorStatusChanges)
                {
                    Debug.Log($"- {change}");
                }
            }
            else { Debug.Log("所有幸存者返回，状态无新变化（除了返回Idle状态）。");}
            Debug.LogWarning($"---------------------------------------------------");

        // 更新UI
        YourGameNamespace.UI.ExplorationDisplay.DisplayOutcome(expedition.Outcome);

        // 为任务系统发送事件
        if (expedition.Outcome.WasSuccessful) // 或者更具体的“成功探索”条件
        {
            this.SendEvent(new POIExploredEvent(poi.Id));
        }
        }
    }
}
