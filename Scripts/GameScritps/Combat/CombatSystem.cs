using QFramework;
using UnityEngine;
using System.Collections.Generic;
using YourGameNamespace.Enemies;
using YourGameNamespace.Survivors;
using YourGameNamespace.Framework; // 用于 GameDataModel
using YourGameNamespace.Events;   // 用于战斗相关事件
using YourGameNamespace.Research; // 用于 TechnologyEffectType
using System;
using MyGameNamespace; // For IObjectPoolSystem (assuming it's here, adjust if different)

namespace YourGameNamespace.Combat
{
    // Interface definition updated and moved to the top of the file
    public interface ICombatSystem : QFramework.QFISystem
    {
        Vector2 BasePosition { get; set; }
        float ZombieAttackRange { get; set; }
        float SurvivorAttackRange { get; set; }
        int BaseSurvivorAttackPower { get; set; }
        float SurvivorAttackPowerMultiplier { get; } // Setter is private in implementation
        GameResourceType AmmoType { get; set; }
        int AmmoCostPerShot { get; set; }

        void ApplyAttackPowerMultiplierBonus(float bonusValue);
        void SetAttackPowerMultiplier(float newValue);
        void ApplyResearchEffectToCombat(TechnologyEffectType effectType, float value);
        void SpawnZombieWaveForDay(int day);
        void UpdateCombat(float deltaTime);
        // FindClosestZombie is private, not part of the interface
    }

    public class CombatSystem : AbstractSystem, ICombatSystem // Implements the updated interface
    {
        private EnemyModel mEnemyModel;
        private SurvivorModel mSurvivorModel;
        private ResourceModel mResourceModel;
        private GameDataModel mGameDataModel;
        private IObjectPoolSystem mObjectPoolSystem; // 对象池系统
        private Dictionary<Guid, ZombieView> mActiveZombieViews = new Dictionary<Guid, ZombieView>();

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
            mGameDataModel = this.GetModel<GameDataModel>();
            mObjectPoolSystem = this.GetSystem<IObjectPoolSystem>(); // 初始化对象池系统
            if (mGameDataModel == null) Debug.LogError("战斗系统：游戏数据模型 (GameDataModel) 为空！");
            if (mObjectPoolSystem == null) Debug.LogError("CombatSystem: 未能获取对象池系统 (IObjectPoolSystem)！");
            mActiveZombieViews.Clear(); //确保在系统初始化时清空
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

                string zombiePrefabName = "Prefabs/Enemies/Zombie_PF"; // 预制件在Resources下的路径
                GameObject zombieGO = null;
                if (mObjectPoolSystem != null) // 确保对象池系统已获取
                {
                    zombieGO = mObjectPoolSystem.Spawn(zombiePrefabName);
                }
                else // Fallback or error if pool system is missing
                {
                    Debug.LogError("CombatSystem: 对象池系统未初始化，无法生成僵尸！");
                    continue; // 如果没有对象池，则不生成此僵尸
                }

                if (zombieGO == null)
                {
                    Debug.LogError($"CombatSystem: 未能从对象池生成预制件 {zombiePrefabName}！请检查路径和对象池设置。");
                    continue;
                }

                // 设置GameObject的初始位置和激活状态
                zombieGO.transform.position = spawnPos;
                zombieGO.SetActive(true); // 对象池可能在回收时禁用了它

                ZombieView zombieView = zombieGO.GetComponent<ZombieView>();
                if (zombieView == null)
                {
                    Debug.LogError($"CombatSystem: 生成的僵尸预制件 {zombiePrefabName} (实例名: {zombieGO.name}) 上没有找到 ZombieView 脚本！");
                    if (mObjectPoolSystem != null) mObjectPoolSystem.Unspawn(zombieGO); // 回收错误的实例
                    else GameObject.Destroy(zombieGO);
                    continue;
                }

                if (mObjectPoolSystem != null)
                {
                    zombieView.InitPool(mObjectPoolSystem); // 注入对象池引用
                }

                Zombie zombieData = new Zombie(stats, spawnPos, BasePosition); // 创建逻辑数据对象
                zombieView.Setup(zombieData); // 关联数据到View

                if (!mActiveZombieViews.ContainsKey(zombieData.Id))
                {
                    mActiveZombieViews.Add(zombieData.Id, zombieView);
                }
                else
                {
                    Debug.LogWarning($"CombatSystem: Zombie Id {zombieData.Id} 已存在于 mActiveZombieViews 字典中。旧的View将被覆盖。");
                    mActiveZombieViews[zombieData.Id] = zombieView;
                }

                mEnemyModel.AddZombie(zombieData); // EnemyModel仍管理逻辑数据对象
                Debug.Log($"已生成僵尸 {zombieData.Id} 于 {spawnPos} (第 {day} 天)");
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
                if (zombie.IsDead.Value)
                {
                    if (!zombiesToRemove.Contains(zombie))
                    {
                        // Instead of adding to a list for later model removal,
                        // get the ZombieView and tell it to start its death sequence.
                        // The ZombieView will then be responsible for calling Unspawn on itself.
                        // We need a way to map Zombie data to ZombieView instance.
                        // This might require CombatSystem to keep a dictionary or EnemyModel to hold the GO reference.
                        // For now, let's assume we can find the GameObject/ZombieView.
                        // Get the ZombieView from the dictionary and command it to recycle
                        if (mActiveZombieViews.TryGetValue(zombie.Id, out ZombieView viewToRecycle))
                        {
                            viewToRecycle.TriggerDeathSequenceAndRecycle(); // Command View to start death sequence and self-recycle
                            mActiveZombieViews.Remove(zombie.Id);          // Remove from active views dictionary
                        }
                        else
                        {
                            // Log if a dead zombie's view was not found in the active views,
                            // which might indicate an issue if it wasn't properly removed before or added.
                            Debug.LogWarning($"CombatSystem: ZombieView for dead zombie {zombie.Id} not found in mActiveZombieViews for recycling.");
                        }

                        // Add to list for model removal AFTER iterating or it modifies collection
                        // This ensures the logical data is removed from EnemyModel
                        zombiesToRemove.Add(zombie);
                    }
                    continue;
                }
                zombie.Move(deltaTime);
                if (Vector2.Distance(zombie.Position, BasePosition) < ZombieAttackRange)
                {
                    zombie.AttackTarget();
                    mGameDataModel.ApplyDamageToBase(zombie.Stats.AttackPower);

                    if (mActiveZombieViews.TryGetValue(zombie.Id, out ZombieView attackingZombieView))
                    {
                        attackingZombieView.PlayAttackAnimation();
                    }

                    Debug.LogWarning($"基地受到僵尸 {zombie.Id} 的 {zombie.Stats.AttackPower} 点伤害。基地生命值：{mGameDataModel.BaseHealth.Value}");
                    if (mGameDataModel.BaseHealth.Value <= 0)
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
