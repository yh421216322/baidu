using UnityEngine;
using System;

namespace YourGameNamespace.Enemies
{
    public class Zombie
    {
        public Guid Id { get; private set; } = Guid.NewGuid();
        public ZombieStats Stats { get; private set; }
        public Vector2 Position { get; set; } // 当前位置
        public Vector2 TargetPosition { get; set; } // 基地或目标的位置
        public bool IsDead { get { return Stats.CurrentHealth <= 0; } }

        public Zombie(ZombieStats stats, Vector2 spawnPosition, Vector2 targetPosition)
        {
            Stats = stats;
            Position = spawnPosition;
            TargetPosition = targetPosition;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead) return;
            Stats.CurrentHealth -= amount;
            if (Stats.CurrentHealth < 0) Stats.CurrentHealth = 0;
            // 根据当前提示使用完整ID
            Debug.Log($"僵尸 {Id} 受到了 {amount} 点伤害，生命值：{Stats.CurrentHealth}");
        }

        public void Move(float deltaTime)
        {
            if (IsDead) return;
            Vector2 direction = (TargetPosition - Position).normalized;
            Position += direction * Stats.MovementSpeed * deltaTime;
        }

        public void AttackTarget()
        {
            if(IsDead) return;
            // 根据当前提示使用完整ID
            Debug.Log($"僵尸 {Id} 攻击了位于 {TargetPosition} 的目标，造成 {Stats.AttackPower} 点伤害！");
        }
    }
}
