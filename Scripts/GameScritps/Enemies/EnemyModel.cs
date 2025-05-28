using QFramework;
using System.Collections.Generic;
using System;
using UnityEngine; // Only if Zombie class itself uses Unity types directly for non-MonoBehaviours

namespace YourGameNamespace.Enemies
{
    public class EnemyModel : AbstractModel
    {
        private List<Zombie> mZombies = new List<Zombie>();

        protected override void OnInit()
        {
           
        }

        public void AddZombie(Zombie zombie)
        {
            if (zombie != null && !mZombies.Contains(zombie))
            {
                mZombies.Add(zombie);
            }
        }

        public void RemoveZombie(Zombie zombie)
        {
            if (zombie != null)
            {
                mZombies.Remove(zombie);
            }
        }

        public List<Zombie> GetAllZombies()
        {
            // 返回一个新列表，以防止从外部修改内部列表。
            return new List<Zombie>(mZombies);
        }

        public void ClearZombies()
        {
            mZombies.Clear();
        }
    }
}
