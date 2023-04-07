using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnController : MonoBehaviour
{
    [SerializeField] EnemyDirector m_director;

    BBB.SimpleTimer m_waveTimer;
    BBB.SimpleTimer m_miniWaveTimer;

    int m_currentMiniWaveRemain = 0;

    [SerializeField] SpawnZone m_spawnZone;
    [SerializeField] SpawnControllerSettings m_settings;

    private void Awake()
    {
        m_waveTimer = new BBB.SimpleTimer();
        m_miniWaveTimer = new BBB.SimpleTimer();

        m_waveTimer.targetTime = m_settings.waveSeperationTime;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        m_waveTimer.Tick(Time.deltaTime);
        if (m_waveTimer.IsTargetReached())
        {
            m_waveTimer.targetTime = m_settings.waveSeperationTime;
            m_waveTimer.SubtractTargetTime();
            // Timer has activated.
            InitiateMiniWave(m_director.currentSpawnCount);
        }

        if (m_currentMiniWaveRemain > 0)
        {
            m_miniWaveTimer.Tick(Time.deltaTime);
            if (m_miniWaveTimer.IsTargetReached())
            {
                m_miniWaveTimer.SubtractTargetTime();
                MiniWaveSpawn();
            }
        }
    }

    public void InitiateMiniWave(int currentActiveAgentCount)
    {
        m_currentMiniWaveRemain = m_settings.CalculateAmountToSpawn(currentActiveAgentCount);
        m_miniWaveTimer.targetTime = m_settings.miniWaveTime;
    }

    void MiniWaveSpawn()
    {
        m_currentMiniWaveRemain--;
        EnemyController enemy = m_director.SpawnEnemy(m_spawnZone.FindRandomSpawnPosition(), m_spawnZone.FindRandomHeading());
    }

    public void ForceSpawn(int count)
    {
        for(int i = 0; i < count; i++)
        {
            m_director.SpawnEnemy(m_spawnZone.FindRandomSpawnPosition(), m_spawnZone.FindRandomHeading());
        }
    }

    public void ResetSpawner()
    {
        m_waveTimer.Reset();
        m_miniWaveTimer.Reset();

        m_currentMiniWaveRemain = 0;
    }
}
