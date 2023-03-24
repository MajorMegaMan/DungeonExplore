using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    [SerializeField] Bounds m_spawnBox = new Bounds(Vector3.zero, Vector3.one);

    public Vector3 FindRandomSpawnPosition()
    {
        Vector3 pos = Vector3.zero;
        pos.x = Random.Range(-m_spawnBox.extents.x, m_spawnBox.extents.x);
        pos.y = Random.Range(-m_spawnBox.extents.y, m_spawnBox.extents.y);
        pos.z = Random.Range(-m_spawnBox.extents.z, m_spawnBox.extents.z);

        return transform.position + pos;
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
