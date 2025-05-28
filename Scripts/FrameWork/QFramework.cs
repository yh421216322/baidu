/****************************************************************************
 * Copyright (c) 2015 ~ 2022 liangxiegame MIT License
 *
 * QFramework v1.0
 *
 * https://qframework.cn
 * https://github.com/liangxiegame/QFramework
 * https://gitee.com/liangxiegame/QFramework
 * 
 * Author:
 *  liangxie        https://github.com/liangxie
 *  soso            https://github.com/so-sos-so
 *
 * Contributor
 *  TastSong        https://github.com/TastSong
 *  京产肠饭         https://gitee.com/JingChanChangFan/hk_-unity-tools
 * 
 * Community
 *  QQ Group: 623597263
 * Latest Update: 2021.5.2 14:08 Return IUnRegister for UnRegisterWhenGameObjectDestroyed
 ****************************************************************************/

using System;
using System.Collections.Generic;
using MyGameNamespace;
using UnityEngine;

namespace QFramework
{
    #region Architecture

    public interface IArchitecture
    {
        void RegisterSystem<T>(T system) where T : QFISystem;

        void RegisterModel<T>(T model) where T : QFIModel;

        void RegisterUtility<T>(T utility) where T : QFIUtility;

        T GetSystem<T>() where T : class, QFISystem;

        T GetModel<T>() where T : class, QFIModel;

        T GetUtility<T>() where T : class, QFIUtility;

        void SendCommand<T>() where T : QFICommand, new();
        void SendCommand<T>(T command) where T : QFICommand;

        TResult SendQuery<TResult>(IQuery<TResult> query);

        void SendEvent<T>() where T : new();
        void SendEvent<T>(T e);

        IUnRegister RegisterEvent<T>(Action<T> onEvent);
        void UnRegisterEvent<T>(Action<T> onEvent);

        void AllUnRegisterEvent<T>();
    }

    public abstract class Architecture<T> : IArchitecture where T : Architecture<T>, new()
    {
        /// <summary>
        /// 是否初始化完成 
        /// </summary>
        private bool mInited = false;

        private List<QFISystem> mSystems = new List<QFISystem>();

        private List<QFIModel> mModels = new List<QFIModel>();

        public static Action<T> OnRegisterPatch = architecture => { };

        private static T mArchitecture;

        public static IArchitecture Interface
        {
            get
            {
                if (mArchitecture == null)
                {
                    MakeSureArchitecture();
                }

                return mArchitecture;
            }
        }


        static void MakeSureArchitecture()
        {
            if (mArchitecture == null)
            {
                mArchitecture = new T();
                mArchitecture.Init();

                OnRegisterPatch?.Invoke(mArchitecture);

                foreach (var architectureModel in mArchitecture.mModels)
                {
                    architectureModel.Init();
                }

                mArchitecture.mModels.Clear();

                foreach (var architectureSystem in mArchitecture.mSystems)
                {
                    architectureSystem.Init();
                }

                mArchitecture.mSystems.Clear();

                mArchitecture.mInited = true;
            }
        }

        protected abstract void Init();

        private IOCContainer mContainer = new IOCContainer();

        public void RegisterSystem<TSystem>(TSystem system) where TSystem : QFISystem
        {
            system.SetArchitecture(this);
            mContainer.Register<TSystem>(system);

            if (!mInited)
            {
                mSystems.Add(system);
            }
            else
            {
                system.Init();
            }
        }

        public void RegisterModel<TModel>(TModel model) where TModel : QFIModel
        {
            model.SetArchitecture(this);
            mContainer.Register<TModel>(model);

            if (!mInited)
            {
                mModels.Add(model);
            }
            else
            {
                model.Init();
            }
        }

        public void RegisterUtility<TUtility>(TUtility utility) where TUtility : QFIUtility
        {
            mContainer.Register<TUtility>(utility);
        }

        public TSystem GetSystem<TSystem>() where TSystem : class, QFISystem
        {
            return mContainer.Get<TSystem>();
        }

        public TModel GetModel<TModel>() where TModel : class, QFIModel
        {
            return mContainer.Get<TModel>();
        }

        public TUtility GetUtility<TUtility>() where TUtility : class, QFIUtility
        {
            return mContainer.Get<TUtility>();
        }

