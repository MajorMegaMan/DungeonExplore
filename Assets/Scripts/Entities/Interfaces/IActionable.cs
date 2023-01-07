using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionable
{
    void BeginAction(IEntityMoveAction playerAction);

    void EndAction();

    Vector3 GetActionHeading();

    Transform GetActionTransform();

    public Vector3 position { get; }

    void ForceMovement(Vector3 moveDir);
}
