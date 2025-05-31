using QFramework;
using UnityEngine;
using System.Collections.Generic;
using YourGameNamespace.Enemies;
using YourGameNamespace.Survivors;
using YourGameNamespace.Framework; // 用于 GameDataModel
using YourGameNamespace.Events;   // 用于战斗相关事件
using YourGameNamespace.Research; // 用于 TechnologyEffectType
using System;

namespace YourGameNamespace.Combat
{
    // 定义战斗系统接口，如果尚未定义
    public interface ICombatSystem : ISystem
    {
        void ApplyResearchEffectToCombat(TechnologyEffectType effectType, float value);
        // 可能还有其他战斗系统需要暴露的方法
    }

    public class CombatSystem : AbstractSystem, ICombatSystem // 实现接口
    {
        private EnemyModel mEnemyModel;
        private SurvivorModel mSurvivorModel;
        private ResourceModel mResourceModel;
        private GameDataModel mGameDataModel; // 新增：游戏数据模型

        public Vector2 BasePosition { get; set; } = Vector2.zero; // 基地位置
        public float ZombieAttackRange { get; set; } = 1.0f; // 僵尸攻击范围
        public float SurvivorAttackRange { get; set; } = 5.0f; // 幸存者攻击范围
        public int BaseSurvivorAttackPower { get; set; } = 10; // 基础幸存者攻击力
        public float SurvivorAttackPowerMultiplier { get; private set; } = 1.0f; // 幸存者攻击力乘数，set改为private
        public GameResourceType AmmoType { get; set; } = GameResourceType.Ammo; // 弹药类型
        public int AmmoCostPerShot { get; set; } = 1; // 每次射击弹药消耗
        private float mTimeSinceLastSurvivorAttack = 0f; // 自上次幸存者攻击以来的时间
        private float mSurvivorAttackCooldown = 1f; // 幸存者攻击冷却时间

        protected override void OnInit()
        {
            mEnemyModel = this.GetModel<EnemyModel>();
            mSurvivorModel = this.GetModel<SurvivorModel>();
            mResourceModel = this.GetModel<ResourceModel>();
            mGameDataModel = this.GetModel<GameDataModel>(); // 初始化游戏数据模型
            if (mGameDataModel == null) Debug.LogError("战斗系统：游戏数据模型 (GameDataModel) 为空！");
        }

        public void ApplyAttackPowerMultiplierBonus(float bonusValue)
        {
            float oldMultiplier = SurvivorAttackPowerMultiplier;
            SurvivorAttackPowerMultiplier += bonusValue;
            this.SendEvent(new Combat_SurvivorAttackPowerMultiplierChangedEvent { NewMultiplier = SurvivorAttackPowerMultiplier, OldMultiplier = oldMultiplier });
            Debug.Log($"幸存者攻击力乘数已更新为：{SurvivorAttackPowerMultiplier} (增加了 {bonusValue})");
        }

        public void SetAttackPowerMultiplier(float newValue)
        {
            float oldMultiplier = SurvivorAttackPowerMultiplier;
            SurvivorAttackPowerMultiplier = newValue;
            this.SendEvent(new Combat_SurvivorAttackPowerMultiplierChangedEvent { NewMultiplier = SurvivorAttackPowerMultiplier, OldMultiplier = oldMultiplier });
            Debug.Log($"幸存者攻击力乘数已设置为：{SurvivorAttackPowerMultiplier}");
        }

        public void ApplyResearchEffectToCombat(TechnologyEffectType effectType, float value)
        {
            if (effectType == TechnologyEffectType.ModifySurvivorStat)
            {
                // 假设此效果类型总是增加攻击力乘数
                // 如果将来有其他类型的属性修改，可能需要更复杂的逻辑或在TechnologyEffectData中添加更多信息来区分
                ApplyAttackPowerMultiplierBonus(value);
            }
            // else if (effectType == ...) { /* 其他未来可能的战斗相关科技效果 */ }
            else
            {
                Debug.LogWarning($"战斗系统收到了一个未处理的科技效果类型：{effectType}");
            }
        }

        public void SpawnZombieWaveForDay(int day)
        {
            int baseZombieCount = 3; // 基础僵尸数量
            int zombiesToSpawn = baseZombieCount + (day / 2); // 每2天增加僵尸数量
            float healthMultiplier = 1f + (day - 1) * 0.1f; // 僵尸在第一天后每天增加10%的生命值
            float attackMultiplier = 1f + (day - 1) * 0.05f; // 僵尸在第一天后每天增加5%的攻击力
            Vector2 spawnAreaCenter = new Vector2(10 + day * 0.5f, 0); // 随着天数增加，生成点越来越远
            float spawnRadius = 2f + day * 0.1f; // 生成半径

            Debug.Log($"生成第 {day} 天的僵尸潮：{zombiesToSpawn} 只僵尸。生命值倍率：{healthMultiplier}，攻击力倍率：{attackMultiplier}");
            for (int i = 0; i < zombiesToSpawn; i++)
            {
                Vector2 spawnPos = spawnAreaCenter + UnityEngine.Random.insideUnitCircle * spawnRadius;
                int baseHealth = UnityEngine.Random.Range(40, 61);
                int baseAttack = UnityEngine.Random.Range(4, 7);
                ZombieStats stats = new ZombieStats(
                    maxHealth: (int)(baseHealth * healthMultiplier),
                    attackPower: (int)(baseAttack * attackMultiplier),
                    movementSpeed: UnityEngine.Random.Range(0.5f, 1.5f)
                );
                Zombie zombie = new Zombie(stats, spawnPos, BasePosition);
                mEnemyModel.AddZombie(zombie);
            }
            Debug.Log($"生成后僵尸总数：{mEnemyModel.GetAllZombies().Count}");
        }

