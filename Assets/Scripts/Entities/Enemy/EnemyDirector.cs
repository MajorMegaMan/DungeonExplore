using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// I'm inspired by blackboard design pattern for all enemies to have a shared knowledge. So this class will be my own interpretation.
public class EnemyDirector : MonoBehaviour
{
    // pooling
    [Header("Pooling")]
    [SerializeField] EnemyController m_enemyPrefab;
    BBB.ObjectPool<EnemyController> m_enemyObjectPool;
    [SerializeField] int m_maxEnemyCount = 32;
    int m_currentSpawnCount = 0;

    // Attack control
    [Header("Attack Control")]
    Dictionary<EnemyController, bool> m_requestedAttacks;
    DirectorAttackQueue m_attackQueue;

    [SerializeField] BBB.SimpleTimer m_attackTimer;
    [SerializeField] AnimationCurve m_timeVarianceCurve = AnimationCurve.Linear(0.0f, 0.5f, 1.0f, 1.0f);
    float m_timeScale = 1.0f;

    // debug
    [Header("Debug")]
    [SerializeField] GameObject debug_AttackTarget;
    IEntity debug_entityAttackTarget = null;
    [SerializeField] int debug_count = 0;

    // getters
    public int currentSpawnCount { get { return m_currentSpawnCount; } }

    private void Awake()
    {
        m_requestedAttacks = new Dictionary<EnemyController, bool>();

        m_enemyObjectPool = new BBB.ObjectPool<EnemyController>(m_maxEnemyCount, InstantiateEnemy, ActivateEnemy, DeactivateEnemy);
        m_attackQueue = new DirectorAttackQueue(m_requestedAttacks.Keys);

        debug_entityAttackTarget = IEntity.ValidateGameObject(debug_AttackTarget);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        debug_count = m_attackQueue.Count;
        if(m_attackTimer.IsTargetReached())
        {
            var enemyToAttack = m_attackQueue.PopNext();
            if(enemyToAttack != null)
            {
                // Successfully Sent Attack
                enemyToAttack.ReceiveAttackSignal(debug_entityAttackTarget);

                m_timeScale = 1.0f / m_timeVarianceCurve.Evaluate(Random.Range(0.0f, 1.0f));
                m_attackTimer.Reset();
                m_attackQueue.Jumble();
            }
        }
        else
        {
            m_attackTimer.Tick(m_timeScale * Time.deltaTime);
        }
    }

    #region ObjectPooling
    EnemyController InstantiateEnemy()
    {
        var enemy = Instantiate(m_enemyPrefab);
        m_requestedAttacks.Add(enemy, false);
        enemy.SetDirector(this);
        enemy.gameObject.SetActive(false);
        return enemy;
    }

    void ActivateEnemy(ref EnemyController enemy)
    {
        enemy.ResetEntityStats();
        enemy.gameObject.SetActive(true);
        m_currentSpawnCount++;
    }

    void DeactivateEnemy(ref EnemyController enemy)
    {
        enemy.gameObject.SetActive(false);
        m_currentSpawnCount--;
    }
    #endregion // ! ObjectPooling

    // used by an enemy that is referenced in the director
    public void RequestAttack(EnemyController enemy)
    {
        // check if the enemy has already requested an attack.
        if(!m_requestedAttacks[enemy])
        {
            m_requestedAttacks[enemy] = true;
            m_attackQueue.Add(enemy);
        }
    }

    // used by an enemy that is referenced in the director
    public void RevokeAttack(EnemyController enemy)
    {
        // check if the enemy has requested an attack.
        if (m_requestedAttacks[enemy])
        {
            m_requestedAttacks[enemy] = false;
            m_attackQueue.Remove(enemy);
        }
    }

    public EnemyController SpawnEnemy()
    {
        if(m_enemyObjectPool.ActivateNext(out EnemyController enemy))
        {
            enemy.AttackFollowTarget(debug_entityAttackTarget);
            return enemy;
        }
        else
        {
            return null;
        }
    }

    public void DespawnEnemy(EnemyController enemy)
    {
        m_enemyObjectPool.DeactivateObject(enemy);
    }

    private void OnValidate()
    {
        var validEntity = IEntity.ValidateGameObject(debug_AttackTarget);
        if(validEntity == null)
        {
            debug_AttackTarget = null;
        }
    }
}

interface IDirectedEnemy
{
    void ReceiveAttackSignal();
}

class DirectorAttackQueue
{
    Dictionary<EnemyController, DirectorAttackNode> m_nodes;
    DirectorAttackNode m_head;
    int m_count = 0;

