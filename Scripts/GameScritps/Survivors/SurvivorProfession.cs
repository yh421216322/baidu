namespace YourGameNamespace.Survivors
{
    // 幸存者的职业枚举
    public enum SurvivorProfession
    {
        Unassigned, // 未分配：幸存者没有特定职业，可能不擅长任何特定工作，或作为通用劳动力
        Doctor,     // 医生：擅长治疗伤病，可能在诊所工作效率更高或能制作更高级的药品
        Engineer,   // 工程师：擅长建造、修理和制作复杂物品，可能在工坊或发电厂工作效率更高
        Soldier,    // 士兵：擅长战斗和防御，在战斗中可能拥有更高的攻击力或防御力
        Farmer      // 农民：擅长农业生产，在农场工作时能产出更多食物
    }
}
