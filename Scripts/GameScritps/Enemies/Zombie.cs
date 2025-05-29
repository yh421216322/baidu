using UnityEngine;
using System;

namespace YourGameNamespace.Enemies
{
    public class Zombie
    {
        public Guid Id { get; private set; } = Guid.NewGuid(); // 僵尸的唯一ID
        public ZombieStats Stats { get; private set; } // 僵尸的属性
        public Vector2 Position { get; set; } // 僵尸当前位置
        public Vector2 TargetPosition { get; set; } // 僵尸的目标位置（通常是基地）
        public bool IsDead { get { return Stats.CurrentHealth <= 0; } } // 僵尸是否死亡

        // 构造函数
        public Zombie(ZombieStats stats, Vector2 spawnPosition, Vector2 targetPosition)
        {
            Stats = stats;
            Position = spawnPosition;
            TargetPosition = targetPosition;
        }

        // 僵尸受到伤害
        public void TakeDamage(int amount)
        {
            if (IsDead) return; // 如果已死亡，则不执行任何操作
            Stats.CurrentHealth -= amount;
            if (Stats.CurrentHealth < 0) Stats.CurrentHealth = 0; // 生命值不能为负
            // 根据当前提示使用完整ID记录伤害日志
            Debug.Log($"僵尸 {Id} 受到了 {amount} 点伤害，当前生命值：{Stats.CurrentHealth}");
        }

        // 僵尸移动
        public void Move(float deltaTime)
        {
            if (IsDead) return; // 如果已死亡，则不移动
            Vector2 direction = (TargetPosition - Position).normalized; // 计算朝向目标的单位向量
            Position += direction * Stats.MovementSpeed * deltaTime; // 根据速度和时间更新位置
        }

        // 僵尸攻击目标
        public void AttackTarget()
        {
            if(IsDead) return; // 如果已死亡，则不攻击
            // 根据当前提示使用完整ID记录攻击日志
            Debug.Log($"僵尸 {Id} 攻击了位于 {TargetPosition} 的目标，造成 {Stats.AttackPower} 点伤害！");
        }
    }
}
