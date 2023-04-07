using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestOpen : MonoBehaviour
{
    [SerializeField] Transform m_chestLid;

    [SerializeField] Vector3 m_openRotation;
    [SerializeField] Vector3 m_closedRotation;

    [SerializeField] float m_openSpeed = 1.0f;
    [SerializeField] AnimationCurve m_openCurve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);

    bool m_isOpen = false;

    float tValue = 0.0f;

    private void Awake()
    {
        Close();
    }

    // Update is called once per frame
    void Update()
    {
        if(m_isOpen)
        {
            tValue += Time.deltaTime * m_openSpeed;
            if(tValue > 1.0f)
            {
                tValue = 1.0f;
                m_chestLid.localEulerAngles = m_openRotation;
                enabled = false;
                return;
            }
        }
        else
        {
            tValue -= Time.deltaTime * m_openSpeed;
            if (tValue < 0.0f)
            {
                tValue = 0.0f;
                m_chestLid.localEulerAngles = m_closedRotation;
                enabled = false;
                return;
            }
        }

        SetRotation(m_closedRotation, m_openRotation, tValue);
    }

    void SetRotation(Vector3 begin, Vector3 end, float t)
    {
        m_chestLid.localEulerAngles = Vector3.Lerp(begin, end, m_openCurve.Evaluate(t));
    }

    public void Open()
    {
        enabled = true;
        m_isOpen = true;
    }

    public void Close()
    {
        enabled = true;
        m_isOpen = false;
    }
}
