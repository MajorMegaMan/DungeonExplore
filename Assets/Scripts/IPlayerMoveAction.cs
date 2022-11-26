using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerMoveAction
{
    float GetActionTime();
    Vector3 GetDestination();
}

public class StraightAttack : IPlayerMoveAction
{
    public float time = 1.0f;

    public Transform origin;
    public Vector3 direction;

    public StraightAttack(float time, Transform origin, Vector3 direction)
    {
        this.time = time;
        this.origin = origin;
        this.direction = direction;
    }

    public float GetActionTime()
    {
        return time;
    }

    public Vector3 GetDestination()
    {
        return origin.position + direction;
    }
}

public class LockOnAttack : IPlayerMoveAction
{
    public float time = 1.0f;
    public ILockOnTarget target;

    public LockOnAttack(float time, ILockOnTarget target)
    {
        this.time = time;
        this.target = target;
    }

    public float GetActionTime()
    {
        return time;
    }

    public Vector3 GetDestination()
    {
        return target.GetTargetPosition();
    }
}
