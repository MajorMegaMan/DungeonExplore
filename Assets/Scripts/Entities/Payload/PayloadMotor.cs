using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PayloadMotor : MonoBehaviour
{
    [SerializeField] DebugSplineTester m_splineCurve;

    [SerializeField] float m_speed = 1.0f;
    float m_value = 0.0f;

    [Header("CheckPoints")]
    [SerializeField] UnityEvent m_finishEvent;
    bool m_hasHitFinish = false;

    [SerializeField] List<Checkpoint> m_checkpointEvents;

    int nextCheckPointIndex = 0;

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

    public float speed { get { return m_speed; } set { m_speed = value; } }
    public float value { get { return m_value; } }

    Vector3 m_next;

    public Vector3 velocity { get { return m_next.normalized * speed; } }

    private void Awake()
    {
        OrderCheckpoints();
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
        if(nextValue > 1.0f)
        {
            if (!m_hasHitFinish)
            {
                m_finishEvent.Invoke();
                m_hasHitFinish = !m_splineCurve.looped;
            }
            else if(m_splineCurve.looped)
            {
                m_finishEvent.Invoke();
                nextCheckPointIndex = nextCheckPointIndex % m_checkpointEvents.Count;
            }
        }

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

    void OrderCheckpoints()
    {
        m_checkpointEvents.Sort(Checkpoint.Comparer);
    }

    void ProcessCheckPoint(float value)
    {
        if(nextCheckPointIndex >= m_checkpointEvents.Count)
        {
            return;
        }

        if(m_checkpointEvents[nextCheckPointIndex].ProcessCheckPoint(value))
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

    private void OnDrawGizmos()
    {
        for(int i = 0; i < m_checkpointEvents.Count; i++)
        {
            float t = m_checkpointEvents[i].value * m_splineCurve.GetLineCount();
            Vector3 position = m_splineCurve.GetSplinePoint(t);

            Color colour = Color.yellow;
            Gizmos.color = colour;
            Gizmos.DrawSphere(position, 0.3f);

            colour.a *= 0.4f;
            Gizmos.color = colour;
            Gizmos.DrawWireSphere(position, 0.3f);
        }

        Vector3 finishPosition = m_splineCurve.GetSplinePoint(m_splineCurve.GetLineCount());

        Color finishColour = Color.red;
        Gizmos.color = finishColour;
        Gizmos.DrawSphere(finishPosition, 0.3f);

        finishColour.a *= 0.4f;
        Gizmos.color = finishColour;
        Gizmos.DrawWireSphere(finishPosition, 0.3f);
    }
}
