using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BBB
{
    // This is a persistent Monobehaviour between scenes. Good for systems logic such as Audio control or the like.
    public abstract class LazyMonoSingletonBase<T> : MonoBehaviour where T : LazyMonoSingletonBase<T>
    {
        static T _instance = null;
        public static T instance { get { return _instanceGetter.Invoke(); } }


        delegate T InstanceGetter();
        static InstanceGetter _instanceGetter = CreateSingleton;

        private static T CreateSingleton()
        {
            var heirarchyArray = FindObjectsOfType<T>();

            GameObject ownerObject;
            T instance;
            if (heirarchyArray.Length > 0)
            {
                instance = heirarchyArray[0];
                ownerObject = instance.gameObject;
            }
            else
            {
                ownerObject = new GameObject($"{typeof(T).Name} (singleton)");
                instance = ownerObject.AddComponent<T>();
                instance.OnCreateInstance();
            }

            DontDestroyOnLoad(ownerObject);

            _instance = instance;
            _instanceGetter = ReturnSingleton;

            return instance;
        }

        private static T ReturnSingleton()
        {
            return _instance;
        }

        protected virtual void OnCreateInstance()
        {

        }
    }

    public abstract class LazySingletonBase<T> where T : LazySingletonBase<T>
    {
        private static readonly Lazy<T> _lazyInstance = new Lazy<T>(() => Activator.CreateInstance(typeof(T), true) as T);
        public static T instance { get { return _lazyInstance.Value; } }
    }

    // Instance can be changed as the user likes.
    // Only the instance reference is a singleton pattern. The user can have many of these object. But instance will only ever point to one of them.
    // If instance is null. The first returned from Find Object will be the instance. If none are still found a new one will be created.
    // If OnDestroy is overidden, it's base MUST be called, otherwise instance could be a reference to a destroyed object.
    public abstract class VariableMonoSingletonBase<T> : MonoBehaviour where T : VariableMonoSingletonBase<T>
    {
        static T _instance = null;
        public static T instance
        {
            get
            {
                return _instanceGetter.Invoke();
            }
            set
            {
                _instance = value;
                if (_instance == null)
                {
                    _instanceGetter = CreateSingleton;
                }
            }
        }

        delegate T InstanceGetter();
        static InstanceGetter _instanceGetter = CreateSingleton;

        private static T CreateSingleton()
        {
            var heirarchyArray = FindObjectsOfType<T>();

            GameObject ownerObject;
            T instance;
            if (heirarchyArray.Length > 0)
            {
                instance = heirarchyArray[0];
                ownerObject = instance.gameObject;
            }
            else
            {
                ownerObject = new GameObject($"{typeof(T).Name} (singleton)");
                instance = ownerObject.AddComponent<T>();
                instance.OnCreateInstance();
            }

            _instance = instance;
            _instanceGetter = ReturnSingleton;

            return instance;
        }

        private static T ReturnSingleton()
        {
            return _instance;
        }

        protected virtual void OnCreateInstance()
        {

        }

        // OnDestroy MUST clean up it's instance variable, otherwise the internal instance may still be a reference to a destroyed object.
        protected virtual void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
                _instanceGetter = CreateSingleton;
            }
        }
    }

    // Simple Singleton that Destoys attemped copies.
    // Awake and On Destroy MUST be called if overridden. 
    public abstract class SimpleMonoSingleton<T> : MonoBehaviour where T : SimpleMonoSingleton<T>
    {
        static T _instance;
        public static T instance { get { return _instance; } }

        protected virtual void Awake()
        {
            // Check if singleton exists.
            if (_instance != null)
            {
                Destroy(this);
                Debug.LogWarning("Only one instance of EnemyManager should Exist");
            }
            _instance = (T)this;
        }

        protected virtual void OnDestroy()
        {
            // If this is the singleton, ensure that the static ref is nulled.
            if (_instance == this)
            {
                _instance = null;
            }
        }
    }
}
