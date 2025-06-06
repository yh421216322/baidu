using QFramework;
using UnityEngine; // 用于 Debug.LogWarning 和 Mathf (数学函数库)
using System;     // 用于 Guid (全局唯一标识符)
using YourGameNamespace.Events; // 用于 System_SurvivorNeedsAttentionEvent
using YourGameNamespace.Workstations; // 用于 WorkstationType (在新方法中需要)

namespace YourGameNamespace.Survivors
{
    // Interface definition moved here
    public interface ISurvivorManagerSystem : QFramework.QFISystem // Inherits from QFISystem
    {
        Survivor CreateNewSurvivor(string name, SurvivorAttributes attributes, SurvivorProfession profession);
        void UpdateSurvivorNeeds(float deltaTime);
        bool SurvivorTryEat(Guid survivorId, int foodToEat);
        void SurvivorSetResting(Guid survivorId, bool isResting);
        void AssignSurvivorToWork(Guid survivorId, Guid workstationId, WorkstationType workstationType);
        void ClearSurvivorWorkAssignment(Guid survivorId);
        void SetSurvivorOnExpeditionStatus(Guid survivorId, bool isOnExpedition);
        void UpdateSurvivorStatus(Guid survivorId, SurvivorStatus newStatus);
    }

    // 幸存者管理器系统，负责处理幸存者的创建、需求更新以及与需求相关的行为（如进食、休息）
    public class SurvivorManagerSystem : AbstractSystem, ISurvivorManagerSystem // Implements the updated interface
    {
        private ResourceModel mResourceModel; // 资源数据模型
        private SurvivorModel mSurvivorModel;   // 幸存者数据模型

        // --- 需求消耗与恢复速率配置 (可根据游戏平衡性调整) ---
        private float mFoodConsumptionRate = 0.1f;     // 每个幸存者每秒消耗的食物量
        private float mRestDecreaseRateWorking = 0.2f; // 工作状态下，幸存者每秒消耗的休息值
        private float mRestDecreaseRateIdle = 0.05f;   // 空闲状态下，幸存者每秒消耗的休息值
        private float mRestIncreaseRate = 0.5f;        // 休息状态下，幸存者每秒恢复的休息值

        // 系统初始化
        protected override void OnInit()
        {
            mResourceModel = this.GetModel<ResourceModel>();
            mSurvivorModel = this.GetModel<SurvivorModel>(); // 为了方便，也获取SurvivorModel的引用
            if (mResourceModel == null) Debug.LogError("幸存者管理系统：未能获取资源模型 (ResourceModel)！系统可能无法正常工作。");
            if (mSurvivorModel == null) Debug.LogError("幸存者管理系统：未能获取幸存者模型 (SurvivorModel)！系统可能无法正常工作。");
        }

        // 创建一个新的幸存者并将其添加到幸存者模型中
        public Survivor CreateNewSurvivor(string name, SurvivorAttributes attributes, SurvivorProfession profession)
        {
            Survivor newSurvivor = new Survivor(name, attributes, profession); // 创建幸存者实例
            mSurvivorModel.AddSurvivor(newSurvivor); // 添加到模型
            return newSurvivor; // 返回新创建的幸存者对象
        }

