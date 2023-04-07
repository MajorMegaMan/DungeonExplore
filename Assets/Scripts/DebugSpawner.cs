using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugSpawner : MonoBehaviour
{
    [SerializeField] EnemyDirector m_director;

    [SerializeField] int m_initialSpawnAmount = 5;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < m_initialSpawnAmount; i++)
        {
            m_director.SpawnEnemy(transform.position, transform.forward);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
