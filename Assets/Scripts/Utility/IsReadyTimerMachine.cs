using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IsReadyTimerMachine
{
    [System.NonSerialized] bool m_isReady = true;

    [System.NonSerialized] float m_timer = 0.0f;
    public float readyTime = 1.0f;

    public float timeScale = 1.0f;

    delegate void UpdateDelegate(float deltaTime);
    [System.NonSerialized] UpdateDelegate m_updateDelegate = Empty;

    public delegate void ReadyEvent();
    [System.NonSerialized] ReadyEvent m_onReadyEvent;

    public bool isReady { get { return m_isReady; } }

    // returns true if ready.
    // If it does return true the status "Pops" and is ready is set to false.
    public bool PopIsReady()
    {
        if(m_isReady)
        {
            m_isReady = false;
            m_updateDelegate = ProcessUpdate;
            return true;
        }
        else
        return false;
    }

    public void Update(float deltaTime)
    {
        m_updateDelegate.Invoke(deltaTime);
    }

    void ProcessUpdate(float deltaTime)
    {
        m_timer += deltaTime * timeScale;
        if (m_timer > readyTime)
        {
            m_isReady = true;
            m_updateDelegate = Empty;
            m_timer = 0.0f;

            m_onReadyEvent?.Invoke();
        }
    }

    public void AddOnReadyEvent(ReadyEvent readyEvent)
    {
        m_onReadyEvent += readyEvent;
    }

    public void RemoveOnReadyEvent(ReadyEvent readyEvent)
    {
        m_onReadyEvent -= readyEvent;
    }

    static void Empty(float deltaTime)
    {

    }
}
