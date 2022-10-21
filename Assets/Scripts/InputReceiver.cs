using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputReceiver : MonoBehaviour
{
    public abstract Vector3 GetMovement();

    public abstract Vector2 GetMouseLook();

    public abstract Vector2 GetGamepadLook();

    public abstract bool GetAttack();

    public abstract bool GetJump();
}