    List<DirectorAttackNode> m_jumbleList;

    public int Count { get { return m_count; } }

    public DirectorAttackQueue(ICollection<EnemyController> enemies)
    {
        Initialise(enemies);
    }

    public void Initialise(ICollection<EnemyController> enemies)
    {
        m_nodes = new Dictionary<EnemyController, DirectorAttackNode>();
        foreach (var enemy in enemies)
        {
            m_nodes.Add(enemy, new DirectorAttackNode(enemy));
        }
        m_jumbleList = new List<DirectorAttackNode>(m_nodes.Count);
    }

    public void Add(EnemyController enemy)
    {
        if(m_nodes.TryGetValue(enemy, out var newAttackNode))
        {
            DirectorAttackNode current = m_head;
            if(current == null)
            {
                // List is empty
                InternalSetHeadToOnlyElement(newAttackNode);
                return;
            }
            else
            {
                if (newAttackNode.m_next != null)
                {
                    // already in attack queue
                    return;
                }
                InternalAddToTail(newAttackNode);
            }
        }
    }

    void InternalSetHeadToOnlyElement(DirectorAttackNode newAttackNode)
    {
        m_head = newAttackNode;
        newAttackNode.m_next = m_head;
        newAttackNode.m_prev = m_head;
        m_count = 1;
    }

    void InternalAddToTail(DirectorAttackNode newAttackNode)
    {
        m_count++;
        DirectorAttackNode tail = m_head.m_prev;
        newAttackNode.ConnectAfter(tail);
    }

    public void Remove(EnemyController enemy)
    {
        if (m_nodes.TryGetValue(enemy, out var targetAttackNode))
        {
            InternalRemove(targetAttackNode);
        }
    }

    void InternalRemove(DirectorAttackNode targetAttackNode)
    {
        if (targetAttackNode.m_next == null)
        {
            return;
        }
        if (m_count > 1)
        {
            if (m_head == targetAttackNode)
            {
                m_head = targetAttackNode.m_next;
            }
            targetAttackNode.Disconnect();
            m_count--;
        }
        else
        {
            m_count = 0;
            m_head = null;

            targetAttackNode.m_prev = null;
            targetAttackNode.m_next = null;
        }
    }

    public EnemyController PopNext()
    {
        if(m_count == 0)
        {
            return null;
        }

        var enemy = m_head.m_enemy;
        InternalRemove(m_head);
        return enemy;
    }

    // Randomise the order.
    public void Jumble()
    {
        if(m_count < 1)
        {
            // No reason to randomise order if there is 1 or less.
            return;
        }

        m_jumbleList.Clear();
        var current = m_head;
        do
        {
            m_jumbleList.Add(current);
            current = current.m_next;
        }
        while (current != m_head);

        // Randomise List
        int n = m_jumbleList.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            var value = m_jumbleList[k];
            m_jumbleList[k] = m_jumbleList[n];
            m_jumbleList[n] = value;
        }

        // Reset Linked nodes to new order
        // start by reseting head and count.
        InternalSetHeadToOnlyElement(m_jumbleList[m_jumbleList.Count - 1]);
        m_jumbleList.RemoveAt(m_jumbleList.Count - 1);

        while(m_jumbleList.Count > 0)
        {
            InternalAddToTail(m_jumbleList[m_jumbleList.Count - 1]);
            m_jumbleList.RemoveAt(m_jumbleList.Count - 1);
        }
    }
}

class DirectorAttackNode
{
    internal EnemyController m_enemy;
    internal DirectorAttackNode m_next;
    internal DirectorAttackNode m_prev;

    public DirectorAttackNode(EnemyController enemy)
    {
        m_enemy = enemy;
        m_next = null;
        m_prev = null;
    }

    public void ConnectAfter(DirectorAttackNode target)
    {
        var a = target;
        var b = target.m_next;

        var c = this;

        if(a == null)
        {
            Debug.Log("a is null");
        }
        if (b == null)
        {
            Debug.Log("b is null");
        }

        // insert c inbetween a and b.
        a.m_next = c;
        c.m_prev = a;

        b.m_prev = c;
        c.m_next = b;
    }

    public void ConnectBefore(DirectorAttackNode target)
    {
        ConnectAfter(target.m_prev);
    }

    public void Disconnect()
    {
        if(m_next == this)
        {
            m_next = null;
            m_prev = null;
        }
        else
        {
            var a = m_next;
            var b = m_prev;

            // remove this inbetween a and b.
            b.m_next = a;
            a.m_prev = b;

            m_next = null;
            m_prev = null;
        }
    }
}