using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PayloadSpline : MonoBehaviour
{
    [SerializeField] CustomSpline<PayloadTravelPoint> m_spline;
    [SerializeField] bool m_looped = false;

    List<float> m_lineLengths;

    [Header("Checkpoints")]
    [SerializeField] UnityEvent m_finishEvent;
    [SerializeField] List<Checkpoint> m_checkpointEvents;

    [System.Serializable]
    public class Checkpoint
    {
        [SerializeField, Range(0.0f, 1.0f)] float m_value;
        [SerializeField] UnityEvent m_event;

        public float value { get { return m_value; } }
        public UnityEvent checkpointEvent { get { return m_event; } }

        static CheckpointCompare _comparer;
        internal static CheckpointCompare Comparer { get { return _comparer; } }

        static Checkpoint()
        {
            _comparer = new CheckpointCompare();
        }

        internal class CheckpointCompare : IComparer<Checkpoint>
        {
            int IComparer<Checkpoint>.Compare(Checkpoint lhs, Checkpoint rhs)
            {
                return lhs.m_value.CompareTo(rhs.m_value);
            }
        }

        internal bool ProcessCheckPoint(float value)
        {
            if (value > m_value)
            {
                m_event.Invoke();
                return true;
            }
            return false;
        }
    }

    public enum FinishFlag
    {
        Single,
        Looped,

        Unfinished
    }

    public List<PayloadTravelPoint> points { get { return m_spline.points; } }
    public bool looped { get { return m_looped; } }

    public UnityEvent finishEvent { get { return m_finishEvent; } }
    public Checkpoint[] checkpoints { get { return m_checkpointEvents.ToArray(); } }

    private void Awake()
    {
        m_lineLengths = new List<float>();
        CalculateLineSegmentLengths();

        OrderCheckpoints();
    }

    #region SplineProcessing
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
        if (!m_looped)
        {
            startLineIndex += 1;
        }

        Vector3 start = m_spline.points[startLineIndex].up;
        Vector3 end = m_spline.points[(startLineIndex + 1) % m_spline.points.Count].up;

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
        for (int i = 0; i < lineCount; i++)
        {
            m_lineLengths.Add(ApproximateLineSegmentLength(i, seperationLength));
        }
    }
    #endregion // SplineProcessing

    #region CheckPoints
    void OrderCheckpoints()
    {
        m_checkpointEvents.Sort(Checkpoint.Comparer);
    }

    public bool ProcessCheckPoint(int nextCheckPointIndex, float value)
    {
        if (nextCheckPointIndex >= m_checkpointEvents.Count)
        {
            return false;
        }

        return m_checkpointEvents[nextCheckPointIndex].ProcessCheckPoint(value);
    }

    public void InvokeFinishEvent()
    {
        m_finishEvent.Invoke();
    }

    public FinishFlag ProcessFinish(float value, ref bool hasHitFinish, ref int nextCheckPointIndex)
    {
        if (value > 1.0f)
        {
            if (!hasHitFinish)
            {
                InvokeFinishEvent();
                hasHitFinish = !looped;
                return FinishFlag.Single;
            }
            else if (looped)
            {
                InvokeFinishEvent();
                nextCheckPointIndex = nextCheckPointIndex % m_checkpointEvents.Count;
                return FinishFlag.Looped;
            }
        }

        return FinishFlag.Unfinished;
    }

    public int GetCheckPointCount()
    {
        return m_checkpointEvents.Count;
    }
    #endregion // CheckPoints

    private void OnDrawGizmos()
    {
        Color lineColour = Color.magenta;

        float tLimit = m_spline.points.Count - 3;
        if (m_looped)
        {
            tLimit = m_spline.points.Count;
        }

        if (tLimit <= 0)
        {
            return;
        }

        // Draw Spline Points
        bool hasNull = false;
        foreach (var point in points)
        {
            if (point.transform == null)
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

        Gizmos.color = lineColour;

        // Draw Spline Line
        Vector3 prev = m_spline.GetSplinePoint(0.0f, m_looped);
        for (float t = 0.0f; t <= tLimit; t += 0.01f)
        {
            Vector3 linePoint = m_spline.GetSplinePoint(t, m_looped);
            Gizmos.DrawLine(prev, linePoint);
            prev = linePoint;
        }

        // Draw Checkpoints
        for (int i = 0; i < m_checkpointEvents.Count; i++)
        {
            float t = m_checkpointEvents[i].value * GetLineCount();
            Vector3 position = GetSplinePoint(t);

            Color colour = Color.yellow;
            Gizmos.color = colour;
            Gizmos.DrawSphere(position, 0.3f);

            colour.a *= 0.4f;
            Gizmos.color = colour;
            Gizmos.DrawWireSphere(position, 0.3f);
        }

        Vector3 finishPosition = GetSplinePoint(GetLineCount());

        Color finishColour = Color.red;
        Gizmos.color = finishColour;
        Gizmos.DrawSphere(finishPosition, 0.3f);

        finishColour.a *= 0.4f;
        Gizmos.color = finishColour;
        Gizmos.DrawWireSphere(finishPosition, 0.3f);
    }

    private void OnValidate()
    {
        if (Application.isPlaying)
        {
            if (m_lineLengths != null)
            {
                CalculateLineSegmentLengths();
            }
        }
    }
}

[System.Serializable]
public class PayloadTravelPoint : ISplinePoint
{
    [SerializeField] Transform m_pointTransform;

    public Transform transform { get { return m_pointTransform; } }
    public Vector3 point { get { return m_pointTransform.position; } }
    public Vector3 up { get { return m_pointTransform.up; } }

    public static implicit operator Vector3(PayloadTravelPoint value)
    {
        return value.m_pointTransform.position;
    }

    public Vector3 GetPoint()
    {
        return m_pointTransform.position;
    }
}
