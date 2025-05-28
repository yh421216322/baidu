using System;
using System.Collections.Generic;
using System.Text;
using MyGameNamespace;
using UnityEngine;
using QFramework;
public class SubPool:ICanSendEvent
{
    Transform m_parent;

    //预设
    GameObject m_prefab;

    //集合
    List<GameObject> m_objects = new List<GameObject>();

    //名字标识
    public string Name
    {
        
        get { return m_prefab.name; }
    }

    //构造
    public SubPool(GameObject prefab)
    {
        this.m_parent = new RectTransform();
        this.m_prefab = prefab;
    }

    //取对象
    public GameObject Spawn()
    {
        GameObject go = null;

        if (m_objects.Count!= 0&&m_objects[0]==null)
        {
            m_objects.Clear();
        }

        foreach (GameObject obj in m_objects)
        {
            
            if (!obj.activeSelf)
            {
                go = obj;
                break;
            }
        }

        if (go == null)
        {
            go = GameObject.Instantiate<GameObject>(m_prefab);
            go.transform.parent = m_parent;
            m_objects.Add(go);
        }
        // if (go.GetComponent<Spawn>())
        // {
        //     go.GetComponent<Spawn>().OnSpawn();
        //   
        // }
        
        go.SetActive(true);
        go.SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);
        return go;
    }

    //回收对象
    public void Unspawn(GameObject go)
    {
        
        if (Contains(go))
        {
            if (go != null) // 检查对象是否为空
            {
                ReusbleObject spawnComponent = go.GetComponentInChildren<ReusbleObject>();
                if (spawnComponent != null)
                {
                    spawnComponent.OnUnspawn();
                }

                go.SendMessage("OnUnspawn", SendMessageOptions.DontRequireReceiver);
                go.SetActive(false);
            }
            else
            {
                //Debug.LogError("Trying to unspawn a null GameObject.");
            }
        }
    }

    //回收该池子的所有对象
    public void UnspawnAll()
    {
        foreach (GameObject item in m_objects)
        {
            if (item.activeSelf)
            {
                Unspawn(item);
            }
        }
    }

    //是否包含对象
    public bool Contains(GameObject go)
    {
        return m_objects.Contains(go);
    }

    public IArchitecture GetArchitecture()
    {
        return RegisterManager.Interface;
    }
}

public abstract class ReusbleObject : MonoBehaviour, IReusable
{

    public abstract void OnSpawn();

    public abstract void OnUnspawn();

}

public interface IReusable
{
    //当取出时调用
    void OnSpawn();

    //当回收时调用
    void OnUnspawn();
}