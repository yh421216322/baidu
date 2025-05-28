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

        void Unspwan(GameObject go);
        GameObject Spanw(String name);
        public void UnspawnAll();

    }


    public class ObjectPoolSystem : AbstractSystem, IObjectPoolSystem
    {
        public string ResourceDir = "";

        Dictionary<string, SubPool> m_pools = new Dictionary<string, SubPool>();

        //取对象
        public GameObject Spanw(string name)
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
                RegisterNew(addName);
            SubPool pool = m_pools[addName];
            return pool.Spawn();

        }

        public void Unspwan(GameObject go)
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
                //  Debug.Log("没有找到预制体,名称为"+ path);
            }

            //创建子对象池
            SubPool pool = new SubPool(prefab);
            m_pools.Add(pool.Name, pool);
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