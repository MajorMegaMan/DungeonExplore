using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpawnZone : MonoBehaviour
{
    [SerializeField] Bounds m_spawnBox = new Bounds(Vector3.zero, Vector3.one);

    [SerializeField] int debug_maxSpawnTry = 20;

    public Vector3 FindRandomSpawnPosition()
    {
        int trycount = 0;
        Vector3 pos = Vector3.zero;
        pos.x = Random.Range(-m_spawnBox.extents.x, m_spawnBox.extents.x);
        pos.y = Random.Range(-m_spawnBox.extents.y, m_spawnBox.extents.y);
        pos.z = Random.Range(-m_spawnBox.extents.z, m_spawnBox.extents.z);

        NavMeshHit hit;
        while (!NavMesh.SamplePosition(pos, out hit, 1.0f, NavMesh.AllAreas))
        {
            pos.x = Random.Range(-m_spawnBox.extents.x, m_spawnBox.extents.x);
            pos.y = Random.Range(-m_spawnBox.extents.y, m_spawnBox.extents.y);
            pos.z = Random.Range(-m_spawnBox.extents.z, m_spawnBox.extents.z);

            trycount++;
            if(trycount > debug_maxSpawnTry)
            {
                Debug.LogError("Could not Find Random Nav Mesh Position. Source::SpawnZone::" + name);
                return transform.position;
            }
        }

        pos = hit.position;

        return transform.position + pos;
    }

    public Vector3 FindRandomHeading()
    {
        return Vector3.SlerpUnclamped(Vector3.forward, Vector3.right, Random.Range(0.0f, 4.0f));
    }

    private void OnDrawGizmos()
    {
        Color colour = Color.blue;
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = colour;
        Gizmos.DrawWireCube(m_spawnBox.center, m_spawnBox.size);

        colour.a *= 0.4f;
        Gizmos.color = colour;
        Gizmos.DrawCube(m_spawnBox.center, m_spawnBox.size);
    }
}
