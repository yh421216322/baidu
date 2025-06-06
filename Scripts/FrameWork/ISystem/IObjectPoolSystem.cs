using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QFramework;
using MyGameNamespace;

namespace MyGameNamespace
{
    public interface IObjectPoolSystem : QFISystem
    {

        void Unspawn(GameObject go); // 更正拼写 Unspwan -> Unspawn
        GameObject Spawn(String name); // 更正拼写 Spanw -> Spawn
        public void UnspawnAll();

    }


    public class ObjectPoolSystem : AbstractSystem, IObjectPoolSystem
    {
        public string ResourceDir = "";

        Dictionary<string, SubPool> m_pools = new Dictionary<string, SubPool>();

        //取对象
        public GameObject Spawn(string name) // 更正拼写 Spanw -> Spawn
        {
            ResourceDir = "";

            //如果name中包含"/"，则先创建子目录
            string addDir;
            string addName;

            int index = name.LastIndexOf('/'); //获取最后一个"/"的索引

            if (index == -1)
            {
                addName = name;
                addDir = "";
            }
            else
            {
                //string[] nameParts = name.Split('/');
                //按照索引位置进行字符串分割得到两个字符串
                string[] nameParts = name.Split('/');
                ResourceDir = name.Substring(0, index);
                addName = name.Substring(index+1, name.Length - index - 1);
                // ResourceDir = nameParts[0]; //获取预设的子目录
                // addName = nameParts[1]; //获取预设的名称*/

            }


            if (!m_pools.ContainsKey(addName))
            {
                RegisterNew(addName); // Attempt to register if not found
            }

            // After attempting to register, check again if the pool exists
            if (m_pools.TryGetValue(addName, out SubPool pool) && pool != null)
            {
                GameObject spawnedObject = pool.Spawn();
                if (spawnedObject == null)
                {
                    // This case might happen if SubPool.Spawn() can return null
                    // (e.g., if the prefab in the pool was actually null but the pool was still created,
                    // or if the pool has an internal limit or condition not met)
                    Debug.LogError($"对象池：子对象池 '{addName}' 存在但未能生成对象实例。预制件可能无效。");
                    return null;
                }
                return spawnedObject;
            }
            else
            {
                Debug.LogError($"对象池：名为 '{addName}' (来源于路径: {name}) 的对象池不存在或无效，无法生成对象。");
                return null; // Return null if pool doesn't exist or is invalid after registration attempt
            }
        }

        public void Unspawn(GameObject go) // 更正拼写 Unspwan -> Unspawn
        {



            SubPool pool = null;

            foreach (SubPool p in m_pools.Values)
            {
                if (p.Contains(go))
                {
                    pool = p;
                    break;
                }
            }

            //Debug.Log(go.name);

            pool.Unspawn(go);
        }

        //回收所有对象
        public void UnspawnAll()
        {
            foreach (SubPool p in m_pools.Values)
                p.UnspawnAll();
        }

        //创建新子池子
        void RegisterNew(string name)
        {


            //预设路径
            string path = "";
            if (string.IsNullOrEmpty(ResourceDir.Trim()))
                path = name;
            else
                path = ResourceDir + "/" + name;
            
            
            //Debug.Log(("对象池内部："+path));

            //加载预设
            GameObject prefab = Resources.Load<GameObject>(path);

//        Debug.Log(path);
            if (prefab == null)
            {
                Debug.LogError($"对象池：未能加载名为 '{path}' 的预制件。请确保它在Resources文件夹下并且路径正确。");
                return; // Do not create a pool if prefab is null
            }

            //创建子对象池
            SubPool pool = new SubPool(prefab);
            // Ensure pool.Name is derived correctly if prefab is valid,
            // or that Add uses 'name' if pool.Name might be null due to prefab issues.
            // Assuming SubPool constructor handles prefab.name correctly.
            if (!string.IsNullOrEmpty(pool.Name)) // Or use 'name' as key if pool.Name can be problematic
            {
                 m_pools.Add(pool.Name, pool);
            }
            else
            {
                Debug.LogError($"对象池：预制件 {path} 加载成功但其名称为空或无效，无法创建对象池。");
            }
        }


        IEnumerator Pause(float duration, GameObject go)
        {

            //  Debug.Log("进入延迟执行入池");
            yield return new WaitForSecondsRealtime(duration);

            SubPool pool = null;

            foreach (SubPool p in m_pools.Values)
            {
                if (p.Contains(go))
                {
                    pool = p;
                    break;
                }
            }

            pool.Unspawn(go);

        }

        protected override void OnInit()
        {

        }


    }
}