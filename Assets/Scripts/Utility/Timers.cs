using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BBB
{
    [SerializeField]
    public class SimpleTimer
    {
        public float targetTime = 1.0f;

        [System.NonSerialized] float m_timer = 0.0f;

        public float time { get { return m_timer; } set { m_timer = value; } }
        public float normalisedTime { get { return m_timer / targetTime; } set { m_timer = value * targetTime; } }

        public SimpleTimer()
        {

        }

        public SimpleTimer(float targetTime)
        {
            this.targetTime = targetTime;
        }

        public void Tick(float deltatime)
        {
            m_timer += deltatime;
        }

        public void Reset()
        {
            m_timer = 0.0f;
        }

        public bool IsTargetReached()
        {
            return m_timer >= targetTime;
        }

        public void SubtractTargetTime()
        {
            m_timer -= targetTime;
        }
    }

    [System.Serializable]
    public class EventTimer
    {
        delegate void TickDelegate(float deltaTime);
        TickDelegate m_tickDelegate = Empty;

        public float targetTime = 1.0f;
        public float timeScale = 1.0f;

        [System.NonSerialized] float m_timer = 0.0f;

        [System.NonSerialized] UnityEvent m_onTickEvent = new UnityEvent();
        [System.NonSerialized] UnityEvent m_targetReachedEvent = new UnityEvent();

        public UnityEvent onTickEvent { get { return m_onTickEvent; } }
        public UnityEvent targetReachedEvent { get { return m_targetReachedEvent; } }

        public float time { get { return m_timer; } }
        public float normalisedTime { get { return m_timer / targetTime; } }

        public EventTimer()
        {

        }

        public EventTimer(float targetTime, float timeScale = 1.0f)
        {
            this.targetTime = targetTime;
            this.timeScale = timeScale;
        }

        public void Update(float deltaTime)
        {
            m_tickDelegate.Invoke(deltaTime);
        }

        public void Start()
        {
            m_tickDelegate = Tick;
        }

        public void Stop()
        {
            m_tickDelegate = Empty;
        }

        void Tick(float deltaTime)
        {
            m_timer += deltaTime * timeScale;
            onTickEvent.Invoke();
            if (m_timer > targetTime)
            {
                Stop();
                m_targetReachedEvent.Invoke();
            }
        }

        static void Empty(float deltaTime)
        {

        }

        public void Reset()
        {
            m_timer = 0.0f;
        }
    }
}
