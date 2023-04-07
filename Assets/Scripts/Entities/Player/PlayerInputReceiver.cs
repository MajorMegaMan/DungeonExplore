using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputReceiver : InputReceiver
{
    [SerializeField] Camera m_playerViewCamera = null;
    Transform m_viewInputTransform = null;
    [SerializeField] float m_minimumMovementMagnitude = 0.01f;

    public bool inputsDisabled = false;

    public Camera playerViewCamera 
    { 
        get 
        { 
            return m_playerViewCamera; 
        } 
        set 
        { 
            m_playerViewCamera = value; 
            if(m_playerViewCamera != null)
            {
                m_viewInputTransform = m_playerViewCamera.transform;
            }
            else
            {
                m_viewInputTransform = null;
            }
        } 
    }
    public Transform viewInputTransform { get { return m_viewInputTransform; } }

    private void Awake()
    {
        if (m_playerViewCamera != null)
        {
            m_viewInputTransform = m_playerViewCamera.transform;
        }
        else
        {
            m_viewInputTransform = null;
        }
    }

    #region ReadInputs
    public override Vector3 GetMovement()
    {
        if(inputsDisabled)
        {
            return Vector3.zero;
        }

        Vector3 moveInput = Vector3.zero;
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.z = Input.GetAxisRaw("Vertical");

        float mag = moveInput.magnitude;
        if(mag < m_minimumMovementMagnitude)
        {
            return Vector3.zero;
        }
        if(mag > 1.0f)
        {
            moveInput = moveInput / mag;
        }
        //moveInput = Vector3.ClampMagnitude(moveInput, 1.0f);

        return ConvertInput(moveInput);
    }

    public override Vector2 GetMouseLook()
    {
        if (inputsDisabled)
        {
            return Vector2.zero;
        }

        Vector2 mouseLook = Vector2.zero;
        mouseLook.x = Input.GetAxisRaw("Mouse X");
        mouseLook.y = Input.GetAxisRaw("Mouse Y");
        return mouseLook;
    }

    public override Vector2 GetGamepadLook()
    {
        if (inputsDisabled)
        {
            return Vector2.zero;
        }

        return Vector2.zero;
    }

    Vector3 ConvertInput(Vector3 input)
    {
        Vector3 result = m_viewInputTransform.TransformVector(input);
        result.y = 0.0f;
        result = result.normalized * input.magnitude;
        return result;
    }

    public override bool GetAttack()
    {
        if (inputsDisabled)
        {
            return false;
        }

        bool attackInput = Input.GetAxisRaw("Fire1") != 0;

        return attackInput;
    }

    public override bool GetJump()
    {
        if (inputsDisabled)
        {
            return false;
        }
        return Input.GetKeyDown(KeyCode.Space);
    }

    public bool GetLockOnDown()
    {
        if (inputsDisabled)
        {
            return false;
        }
        return Input.GetKeyDown(KeyCode.Tab);
    }
    #endregion // ! ReadInputs
}
