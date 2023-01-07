using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAttackTarget
{
    int GetTeam();
    Vector3 GetAttackTargetPosition();

    float GetRadius();
}
