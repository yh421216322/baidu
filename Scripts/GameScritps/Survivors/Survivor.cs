using System;
using QFramework; // 用于 BindableProperty
using UnityEngine; // 用于 Mathf.Clamp

namespace YourGameNamespace.Survivors
{
    // 代表一个幸存者的数据类
    public class Survivor
    {
        public Guid Id { get; private set; } // 幸存者的唯一ID
        public BindableProperty<string> Name { get; private set; } // 幸存者的名字 (玩家可自定义或随机生成)
        public SurvivorAttributes Attributes { get; private set; } // 幸存者的属性 (例如：力量、敏捷、智力)
        public BindableProperty<SurvivorProfession> Profession { get; private set; } // 幸存者的职业
        public BindableProperty<SurvivorStatus> Status { get; private set; } // 幸存者的当前状态 (例如：空闲、工作中、受伤)
        public BindableProperty<Guid?> WorkstationId { get; private set; } // 如果幸存者被分配到工作站，则记录工作站的ID；否则为null

        // 幸存者的需求等级
        public BindableProperty<float> FoodLevel { get; private set; } // 当前食物等级
        public float MaxFoodLevel { get; private set; } = 100f;     // 最大食物等级，默认为100
        public BindableProperty<float> RestLevel { get; private set; } // 当前休息等级
        public float MaxRestLevel { get; private set; } = 100f;     // 最大休息等级，默认为100

        // 构造函数
        public Survivor(string name, SurvivorAttributes attributes, SurvivorProfession profession)
        {
            Id = Guid.NewGuid(); // 自动生成新的唯一ID
            Attributes = attributes;
            // MaxFoodLevel 和 MaxRestLevel 使用其字段初始化器默认值 (100f)

            // 初始化 BindableProperty
            Name = new BindableProperty<string>(name);
            Profession = new BindableProperty<SurvivorProfession>(profession);
            Status = new BindableProperty<SurvivorStatus>(SurvivorStatus.Idle); // 新创建的幸存者默认为空闲状态
            WorkstationId = new BindableProperty<Guid?>(null);         // 初始未分配到任何工作站
            FoodLevel = new BindableProperty<float>(MaxFoodLevel);      // 初始食物等级为最大值
            RestLevel = new BindableProperty<float>(MaxRestLevel);      // 初始休息等级为最大值
        }

        // 公共方法来修改这些 BindableProperty 的值

        public void UpdateName(string newName) // 如果名字允许更改
        {
            Name.Value = newName;
        }

        public void UpdateProfession(SurvivorProfession newProfession)
        {
            Profession.Value = newProfession;
        }

        public void UpdateStatus(SurvivorStatus newStatus)
        {
            // 通常状态变更有副作用或条件，直接赋值可能不妥，
            // 但作为BindableProperty的setter，如果外部系统已处理逻辑，则可直接设置。
            // 为保持简单，此处直接设置。更复杂的逻辑应在System层处理。
            if (Status.Value != newStatus) // 避免不必要的通知
            {
                Status.Value = newStatus;
            }
        }

        public void AssignWorkstation(Guid? newWorkstationId)
        {
            WorkstationId.Value = newWorkstationId;
        }

        public void AdjustFoodLevel(float amount)
        {
            FoodLevel.Value = Mathf.Clamp(FoodLevel.Value + amount, 0f, MaxFoodLevel);
        }

        public void AdjustRestLevel(float amount)
        {
            RestLevel.Value = Mathf.Clamp(RestLevel.Value + amount, 0f, MaxRestLevel);
        }
    }
}
