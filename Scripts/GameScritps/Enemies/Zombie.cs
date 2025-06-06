using UnityEngine;
using System;
using QFramework; // 添加 using 语句

namespace YourGameNamespace.Enemies
{
    public class Zombie
    {
        public Guid Id { get; private set; } = Guid.NewGuid(); // 僵尸的唯一ID
        public ZombieStats Stats { get; private set; } // 僵尸的属性
        public Vector2 Position { get; set; } // 僵尸当前位置
        public Vector2 TargetPosition { get; set; } // 僵尸的目标位置（通常是基地）

        // 将 CurrentHealth（原在Stats中）和 IsDead 修改为 BindableProperty
        // 注意：这意味着 Zombie 类现在是 CurrentHealth 和 IsDead 状态的主要来源，
        // ZombieStats 中的 CurrentHealth 可能变为仅用于初始值或不再直接由外部逻辑依赖。
        // 或者，需要额外的逻辑来保持 ZombieStats.CurrentHealth 与此 BindableProperty 同步（如果仍需双向同步）。
        // 当前重构假设 Zombie 类的 BindableProperty 是权威来源。
        public BindableProperty<int> CurrentHealth { get; private set; }
        public BindableProperty<bool> IsDead { get; private set; }

        // 构造函数
        public Zombie(ZombieStats stats, Vector2 spawnPosition, Vector2 targetPosition)
        {
            Stats = stats; // ZombieStats 仍然用于存储基础属性如MaxHealth, AttackPower, MovementSpeed
            Position = spawnPosition;
            TargetPosition = targetPosition;

            // 初始化 BindableProperty
            this.CurrentHealth = new BindableProperty<int>(this.Stats.MaxHealth);
            this.IsDead = new BindableProperty<bool>(false);
        }

        // 僵尸受到伤害
        public void TakeDamage(int amount)
        {
            if (IsDead.Value) // 读取 .Value
            {
                return; // 如果已死亡，则不执行任何操作
            }

            CurrentHealth.Value -= amount; // 修改 .Value

            // Debug.Log($"僵尸 {Id} 受到了 {amount} 点伤害，当前生命值：{CurrentHealth.Value}"); // 可选日志

            if (CurrentHealth.Value <= 0)
            {
                CurrentHealth.Value = 0; // 修改 .Value，确保生命值不为负
                IsDead.Value = true;    // 修改 .Value，标记为死亡
                // Debug.Log($"僵尸 {Id} 已死亡。"); // 可选日志
            }
        }

        // 僵尸移动
        public void Move(float deltaTime)
        {
            if (IsDead.Value) return; // 如果已死亡，则不移动 (读取 .Value)
            Vector2 direction = (TargetPosition - Position).normalized; // 计算朝向目标的单位向量
            Position += direction * Stats.MovementSpeed * deltaTime; // 根据速度和时间更新位置
        }

        // 僵尸攻击目标
        public void AttackTarget()
        {
            if(IsDead.Value) return; // 如果已死亡，则不攻击 (读取 .Value)
            // 根据当前提示使用完整ID记录攻击日志
            Debug.Log($"僵尸 {Id} 攻击了位于 {TargetPosition} 的目标，造成 {Stats.AttackPower} 点伤害！");
        }
    }
}
