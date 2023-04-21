using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PayloadMotor : MonoBehaviour
{
    [SerializeField] PayloadSpline m_splineCurve;

    [SerializeField] float m_speed = 1.0f;
    float m_value = 0.0f;

    // Checkpointing
    bool m_hasHitFinish = false;
    int nextCheckPointIndex = 0;

    public float speed { get { return m_speed; } set { m_speed = value; } }
    public float value { get { return m_value; } }

    Vector3 m_next;

    public Vector3 velocity { get { return m_next.normalized * speed; } }

    private void Awake()
    {
        nextCheckPointIndex = 0;
    }

    // Start is called before the first frame update
    void Start()
    {
        m_next = m_splineCurve.GetSplineGradient(0);
    }

    // Update is called once per frame
    void Update()
    {
        UpdateValue();
        MovePayload();
    }

    void UpdateValue()
    {
        int splineCount = m_splineCurve.GetLineCount();
        float t = value * splineCount;
        int lineIndex = Mathf.Min((int)t, splineCount - 1);
        float lineLength = m_splineCurve.GetLineSegmentLength(lineIndex);
        t += Time.deltaTime * (speed / lineLength);

        float nextValue = t / splineCount;
        //if(nextValue > 1.0f)
        //{
        //    if (!m_hasHitFinish)
        //    {
        //        m_splineCurve.InvokeFinishEvent();
        //        m_hasHitFinish = !m_splineCurve.looped;
        //    }
        //    else if(m_splineCurve.looped)
        //    {
        //        m_splineCurve.InvokeFinishEvent();
        //        nextCheckPointIndex = nextCheckPointIndex % m_checkpointEvents.Count;
        //    }
        //}
        m_splineCurve.ProcessFinish(nextValue, ref m_hasHitFinish, ref nextCheckPointIndex);

        ProcessCheckPoint(nextValue);

        SetValue(t / splineCount);
    }

    void MovePayload()
    {
        float t = value * m_splineCurve.GetLineCount();

        transform.position = m_splineCurve.GetSplinePoint(t);
        transform.LookAt(transform.position + m_splineCurve.GetSplineGradient(t), m_splineCurve.GetSplineUp(t));
        m_next = m_splineCurve.GetSplineGradient(t);
    }

    void ProcessCheckPoint(float value)
    {
        if(m_splineCurve.ProcessCheckPoint(nextCheckPointIndex, value))
        {
            nextCheckPointIndex++;
        }
    }

    public void SetValue(float value)
    {
        if (m_splineCurve.looped)
        {
            this.m_value = Mathf.Repeat(value, 1.0f);
        }
        else
        {
            this.m_value = Mathf.Clamp(value, 0.0f, 1.0f);
        }
    }

    public void ResetProgression()
    {
        SetValue(0.0f);
        nextCheckPointIndex = 0;
        m_hasHitFinish = false;
    }

    public void SetPath(PayloadSpline path)
    {
        m_splineCurve = path;
    }

    public PayloadSpline GetPath()
    {
        return m_splineCurve;
    }
}
