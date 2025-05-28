using System;

namespace YourGameNamespace.Survivors
{
    public class Survivor
    {
        public Guid Id { get; private set; }
        public string Name { get; set; }
        public SurvivorAttributes Attributes { get; private set; }
        public SurvivorProfession Profession { get; set; }
        public SurvivorStatus Status { get; set; }
        public Guid? WorkstationId { get; set; } = null; 

        public float FoodLevel { get; set; } = 100f;
        public float MaxFoodLevel { get; private set; } = 100f;
        public float RestLevel { get; set; } = 100f;
        public float MaxRestLevel { get; private set; } = 100f;

        public Survivor(string name, SurvivorAttributes attributes, SurvivorProfession profession)
        {
            Id = Guid.NewGuid();
            Name = name;
            Attributes = attributes;
            Profession = profession;
            Status = SurvivorStatus.Idle;
            WorkstationId = null; 
            // FoodLevel 和 RestLevel 由其属性初始化器初始化
        }
    }
}
