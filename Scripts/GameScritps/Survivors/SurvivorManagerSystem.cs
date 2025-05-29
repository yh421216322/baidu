using QFramework;
using UnityEngine; // 用于 Debug.LogWarning 和 Mathf (数学函数库)
using System;     // 用于 Guid (全局唯一标识符)

namespace YourGameNamespace.Survivors
{
    // 幸存者管理器系统，负责处理幸存者的创建、需求更新以及与需求相关的行为（如进食、休息）
    public class SurvivorManagerSystem : AbstractSystem
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
                survivor.FoodLevel -= mFoodConsumptionRate * deltaTime; // 根据消耗速率和时间差减少食物水平
                if (survivor.FoodLevel < 0) survivor.FoodLevel = 0;   // 食物水平不能低于0

                // 2. 休息值消耗/恢复逻辑
                if (survivor.Status == SurvivorStatus.Working) // 如果幸存者正在工作
                {
                    survivor.RestLevel -= mRestDecreaseRateWorking * deltaTime; // 消耗工作状态的休息值
                }
                else if (survivor.Status == SurvivorStatus.Idle || survivor.Status == SurvivorStatus.NeedsAttention) // 如果幸存者空闲或需要关注
                {
                    // 处于“需要关注”状态的幸存者如果不在主动休息（例如玩家未命令其休息），其休息值仍会按空闲速率消耗
                    survivor.RestLevel -= mRestDecreaseRateIdle * deltaTime;
                }
                else if (survivor.Status == SurvivorStatus.Resting) // 如果幸存者正在休息
                {
                    survivor.RestLevel += mRestIncreaseRate * deltaTime; // 恢复休息值
                    if (survivor.RestLevel > survivor.MaxRestLevel) survivor.RestLevel = survivor.MaxRestLevel; // 休息值不能超过最大值
                }
                else if (survivor.Status == SurvivorStatus.Injured) // 如果幸存者受伤
                {
                     // 受伤幸存者的休息逻辑目前按空闲处理，也可以为其设置一个不同的消耗速率或暂停消耗。
                     survivor.RestLevel -= mRestDecreaseRateIdle * deltaTime; 
                }
                
                if (survivor.RestLevel < 0) survivor.RestLevel = 0; // 休息值不能低于0

