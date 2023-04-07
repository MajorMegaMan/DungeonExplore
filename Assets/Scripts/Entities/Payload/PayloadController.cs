using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class PayloadController : MonoBehaviour, IEntity
{
    [SerializeField] PayloadMotor m_motor;

    [Header("Entity")]
    [SerializeField] Transform m_lockOnTransform;
    [SerializeField] Renderer m_renderer;
    [SerializeField] float m_entityRadius = 1.0f;
    [SerializeField] int m_team = 0;
    [SerializeField] EntityStats m_stats;

    [Header("Movement")]
    [SerializeField] float m_speed = 5.0f;
    [SerializeField] NavMeshObstacle m_navObstacle;
    [SerializeField] Collider m_hitReceiverCollider;

    [Header("Parts")]
    [SerializeField] Rigidbody[] m_rigidBodyParts;
    Vector3[] m_originPositions;
    Quaternion[] m_originRotations;
    [SerializeField] float m_explodeImpulse = 5.0f;
    [SerializeField] float m_randomExplodeInfluence = 1.0f;

    [Header("Debug")]
    [SerializeField] bool debug_die = false;
    [SerializeField] bool debug_revive = false;

    public string entityName { get { return "Enemy, " + name; } }
    public Vector3 position { get { return transform.position; } }
    public float speed { get { return m_speed; } }
    public Vector3 velocity { get { return m_motor.velocity; } }
    public float currentSpeed { get { return m_motor.speed; } }

    public Vector3 heading { get { return velocity.normalized; } }
    public EntityStats entityStats { get { return m_stats; } }

    public float progressionValue { get { return m_motor.value; } }

    public UnityEvent finishEvent { get { return m_motor.finishEvent; } }
    public PayloadMotor.Checkpoint[] checkpoints { get { return m_motor.checkpoints; } }

    private void Awake()
    {
        EnableRigidBodyParts(false);
        SetOriginPositions();
        StopMoving();
    }

    private void Update()
    {
        m_navObstacle.velocity = velocity;
    }

    public void StartMoving()
    {
        m_motor.speed = m_speed;
    }

    public void StopMoving()
    {
        m_motor.speed = 0.0f;
    }

    #region IEntity
    public Bounds GetAABB()
    {
        return m_renderer.bounds;
    }

    public Transform GetCameraLookTransform()
    {
        return m_lockOnTransform;
    }

    public float GetTargetRadius()
    {
        return m_entityRadius;
    }

    public Vector3 GetAttackTargetPostion(IEntity attacker)
    {
        return m_hitReceiverCollider.ClosestPoint(attacker.position);
    }

    public int GetTeam()
    {
        return m_team;
    }

    public void ReceiveHit(IEntity attacker)
    {
        entityStats.ReceiveDamage(attacker.entityStats.CalculateAttackStrength());

        if (!entityStats.IsDead())
        {
            Debug.Log("I Got Hit.");
        }
        else
        {
            Debug.Log("I'm Dead.");
            Die();
        }
    }
    #endregion // ! IEntity

    void EnableRigidBodyParts(bool enabled)
    {
        for(int i = 0; i < m_rigidBodyParts.Length; i++)
        {
            m_rigidBodyParts[i].isKinematic = !enabled;
        }
    }

    void SetOriginPositions()
    {
        if(m_rigidBodyParts.Length == 0)
        {
            m_originPositions = null;
            m_originRotations = null;
            return;
        }

        m_originPositions = new Vector3[m_rigidBodyParts.Length];
        m_originRotations = new Quaternion[m_rigidBodyParts.Length];

        for (int i = 0; i < m_rigidBodyParts.Length; i++)
        {
            m_originPositions[i] = m_rigidBodyParts[i].transform.localPosition;
            m_originRotations[i] = m_rigidBodyParts[i].transform.localRotation;
        }
    }

    void ResetRigidBodyPositions()
    {
        for (int i = 0; i < m_rigidBodyParts.Length; i++)
        {
            m_rigidBodyParts[i].transform.localPosition = m_originPositions[i];
            m_rigidBodyParts[i].transform.localRotation = m_originRotations[i];
        }
    }

    void Explode(float impulse)
    {
        for (int i = 0; i < m_rigidBodyParts.Length; i++)
        {
            Vector3 toRigidPosition = m_rigidBodyParts[i].position - position;
            Vector3 randomDir = Vector3.zero;
            randomDir.x = Random.Range(0.0f, 1.0f);
            randomDir.y = Random.Range(0.0f, 1.0f);
            randomDir.z = Random.Range(0.0f, 1.0f);

            Vector3 direction = toRigidPosition + randomDir * m_randomExplodeInfluence;

            m_rigidBodyParts[i].AddForce(direction.normalized * impulse, ForceMode.Impulse);
        }
    }

    void Die()
    {
        m_hitReceiverCollider.enabled = false;

        EnableRigidBodyParts(true);
        Explode(m_explodeImpulse);

        m_motor.speed = 0.0f;
    }

    void Revive()
    {
        m_hitReceiverCollider.enabled = true;

        EnableRigidBodyParts(false);

        ResetRigidBodyPositions();

        m_stats.HealToFull();
    }

    public void ResetPayload()
    {
        m_motor.ResetProgression();
        Revive();
    }

    private void OnValidate()
    {
        if(Application.isPlaying)
        {
            if (debug_die)
            {
                debug_die = false;
                m_stats.Die();
                Die();
            }

            if (debug_revive)
            {
                debug_revive = false;
                Revive();
            }
        }
    }
}
