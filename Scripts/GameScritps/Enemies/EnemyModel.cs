using QFramework;
using System.Collections.Generic;
using System;
using UnityEngine; // 仅当 Zombie 类本身直接为非 MonoBehaviour 使用 Unity 类型时才需要
// 假设事件定义在 YourGameNamespace.Events 命名空间下
using YourGameNamespace.Events;

namespace YourGameNamespace.Enemies
{
    public class EnemyModel : AbstractModel
    {
        private List<Zombie> mZombies = new List<Zombie>(); // 存储所有僵尸的列表

        protected override void OnInit()
        {
           // 初始化时不需要特殊操作
        }

        // 添加僵尸到模型中
        public void AddZombie(Zombie zombie)
        {
            if (zombie != null && !mZombies.Contains(zombie)) // 确保僵尸不为空且未重复添加
            {
                mZombies.Add(zombie);
                this.SendEvent(new Model_EnemyAddedEvent() { Enemy = zombie }); // 发送敌人添加事件
            }
        }

        // 从模型中移除僵尸
        public void RemoveZombie(Zombie zombie)
        {
            if (zombie != null && mZombies.Contains(zombie)) // 确保僵尸不为空且存在于列表中
            {
                Guid zombieId = zombie.Id; // 先获取ID，因为对象移除后可能无法访问
                mZombies.Remove(zombie);
                this.SendEvent(new Model_EnemyRemovedEvent() { ZombieId = zombieId }); // 发送敌人移除事件
            }
        }

        // 获取所有僵尸的列表
        public List<Zombie> GetAllZombies()
        {
            // 返回一个新列表副本，以防止外部直接修改内部列表。
            return new List<Zombie>(mZombies);
        }

        // 清空所有僵尸
        public void ClearZombies()
        {
            mZombies.Clear();
            this.SendEvent(new Model_AllEnemiesClearedEvent()); // 发送所有敌人已清除事件
        }
    }
}
