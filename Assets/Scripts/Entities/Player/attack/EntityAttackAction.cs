using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EntityAttackAction
{
    int m_animationHashID = 0;

    [SerializeField] WeaponSettings m_weaponSettings;
    List<ScriptableAttackAction> m_attackActionList;
    List<int> m_animationHashIDList;
    List<int> m_animationDriveList;

    [SerializeField] WeaponCollider m_weaponCollider;

    public int weaponActionCount { get { return m_attackActionList.Count; } }

    StraightAttack m_straightMoveAction;
    LockOnAttack m_lockOnMoveAction;

    public void Initialise(Transform origin, IEntity entity)
    {
        m_straightMoveAction = new StraightAttack(origin);
        m_lockOnMoveAction = new LockOnAttack(origin);

        m_attackActionList = new List<ScriptableAttackAction>();
        m_animationHashIDList = new List<int>();
        m_animationDriveList = new List<int>();

        SetWeaponSettings(m_weaponSettings);

        SetAttackAction(0);
        SetWeaponCollider(m_weaponCollider, entity);
    }

    public void SetWeaponSettings(WeaponSettings weaponSettings)
    {
        m_attackActionList.Clear();
        m_animationHashIDList.Clear();
        m_animationDriveList.Clear();

        if (weaponSettings != null)
        {
            var actionArray = weaponSettings.actions;
            for(int i = 0; i < actionArray.Length; i++)
            {
                var action = actionArray[i];
                if(action != null)
                {
                    m_attackActionList.Add(action);
                    m_animationHashIDList.Add(Animator.StringToHash(action.animationStateID));
                    m_animationDriveList.Add(Animator.StringToHash(action.animationDriveParameter));
                }
                else
                {
                    Debug.LogWarning("Null Action was found in WeaponSettings::" + weaponSettings.name + "::INDEX-" + i);
                }
            }
        }
    }

    void SetAttackAction(int index)
    {
        index = index % m_attackActionList.Count;
        var action = m_attackActionList[index];
        m_straightMoveAction.SetAsAttackAction(action);
        m_lockOnMoveAction.SetAsAttackAction(action);
        m_animationHashID = m_animationHashIDList[index];
    }

    public void SetWeaponCollider(WeaponCollider weaponCollider, IEntity entity)
    {
        if (weaponCollider != null)
        {
            weaponCollider.SetOwner(entity);
            m_straightMoveAction.SetWeaponCollider(weaponCollider);
            m_lockOnMoveAction.SetWeaponCollider(weaponCollider);
        }
    }

    public StraightAttack BeginStraghtAttack(int index = 0)
    {
        SetAttackAction(index);
        return m_straightMoveAction;
    }

    public LockOnAttack BeginLockOnAttack(int index = 0)
    {
        SetAttackAction(index);
        return m_lockOnMoveAction;
    }

    public int GetAnimationHashID()
    {
        return m_animationHashID;
    }

    public float GetAttackDistance(int index = 0)
    {
        index = index % m_attackActionList.Count;
        return m_attackActionList[index].attackDistance;
    }

    public float GetAnimationTransitionTime(int index = 0)
    {
        index = index % m_attackActionList.Count;
        return m_attackActionList[index].animationTransitionTime;
    }

    public void Animate(Animator anim, int index = 0)
    {
        anim.SetFloat(m_animationDriveList[index], m_attackActionList[index].animationDriveValue);
        anim.CrossFade(GetAnimationHashID(), GetAnimationTransitionTime(index), 0, 0.0f);
    }
}

class HitBoxEnabler
{
    delegate void EnableHitBoxAction(ScriptableAttackAction scriptableAttackAction, float t);

    EnableHitBoxAction m_enableHitBoxDelegate;
    public WeaponCollider weaponCollider;

    internal void Reset()
    {
        m_enableHitBoxDelegate = WaitForEnable;
    }

    internal void Tick(ScriptableAttackAction scriptableAttackAction, float t)
    {
        m_enableHitBoxDelegate.Invoke(scriptableAttackAction, t);
    }

    void WaitForEnable(ScriptableAttackAction scriptableAttackAction, float t)
    {
        if (t > scriptableAttackAction.hitEnableTime)
        {
            // weapon should Enable
            m_enableHitBoxDelegate = WaitForDisable;
            weaponCollider.isActive = true;
        }
    }

