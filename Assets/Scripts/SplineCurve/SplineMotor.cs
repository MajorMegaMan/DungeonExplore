using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineMotor : MonoBehaviour
{
    [SerializeField] DebugSplineTester m_splineCurve;

    [SerializeField] float speed = 1.0f;
    [SerializeField] float value = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int splineCount = m_splineCurve.GetLineCount();
        float t = value * splineCount;
        int lineIndex = Mathf.Min((int)t, splineCount - 1);
        float lineLength = m_splineCurve.GetLineSegmentLength(lineIndex);
        t += Time.deltaTime * (speed / lineLength);
        transform.position = m_splineCurve.GetSplinePoint(t);

        //transform.forward = m_splineCurve.GetSplineGradient(t);
        transform.LookAt(transform.position + m_splineCurve.GetSplineGradient(t), m_splineCurve.GetSplineUp(t));

        SetValue(t / splineCount);
    }

    public void SetValue(float value)
    {
        if(m_splineCurve.looped)
        {
            this.value = Mathf.Repeat(value, 1.0f);
        }
        else
        {
            this.value = Mathf.Clamp(value, 0.0f, 1.0f);
        }
    }

    private void OnValidate()
    {
        SetValue(value);
    }

    private void OnDrawGizmos()
    {
        int splineCount = m_splineCurve.GetLineCount();
        float t = value * splineCount;
        Gizmos.DrawLine(transform.position, transform.position + m_splineCurve.GetSplineGradient(t));
    }
}
