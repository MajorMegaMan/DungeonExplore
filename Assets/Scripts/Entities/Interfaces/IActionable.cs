using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IActionable
{
    // Called when an action Begins
    void BeginAction(IEntityMoveAction playerAction);

    // Called when an action Ends 
    void EndAction();

    // Called when an action is Cancelled
    void CancelAction();

    // Called when an action is switched to another action. Actionable Entities may need additional logic when using another action.
    void SwitchAction();

    Vector3 GetActionHeading();

    Transform GetActionTransform();

    public Vector3 position { get; }

    void ForceMovement(Vector3 moveDir);
}