        public void SendCommand<TCommand>() where TCommand : QFICommand, new()
        {
            var command = new TCommand();
            command.SetArchitecture(this);
            command.Execute();
        }

        public void SendCommand<TCommand>(TCommand command) where TCommand : QFICommand
        {
            command.SetArchitecture(this);
            command.Execute();
        }

        public TResult SendQuery<TResult>(IQuery<TResult> query)
        {
            query.SetArchitecture(this);
            return query.Do();
        }

        private TypeEventSystem mTypeEventSystem = new TypeEventSystem();

        public void SendEvent<TEvent>() where TEvent : new()
        {
            mTypeEventSystem.Send<TEvent>();
        }

        public void SendEvent<TEvent>(TEvent e)
        {
            mTypeEventSystem.Send<TEvent>(e);
        }

        public IUnRegister RegisterEvent<TEvent>(Action<TEvent> onEvent)
        {
            return mTypeEventSystem.Register<TEvent>(onEvent);
        }

        public void UnRegisterEvent<TEvent>(Action<TEvent> onEvent)
        {
            mTypeEventSystem.UnRegister<TEvent>(onEvent);
        }

        public void AllUnRegisterEvent<TEvent>()
        {
            mTypeEventSystem.AllUnRegister<TEvent>();
        }
    }

    public interface IOnEvent<T>
    {
        void OnEvent(T e);
    }

    public static class OnGlobalEventExtension
    {
        public static IUnRegister RegisterEvent<T>(this IOnEvent<T> self) where T : struct
        {
            return TypeEventSystem.Global.Register<T>(self.OnEvent);
        }

        public static void UnRegisterEvent<T>(this IOnEvent<T> self) where T : struct
        {
            TypeEventSystem.Global.UnRegister<T>(self.OnEvent);
        }
    }

    #endregion

    #region Controller

    public interface IController : IBelongToArchitecture, ICanSendCommand, ICanGetSystem, ICanGetModel,
        ICanRegisterEvent, ICanSendQuery
    {
        
    }

    public class AbstractRegisterManagerController :MonoBehaviour,IController
    {
        public  IArchitecture GetArchitecture()
        {
            return RegisterManager.Interface;
        }
    }

    #endregion

    #region System

    public interface QFISystem : IBelongToArchitecture, ICanSetArchitecture, ICanGetModel, ICanGetUtility,
        ICanRegisterEvent, ICanSendEvent, ICanGetSystem
    {
        void Init();
    }

