namespace YourGameNamespace.Enemies
{
    public class ZombieStats
    {
        public int MaxHealth { get; set; } // 最大生命值
        public int CurrentHealth { get; set; } // 当前生命值
        public int AttackPower { get; set; } // 攻击力
        public float MovementSpeed { get; set; } // 移动速度

        public ZombieStats(int maxHealth, int attackPower, float movementSpeed)
        {
            MaxHealth = maxHealth;
            CurrentHealth = MaxHealth; // 初始时，当前生命值等于最大生命值
            AttackPower = attackPower;
            MovementSpeed = movementSpeed;
        }
    }
}
