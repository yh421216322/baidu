using QFramework;
using UnityEngine; // 用于 Debug.LogWarning 和 Mathf

using System; // 用于 Guid

namespace YourGameNamespace.Survivors
{
    public class SurvivorManagerSystem : AbstractSystem
    {
        private ResourceModel mResourceModel;
        private SurvivorModel mSurvivorModel;
        private float mFoodConsumptionRate = 0.1f; // 每个幸存者每秒消耗的食物量（为平衡性可调整）
        private float mRestDecreaseRateWorking = 0.2f; // 工作时每秒消耗的休息值（为平衡性可调整）
        private float mRestDecreaseRateIdle = 0.05f; // 空闲时每秒消耗的休息值（为平衡性可调整）
        private float mRestIncreaseRate = 0.5f; // 休息时每秒恢复的休息值（为平衡性可调整）

        protected override void OnInit()
        {
      
            mResourceModel = this.GetModel<ResourceModel>();
            mSurvivorModel = this.GetModel<SurvivorModel>(); // 为方便起见，也获取SurvivorModel
            if (mResourceModel == null) Debug.LogError("幸存者管理系统：资源模型 (ResourceModel) 为空！");
            if (mSurvivorModel == null) Debug.LogError("幸存者管理系统：幸存者模型 (SurvivorModel) 为空！");
        }

        public Survivor CreateNewSurvivor(string name, SurvivorAttributes attributes, SurvivorProfession profession)
        {
            Survivor newSurvivor = new Survivor(name, attributes, profession);
            mSurvivorModel.AddSurvivor(newSurvivor);
            return newSurvivor;
        }

        public void UpdateSurvivorNeeds(float deltaTime)
        {
            if (mSurvivorModel == null) return;
            var survivors = mSurvivorModel.GetAllSurvivors();

            foreach (var survivor in survivors)
            {
                // 食物消耗
                survivor.FoodLevel -= mFoodConsumptionRate * deltaTime;
                if (survivor.FoodLevel < 0) survivor.FoodLevel = 0;

                // 休息值消耗/恢复
                if (survivor.Status == SurvivorStatus.Working)
                {
                    survivor.RestLevel -= mRestDecreaseRateWorking * deltaTime;
                }
                else if (survivor.Status == SurvivorStatus.Idle || survivor.Status == SurvivorStatus.NeedsAttention)
                {
                    // 状态为 NeedsAttention 的幸存者如果不在主动休息，仍会消耗休息值
                    survivor.RestLevel -= mRestDecreaseRateIdle * deltaTime;
                }
                else if (survivor.Status == SurvivorStatus.Resting)
                {
                    survivor.RestLevel += mRestIncreaseRate * deltaTime;
                    if (survivor.RestLevel > survivor.MaxRestLevel) survivor.RestLevel = survivor.MaxRestLevel;
                }
                // 受伤幸存者的休息逻辑可能不同或暂停，目前按Idle处理。
                else if (survivor.Status == SurvivorStatus.Injured)
                {
                     survivor.RestLevel -= mRestDecreaseRateIdle * deltaTime; // 或者为受伤状态设置不同的消耗速率
                }


                if (survivor.RestLevel < 0) survivor.RestLevel = 0;

                // 检查紧急需求
                if (survivor.FoodLevel == 0 || survivor.RestLevel == 0)
                {
                    // 仅当幸存者当前状态不是不可中断状态或已是NeedsAttention时，才转换为NeedsAttention
                    if (survivor.Status != SurvivorStatus.NeedsAttention && 
                        survivor.Status != SurvivorStatus.Injured && // 假设受伤状态优先
                        survivor.Status != SurvivorStatus.Resting) // 如果已在休息，则让他们继续
                    {
                        Debug.LogWarning($"幸存者 {survivor.Name} 需要关注！食物：{survivor.FoodLevel:F1}，休息：{survivor.RestLevel:F1}，当前状态：{survivor.Status}");
                        survivor.Status = SurvivorStatus.NeedsAttention;
                        // 如果他们正在工作，应该停止。
                        // 如果Status.Working意味着分配了工作站，这可能需要将他们从工作站取消分配。
                        // 目前仅更改状态。WorkstationSystem可能需要检查幸存者状态。
                        if (survivor.WorkstationId.HasValue && survivor.Status != SurvivorStatus.Working)
                        {
                            // 此逻辑比较棘手：如果NeedsAttention状态停止工作，Workstation需要知道。
                            // 目前假设Workstation.UpdateProduction会检查分配的幸存者是否仍为Status.Working。
                        }
                    }
                }
                // 如果需求得到满足（例如通过玩家干预或AI），则从NeedsAttention状态恢复
                else if (survivor.Status == SurvivorStatus.NeedsAttention && survivor.FoodLevel > 10 && survivor.RestLevel > 10) 
                {
                    Debug.Log($"{survivor.Name} 已从“需要关注”状态恢复。");
                    survivor.Status = SurvivorStatus.Idle;
                }
            }
        }

        public bool SurvivorTryEat(Guid survivorId, int foodToEat)
        {
            if (mSurvivorModel == null || mResourceModel == null) return false;
            var survivor = mSurvivorModel.GetSurvivorById(survivorId);
            if (survivor != null)
            {
                // 检查仓库中是否有食物可吃
                if (mResourceModel.GetAmount(GameResourceType.Food) >= foodToEat)
                {
                    if (mResourceModel.ConsumeResource(GameResourceType.Food, foodToEat))
                    {
                        survivor.FoodLevel += foodToEat * 5; // 假设1单位食物资源提供5点食物值
                        if (survivor.FoodLevel > survivor.MaxFoodLevel) survivor.FoodLevel = survivor.MaxFoodLevel;
                        Debug.Log($"{survivor.Name} 吃了 {foodToEat} 单位食物。食物水平：{survivor.FoodLevel:F1}");
                        return true;
                    }
                    else
                    {
                        Debug.LogWarning($"{survivor.Name} 尝试进食，但资源消耗意外失败。");
                    }
                }
                else
                {
                     Debug.Log($"{survivor.Name} 尝试进食，但仓库中食物不足。");
                }
            }
            return false;
        }

        public void SurvivorSetResting(Guid survivorId, bool isResting)
        {
            if (mSurvivorModel == null) return;
            var survivor = mSurvivorModel.GetSurvivorById(survivorId);
            if (survivor != null && survivor.Status != SurvivorStatus.Injured) // 如果受伤则不能休息？或者可以？目前允许。
            {
                // 如果开始休息，确保他们不是因为休息值过低而处于NeedsAttention状态。
                // 如果停止休息，且需求紧急，他们可能会回到NeedsAttention状态。
                if (isResting)
                {
                    survivor.Status = SurvivorStatus.Resting;
                }
                else // 不再休息
                {
                    // 如果他们正在休息现在停止，则设为Idle。UpdateSurvivorNeeds随后将确定是否为NeedsAttention。
                    survivor.Status = SurvivorStatus.Idle; 
                }
                Debug.Log($"{survivor.Name} 当前状态为 {(survivor.Status)}。休息水平：{survivor.RestLevel:F1}");
            }
        }
    }
}