    public abstract class AbstractSystem : QFISystem
    {
        private IArchitecture mArchitecture;

        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return mArchitecture;
        }

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture)
        {
            mArchitecture = architecture;
        }

        void QFISystem.Init()
        {
            OnInit();
        }

        protected abstract void OnInit();
    }

    #endregion

    #region Model

    public interface QFIModel : IBelongToArchitecture, ICanSetArchitecture, ICanGetUtility, ICanSendEvent
    {
        void Init();
    }

    public abstract class AbstractModel : QFIModel
    {
        private IArchitecture mArchitecturel;

        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return mArchitecturel;
        }

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture)
        {
            mArchitecturel = architecture;
        }

        void QFIModel.Init()
        {
            OnInit();
        }

        protected abstract void OnInit();
    }

    #endregion

    #region Utility

    public interface QFIUtility
    {
    }

    #endregion

    #region Command

    public interface QFICommand : IBelongToArchitecture, ICanSetArchitecture, ICanGetSystem, ICanGetModel, ICanGetUtility,
        ICanSendEvent, ICanSendCommand, ICanSendQuery
    {
        void Execute();
    }

    public abstract class AbstractCommand : QFICommand
    {
        private IArchitecture mArchitecture;

        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return mArchitecture;
        }

        void ICanSetArchitecture.SetArchitecture(IArchitecture architecture)
        {
            mArchitecture = architecture;
        }

        void QFICommand.Execute()
        {
            OnExecute();
        }

        protected abstract void OnExecute();
    }

    #endregion

    #region Query

    public interface IQuery<TResult> : IBelongToArchitecture, ICanSetArchitecture, ICanGetModel, ICanGetSystem,
        ICanSendQuery
    {
        TResult Do();
    }

    public abstract class AbstractQuery<T> : IQuery<T>
    {
        public T Do()
        {
            return OnDo();
        }

        protected abstract T OnDo();


        private IArchitecture mArchitecture;

        public IArchitecture GetArchitecture()
        {
            return mArchitecture;
        }

        public void SetArchitecture(IArchitecture architecture)
        {
            mArchitecture = architecture;
        }
    }

    #endregion

    #region Rule

    public interface IBelongToArchitecture
    {
        IArchitecture GetArchitecture();

    }

    public interface ICanSetArchitecture
    {
        void SetArchitecture(IArchitecture architecture);
    }

    public interface ICanGetModel : IBelongToArchitecture
    {
    }

    public static class CanGetModelExtension
    {
        public static T GetModel<T>(this ICanGetModel self) where T : class, QFIModel
        {
            return self.GetArchitecture().GetModel<T>();
        }
    }

    public interface ICanGetSystem : IBelongToArchitecture
    {
    }

    public static class CanGetSystemExtension
    {
        public static T GetSystem<T>(this ICanGetSystem self) where T : class, QFISystem
        {
            return self.GetArchitecture().GetSystem<T>();
        }
    }

    public interface ICanGetUtility : IBelongToArchitecture
    {
    }

    public static class CanGetUtilityExtension
    {
        public static T GetUtility<T>(this ICanGetUtility self) where T : class, QFIUtility
        {
            return self.GetArchitecture().GetUtility<T>();
        }
    }

    public interface ICanRegisterEvent : IBelongToArchitecture
    {
    }

    public static class CanRegisterEventExtension
    {
        public static IUnRegister RegisterEvent<T>(this ICanRegisterEvent self, Action<T> onEvent)
        {
            return self.GetArchitecture().RegisterEvent<T>(onEvent);
        }

        public static void UnRegisterEvent<T>(this ICanRegisterEvent self, Action<T> onEvent)
        {
            self.GetArchitecture().UnRegisterEvent<T>(onEvent);
        }

        public static void AllUnRegisterEventt<T>(this ICanRegisterEvent self)
        {
            self.GetArchitecture().AllUnRegisterEvent<T>();
        }
    }

    public interface ICanSendCommand : IBelongToArchitecture
    {
    }

    public static class CanSendCommandExtension
    {
        public static void SendCommand<T>(this ICanSendCommand self) where T : QFICommand, new()
        {
            self.GetArchitecture().SendCommand<T>();
        }

        public static void SendCommand<T>(this ICanSendCommand self, T command) where T : QFICommand
        {
            self.GetArchitecture().SendCommand<T>(command);
        }
    }

    public interface ICanSendEvent : IBelongToArchitecture
    {
    }

    public static class CanSendEventExtension
    {
        public static void SendEvent<T>(this ICanSendEvent self) where T : new()
        {
            self.GetArchitecture().SendEvent<T>();
        }

        public static void SendEvent<T>(this ICanSendEvent self, T e)
        {
            self.GetArchitecture().SendEvent<T>(e);
        }
    }

    public interface ICanSendQuery : IBelongToArchitecture
    {
    }

    public static class CanSendQueryExtension
    {
        public static TResult SendQuery<TResult>(this ICanSendQuery self, IQuery<TResult> query)
        {
            return self.GetArchitecture().SendQuery(query);
        }
    }

    #endregion

    #region TypeEventSystem

    public interface IUnRegister
    {
        void UnRegister();
    }

    public interface IUnRegisterList
    {
        List<IUnRegister> UnregisterList { get; }
    }

    public static class IUnRegisterListExtension
    {
        public static void AddToUnregisterList(this IUnRegister self, IUnRegisterList unRegisterList)
        {
            unRegisterList.UnregisterList.Add(self);
        }

        public static void UnRegisterAll(this IUnRegisterList self)
        {
            foreach (var unRegister in self.UnregisterList)
            {
                unRegister.UnRegister();
            }

            self.UnregisterList.Clear();
        }
    }

    /// <summary>
    /// 自定义可注销的类
    /// </summary>
    public struct CustomUnRegister : IUnRegister
    {
        /// <summary>
        /// 委托对象
        /// </summary>
        private Action mOnUnRegister { get; set; }

        /// <summary>
        /// 带参构造函数
        /// </summary>
        /// <param name="onDispose"></param>
        public CustomUnRegister(Action onUnRegsiter)
        {
            mOnUnRegister = onUnRegsiter;
        }

        /// <summary>
        /// 资源释放
        /// </summary>
        public void UnRegister()
        {
            mOnUnRegister.Invoke();
            mOnUnRegister = null;
        }
    }

    /// <summary>
    /// 销毁时注销所有事件
    /// </summary>
    public class UnRegisterOnDestroyTrigger : MonoBehaviour
    {
        private readonly HashSet<IUnRegister> mUnRegisters = new HashSet<IUnRegister>();

        public void AddUnRegister(IUnRegister unRegister)
        {
            mUnRegisters.Add(unRegister);
        }

        public void RemoveUnRegister(IUnRegister unRegister)
        {
            mUnRegisters.Remove(unRegister);
        }

        private void OnDestroy()
        {
           // Debug.Log("这是销毁");
            foreach (var unRegister in mUnRegisters)
            {
                unRegister.UnRegister();
            }
        
            mUnRegisters.Clear();
        }
        
        private void OnDisable()
        {
              // Debug.Log("诶唷，触发了哟！");
            foreach (var unRegister in mUnRegisters)
            {
                unRegister.UnRegister();
            }

            mUnRegisters.Clear();
        }
        
    }
    /// <summary>
    /// 停用时注销所有事件
    /// </summary>
    public class UnRegisterOnUnSpawnTrigger : MonoBehaviour
    {
        private readonly HashSet<IUnRegister> mUnRegisters = new HashSet<IUnRegister>();

        public void AddUnRegister(IUnRegister unRegister)
        {
            mUnRegisters.Add(unRegister);
        }

        public void RemoveUnRegister(IUnRegister unRegister)
        {
            mUnRegisters.Remove(unRegister);
        }


        private void OnDisable()
        {
//            Debug.Log("诶唷，触发了哟！");
            foreach (var unRegister in mUnRegisters)
            {
                unRegister.UnRegister();
            }

            mUnRegisters.Clear();
        }

        
    }
    


    public static class UnRegisterExtension
    {
        public static IUnRegister UnRegisterWhenGameObjectDestroyed(this IUnRegister unRegister, GameObject gameObject)
        {
            var trigger = gameObject.GetComponent<UnRegisterOnDestroyTrigger>();

            if (!trigger)
            {
                trigger = gameObject.AddComponent<UnRegisterOnDestroyTrigger>();
            }

            trigger.AddUnRegister(unRegister);
            
            return unRegister;
        }
        
        public static IUnRegister UnRegisterWhenGameObjectUnSpawned(this IUnRegister unRegister, GameObject gameObject)
        {
            var trigger = gameObject.GetComponent<UnRegisterOnUnSpawnTrigger>();

            if (!trigger)
            {
                trigger = gameObject.AddComponent<UnRegisterOnUnSpawnTrigger>();
            }

            trigger.AddUnRegister(unRegister);
            
            return unRegister;
        }
    }

    public class TypeEventSystem
    {
        private readonly EasyEvents mEvents = new EasyEvents();


        public static readonly TypeEventSystem Global = new TypeEventSystem();

        public void Send<T>() where T : new()
        {
            mEvents.GetEvent<EasyEvent<T>>()?.Trigger(new T());
        }

        public void Send<T>(T e)
        {
            mEvents.GetEvent<EasyEvent<T>>()?.Trigger(e);
        }

        public IUnRegister Register<T>(Action<T> onEvent)
        {
            var e = mEvents.GetOrAddEvent<EasyEvent<T>>();
            return e.Register(onEvent);
        }

        public void UnRegister<T>(Action<T> onEvent)
        {
            var e = mEvents.GetEvent<EasyEvent<T>>();
            if (e != null)
            {
                e.UnRegister(onEvent);
            }
        }

        public void AllUnRegister<T>()
        {var e = mEvents.GetEvent<EasyEvent<T>>();
        e.AllUnRegister();
            
        }
    }

    #endregion

    #region IOC

    public class IOCContainer
    {
        private Dictionary<Type, object> mInstances = new Dictionary<Type, object>();

        public void Register<T>(T instance)
        {
            var key = typeof(T);

            if (mInstances.ContainsKey(key))
            {
                mInstances[key] = instance;
            }
            else
            {
                mInstances.Add(key, instance);
            }
        }

        public T Get<T>() where T : class
        {
            var key = typeof(T);

            if (mInstances.TryGetValue(key, out var retInstance))
            {
                return retInstance as T;
            }

            return null;
        }
    }

    #endregion

    #region BindableProperty

    public interface IBindableProperty<T> : IReadonlyBindableProperty<T>
    {
        new T Value { get; set; }
        void SetValueWithoutEvent(T newValue);
    }

    public interface IReadonlyBindableProperty<T>
    {
        T Value { get; }
        
        IUnRegister RegisterWithInitValue(Action<T> action);
       
        void UnRegister(Action<T> onValueChanged);
        IUnRegister Register(Action<T> onValueChanged);
    }

    public class BindableProperty<T> : IBindableProperty<T>
    {
        public BindableProperty(T defaultValue = default)
        {
            mValue = defaultValue;
        }

        protected T mValue;

        public T Value
        {
            get => GetValue();
            set
            {
                if (value == null && mValue == null) return;
                if (value != null && value.Equals(mValue)) return;
            
                
                /*SetValue(value);
                mOnValueChanged?.Invoke(value);*/
                
                T oldValue = mValue; // 保存旧值
                SetValue(value); // 更新值

                mOnValueChanged?.Invoke( value); // 传递旧值和新值
                //mOnValueChangedBefore?.Invoke(oldValue, value); // 传递旧值和新值
               
            }
        }

        protected virtual void SetValue(T newValue)
        {
            mValue = newValue;
        }

        protected virtual T GetValue()
        {
            return mValue;
        }

        public void SetValueWithoutEvent(T newValue)
        {
            mValue = newValue;
        }

        private Action<T> mOnValueChanged = (v) => { };
        
        private Action<T, T> mOnValueChangedBefore = (oldValue, newValue) => { };
       
        private List<Action<T>> registeredHandlers = new List<Action<T>>();
        
        private List<Action<T, T>> registeredBeforeHandlers = new List<Action<T, T>>();

        public IUnRegister Register(Action<T> onValueChanged)
        {

            if (!registeredHandlers.Contains(onValueChanged))
            {
                mOnValueChanged += onValueChanged;
                registeredHandlers.Add(onValueChanged);
            }
            else
            {
                Debug.Log("重复注册了！"+onValueChanged);
            }

            return new BindablePropertyUnRegister<T>()
            {
                BindableProperty = this,
                OnValueChanged = onValueChanged
              
            };
        }

        public IUnRegister RegisterBefore(Action<T, T> onValueChangedBefore)
        {
            if (!registeredBeforeHandlers.Contains(onValueChangedBefore))
            {
                mOnValueChangedBefore += onValueChangedBefore;
                registeredBeforeHandlers.Add(onValueChangedBefore);
            }
            else
            {
                // 已经注册了相同的处理器
            }

            return new BindablePropertyUnRegisterBefore<T>()
            {
                BindableProperty = this,
                OnValueChangedBefore = onValueChangedBefore
            };
        }
        
        

        public IUnRegister RegisterWithInitValue(Action<T> onValueChanged)
        {
            onValueChanged(mValue);
            return Register(onValueChanged);
        }
        
        public IUnRegister RegisterWithInitValueBefore(Action<T, T> onValueChangedBefore)
        {
            onValueChangedBefore(mValue, mValue); // 使用当前值调用事件处理器
            return RegisterBefore(onValueChangedBefore); // 进行常规注册
        }


        public static implicit operator T(BindableProperty<T> property)
        {
            return property.Value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }

        public void UnRegister(Action<T> onValueChanged)
        {
            mOnValueChanged -= onValueChanged;

            if (registeredHandlers.Contains(onValueChanged))
            {
                registeredHandlers.Remove(onValueChanged);
            }
        }
       

        public void UnRegisterBefore(Action<T, T> onValueChangedBefore)
        {
            mOnValueChangedBefore -= onValueChangedBefore;
            if (registeredBeforeHandlers.Contains(onValueChangedBefore))
            {
                registeredBeforeHandlers.Remove(onValueChangedBefore);
            }
        }

        
    }
    public class BindablePropertyUnRegister<T> : IUnRegister
    {
        public BindableProperty<T> BindableProperty { get; set; }
        public Action<T> OnValueChanged { get; set; }

        public void UnRegister()
        {
            BindableProperty.UnRegister(OnValueChanged);
            BindableProperty = null;
            OnValueChanged = null;
        }
    }
    
    public class BindablePropertyUnRegisterBefore<T> : IUnRegister
    {
        public BindableProperty<T> BindableProperty { get; set; }
        public Action<T, T> OnValueChangedBefore { get; set; }

        public void UnRegister()
        {
            BindableProperty.UnRegisterBefore(OnValueChangedBefore);
            BindableProperty = null;
            OnValueChangedBefore = null;
        }
    }


    /*public class BindablePropertyUnRegister<T> : IUnRegister
    {
        public BindableProperty<T> BindableProperty { get; set; }

        public Action<T> OnValueChanged { get; set; }


        public void UnRegister()
        {
            BindableProperty.UnRegister(OnValueChanged);

            BindableProperty = null;
            OnValueChanged = null;
        }
    }*/

    #endregion

    #region EasyEvent

    public interface IEasyEvent
    {
    }
    
    public class EasyEvent : IEasyEvent
    {
        private Action mOnEvent = () => { };

        public IUnRegister Register(Action onEvent)
        {
            mOnEvent += onEvent;
            return new CustomUnRegister(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger()
        {
            mOnEvent?.Invoke();
        }

        public void AllUnRegister()
        {
            mOnEvent= () => { };


        }
    }

    public class EasyEvent<T> : IEasyEvent
    {
        private Action<T> mOnEvent = e => { };

        public IUnRegister Register(Action<T> onEvent)
        {
            mOnEvent += onEvent;
            return new CustomUnRegister(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action<T> onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger(T t)
        {
            mOnEvent?.Invoke(t);
        }
        public void AllUnRegister()
        {
            //mOnEvent= e => { };
            mOnEvent = null;


        }
    }

    public class EasyEvent<T, K> : IEasyEvent
    {
        private Action<T, K> mOnEvent = (t, k) => { };

        public IUnRegister Register(Action<T, K> onEvent)
        {
            mOnEvent += onEvent;
            return new CustomUnRegister(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action<T, K> onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger(T t, K k)
        {
            mOnEvent?.Invoke(t, k);
        }
        
        public void AllUnRegister()
        {
            mOnEvent= (t, k) => { };


        }
    }

    public class EasyEvent<T, K, S> : IEasyEvent
    {
        private Action<T, K, S> mOnEvent = (t, k, s) => { };

        public IUnRegister Register(Action<T, K, S> onEvent)
        {
            mOnEvent += onEvent;
            return new CustomUnRegister(() => { UnRegister(onEvent); });
        }

        public void UnRegister(Action<T, K, S> onEvent)
        {
            mOnEvent -= onEvent;
        }

        public void Trigger(T t, K k, S s)
        {
            mOnEvent?.Invoke(t, k, s);
        }
        
        public void AllUnRegister()
        {
            mOnEvent= (t, k, s) => { };


        }
    }

    public class EasyEvents
    {
        private static EasyEvents mGlobalEvents = new EasyEvents();

        public static T Get<T>() where T : IEasyEvent
        {
            return mGlobalEvents.GetEvent<T>();
        }
        

        public static void Register<T>() where T : IEasyEvent, new()
        {
            mGlobalEvents.AddEvent<T>();
        }

        private Dictionary<Type, IEasyEvent> mTypeEvents = new Dictionary<Type, IEasyEvent>();
        
        public void AddEvent<T>() where T : IEasyEvent, new()
        {
            mTypeEvents.Add(typeof(T), new T());
        }

        public T GetEvent<T>() where T : IEasyEvent
        {
            IEasyEvent e;

            if (mTypeEvents.TryGetValue(typeof(T), out e))
            {
                return (T)e;
            }

            return default;
        }

        public T GetOrAddEvent<T>() where T : IEasyEvent, new()
        {
            var eType = typeof(T);
            if (mTypeEvents.TryGetValue(eType, out var e))
            {
                return (T)e;
            }

            var t = new T();
            mTypeEvents.Add(eType, t);
            return t;
        }
        
    }

    #endregion

#if UNITY_EDITOR
    internal class EditorMenus
    {
        [UnityEditor.MenuItem("QFramework/Install QFrameworkWithToolKits")]
        public static void InstallPackageKit()
        {
            Application.OpenURL("https://qframework.cn/qf");
        }
    }
#endif
}