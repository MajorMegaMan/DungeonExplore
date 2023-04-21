using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    PlayerReference m_player;
    PayloadController m_payload;

    [Header("UI")]
    [SerializeField] int m_uiFrameDelay = 5; // How many frames before an update for UI

    delegate void UIUpdater();
    int m_currentUIFrameIndex = -1;
    UIUpdater[] m_uiFrameProgresser;

    [SerializeField] Slider m_payloadProgressionSlider;
    [SerializeField] Slider m_payloadHealthSlider;
    [SerializeField] Slider m_playerHealthSlider;

    private void Awake()
    {
        int frameCount = Mathf.Max(m_uiFrameDelay, 1);
        m_uiFrameProgresser = new UIUpdater[frameCount];
        m_uiFrameProgresser[0] = InternalUpdateUI;
        for (int i = 1; i < m_uiFrameDelay; i++)
        {
            // Add empty delegates
            m_uiFrameProgresser[i] = () => { };
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_player = GameManager.instance.player;
        m_payload = GameManager.instance.payload;
    }

    void LateUpdate()
    {
        UpdateUI();
    }

    #region UIFunctions
    // Friendly call to update ui
    void UpdateUI()
    {
        m_currentUIFrameIndex++;
        m_currentUIFrameIndex = m_currentUIFrameIndex % m_uiFrameProgresser.Length;
        m_uiFrameProgresser[m_currentUIFrameIndex].Invoke();
    }

    void InternalUpdateUI()
    {
        m_payloadProgressionSlider.value = m_payload.progressionValue;
        m_payloadHealthSlider.value = m_payload.entityStats.currentHealth / m_payload.entityStats.maxHealth;

        m_playerHealthSlider.value = m_player.controller.entityStats.currentHealth / m_player.controller.entityStats.maxHealth;
    }
    #endregion // UIFunctions
}
