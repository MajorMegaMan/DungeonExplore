using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSplineTester : MonoBehaviour
{
    [SerializeField] CustomSpline<RoadPoint> m_spline;
    [SerializeField] bool m_looped = false;

    List<float> m_lineLengths;

    public List<RoadPoint> points { get { return m_spline.points; } }
    public bool looped { get { return m_looped; } }

    private void Awake()
    {
        m_lineLengths = new List<float>();
        CalculateLineSegmentLengths();
    }

    public int GetLineCount()
    {
        int count = m_spline.points.Count - 3;
        if (m_looped)
        {
            count = m_spline.points.Count;
        }
        return count;
    }

    public Vector3 GetSplinePoint(float t)
    {
        return m_spline.GetSplinePoint(t, m_looped);
    }

    public Vector3 GetSplinePoint(int lineIndex, float t)
    {
        lineIndex = lineIndex % GetLineCount();
        t = lineIndex + t;
        return m_spline.GetSplinePoint(t, m_looped);
    }

    public Vector3 GetSplineGradient(float t)
    {
        return m_spline.GetSplineGradient(t, m_looped);
    }

    public Vector3 GetSplineGradient(int lineIndex, float t)
    {
        lineIndex = lineIndex % GetLineCount();
        t = lineIndex + t;
        return m_spline.GetSplineGradient(t, m_looped);
    }

    public Vector3 GetSplineUp(float t)
    {
        int startLineIndex = (int)t % GetLineCount();
        if (!m_looped)
        {
            startLineIndex += 1;
        }

        Vector3 start = m_spline.points[startLineIndex].up;
        Vector3 end = m_spline.points[(startLineIndex + 1) % m_spline.points.Count].up;

        t = t - startLineIndex;

        return Vector3.Slerp(start, end, t);
    }

    public Vector3 GetSplineUp(int startLineIndex, float t)
    {
        startLineIndex = startLineIndex % GetLineCount();
        if(!m_looped)
        {
            startLineIndex += 1;
        }

        Vector3 start = m_spline.points[startLineIndex].up;
        Vector3 end = m_spline.points[(startLineIndex  + 1) % m_spline.points.Count].up;

        return Vector3.Slerp(start, end, t);
    }

    public float GetLineSegmentLength(int startLineIndex)
    {
        return m_lineLengths[startLineIndex];
    }

    public float ApproximateLineSegmentLength(int startLineIndex, float seperationLength = 0.01f)
    {
        startLineIndex = startLineIndex % GetLineCount();
        if (!m_looped)
        {
            startLineIndex += 1;
        }

        float length = 0.0f;

        float tLimit = m_spline.points.Count - 3;
        if (m_looped)
        {
            tLimit = m_spline.points.Count;
        }
        // Draw Spline Line
        Vector3 prev = m_spline.GetSplinePoint(0.0f, m_looped);
        for (float t = 0.0f; t < tLimit; t += seperationLength)
        {
            Vector3 linePoint = GetSplinePoint(startLineIndex, t);
            length += (linePoint - prev).magnitude;
            prev = linePoint;
        }

        Vector3 lastPoint = GetSplinePoint(startLineIndex, 1.0f);
        length += (lastPoint - prev).magnitude;

        return length;
    }

    public void CalculateLineSegmentLengths(float seperationLength = 0.01f)
    {
        m_lineLengths.Clear();

        int lineCount = GetLineCount();
        for(int i = 0; i < lineCount; i++)
        {
            m_lineLengths.Add(ApproximateLineSegmentLength(i, seperationLength));
        }
    }

    private void OnDrawGizmos()
    {
        float tLimit = m_spline.points.Count - 3;
        if(m_looped)
        {
            tLimit = m_spline.points.Count;
        }

        if(tLimit <= 0)
        {
            return;
        }

        // Draw Spline Points
        bool hasNull = false;
        foreach (var point in points)
        {
            if(point.transform == null)
            {
                hasNull = true;
                continue;
            }
            Gizmos.DrawSphere(point, 0.1f);
            Gizmos.DrawLine(point, point + point.up);
        }
        if (hasNull)
        {
            return;
        }

        // Draw Spline Line
        Vector3 prev = m_spline.GetSplinePoint(0.0f, m_looped);
        for (float t = 0.0f; t <= tLimit; t += 0.01f)
        {
            Vector3 linePoint = m_spline.GetSplinePoint(t, m_looped);
            Gizmos.DrawLine(prev, linePoint);
            prev = linePoint;
        }
    }

    private void OnValidate()
    {
        if(Application.isPlaying)
        {
            if(m_lineLengths != null)
            {
                CalculateLineSegmentLengths();
            }
        }
    }
}

[System.Serializable]
public class RoadPoint : ISplinePoint
{
    [SerializeField] Transform m_pointTransform;

    public Transform transform { get { return m_pointTransform; } }
    public Vector3 point { get { return m_pointTransform.position; } }
    public Vector3 up { get { return m_pointTransform.up; } }

    public static implicit operator Vector3(RoadPoint value)
    {
        return value.m_pointTransform.position;
    }

    public Vector3 GetPoint()
    {
        return m_pointTransform.position;
    }
}
