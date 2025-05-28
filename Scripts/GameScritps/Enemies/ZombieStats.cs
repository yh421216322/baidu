namespace YourGameNamespace.Enemies
{
    public class ZombieStats
    {
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public int AttackPower { get; set; }
        public float MovementSpeed { get; set; }

        public ZombieStats(int maxHealth, int attackPower, float movementSpeed)
        {
            MaxHealth = maxHealth;
            CurrentHealth = MaxHealth; // 当前生命值设为最大生命值
            AttackPower = attackPower;
            MovementSpeed = movementSpeed;
        }
    }
}