        // 更新所有幸存者的需求状态
        public void UpdateSurvivorNeeds(float deltaTime)
        {
            if (mSurvivorModel == null) return; // 如果幸存者模型不存在，则不执行任何操作
            var survivors = mSurvivorModel.GetAllSurvivors(); // 获取所有幸存者列表

            foreach (var survivor in survivors)
            {
                // 1. 食物消耗逻辑
                survivor.AdjustFoodLevel(-mFoodConsumptionRate * deltaTime);

                // 2. 休息值消耗/恢复逻辑
                if (survivor.Status.Value == SurvivorStatus.Working) // 如果幸存者正在工作
                {
                    survivor.AdjustRestLevel(-mRestDecreaseRateWorking * deltaTime);
                }
                else if (survivor.Status.Value == SurvivorStatus.Idle || survivor.Status.Value == SurvivorStatus.NeedsAttention) // 如果幸存者空闲或需要关注
                {
                    survivor.AdjustRestLevel(-mRestDecreaseRateIdle * deltaTime);
                }
                else if (survivor.Status.Value == SurvivorStatus.Resting) // 如果幸存者正在休息
                {
                    survivor.AdjustRestLevel(mRestIncreaseRate * deltaTime);
                }
                else if (survivor.Status.Value == SurvivorStatus.Injured) // 如果幸存者受伤
                {
                     survivor.AdjustRestLevel(-mRestDecreaseRateIdle * deltaTime);
                }

                // 3. 检查是否因紧急需求（食物或休息值为0）而需要关注
                bool needsAttentionNow = survivor.FoodLevel.Value == 0 || survivor.RestLevel.Value == 0;
                if (needsAttentionNow && survivor.Status.Value != SurvivorStatus.NeedsAttention &&
                    survivor.Status.Value != SurvivorStatus.Injured &&
                    survivor.Status.Value != SurvivorStatus.Resting)
                {
                    SurvivorStatus oldStatus = survivor.Status.Value;
                    survivor.UpdateStatus(SurvivorStatus.NeedsAttention);
                    Debug.LogWarning($"幸存者 {survivor.Name.Value} 需要关注！食物：{survivor.FoodLevel.Value:F1}，休息：{survivor.RestLevel.Value:F1}，先前状态：{oldStatus}");
                    this.SendEvent(new System_SurvivorNeedsAttentionEvent() { SurvivorId = survivor.Id, SurvivorName = survivor.Name.Value });

                    if (survivor.WorkstationId.Value.HasValue && survivor.Status.Value != SurvivorStatus.Working)
                    {
                        // 之前关于工作站的注释逻辑保留，但实际解除分配应由WorkstationSystem或通过此系统的新方法处理
                    }
                }
                // 如果幸存者的需求得到满足，并且之前处于“需要关注”状态，则恢复到“空闲”状态
                else if (survivor.Status.Value == SurvivorStatus.NeedsAttention && survivor.FoodLevel.Value > 10 && survivor.RestLevel.Value > 10)
                {
                    Debug.Log($"幸存者 {survivor.Name.Value} 的需求已得到满足，已从“需要关注”状态恢复为空闲。");
                    survivor.UpdateStatus(SurvivorStatus.Idle);
                }
            }
        }

        // 尝试让指定的幸存者进食
        public bool SurvivorTryEat(Guid survivorId, int foodToEat)
        {
            if (mSurvivorModel == null || mResourceModel == null) return false;
            var survivor = mSurvivorModel.GetSurvivorById(survivorId);
            if (survivor != null)
            {
                if (mResourceModel.GetAmount(GameResourceType.Food) >= foodToEat)
                {
                    if (mResourceModel.ConsumeResource(GameResourceType.Food, foodToEat))
                    {
                        survivor.AdjustFoodLevel(foodToEat * 5f); // 确保5是浮点数
                        Debug.Log($"幸存者 {survivor.Name.Value} 食用了 {foodToEat} 单位食物。当前食物水平：{survivor.FoodLevel.Value:F1}");
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning($"幸存者 {survivor.Name.Value} 尝试进食，但资源消耗操作意外失败（可能并发或其他原因）。");
                    }
                }
                else
                {
                     Debug.Log($"幸存者 {survivor.Name.Value} 尝试进食，但仓库中食物 ({GameResourceType.Food}) 不足。");
                }
            }
            return false;
        }

        // 设置指定幸存者的休息状态
        public void SurvivorSetResting(Guid survivorId, bool isResting)
        {
            if (mSurvivorModel == null) return;
            var survivor = mSurvivorModel.GetSurvivorById(survivorId);
            if (survivor != null && survivor.Status.Value != SurvivorStatus.Injured)
            {
                if (isResting)
                {
                    survivor.UpdateStatus(SurvivorStatus.Resting);
                }
                else
                {
                    survivor.UpdateStatus(SurvivorStatus.Idle);
                }
                Debug.Log($"幸存者 {survivor.Name.Value} 的当前状态已设置为：{(survivor.Status.Value)}。休息水平：{survivor.RestLevel.Value:F1}");
            }
        }

