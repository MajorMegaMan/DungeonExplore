using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntityMoveAction
{
    float GetActionTime();
    Vector3 GetDestination();

    void BeginAction(IActionable actionableEntity, ILockOnTarget target);
    void PerformAction(IActionable actionableEntity, float t);
}