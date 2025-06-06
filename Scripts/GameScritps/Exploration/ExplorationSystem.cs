using QFramework;
using UnityEngine; // 用于 Debug.Log, Time.deltaTime
using System.Collections.Generic; // 用于 List
using System;
using MyGameNamespace; // 用于 Guid
using YourGameNamespace.Survivors; // 用于 SurvivorModel, SurvivorStatus, ISurvivorManagerSystem
using YourGameNamespace.Events;   // 用于 POIExploredEvent, Exploration_ExpeditionOutcomeResolvedEvent
// Removed: using YourGameNamespace.Exploration; // No longer needed if interface is in the same file

namespace YourGameNamespace.Exploration
{
    // Interface definition moved here and updated
    public interface IExplorationSystem : QFramework.QFISystem
    {
        bool CanStartExpeditionToPOI(string poiId, List<Guid> survivorIds, out string reason);
        bool StartExpedition(string poiId, List<Guid> survivorIds);
        void UpdateActiveExpeditions(float deltaTime);
    }

    // 探索系统，管理远征和兴趣点(POI)的逻辑
    public class ExplorationSystem : AbstractSystem, IExplorationSystem, IController // 实现IController以便获取System
    {
        private ExplorationModel mExplorationModel; // 探索数据模型
        private SurvivorModel mSurvivorModel;       // 幸存者数据模型
        private ResourceModel mResourceModel;       // 资源数据模型
        private SurvivorManagerSystem mSurvivorManagerSystem; // 幸存者管理系统

        // IController 接口要求
        public IArchitecture GetArchitecture() => RegisterManager.Interface;

        protected override void OnInit()
        {
            mExplorationModel = this.GetModel<ExplorationModel>();
            mSurvivorModel = this.GetModel<SurvivorModel>();
            mResourceModel = this.GetModel<ResourceModel>();
            mSurvivorManagerSystem = this.GetSystem<SurvivorManagerSystem>(); // 初始化幸存者管理系统

            if (mSurvivorManagerSystem == null)
            {
                Debug.LogError("探索系统 (ExplorationSystem)：未能获取幸存者管理系统 (ISurvivorManagerSystem)！");
            }
        }

        // 检查是否可以开始前往指定POI的远征
        public bool CanStartExpeditionToPOI(string poiId, List<Guid> survivorIds, out string reason)
        {
            reason = string.Empty;
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
                // 使用 .Value 访问 BindableProperty 的值
                if (survivor.Status.Value != SurvivorStatus.Idle) { reason = $"幸存者 {survivor.Name.Value} 当前状态不为空闲 (状态：{survivor.Status.Value})。"; return false; }
            }
            return true;
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
            float actualExplorationTimeAtPoi = poi.BaseExplorationTime; 

            Expedition newExpedition = new Expedition(poiId, survivorIds, actualExplorationTimeAtPoi);
            newExpedition.Status = ExpeditionStatus.Departing;
            
            mExplorationModel.AddExpedition(newExpedition);
            mExplorationModel.UpdatePOIStatus(poiId, POIStatus.BeingExplored);

            foreach (var survivorId in survivorIds)
            {
                // 通过 SurvivorManagerSystem 更新幸存者状态
                mSurvivorManagerSystem.SetSurvivorOnExpeditionStatus(survivorId, true);
            }
            
            Debug.Log($"远征 {newExpedition.ExpeditionId} 已开始，目的地：{poi.Name}，参与者：{survivorIds.Count} 名幸存者。前往时间：{newExpedition.TravelTimeToPoi}秒，探索时间：{actualExplorationTimeAtPoi}秒，返回时间：{newExpedition.TravelTimeBackToBase}秒。");
            return true;
        }