                // 3. 检查是否因紧急需求（食物或休息值为0）而需要关注
                if (survivor.FoodLevel == 0 || survivor.RestLevel == 0)
                {
                    // 仅当幸存者当前状态不是不可中断的状态（如受伤、已在休息）或已是“需要关注”时，才将其状态转换为“需要关注”
                    if (survivor.Status != SurvivorStatus.NeedsAttention && 
                        survivor.Status != SurvivorStatus.Injured && // 假设受伤状态具有更高优先级或不同的处理逻辑
                        survivor.Status != SurvivorStatus.Resting)   // 如果幸存者已经在休息，则允许其继续休息以恢复
                    {
                        Debug.LogWarning($"幸存者 {survivor.Name} 当前状态紧急，需要关注！食物水平：{survivor.FoodLevel:F1}，休息水平：{survivor.RestLevel:F1}，当前状态：{survivor.Status}");
                        survivor.Status = SurvivorStatus.NeedsAttention; // 设置状态为“需要关注”
                        
                        // 如果幸存者因为需求紧急而停止工作，理想情况下应有机制通知WorkstationSystem。
                        // 例如，如果Status.Working意味着幸存者被分配到了某个工作站，
                        // 那么这里可能需要将他们从工作站取消分配。
                        // 当前实现仅更改状态。WorkstationSystem在更新生产时可能需要检查分配的幸存者状态是否仍然为Working。
                        if (survivor.WorkstationId.HasValue && survivor.Status != SurvivorStatus.Working)
                        {
                            // 此处的逻辑比较复杂：如果“需要关注”状态导致幸存者停止工作，工作站系统需要知道这一变化。
                            // 目前的假设是 Workstation.UpdateProduction 方法会检查分配的幸存者是否仍处于 Status.Working 状态。
                        }
                    }
                }
                // 如果幸存者的需求得到满足（例如通过玩家干预或AI行为），并且之前处于“需要关注”状态，则恢复到“空闲”状态
                else if (survivor.Status == SurvivorStatus.NeedsAttention && survivor.FoodLevel > 10 && survivor.RestLevel > 10) // 假设需求满足的阈值为10以上
                {
                    Debug.Log($"幸存者 {survivor.Name} 的需求已得到满足，已从“需要关注”状态恢复为空闲。");
                    survivor.Status = SurvivorStatus.Idle; // 状态恢复为空闲
                }
            }
        }

        // 尝试让指定的幸存者进食
        public bool SurvivorTryEat(Guid survivorId, int foodToEat)
        {
            if (mSurvivorModel == null || mResourceModel == null) return false; // 确保模型存在
            var survivor = mSurvivorModel.GetSurvivorById(survivorId); // 获取幸存者对象
            if (survivor != null) // 如果幸存者存在
            {
                // 检查仓库中是否有足够的食物可供食用
                if (mResourceModel.GetAmount(GameResourceType.Food) >= foodToEat)
                {
                    if (mResourceModel.ConsumeResource(GameResourceType.Food, foodToEat)) // 尝试消耗食物资源
                    {
                        survivor.FoodLevel += foodToEat * 5; // 假设1单位食物资源能提供5点食物值（此转换系数可配置）
                        if (survivor.FoodLevel > survivor.MaxFoodLevel) survivor.FoodLevel = survivor.MaxFoodLevel; // 食物水平不超过上限
                        Debug.Log($"幸存者 {survivor.Name} 食用了 {foodToEat} 单位食物。当前食物水平：{survivor.FoodLevel:F1}");
                        return true; // 进食成功
                    }
                    else
                    {
                        Debug.LogWarning($"幸存者 {survivor.Name} 尝试进食，但资源消耗操作意外失败（可能并发或其他原因）。");
                    }
                }
                else
                {
                     Debug.Log($"幸存者 {survivor.Name} 尝试进食，但仓库中食物 ({GameResourceType.Food}) 不足。");
                }
            }
            return false; // 进食失败
        }

        // 设置指定幸存者的休息状态
        public void SurvivorSetResting(Guid survivorId, bool isResting)
        {
            if (mSurvivorModel == null) return; // 确保模型存在
            var survivor = mSurvivorModel.GetSurvivorById(survivorId); // 获取幸存者对象
            // 如果幸存者存在且当前未受伤（受伤状态下是否能休息取决于游戏设计，目前允许）
            if (survivor != null && survivor.Status != SurvivorStatus.Injured) 
            {
                // 如果命令开始休息：
                //   - 如果幸存者是因为休息值过低而处于“需要关注”状态，开始休息后状态会变为“休息中”。
                // 如果命令停止休息：
                //   - 如果幸存者需求仍然紧急（例如食物或休息值依然很低），其状态可能会在UpdateSurvivorNeeds中再次变为“需要关注”。
                if (isResting) // 如果设置为开始休息
                {
                    survivor.Status = SurvivorStatus.Resting; // 设置状态为休息中
                }
                else // 如果设置为停止休息
                {
                    // 如果他们正在休息现在停止，则将其状态设为Idle (空闲)。
                    // 后续的 UpdateSurvivorNeeds() 调用会根据其当前的食物和休息水平来决定是否应转为 NeedsAttention (需要关注) 状态。
                    survivor.Status = SurvivorStatus.Idle; 
                }
                Debug.Log($"幸存者 {survivor.Name} 的当前状态已设置为：{(survivor.Status)}。休息水平：{survivor.RestLevel:F1}");
            }
        }
    }
}
