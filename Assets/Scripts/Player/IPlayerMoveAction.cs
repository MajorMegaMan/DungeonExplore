using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlayerMoveAction
{
    float GetActionTime();
    Vector3 GetDestination();

    void PerformAction(PlayerController player, float t);
}