        // 更新所有活动远征的状态
        public void UpdateActiveExpeditions(float deltaTime)
        {
            List<Expedition> completedExpeditions = new List<Expedition>();
            List<Expedition> expeditions = mExplorationModel.GetActiveExpeditions();

            if (expeditions.Count > 0)
            {
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
                            Debug.Log($"远征 {expedition.ExpeditionId} 完成对 {expedition.TargetPoiId} 的探索。开始返回阶段。");
                        }
                        break;
                    case ExpeditionStatus.Returning:
                        if (expedition.TimeElapsedOnCurrentPhase >= expedition.TravelTimeBackToBase)
                        {
                            expedition.Status = ExpeditionStatus.Completed;
                            Debug.Log($"远征 {expedition.ExpeditionId} 已从 {expedition.TargetPoiId} 返回。正在处理远征结果...");
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

        // 处理远征的结果
        private void ResolveExpeditionOutcome(Expedition expedition)
        {
            var poi = mExplorationModel.GetPOI(expedition.TargetPoiId);
            if (poi == null)
            {
                Debug.LogError($"处理远征 {expedition.ExpeditionId} 的结果时未找到兴趣点 {expedition.TargetPoiId}。");
                expedition.Outcome.WasSuccessful = false;
                expedition.Outcome.NarrativeLog = "严重错误：处理远征结果时兴趣点数据丢失。";
                foreach (var survivorId in expedition.AssignedSurvivorIds)
                {
                    // 即使出错，也尝试将幸存者状态设为非远征状态
                    mSurvivorManagerSystem.SetSurvivorOnExpeditionStatus(survivorId, false);
                }
                this.SendEvent(new Exploration_ExpeditionOutcomeResolvedEvent() { Outcome = expedition.Outcome }); // 发送结果事件
                return;
            }

            expedition.Outcome.NarrativeLog = $"远征队对 {poi.Name} 的探索报告：\n";

            bool anyResourceFound = false;
            foreach (var rewardDef in poi.PotentialRewards)
            {
                if (UnityEngine.Random.value <= rewardDef.Probability)
                {
                    int amountGranted = UnityEngine.Random.Range(rewardDef.MinQuantity, rewardDef.MaxQuantity + 1);
                    if (amountGranted > 0)
                    {
                        expedition.Outcome.AddResource(rewardDef.ResourceType, amountGranted);
                        mResourceModel.AddResource(rewardDef.ResourceType, amountGranted);
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

            float baseInjuryChance = 0.05f;
            foreach (var survivorId in expedition.AssignedSurvivorIds)
            {
                mSurvivorManagerSystem.SetSurvivorOnExpeditionStatus(survivorId, false); // 标记幸存者不再远征（状态会先变为Idle）

                var survivor = mSurvivorModel.GetSurvivorById(survivorId); // 重新获取以确保数据最新
                if (survivor == null) continue; // 如果幸存者在此期间消失了（理论上不应发生）

                float injuryRisk = baseInjuryChance * poi.Difficulty;
                if (UnityEngine.Random.value <= injuryRisk)
                {
                    // 通过 SurvivorManagerSystem 更新幸存者状态为受伤
                    mSurvivorManagerSystem.UpdateSurvivorStatus(survivorId, SurvivorStatus.Injured);
                    string injuryMsg = $"{survivor.Name.Value} 在远征中受伤了。"; // 使用 .Value
                    expedition.Outcome.SurvivorStatusChanges.Add(injuryMsg);
                    expedition.Outcome.NarrativeLog += $"- {injuryMsg}\n";
                }
                // 如果未受伤，SetSurvivorOnExpeditionStatus(false) 已将其设为Idle，无需额外操作
            }
            
            expedition.Outcome.WasSuccessful = true;

            mExplorationModel.UpdatePOIStatus(poi.Id, POIStatus.Explored);
            expedition.Outcome.NarrativeLog += $"- {poi.Name} 区域已被探索完毕。\n";

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

        // 移除对 UI 的直接调用
        // YourGameNamespace.UI.ExplorationDisplay.DisplayOutcome(expedition.Outcome);

        // 发送远征结果已解析的事件
        this.SendEvent(new Exploration_ExpeditionOutcomeResolvedEvent() { Outcome = expedition.Outcome });

        if (expedition.Outcome.WasSuccessful)
        {
            this.SendEvent(new POIExploredEvent(poi.Id));
        }
        }
    }
}
