using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PayloadMotor : MonoBehaviour
{
    [SerializeField] DebugSplineTester m_splineCurve;

    [SerializeField] float m_speed = 1.0f;
    float m_value = 0.0f;

    public float speed { get { return m_speed; } set { m_speed = value; } }
    public float value { get { return m_value; } }

    Vector3 m_next;

    public Vector3 velocity { get { return m_next.normalized * speed; } }

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

        SetValue(t / splineCount);
    }

    void MovePayload()
    {
        float t = value * m_splineCurve.GetLineCount();

        transform.position = m_splineCurve.GetSplinePoint(t);
        transform.LookAt(transform.position + m_splineCurve.GetSplineGradient(t), m_splineCurve.GetSplineUp(t));
        m_next = m_splineCurve.GetSplineGradient(t);
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
}
