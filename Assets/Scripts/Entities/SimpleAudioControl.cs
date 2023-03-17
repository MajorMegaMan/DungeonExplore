using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAudioControl : MonoBehaviour
{
    [SerializeField] AudioSource m_source;

    [SerializeField] float m_targetPitch = 1.0f;
    [SerializeField] float m_pitchRange = 0.1f;

    [SerializeField] AudioClip m_hurtClip;
    

    public void PlayHurt()
    {
        float randPitch = Random.Range(-m_pitchRange, m_pitchRange);
        m_source.pitch = m_targetPitch + randPitch;
        m_source.PlayOneShot(m_hurtClip);
    }
}
