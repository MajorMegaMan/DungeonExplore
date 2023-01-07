using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ILockOnTargeter
{
    ILockOnTarget lockOnTarget { get; set; }
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
        ILockOnTarget ILockOnTargeter.lockOnTarget { get; set; }

        static EmptyLockOnTargeter _instance;
        public static EmptyLockOnTargeter instance { get { return _instance; } }

        static EmptyLockOnTargeter()
        {
            _instance = new EmptyLockOnTargeter();
        }
    }
}