    void WaitForDisable(ScriptableAttackAction scriptableAttackAction, float t)
    {
        if (t > scriptableAttackAction.hitDisableTime)
        {
            // weapon should Disable
            m_enableHitBoxDelegate = WaitForEnd;
            weaponCollider.isActive = false;
        }
    }

    void WaitForEnd(ScriptableAttackAction scriptableAttackAction, float t) { }
}

public class StraightAttack : IEntityMoveAction
{
    ScriptableAttackAction m_scriptableAttackAction;

    Transform m_origin;
    Vector3 m_direction;

    HitBoxEnabler m_enableHitBox;

    public string actionName { get { return "StraightAttackAction"; } }

    public StraightAttack(Transform origin)
    {
        this.m_origin = origin;
        m_enableHitBox = new HitBoxEnabler();
    }

    public void SetDirection(Vector3 direction)
    {
        this.m_direction = direction;
    }

    public void SetAsAttackAction(ScriptableAttackAction scriptableAttackAction)
    {
        m_scriptableAttackAction = scriptableAttackAction;
    }

    public void SetWeaponCollider(WeaponCollider weaponCollider)
    {
        m_enableHitBox.weaponCollider = weaponCollider;
    }

    public float GetActionTime()
    {
        return m_scriptableAttackAction.moveTime;
    }

    public Vector3 GetDestination()
    {
        return m_origin.position + m_direction;
    }

    public float GetTimeSpeed()
    {
        return m_scriptableAttackAction.animationDriveValue;
    }

    public void BeginAction(IActionable actionableEntity, IEntity target)
    {
        if(target == null)
        {
            SetDirection(actionableEntity.GetActionHeading());
        }
        else
        {
            var toTarget = target.position - actionableEntity.position;
            SetDirection(toTarget.normalized);
        }

        m_enableHitBox.Reset();
    }

    public void PerformAction(IActionable actionableEntity, float t)
    {
        actionableEntity.ForceMovement(Vector3.ClampMagnitude(m_direction, m_scriptableAttackAction.velocityCurve.Evaluate(t)));

        m_enableHitBox.Tick(m_scriptableAttackAction, t);
    }

    public void EndAction(IActionable actionableEntity)
    {

    }

    public void CancelAction(IActionable actionableEntity)
    {
        m_enableHitBox.weaponCollider.isActive = false;
    }
}

public class LockOnAttack : IEntityMoveAction
{
    ScriptableAttackAction m_scriptableAttackAction;

    Transform m_origin;
    IEntity m_target;

    HitBoxEnabler m_enableHitBox;

    public string actionName { get { return "LockOnAttackAction"; } }

    public LockOnAttack(Transform origin)
    {
        this.m_origin = origin;
        m_enableHitBox = new HitBoxEnabler();
    }

    public void SetTarget(IEntity target)
    {
        this.m_target = target;
    }

    public void SetAsAttackAction(ScriptableAttackAction scriptableAttackAction)
    {
        m_scriptableAttackAction = scriptableAttackAction;
    }

    public void SetWeaponCollider(WeaponCollider weaponCollider)
    {
        m_enableHitBox.weaponCollider = weaponCollider;
    }

    public float GetActionTime()
    {
        return m_scriptableAttackAction.moveTime;
    }

    public Vector3 GetDestination()
    {
        Vector3 pos = m_target.position;
        Vector3 toTarget = pos - m_origin.position;

        return pos - toTarget.normalized * m_target.GetTargetRadius();
    }

    public float GetTimeSpeed()
    {
        return m_scriptableAttackAction.animationDriveValue;
    }

    public void BeginAction(IActionable actionableEntity, IEntity target)
    {
        SetTarget(target);

        m_enableHitBox.Reset();
    }

    public void PerformAction(IActionable actionableEntity, float t)
    {
        Vector3 toDestination = GetDestination() - actionableEntity.position;
        actionableEntity.ForceMovement(Vector3.ClampMagnitude(toDestination, m_scriptableAttackAction.velocityCurve.Evaluate(t)));

        m_enableHitBox.Tick(m_scriptableAttackAction, t);
    }

    public void EndAction(IActionable actionableEntity)
    {

    }

    public void CancelAction(IActionable actionableEntity)
    {
        m_enableHitBox.weaponCollider.isActive = false;
    }
}

