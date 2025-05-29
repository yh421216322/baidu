using System;

namespace YourGameNamespace.Survivors
{
    // 代表一个幸存者的数据类
    public class Survivor
    {
        public Guid Id { get; private set; } // 幸存者的唯一ID
        public string Name { get; set; } // 幸存者的名字 (玩家可自定义或随机生成)
        public SurvivorAttributes Attributes { get; private set; } // 幸存者的属性 (例如：力量、敏捷、智力)
        public SurvivorProfession Profession { get; set; } // 幸存者的职业
        public SurvivorStatus Status { get; set; } // 幸存者的当前状态 (例如：空闲、工作中、受伤)
        public Guid? WorkstationId { get; set; } = null; // 如果幸存者被分配到工作站，则记录工作站的ID；否则为null

        // 幸存者的需求等级
        public float FoodLevel { get; set; } = 100f;      // 当前食物等级，默认为100
        public float MaxFoodLevel { get; private set; } = 100f; // 最大食物等级，默认为100
        public float RestLevel { get; set; } = 100f;      // 当前休息等级，默认为100
        public float MaxRestLevel { get; private set; } = 100f; // 最大休息等级，默认为100

        // 构造函数
        public Survivor(string name, SurvivorAttributes attributes, SurvivorProfession profession)
        {
            Id = Guid.NewGuid(); // 自动生成新的唯一ID
            Name = name;
            Attributes = attributes;
            Profession = profession;
            Status = SurvivorStatus.Idle; // 新创建的幸存者默认为空闲状态
            WorkstationId = null;         // 初始未分配到任何工作站
            // FoodLevel (食物等级) 和 RestLevel (休息等级) 由其属性定义中的初始化器直接设置为默认值 (100f)。
        }
    }
}
