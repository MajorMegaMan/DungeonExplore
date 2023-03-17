using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEntityMoveAction
{
    string actionName { get; }
    float GetActionTime();
    Vector3 GetDestination();
    float GetTimeSpeed();

    void BeginAction(IActionable actionableEntity, IEntity target);
    void PerformAction(IActionable actionableEntity, float t);
    void CancelAction(IActionable actionableEntity);
}