        public void UpdateCombat(float deltaTime)
        {
            if (mEnemyModel == null || mSurvivorModel == null || mResourceModel == null || mGameDataModel == null)
            {
                Debug.LogError("战斗系统缺少一个或多个模型引用。");
                return;
            }
             if (mGameDataModel.BaseHealth.Value <= 0) return; // 游戏结束，不再处理战斗 (使用 .Value)

            List<Zombie> zombiesToRemove = new List<Zombie>();

            // 僵尸行动
            foreach (var zombie in mEnemyModel.GetAllZombies())
            {
                if (zombie.IsDead.Value) // 使用 .Value
                {
                    // 确保死亡的僵尸只添加一次到移除列表
                    if (!zombiesToRemove.Contains(zombie))
                    {
                        zombiesToRemove.Add(zombie);
                    }
                    continue;
                }
                zombie.Move(deltaTime);
                if (Vector2.Distance(zombie.Position, BasePosition) < ZombieAttackRange)
                {
                    zombie.AttackTarget(); // 僵尸记录其攻击意图
                    mGameDataModel.ApplyDamageToBase(zombie.Stats.AttackPower); // 使用 ApplyDamageToBase
                    Debug.LogWarning($"基地受到僵尸 {zombie.Id} 的 {zombie.Stats.AttackPower} 点伤害。基地生命值：{mGameDataModel.BaseHealth.Value}"); // 使用 .Value
                    if (mGameDataModel.BaseHealth.Value <= 0) // 使用 .Value
                    {
                        // mGameDataModel.BaseHealth.Value = 0; // ApplyDamageToBase 内部会处理 Clamp
                        Debug.LogError("游戏结束！基地生命值耗尽。");
                    }
                }
            }
            
            // 如果基地被摧毁，则停止进一步的攻击行动
            if (mGameDataModel.BaseHealth.Value <= 0)  // 使用 .Value
            {
                 foreach (var deadZombie in zombiesToRemove) { mEnemyModel.RemoveZombie(deadZombie); }
                 return;
            }

            // 幸存者行动
            mTimeSinceLastSurvivorAttack += deltaTime;
            if (mTimeSinceLastSurvivorAttack >= mSurvivorAttackCooldown)
            {
                List<Survivor> availableDefenders = mSurvivorModel.GetAllSurvivors()
                    .FindAll(s => s.Status.Value == SurvivorStatus.Idle || s.Profession.Value == SurvivorProfession.Soldier); // 使用 .Value

                bool livingZombiesExist = false;
                foreach(var z in mEnemyModel.GetAllZombies()) {
                    if (!z.IsDead.Value) { // 使用 .Value
                        livingZombiesExist = true;
                        break;
                    }
                }

                if (availableDefenders.Count > 0 && livingZombiesExist)
                {
                    if (mResourceModel.HasEnough(AmmoType, AmmoCostPerShot * availableDefenders.Count))
                    {
                        if (mResourceModel.ConsumeResource(AmmoType, AmmoCostPerShot * availableDefenders.Count))
                        {
                            mTimeSinceLastSurvivorAttack = 0f;
                            Debug.Log($"{availableDefenders.Count} 名防御者正在攻击！");
                            foreach (var defender in availableDefenders)
                            {
                                Zombie targetZombie = FindClosestZombie(BasePosition); 
                                if (targetZombie != null && !targetZombie.IsDead.Value && Vector2.Distance(BasePosition, targetZombie.Position) < SurvivorAttackRange) // 使用 .Value
                                {
                                    int currentAttackPower = (int)(BaseSurvivorAttackPower * SurvivorAttackPowerMultiplier);
                                    int damage = currentAttackPower;
                                    if (defender.Profession.Value == SurvivorProfession.Soldier) // 使用 .Value
                                    {
                                        damage = (int)(currentAttackPower * 1.5f);
                                    }
                                    Debug.Log($"{defender.Name.Value} 攻击僵尸 {targetZombie.Id}，造成 {damage} 点伤害 (基础: {BaseSurvivorAttackPower}, 倍率: {SurvivorAttackPowerMultiplier:F2})。"); // 使用 .Value
                                    targetZombie.TakeDamage(damage);

                                    if (targetZombie.IsDead.Value) // 检查 BindableProperty 的值
                                    {
                                        Debug.Log($"僵尸 {targetZombie.Id} 已被击杀。"); // 日志确认
                                        this.SendEvent(new Combat_ZombieDiedEvent() { ZombieId = targetZombie.Id });
                                        if (!zombiesToRemove.Contains(targetZombie)) // 确保不重复添加
                                        {
                                            zombiesToRemove.Add(targetZombie);
                                        }
                                    }
                                }
                                else if (targetZombie == null) { break; }
                            }
                        }
                        else { Debug.LogWarning("弹药检查通过但消耗资源失败。"); }
                    }
                    else { Debug.Log("防御者想攻击，但弹药不足！"); }
                }
            }

            foreach (var deadZombie in zombiesToRemove)
            {
                mEnemyModel.RemoveZombie(deadZombie);
            }
        }

        private Zombie FindClosestZombie(Vector2 position)
        {
            Zombie closest = null;
            float minDist = float.MaxValue;
            foreach (var zombie in mEnemyModel.GetAllZombies())
            {
                if (zombie.IsDead.Value) continue; // 跳过已死亡的僵尸 (使用 .Value)
                float dist = Vector2.Distance(position, zombie.Position);
                if (dist < minDist)
                {
                    closest = zombie;
                    minDist = dist;
                }
            }
            return closest;
        }
    }
}
