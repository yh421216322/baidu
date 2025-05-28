using QFramework;
using UnityEngine;
using System.Collections.Generic;
using YourGameNamespace.Enemies;
using YourGameNamespace.Survivors;

using YourGameNamespace.Framework; // For GameDataModel
using System;

namespace YourGameNamespace.Combat
{
    public class CombatSystem : AbstractSystem
    {
        private EnemyModel mEnemyModel;
        private SurvivorModel mSurvivorModel;
        private ResourceModel mResourceModel;
        private GameDataModel mGameDataModel; // 新增 GameDataModel

        public Vector2 BasePosition { get; set; } = Vector2.zero;
        public float ZombieAttackRange { get; set; } = 1.0f; 
        public float SurvivorAttackRange { get; set; } = 5.0f; 
        public int BaseSurvivorAttackPower { get; set; } = 10; // 从 SurvivorAttackPower 重命名
        public float SurvivorAttackPowerMultiplier { get; set; } = 1.0f; // 新增
        public GameResourceType AmmoType { get; set; } = GameResourceType.Ammo; 
        public int AmmoCostPerShot { get; set; } = 1; 
        private float mTimeSinceLastSurvivorAttack = 0f;
        private float mSurvivorAttackCooldown = 1f;

        protected override void OnInit()
        {
         
            mEnemyModel = this.GetModel<EnemyModel>();
            mSurvivorModel = this.GetModel<SurvivorModel>();
            mResourceModel = this.GetModel<ResourceModel>();
            mGameDataModel = this.GetModel<GameDataModel>(); // 初始化 GameDataModel
            if (mGameDataModel == null) Debug.LogError("战斗系统：游戏数据模型 (GameDataModel) 为空！");
        }

        public void SpawnZombieWaveForDay(int day)
        {
            int baseZombieCount = 3;
            int zombiesToSpawn = baseZombieCount + (day / 2); // 每2天增加数量
            float healthMultiplier = 1f + (day - 1) * 0.1f; // 僵尸在第一天后每天增加10%的生命值
            float attackMultiplier = 1f + (day - 1) * 0.05f; // 僵尸在第一天后每天增加5%的攻击力
            Vector2 spawnAreaCenter = new Vector2(10 + day * 0.5f, 0); // 随着天数增加，生成点越来越远
            float spawnRadius = 2f + day * 0.1f;

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
                // Debug.Log($"已生成僵尸 {zombie.Id} 于 {spawnPos} (第 {day} 天)"); // 可选的详细日志
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
             if (mGameDataModel.BaseHealth <= 0) return; // 游戏结束，不再处理战斗

            List<Zombie> zombiesToRemove = new List<Zombie>();

            // 僵尸行动
            foreach (var zombie in mEnemyModel.GetAllZombies())
            {
                if (zombie.IsDead)
                {
                    if (!zombiesToRemove.Contains(zombie)) zombiesToRemove.Add(zombie);
                    continue;
                }
                zombie.Move(deltaTime);
                if (Vector2.Distance(zombie.Position, BasePosition) < ZombieAttackRange)
                {
                    zombie.AttackTarget(); // 僵尸记录其攻击意图
                    mGameDataModel.BaseHealth -= zombie.Stats.AttackPower; // 基地受到伤害
                    Debug.LogWarning($"基地受到僵尸 {zombie.Id} 的 {zombie.Stats.AttackPower} 点伤害。基地生命值：{mGameDataModel.BaseHealth}");
                    if (mGameDataModel.BaseHealth <= 0)
                    {
                        mGameDataModel.BaseHealth = 0;
                        Debug.LogError("游戏结束！基地生命值耗尽。");
                        // DayNightSystem 也会检查并停止。
                        // 如果基地已被摧毁，则无需处理进一步的僵尸攻击。
                        // 然而，幸存者的攻击可能仍会清除剩余的僵尸。
                    }
                }
            }
            
            // 如果基地被摧毁，则停止进一步的攻击行动
            if (mGameDataModel.BaseHealth <= 0) 
            {
                 foreach (var deadZombie in zombiesToRemove) { mEnemyModel.RemoveZombie(deadZombie); }
                 return;
            }


            // 幸存者行动
            mTimeSinceLastSurvivorAttack += deltaTime;
            if (mTimeSinceLastSurvivorAttack >= mSurvivorAttackCooldown)
            {
                List<Survivor> availableDefenders = mSurvivorModel.GetAllSurvivors()
                    .FindAll(s => s.Status == SurvivorStatus.Idle || s.Profession == SurvivorProfession.Soldier);

                bool livingZombiesExist = false;
                foreach(var z in mEnemyModel.GetAllZombies()) { if (!z.IsDead) { livingZombiesExist = true; break; } }

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
                                if (targetZombie != null && !targetZombie.IsDead && Vector2.Distance(BasePosition, targetZombie.Position) < SurvivorAttackRange)
                                {
                                    // 用乘数计算伤害
                                    int currentAttackPower = (int)(BaseSurvivorAttackPower * SurvivorAttackPowerMultiplier);
                                    int damage = currentAttackPower;
                                    if (defender.Profession == SurvivorProfession.Soldier)
                                    {
                                        damage = (int)(currentAttackPower * 1.5f); // 士兵在其基础上仍有加成
                                    }
                                    Debug.Log($"{defender.Name} 攻击僵尸 {targetZombie.Id}，造成 {damage} 点伤害 (基础: {BaseSurvivorAttackPower}, 倍率: {SurvivorAttackPowerMultiplier:F2})。");
                                    targetZombie.TakeDamage(damage);
                                    if (targetZombie.IsDead)
                                    {
                                        Debug.Log($"僵尸 {targetZombie.Id} 已死亡。");
                                        if (!zombiesToRemove.Contains(targetZombie)) zombiesToRemove.Add(targetZombie);
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
                if (zombie.IsDead) continue;
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
