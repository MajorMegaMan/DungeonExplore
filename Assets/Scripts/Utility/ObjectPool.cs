using System.Collections.Generic;

namespace BBB
{
    public class ObjectPool<T>
    {
        Dictionary<T, PooledObject<T>> m_objectDictionary;

        Dictionary<T, PooledObject<T>>.Enumerator m_currentNextEnumerator;

        int m_currentActiveCount = 0;

        public delegate T PoolCreateAction();
        PoolCreateAction m_createDelegate;

        public delegate void PoolAction(ref T pooledObject);
        PoolAction m_activateDelegate;
        PoolAction m_deactivateDelegate;

        public int maxCapacity { get { return m_objectDictionary.Count; } }
        public int activeCount { get { return m_currentActiveCount; } }

        public ObjectPool(int maxCapacity, PoolCreateAction createDelegate, PoolAction activateDelegate, PoolAction deactivateDelegate)
        {
            Initialise(maxCapacity, createDelegate, activateDelegate, deactivateDelegate);
        }

        void Initialise(int maxCapacity, PoolCreateAction createDelegate, PoolAction activateDelegate, PoolAction deactivateDelegate)
        {
            m_createDelegate = createDelegate;
            m_activateDelegate = activateDelegate;
            m_deactivateDelegate = deactivateDelegate;

            m_objectDictionary = new Dictionary<T, PooledObject<T>>(maxCapacity);
            for (int i = 0; i < maxCapacity; i++)
            {
                PooledObject<T> newPooledObject = new PooledObject<T>();
                newPooledObject.m_objectRef = m_createDelegate.Invoke();
                m_objectDictionary.Add(newPooledObject.m_objectRef, newPooledObject);
            }

            m_currentNextEnumerator = m_objectDictionary.GetEnumerator();
            m_currentNextEnumerator.MoveNext();
        }

        public bool ActivateNext(out T pooledObject)
        {
            PooledObject<T> startPooledObject = m_currentNextEnumerator.Current.Value;
            PooledObject<T> currentPooledObject = startPooledObject;

            do
            {
                if (!currentPooledObject.m_isActive)
                {
                    ActivateObject(currentPooledObject);
                    pooledObject = currentPooledObject.m_objectRef;
                    MoveNextEnumerator();
                    return true;
                }

                MoveNextEnumerator();

                currentPooledObject = m_currentNextEnumerator.Current.Value;
            }
            while (startPooledObject != currentPooledObject);

            pooledObject = default;
            return false;
        }

        void MoveNextEnumerator()
        {
            if (!m_currentNextEnumerator.MoveNext())
            {
                m_currentNextEnumerator = m_objectDictionary.GetEnumerator();
                m_currentNextEnumerator.MoveNext();
            }
        }

        void ActivateObject(PooledObject<T> pooledObject)
        {
            if(pooledObject.m_isActive)
            {
                return;
            }

            m_currentActiveCount++;
            pooledObject.m_isActive = true;
            m_activateDelegate.Invoke(ref pooledObject.m_objectRef);
        }

        void DeactivateObject(PooledObject<T> pooledObject)
        {
            if (!pooledObject.m_isActive)
            {
                return;
            }

            m_currentActiveCount--;
            pooledObject.m_isActive = false;
            m_deactivateDelegate.Invoke(ref pooledObject.m_objectRef);
        }

        public void ActivateObject(T pooledObject)
        {
            ActivateObject(m_objectDictionary[pooledObject]);
        }

        public void DeactivateObject(T pooledObject)
        {
            DeactivateObject(m_objectDictionary[pooledObject]);
        }

        // Increases the Count of created pooled objects
        public void ExtendPool(int increaseCount = 1)
        {
            if(increaseCount < 1)
            {
                return;
            }

            int newCapacity = maxCapacity + increaseCount;
            Dictionary<T, PooledObject<T>> newDictionary = new Dictionary<T, PooledObject<T>>(newCapacity);

            // copy old Dictionary
            foreach(var keyValue in m_objectDictionary)
            {
                newDictionary.Add(keyValue.Key, keyValue.Value);
            }

            // Create new objects
            for (int i = 0; i < increaseCount; i++)
            {
                PooledObject<T> newPooledObject = new PooledObject<T>();
                newPooledObject.m_objectRef = m_createDelegate.Invoke();
                newDictionary.Add(newPooledObject.m_objectRef, newPooledObject);
            }

            // Set Dictionary
            m_objectDictionary = newDictionary;

            m_currentNextEnumerator = m_objectDictionary.GetEnumerator();
            m_currentNextEnumerator.MoveNext();
        }
    }

    internal class PooledObject<T>
    {
        internal T m_objectRef;
        internal bool m_isActive = false;
    }
}
