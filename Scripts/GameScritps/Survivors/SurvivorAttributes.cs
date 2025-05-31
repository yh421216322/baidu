namespace YourGameNamespace.Survivors
{
    // 代表幸存者核心属性的数据类
    public class SurvivorAttributes
    {
        // 力量：可能影响战斗伤害、携带能力或某些体力劳动任务的效率
        public int Strength { get; private set; }
        // 敏捷：可能影响战斗中的闪避、攻击速度或某些精细操作任务的效率
        public int Dexterity { get; private set; }
        // 智力：可能影响研究速度、制作高级物品的成功率或某些技术任务的效率
        public int Intelligence { get; private set; }

        // 构造函数
        public SurvivorAttributes(int strength, int dexterity, int intelligence)
        {
            Strength = strength;
            Dexterity = dexterity;
            Intelligence = intelligence;
        }
    }
}