        // 将幸存者分配到工作（由WorkstationSystem调用）
        public void AssignSurvivorToWork(Guid survivorId, Guid workstationId, WorkstationType workstationType)
        {
            var survivor = mSurvivorModel.GetSurvivorById(survivorId);
            if (survivor != null)
            {
                if (survivor.Status.Value == SurvivorStatus.Idle) // 只能分配空闲的幸存者
                {
                    survivor.AssignWorkstation(workstationId); // 调用Survivor的AssignWorkstation方法
                    survivor.UpdateStatus(SurvivorStatus.Working); // 调用Survivor的UpdateStatus方法
                    Debug.Log($"{survivor.Name.Value} 已被分配到工作站 {workstationId} ({workstationType})。");
                    // 可选：发送一个System级的事件，如 System_SurvivorAssignedToWorkEvent
                    // this.SendEvent(new System_SurvivorAssignedToWorkEvent() { SurvivorId = survivorId, WorkstationId = workstationId, WorkstationType = workstationType });
                }
                else
                {
                    Debug.LogWarning($"无法分配 {survivor.Name.Value} 到工作站：幸存者不是空闲状态 (当前状态：{survivor.Status.Value})。");
                }
            }
        }

        // 清除幸存者的工作分配（由WorkstationSystem调用）
        public void ClearSurvivorWorkAssignment(Guid survivorId)
        {
            var survivor = mSurvivorModel.GetSurvivorById(survivorId);
            if (survivor != null && survivor.WorkstationId.Value.HasValue)
            {
                Guid oldWorkstationId = survivor.WorkstationId.Value.Value; // 获取旧ID
                survivor.AssignWorkstation(null); // 清除工作站ID
                // 如果幸存者当前是Working状态，应将其改回Idle，除非有其他逻辑决定其新状态
                if (survivor.Status.Value == SurvivorStatus.Working)
                {
                    survivor.UpdateStatus(SurvivorStatus.Idle);
                }
                Debug.Log($"{survivor.Name.Value} 已从工作站 {oldWorkstationId} 解除分配。");
                // 可选：发送一个System级的事件，如 System_SurvivorUnassignedFromWorkEvent
                // this.SendEvent(new System_SurvivorUnassignedFromWorkEvent() { SurvivorId = survivorId, OldWorkstationId = oldWorkstationId });
            }
        }

        // 新增方法：设置幸存者的远征状态
        public void SetSurvivorOnExpeditionStatus(Guid survivorId, bool isOnExpedition)
        {
            var survivor = mSurvivorModel.GetSurvivorById(survivorId);
            if (survivor != null)
            {
                if (isOnExpedition)
                {
                    // 确保幸存者是空闲的才能开始远征
                    if (survivor.Status.Value == SurvivorStatus.Idle)
                    {
                        survivor.UpdateStatus(SurvivorStatus.OnExpedition);
                        UnityEngine.Debug.Log($"幸存者 {survivor.Name.Value} 已开始远征。");
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"试图将非空闲状态的幸存者 {survivor.Name.Value} (状态: {survivor.Status.Value}) 设置为远征中。");
                    }
                }
                else // 从远征返回
                {
                    // 只有当幸存者确实在远征中时，才将其设置为空闲
                    // 其他状态（如受伤）应由调用方（如ExplorationSystem）在此之后单独设置
                    if (survivor.Status.Value == SurvivorStatus.OnExpedition)
                    {
                        survivor.UpdateStatus(SurvivorStatus.Idle);
                        UnityEngine.Debug.Log($"幸存者 {survivor.Name.Value} 已结束远征，状态恢复为空闲。");
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning($"SetSurvivorOnExpeditionStatus: 未找到ID为 {survivorId} 的幸存者。");
            }
        }

        // 新增方法：通用幸存者状态更新
        public void UpdateSurvivorStatus(Guid survivorId, SurvivorStatus newStatus)
        {
            var survivor = mSurvivorModel.GetSurvivorById(survivorId);
            if (survivor != null)
            {
                survivor.UpdateStatus(newStatus); // 调用Survivor实体自身的UpdateStatus
                // UnityEngine.Debug.Log($"幸存者 {survivor.Name.Value} 状态已更新为: {newStatus}"); // 可选日志
            }
            else
            {
                UnityEngine.Debug.LogWarning($"UpdateSurvivorStatus: 未找到ID为 {survivorId} 的幸存者。");
            }
        }
    }
}
