using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILockOnTargeter
{
    IEntity lockOnTarget { get; }

    public void SetLockOnTarget(IEntity lockOnTarget);
}

static class ILockOnTargeterExtensions
{
    public static bool IsLockedOn(this ILockOnTargeter targeter)
    {
        return targeter.lockOnTarget != null;
    }
}

namespace BBB.LockOn.Internal
{
    public class EmptyLockOnTargeter : ILockOnTargeter
    {
        public IEntity lockOnTarget { get; private set; }

        static EmptyLockOnTargeter _instance;
        public static EmptyLockOnTargeter instance { get { return _instance; } }

        static EmptyLockOnTargeter()
        {
            _instance = new EmptyLockOnTargeter();
        }

        void ILockOnTargeter.SetLockOnTarget(IEntity lockOnTarget)
        {
            this.lockOnTarget = lockOnTarget;
        }
    }
}
