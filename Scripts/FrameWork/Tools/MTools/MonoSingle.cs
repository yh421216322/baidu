﻿using System.Reflection;
using System;
using UnityEngine;
using System.Threading.Tasks;

namespace MyGameNamespace
{
    public interface ISingleton { void Init(); }
    public abstract class Singleton<S> where S : class, ISingleton
    {
        private static S mInstance;
        public static S GetIns()
        {
            if (mInstance == null)
            {
                var ctor = Array.Find(
                    typeof(S).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic),
                    c => c.GetParameters().Length == 0);
                if (ctor == null) throw new Exception($"{typeof(S).Name}缺少私有构造函数");
                mInstance = ctor.Invoke(null) as S;
                mInstance.Init();
            }
            return mInstance;
        }
    }
    /// <summary>
    /// Unity的Mono单例基类
    /// </summary>
    public abstract class MonoSingle<T> : MonoBehaviour where T : MonoSingle<T>
    {
        private static bool mInited = false;
        private static T _instance;
        protected static bool _applicationIsQuitting = false;

        public static T GetIns()
        {
#if UNITY_EDITOR
            // 防止编辑器意外创建 主要是 ExecuteInEditMode
            if (!UnityEditor.EditorApplication.isPlaying) return _instance;
#endif
            if (_instance == null && !_applicationIsQuitting)
            {
                _instance = new GameObject(typeof(T).ToString()).AddComponent<T>();
                if (!mInited)
                {
                    _instance.Init();
                    mInited = true;
                }
                DontDestroyOnLoad(_instance.gameObject);
            }
            return _instance;
        }
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this as T;
                if (!mInited)
                {
                    _instance.Init();
                    mInited = true;
                }
                DontDestroyOnLoad(gameObject);
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        protected virtual void Init() { }
        protected abstract void DeInit();
        private async void OnDestroy()
        {
            if (_instance == this)
            {
                _applicationIsQuitting = true;
                await Task.Yield();
                _instance.DeInit();
            }
        }
        private void OnApplicationQuit()
        {
            _applicationIsQuitting = true;
        }
    }
}